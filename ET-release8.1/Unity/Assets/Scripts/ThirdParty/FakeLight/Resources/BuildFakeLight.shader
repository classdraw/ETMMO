Shader "Hidden/DianDian/BuildFakeLight"
{
    HLSLINCLUDE

	#include "UnityCG.cginc"

	struct Attributes
	{
		uint vertexID     : SV_VertexID;
	};

	struct Varyings
	{
		float4 positionCS : SV_POSITION;
		float2 uv         : TEXCOORD0;
	};
	
    #define MAX_LIGHTS_PER_TILE 7
    #define TilesX 10
    #define MAX_TILE_DATA_NUM 700
	#define MAX_LIGHT_COUNT 32
	
	float2 _DstTex_PixelSize;//xy:PixelSize
	float4 _FakeLightsData[MAX_LIGHT_COUNT];
	float _LightTilesData[MAX_TILE_DATA_NUM];
    float2 _TileSize;
	float4 _FakeSpotLightFactors[MAX_LIGHT_COUNT];
	
	float4 GetFullScreenTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
	{
		// note: the triangle vertex position coordinates are x2 so the returned UV coordinates are in range -1, 1 on the screen.
		float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
		float4 pos = float4(uv * 2.0 - 1.0, z, 1.0);
	#ifdef UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION
		pos = ApplyPretransformRotation(pos);
	#endif
		return pos;
	}

	float2 GetFullScreenTriangleTexCoord(uint vertexID)
	{
	#if UNITY_UV_STARTS_AT_TOP
		return float2((vertexID << 1) & 2, 1.0 - (vertexID & 2));
	#else
		return float2((vertexID << 1) & 2, vertexID & 2);
	#endif
	}

	Varyings vert(Attributes input)
	{
		Varyings output;
		output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
		output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
		return output;
	}

	int GetTileLightCount(int tileIndex)
    {
        int lightCount = 0;
        for (int i = 0; i < MAX_LIGHTS_PER_TILE; i++)
        {
            lightCount += _LightTilesData[tileIndex + i] >= 0 ? 1 : 0;
        }

        return lightCount;
    }
	
    half GetLightIndex(int lightIndex, float2 pixel)
	{
		float currentLightIndex = _FakeLightsData[lightIndex].w;
		float2 lightPixelPos = _FakeLightsData[lightIndex].xy;
		float2 lightVector = lightPixelPos - pixel;
		float distanceSqr = dot(lightVector, lightVector);
		
		float4 fakeSpotLightFactor = _FakeSpotLightFactors[lightIndex];
		float mask = 0;
		
		if(fakeSpotLightFactor.w >= 0)//Spot
		{
			float2 endPoint = fakeSpotLightFactor.xy;
			float2 startEndVector = lightPixelPos - endPoint;
			float cosAngle = dot(normalize(lightVector), normalize(startEndVector));
			float withinAngle = step(fakeSpotLightFactor.z, cosAngle) * step(distanceSqr, dot(startEndVector, startEndVector));

			float2 endVector = float2(pixel.x, pixel.y) - endPoint;
			float withinRadius = step(dot(endVector, endVector), fakeSpotLightFactor.w * fakeSpotLightFactor.w);
			mask = step(1, withinAngle + withinRadius);
		}
		else
		{
			float lightAttenuation = _FakeLightsData[lightIndex].z;
			
			half factor = half(distanceSqr * lightAttenuation);
			mask = step(factor, 1);
		}

		return currentLightIndex * mask;
	}

	half4 fragIndex(Varyings input) : SV_Target
    {
 
        half4 finalIndex = 0;
        float2 pixel = input.uv * _DstTex_PixelSize.xy;
    
        int2 tileID = int2(pixel / _TileSize);
    	int tileIndex = (tileID.y * TilesX + tileID.x) * MAX_LIGHTS_PER_TILE;
 
    	int lightCount = GetTileLightCount(tileIndex);
    	
		if(lightCount <= 0)
			return 0;
 
        half indices[4] = {0, 0, 0, 0};
 
        int channel = 0;
        for(int i = 0; i < lightCount && channel < 4; i++)
        {
        	int index = _LightTilesData[tileIndex + i];
            half lightIndex = GetLightIndex(index, pixel); 
            half mask = (lightIndex > 0 ? 1 : 0) * step(channel, 3);
			indices[channel] += lightIndex * mask;
			channel += mask;
        }
    	
        finalIndex = half4(indices[0], indices[1], indices[2], indices[3]);
        return finalIndex;
    }
	
	ENDHLSL

	SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

		Pass
        {
            Name "BuildFakeLightIndex"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragIndex
            ENDHLSL
        }
    }
}
