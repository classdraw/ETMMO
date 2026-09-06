using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CenturyGame.FakeLight
{
    public enum MaxTextureResolution
    {
        _64 = 64,
        _128 = 128,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048
    }

    public enum FakeLightDebugMode
    {
        Disable,
        Tile,
        Pixel
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class FakeLightManager : MonoBehaviour
    {
        public Camera Camera { get => camera; set { MarkDirty(); camera = value; } }
        public MaxTextureResolution MaxResolution { get => maxResolution; set { MarkDirty(); maxResolution = value; } }
        public bool Dirty { get => isDirty; }

        private new Camera camera;
        [SerializeField] private MaxTextureResolution maxResolution = MaxTextureResolution._512;

        private CommandBuffer cmd;
        private RenderTexture fakeLightRT;
        private Material buildFakeLightMaterial;
        private Vector3 fakeLightRegionStartPos = new Vector3(-256, 0, -256);
        private Vector3 fakeLightRegion = new Vector3(512, 50, 512);
        private bool isDirty = true;
        private Plane[] cameraPlanes = new Plane[6];
        private List<FakeLight> visibleActiveLights = new List<FakeLight>();
        private int lastVisibleLightsHash = 0;
        private float pixelRate;
        private float tileSizeX;
        private float tileSizeY;

        private float[] tileVectors = new float[MAX_TILE_DATA_NUM];
        private Vector4[] fakeLightBuffer = new Vector4[MaxLightCount];
        private Vector4[] fakeLightPoses = new Vector4[MaxLightCount];//w:lightAttenuation
        private Vector4[] fakeLightColors = new Vector4[MaxLightCount];//w:intensity
        private float[] fakeLightRanges = new float[MaxLightCount];
        private Vector4[] fakeSpotLightAttens = new Vector4[MaxLightCount];
        private Vector4[] fakeSpotLightDirs = new Vector4[MaxLightCount];
        private Vector4[] fakeSpotLightFactors = new Vector4[MaxLightCount];

        private float[] tileLightCount = new float[TILE_COUNT];
        //Debug
#if UNITY_EDITOR
        public FakeLightDebugMode DebugMode { get => debugMode; set { debugMode = value; } }
        [SerializeField] private FakeLightDebugMode debugMode = FakeLightDebugMode.Disable;
        private int[] debugTileLightIndex = new int[TILE_COUNT * MaxLightCount];
        private ComputeBuffer debugTileLightIndexBuffer;
        private RenderTexture fakeLightDebugRT;
        private Material debugFakeLightMaterial;
#endif
        
        const int MaxLightCount = 32;
        const int MAX_TILE_DATA_NUM = 700;
        const int TILE_COUNT = 100;
        const int TILE_LIGHT_MAX = 7;
        const int DEBUG_LIGHT_MAX = 4;
        const int tilesX = 10, tilesY = 10;

        private void Awake()
        {
            var shader = Shader.Find("Hidden/DianDian/BuildFakeLight");
            buildFakeLightMaterial = new Material(shader);
#if UNITY_EDITOR
            var debugShader = Shader.Find("Hidden/DianDian/FakeLightDebug");
            debugFakeLightMaterial = new Material(debugShader);
#endif
            camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            MarkDirty();
            SetRenderCallback();
        }

        private void OnDisable()
        {
            ClearRenderCallback();
            DestroyLightRT();
            
#if UNITY_EDITOR
            if (debugTileLightIndexBuffer != null) {
                debugTileLightIndexBuffer.Release();
                debugTileLightIndexBuffer = null;
            }

            debugMode = FakeLightDebugMode.Disable;
            FakeLightDebug();
#endif
        }

        private void OnDestroy()
        {
            UnityDestroy(buildFakeLightMaterial);
            buildFakeLightMaterial = null;
            
#if UNITY_EDITOR
            UnityDestroy(debugFakeLightMaterial);
            debugFakeLightMaterial = null;
            if (debugTileLightIndexBuffer != null)
            {
                debugTileLightIndexBuffer.Release();
                debugTileLightIndexBuffer = null;
            }
            debugMode = FakeLightDebugMode.Disable;
            FakeLightDebug();
#endif
            
            if (cmd != null)
                cmd.Dispose();
            cmd = null;
        }

        private void Update()
        {
            CollectLights();
            int visibleLightsHash = GetVisibleLightsHash();
            if (lastVisibleLightsHash != visibleLightsHash)
            {
                MarkDirty();
                lastVisibleLightsHash = visibleLightsHash;
            }
            if (Dirty)
                UpdateLightsRegion();
            GenerateLightRT();
            if (Dirty)
            {
                CullLights();
                DrawLightRT();
                isDirty = false;
            }

            SetupShaderProperties();
        }

        private void OnValidate()
        {
            MarkDirty();
            
#if UNITY_EDITOR
            FakeLightDebug();
#endif
        }
        
#if UNITY_EDITOR
        private void FakeLightDebug()
        {
            if (DebugMode == FakeLightDebugMode.Tile)
                Shader.EnableKeyword("FAKELIGHT_TILE_DEBUG");
            else
                Shader.DisableKeyword("FAKELIGHT_TILE_DEBUG");
            
            if (DebugMode == FakeLightDebugMode.Pixel)
                Shader.EnableKeyword("FAKELIGHT_PIXEL_DEBUG");
            else 
                Shader.DisableKeyword("FAKELIGHT_PIXEL_DEBUG");
        }
#endif
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            
            float tileWidth = fakeLightRegion.x / tilesX;
            float tileHeight = fakeLightRegion.z / tilesY;
            
            for (int i = 0; i <= tilesY; i++)
            {
                float z = fakeLightRegionStartPos.z + i * tileHeight;
                Vector3 start = new Vector3(fakeLightRegionStartPos.x, fakeLightRegionStartPos.y, z);
                Vector3 end = new Vector3(fakeLightRegionStartPos.x + fakeLightRegion.x, fakeLightRegionStartPos.y, z);
                Gizmos.DrawLine(start, end);
            }

            for (int i = 0; i <= tilesX; i++)
            {
                float x = fakeLightRegionStartPos.x + i * tileWidth;
                Vector3 start = new Vector3(x, fakeLightRegionStartPos.y, fakeLightRegionStartPos.z);
                Vector3 end = new Vector3(x, fakeLightRegionStartPos.y, fakeLightRegionStartPos.z + fakeLightRegion.z);
                Gizmos.DrawLine(start, end);
            }

            for (int i = 0; i < tileLightCount.Length; i++)
            {
                int tileY = i / tilesX;
                int tileX = i % tilesX;
                    
                float tileStartX = fakeLightRegionStartPos.x + tileX * tileWidth;
                float tileStartZ = fakeLightRegionStartPos.z + tileY * tileHeight;
                float tileEndX = tileStartX + tileWidth;
                float tileEndZ = tileStartZ + tileHeight;
                
                Vector3 leftBottom = new Vector3(tileStartX, fakeLightRegionStartPos.y, tileStartZ);
                Vector3 leftTop = new Vector3(tileStartX, fakeLightRegionStartPos.y, tileEndZ);
                Vector3 rightBottom = new Vector3(tileEndX, fakeLightRegionStartPos.y, tileStartZ);
                Vector3 rightTop = new Vector3(tileEndX, fakeLightRegionStartPos.y, tileEndZ);
                if (tileLightCount[i] < DEBUG_LIGHT_MAX && tileLightCount[i] > 0)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(leftBottom, leftTop);
                    Gizmos.DrawLine(leftBottom, rightBottom);
                    Gizmos.DrawLine(leftTop, rightTop);
                    Gizmos.DrawLine(rightBottom, rightTop);
                }
                else if (tileLightCount[i] == DEBUG_LIGHT_MAX)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(leftBottom, leftTop);
                    Gizmos.DrawLine(leftBottom, rightBottom);
                    Gizmos.DrawLine(leftTop, rightTop);
                    Gizmos.DrawLine(rightBottom, rightTop);
                }
                else if (tileLightCount[i] > DEBUG_LIGHT_MAX)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(leftBottom, leftTop);
                    Gizmos.DrawLine(leftBottom, rightBottom);
                    Gizmos.DrawLine(leftTop, rightTop);
                    Gizmos.DrawLine(rightBottom, rightTop);
                }
            }
        }

        public void MarkDirty()
        {
            isDirty = true;
        }

        void CollectLights()
        {
            FakeLightListManager.Instance.OrderUpdate();

            visibleActiveLights.Clear();
            if (camera != null)
            {
                int cameraCullingMask = camera.cullingMask;
                
                GeometryUtility.CalculateFrustumPlanes(camera, cameraPlanes);
                for (int i = 0; i < FakeLightListManager.Instance.activeLights.Count && visibleActiveLights.Count < MaxLightCount; ++i)
                {
                    FakeLight fakeLight = FakeLightListManager.Instance.activeLights[i];
                    
                    if ((cameraCullingMask & (1 << fakeLight.gameObject.layer)) == 0)
                        continue;
                    
                    if (GeometryUtility.TestPlanesAABB(cameraPlanes, fakeLight.bounds))
                    {
                        visibleActiveLights.Add(fakeLight);
                    }
                }
            }
            else
            {
                for (int i = 0; i < FakeLightListManager.Instance.activeLights.Count && visibleActiveLights.Count < MaxLightCount; ++i)
                {
                    FakeLight fakeLight = FakeLightListManager.Instance.activeLights[i];
                    visibleActiveLights.Add(fakeLight);
                }
            }
        }

        int GetVisibleLightsHash()
        {
            int hash = HashCode.Combine(visibleActiveLights.Count);
            foreach (var light in visibleActiveLights)
            {
                Vector3 lightPos = light.transform.position;
                Quaternion lightRot = light.transform.rotation;
                hash = HashCode.Combine(hash, lightPos.x, lightPos.y, lightPos.z);
                hash = HashCode.Combine(hash, lightRot.x, lightRot.y, lightRot.z);
                hash = HashCode.Combine(hash, light.lightType, light.range, light.priority);
                hash = HashCode.Combine(hash, light.innerSpotAngle, light.outerSpotAngle);
            }

            return hash;
        }

        void GenerateLightRT()
        {
            (int texWidth, int texHeight) = CalculateTextureData();

            if (fakeLightRT == null && (texWidth == 0 || texHeight == 0))
                return;

            if (fakeLightRT == null || texWidth != fakeLightRT.width || texHeight != fakeLightRT.height)
            {
                DestroyLightRT();

                if (texWidth == 0 || texHeight == 0)
                    return;

                var desc = new RenderTextureDescriptor(texWidth, texHeight)
                {
                    graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm,
                    depthBufferBits = 0,
                    msaaSamples = 1,
                    sRGB = false,
                    enableRandomWrite = false,
                    useMipMap = false,
                    autoGenerateMips = false,
                    dimension = TextureDimension.Tex2D,
                };
                fakeLightRT = new RenderTexture(desc);
                fakeLightRT.name = "Fake Light Index Texture";
                fakeLightRT.filterMode = FilterMode.Point;
                fakeLightRT.wrapMode = TextureWrapMode.Clamp;
                fakeLightRT.Create();
#if UNITY_EDITOR
                fakeLightDebugRT = new RenderTexture(desc);
                fakeLightDebugRT.name = "Fake Light Debug Texture";
                fakeLightDebugRT.filterMode = FilterMode.Point;
                fakeLightDebugRT.wrapMode = TextureWrapMode.Clamp;
                fakeLightDebugRT.Create();
#endif
            }
        }

        private void UpdateLightsRegion()
        {
            if (visibleActiveLights.Count == 0)
            {
                fakeLightRegionStartPos = new Vector3(0, 0, 0);
                fakeLightRegion = new Vector3(0, 0, 0);
                return;
            }
            Vector3 minPos = Vector3.positiveInfinity;
            Vector3 maxPos = Vector3.negativeInfinity;

            for (int i = 0; i < visibleActiveLights.Count; i++)
            {
                FakeLight light = visibleActiveLights[i];
                Bounds lightBounds = light.bounds;

                minPos = Vector3.Min(minPos, lightBounds.min);
                maxPos = Vector3.Max(maxPos, lightBounds.max);
            }

            Vector3 newRegionStart = new Vector3(Mathf.Floor(minPos.x - 1f), 0, Mathf.Floor(minPos.z - 1f));
            Vector3 newRegion = new Vector3(Mathf.Ceil(maxPos.x - newRegionStart.x + 1f), 0, Mathf.Ceil(maxPos.z - newRegionStart.z + 1f));

            fakeLightRegionStartPos = newRegionStart;
            fakeLightRegion = newRegion;
        }

        private (int texWidth, int texHeight) CalculateTextureData()
        {
            int texWidth;
            int texHeight;
            if (fakeLightRegion.x > (int)MaxResolution || fakeLightRegion.z > (int)MaxResolution)
            {
                int resolution = (int)MaxResolution;
                texWidth = resolution;
                texHeight = resolution;
                if (fakeLightRegion.x > fakeLightRegion.z)
                {
                    float ratio = fakeLightRegion.z / fakeLightRegion.x;
                    texHeight = Mathf.CeilToInt(resolution * ratio);
                    pixelRate = texWidth / fakeLightRegion.x;
                }
                else
                {
                    float ratio = fakeLightRegion.x / fakeLightRegion.z;
                    texWidth = Mathf.CeilToInt(resolution * ratio);
                    pixelRate = texHeight / fakeLightRegion.z;
                }
            }
            else
            {
                texWidth = (int)fakeLightRegion.x;
                texHeight = (int)fakeLightRegion.z;
                pixelRate = 1.0f;
            }

            tileSizeX = texWidth / 10.0f;
            tileSizeY = texHeight / 10.0f;
            return (texWidth, texHeight);
        }

        void CullLights()
        {
            if (visibleActiveLights.Count == 0)
                return;
            
            for (int k = 0; k < tileVectors.Length; k++)
            {
                tileVectors[k] = -1;
            }

            for (int k = 0; k < tileLightCount.Length; k++)
            {
                tileLightCount[k] = 0;
            }

#if UNITY_EDITOR
            if (debugTileLightIndexBuffer == null || debugTileLightIndexBuffer.count != TILE_COUNT * MaxLightCount)
            {
                if (debugTileLightIndexBuffer != null)
                    debugTileLightIndexBuffer.Release();
                debugTileLightIndexBuffer = new ComputeBuffer(TILE_COUNT * MaxLightCount, sizeof(int));
            }
#endif
            
            for (int i = 0; i < visibleActiveLights.Count; ++i)
            {
                FakeLight fakeLight = visibleActiveLights[i];
                Vector3 lightPos = fakeLight.transform.position;
                Vector3 lightOffset = lightPos - fakeLightRegionStartPos;
                lightOffset *= pixelRate;

                // 计算灯光在纹理空间的位置和范围
                Vector2 lightTexPos = new Vector2(lightOffset.x, lightOffset.z);
                float lightTexRange = fakeLight.range * pixelRate;
                
                fakeLightBuffer[i].x = lightTexPos.x;
                fakeLightBuffer[i].y = lightTexPos.y;
                fakeLightBuffer[i].z = 1.0f / (lightTexRange * lightTexRange);
                fakeLightBuffer[i].w = (float)(i + 1) / 255;
                fakeLightRanges[i] = fakeLight.range;
                fakeLightPoses[i] = lightPos;
                fakeLightPoses[i].w = 1.0f / (fakeLight.range * fakeLight.range);
                
                Bounds lightBounds = fakeLight.bounds;
                
                Vector3 minPos = (lightBounds.min - fakeLightRegionStartPos)* pixelRate;
                Vector3 maxPos = (lightBounds.max - fakeLightRegionStartPos)* pixelRate;

                float cosOuter = 0;
                float halfAngle = 0;
                
                if (visibleActiveLights[i].lightType == FakeLight.FakeLightType.Point)
                {
                    fakeSpotLightAttens[i] = new Vector4(-1, -1, -1, -1);
                    fakeSpotLightFactors[i] = new Vector4(-1, -1, -1, -1);
                }
                else
                {
                    halfAngle = Mathf.Deg2Rad * 0.5f * fakeLight.outerSpotAngle;
                    cosOuter = Mathf.Cos(halfAngle);
                    float cosInner = Mathf.Cos(Mathf.Deg2Rad * 0.5f * fakeLight.innerSpotAngle);

                    float smoothAngleRange = Mathf.Max(0.001f, cosInner - cosOuter);
                    float invAngleRange = 1.0f / smoothAngleRange;
                    float add = -cosOuter * invAngleRange;
                    fakeSpotLightAttens[i].x = invAngleRange;
                    fakeSpotLightAttens[i].y = add;

                    Vector3 spotLightDir = fakeLight.transform.forward;
                    fakeSpotLightDirs[i] = -spotLightDir;
                    
                    float sinMaskAngle = Mathf.Sin(halfAngle);
                    float cosMaskAngle = Mathf.Cos(halfAngle + 30 * Mathf.Deg2Rad);//扩大一点范围
                    float maskRadius = lightTexRange * sinMaskAngle + 2;
                    Vector3 fakeSpotLightEndPoint = lightPos + spotLightDir * fakeLight.range * cosOuter;
                    Vector3 endPointTex = (fakeSpotLightEndPoint - fakeLightRegionStartPos) * pixelRate;
                    fakeSpotLightFactors[i] = new Vector4(endPointTex.x, endPointTex.z, cosMaskAngle, maskRadius);
                }
               
                int minTileX = Mathf.Max(0, (int)(minPos.x / tileSizeX));
                int maxTileX = Mathf.Min(tilesX - 1, (int)(maxPos.x / tileSizeX));
                int minTileY = Mathf.Max(0, (int)(minPos.z / tileSizeY));
                int maxTileY = Mathf.Min(tilesY - 1, (int)(maxPos.z / tileSizeY));
                
                for (int tileY = minTileY; tileY <= maxTileY; tileY++)
                {
                    for (int tileX = minTileX; tileX <= maxTileX; tileX++)
                    {
                        if (IsLightIntersectTile(lightTexPos, lightTexRange, tileX, tileY, fakeLight, minPos, maxPos))
                        {
                            int tileIndex = tileY * tilesX + tileX;
                            
                            int index = tileIndex * TILE_LIGHT_MAX;
                            int lightCount = (int)tileLightCount[tileIndex];
                            if (lightCount < TILE_LIGHT_MAX)
                            {
                                int lightIndex = index + lightCount;
                                //排序
                                int changeIndex = -1;

                                if (lightCount == 0)
                                {
                                    tileVectors[lightIndex] = i;
                                }
                                else
                                {
                                    for (int j = 0; j < lightCount; j++)
                                    {
                                        int preLightIndex = index + j;
                                        int preLight = (int)tileVectors[preLightIndex];
                                        if (visibleActiveLights[preLight].priority < visibleActiveLights[i].priority)
                                        {
                                            changeIndex = preLightIndex;
                                            break;
                                        }
                                    }
                                    if (changeIndex != -1)
                                    {
                                        for (int k = changeIndex + 1; k <= lightIndex; k++)
                                        {
                                            tileVectors[k] = tileVectors[(k - 1)];
                                        }
                                        tileVectors[changeIndex] = i;
                                    }
                                    else
                                    {
                                        tileVectors[lightIndex] = i;
                                    }
                                }
                            }
#if UNITY_EDITOR
                            debugTileLightIndex[tileIndex * MaxLightCount + lightCount] = i;
#endif
                            tileLightCount[tileIndex]++;
                        }
                    }
                }
            }
#if UNITY_EDITOR
            debugTileLightIndexBuffer.SetData(debugTileLightIndex);
#endif
        }

        private bool IsLightIntersectTile(Vector2 lightTexPos, float lightTexRange, int tileX, int tileY, FakeLight fakeLight, Vector3 minPos, Vector3 maxPos)
        {
            float tileStartX = tileX * tileSizeX;
            float tileStartY = tileY * tileSizeY;
            float tileEndX = tileStartX + tileSizeX;
            float tileEndY = tileStartY + tileSizeY;
            
            if (fakeLight.lightType == FakeLight.FakeLightType.Point)
            {
                // 检查光源是否完全在tile外部
                float closestX = Mathf.Clamp(lightTexPos.x, tileStartX, tileEndX);
                float closestY = Mathf.Clamp(lightTexPos.y, tileStartY, tileEndY);

                float distanceSquared = (closestX - lightTexPos.x) * (closestX - lightTexPos.x) +
                                        (closestY - lightTexPos.y) * (closestY - lightTexPos.y);

                return distanceSquared < (lightTexRange * lightTexRange);
            }
            else
            {
                if (tileEndX <= minPos.x || tileStartX >= maxPos.x || tileStartY >= maxPos.z || tileEndY <= minPos.z)
                    return false;

                return true;
            }
        }

        void DrawLightRT()
        {
            if (visibleActiveLights.Count == 0)
                return;
            
            if (cmd == null)
                cmd = new CommandBuffer();
            cmd.Clear();
            cmd.SetGlobalVectorArray("_FakeLightsData", fakeLightBuffer);
            cmd.SetGlobalVectorArray("_FakeSpotLightFactors", fakeSpotLightFactors);
            cmd.SetGlobalVector("_DstTex_PixelSize", new Vector4(fakeLightRT.width, fakeLightRT.height));
            cmd.SetGlobalVector("_TileSize", new Vector2(tileSizeX, tileSizeY));
            cmd.SetGlobalFloatArray("_LightTilesData", tileVectors);
            cmd.SetRenderTarget(fakeLightRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.DrawProcedural(Matrix4x4.identity, buildFakeLightMaterial, 0, MeshTopology.Triangles, 3, 1, null);
#if UNITY_EDITOR
            if (debugTileLightIndexBuffer != null)
            {
                cmd.SetGlobalFloatArray("_TileLightCount", tileLightCount);
                cmd.SetGlobalBuffer("_DebugTileLightIndexBuffer", debugTileLightIndexBuffer);
                cmd.SetRenderTarget(fakeLightDebugRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmd.DrawProcedural(Matrix4x4.identity, debugFakeLightMaterial, 0, MeshTopology.Triangles, 3, 1, null);
            }
#endif
            Graphics.ExecuteCommandBuffer(cmd);
        }

        void SetupShaderProperties()
        {
            if (visibleActiveLights.Count == 0 || fakeLightRT == null)
            {
                Shader.DisableKeyword("_FAKE_ADDITIONAL_LIGHTS");
                Shader.SetGlobalTexture("_FakeLightTexture", Texture2D.blackTexture);
#if UNITY_EDITOR
                Shader.SetGlobalTexture("_DebugFakeLightTexture", Texture2D.blackTexture);
#endif
            }
            else
            {
                for (int i = 0; i < visibleActiveLights.Count; i++)
                {
                    Color fakeLightLinearColor = visibleActiveLights[i].color.linear;
                    float intensity = visibleActiveLights[i].intensity;

                    fakeLightColors[i] = new Vector4(fakeLightLinearColor.r * intensity, fakeLightLinearColor.g * intensity, fakeLightLinearColor.b * intensity, 1);
                }

                Shader.SetGlobalVectorArray("_FakeLightColors", fakeLightColors);
                Shader.SetGlobalTexture("_FakeLightTexture", fakeLightRT);

                Shader.SetGlobalVector("_FakeLightTexUVRate", new Vector4(1.0f / fakeLightRegion.x, 1.0f / fakeLightRegion.z, 0, 0));
                Shader.SetGlobalVector("_FakeLightRegionStart", new Vector4(fakeLightRegionStartPos.x, fakeLightRegionStartPos.y, fakeLightRegionStartPos.z, 0));

                Shader.SetGlobalVectorArray("_FakeLightPoses", fakeLightPoses);
                Shader.SetGlobalFloatArray("_FakeLightRanges", fakeLightRanges);

                Shader.SetGlobalVectorArray("_FakeSpotLightDirs", fakeSpotLightDirs);
                Shader.SetGlobalVectorArray("_FakeSpotLightAttens", fakeSpotLightAttens);

                Shader.EnableKeyword("_FAKE_ADDITIONAL_LIGHTS");
#if UNITY_EDITOR
                Shader.SetGlobalTexture("_DebugFakeLightTexture", fakeLightDebugRT);
#endif
            }
        }

        void DestroyLightRT()
        {
            UnityDestroy(fakeLightRT);
            fakeLightRT = null;
#if UNITY_EDITOR
            UnityDestroy(fakeLightDebugRT);
            fakeLightDebugRT = null;
#endif
        }

        void UnityDestroy(UnityEngine.Object obj)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying && !UnityEditor.EditorApplication.isPaused)
                    UnityEngine.Object.Destroy(obj);
                else
                    UnityEngine.Object.DestroyImmediate(obj);
#else
                UnityEngine.Object.Destroy(obj);
#endif
            }
        }

        void SetRenderCallback()
        {
            if (IsSRP)
            {
                RenderPipelineManager.beginCameraRendering += BeforeRender;
            }
            else
            {
                Camera.onPreRender += BeforeRender;
                Camera.onPostRender += AfterRender;
            }
        }

        void ClearRenderCallback()
        {
            if (IsSRP)
            {
                RenderPipelineManager.beginCameraRendering -= BeforeRender;
            }
            else
            {
                Camera.onPreRender -= BeforeRender;
                Camera.onPostRender -= AfterRender;
            }
        }

        private void BeforeRender(ScriptableRenderContext context, Camera cam)
        {
            BeforeRender(cam);
        }

        public void BeforeRender(Camera cam)
        {
            if (cam != camera)
                return;

            SetupShaderProperties();
        }

        public void AfterRender(Camera cam)
        {
            if (cam != camera)
                return;

            if (cmd == null)
                cmd = new CommandBuffer();
            cmd.Clear();

            cmd.DisableShaderKeyword("_FAKE_ADDITIONAL_LIGHTS");
            cmd.SetGlobalTexture("_FakeLightTexture", Texture2D.blackTexture);

            Graphics.ExecuteCommandBuffer(cmd);
        }

        private bool IsSRP => GraphicsSettings.currentRenderPipeline != null;
    }
}
