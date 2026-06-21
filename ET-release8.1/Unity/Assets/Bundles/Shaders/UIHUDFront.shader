Shader "Unlit/HUDFont"
{
	Properties
	{
		_MainTex ("Alpha (A)", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_ReverseY("ReverseY", Float) = 1.0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
			"UniversalMaterialType" = "Unlit"
		}

		Cull Front
		ZWrite Off
		ZTest Off
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "UnlitForward"
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#pragma target 3.5
			#pragma prefer_hlslcc gles

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				half4 _Color;
				half _ReverseY;
			CBUFFER_END

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			struct Attributes
			{
				float4 positionOS : POSITION;
				half4 color : COLOR;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				half4 color : COLOR;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert(Attributes v)
			{
				Varyings o;
				float fScale = min(13.34 / _ScreenParams.x, 7.5 / _ScreenParams.y);
				float2 uvOffset = v.uv2;
				uvOffset.x *= fScale;
				uvOffset.y *= fScale;

				float3 right = UNITY_MATRIX_IT_MV[0].xyz;
				float3 up = UNITY_MATRIX_IT_MV[1].xyz;
				float3 vPos = v.positionOS.xyz + uvOffset.x * right + uvOffset.y * up;
				float4 vFinal = float4(vPos, 1.0);
				o.positionCS = TransformObjectToHClip(vFinal.xyz);

				o.color = v.color * _Color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}

			half4 frag(Varyings i) : SV_Target
			{
				half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				// 字体只取贴图 Alpha 做遮罩，颜色完全由顶点色决定
				half4 col;
				col.rgb = i.color.rgb;
				col.a = tex.a * i.color.a;
				return col;
			}
			ENDHLSL
		}
	}
	Fallback Off
}
