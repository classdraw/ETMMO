using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SSCS {

    public enum MaskTextureBrushMode {
        [InspectorName("Restore Shadows (Shift+C)")]
        RestoreShadows = 0,
        [InspectorName("Remove Shadows (Shift+X)")]
        RemoveShadows = 1
    }

    public enum CloudShadowApplication {
        [InspectorName("Screen Space Overlay")]
        ScreenSpace = 0,
        [InspectorName("Shader Integration (GetShadowAttenuation)")]
        ShaderIntegration = 1
    }

    [ExecuteAlways]
    [HelpURL("https://kronnect.com/guides-category/cloud-shadows-fx/")]
    public class CloudShadows : MonoBehaviour {

        [Tooltip("Which light should be used as the Sun")]
        public Light sun;

        [Tooltip("Which cameras can render this cloud shadow effect")]
        public LayerMask cameraLayerMask = -1;

        [Tooltip("Render queue for the shadows. Clouds render at render queue + 1.")]
        public int renderQueue = 3001;

        [Tooltip("Screen Space: fullscreen multiply after the scene. Shader Integration: GetShadowAttenuation() in ground/tree shaders.")]
        public CloudShadowApplication shadowApplication = CloudShadowApplication.ShaderIntegration;

        public CloudShadowsProfile profile;

        public bool coverageMask;
        public Texture2D coverageMaskTexture;
        public Vector3 coverageMaskWorldSize = new Vector3(2000, 0, 2000);
        public Vector3 coverageMaskWorldCenter = new Vector3(0, 0, 0);
        public bool maskEditorEnabled;
        public int maskTextureResolution = 1024;
        public MaskTextureBrushMode maskBrushMode = MaskTextureBrushMode.RemoveShadows;

        public bool useCustomBounds;
        public Bounds bounds = new Bounds(Vector3.zero, new Vector3(1000, 10, 1000));

        [Range(1, 128)]
        public float maskBrushWidth = 25;

        [Range(0, 1)]
        public float maskBrushFuzziness = 0.5f;

        [Range(0, 1)]
        public float maskBrushOpacity = 0.5f;


        static class ShaderParams {
            public const string ShadowsShaderName = "Hidden/Kronnect/SSCS/ScreenSpaceCloudShadows";
            public const string CloudsShaderName = "Hidden/Kronnect/SSCS/ScreenSpaceClouds";

            public static int CloudsTex = Shader.PropertyToID("_CloudsTex");
            public static int Clouds2Tex = Shader.PropertyToID("_Clouds2Tex");
            public static int CloudsBumpMap = Shader.PropertyToID("_CloudsBumpMap");
            public static int CoverageMaskTex = Shader.PropertyToID("_CoverageMask");

            public static int ShadowColor = Shader.PropertyToID("_ShadowColor");
            public static int CloudsDarkColor = Shader.PropertyToID("_CloudsDarkColor");
            public static int CloudsLightColor = Shader.PropertyToID("_CloudsLightColor");
            public static int LightColor = Shader.PropertyToID("_LightColor");

            public static int LightDir = Shader.PropertyToID("_SSCS_LightDir");
            public static int Offsets = Shader.PropertyToID("_Offsets");

            public static int Layer1CloudsData = Shader.PropertyToID("_CloudsLayer1");
            public static int Layer1CloudsData2 = Shader.PropertyToID("_SSCS_CloudsLayer1Extra");
            public static int Layer1CloudsData3 = Shader.PropertyToID("_CloudsLayer2Extra");
            public static int Layer1CloudsData4 = Shader.PropertyToID("_CloudsLayer3Extra");
            public static int ShadowsExtraData = Shader.PropertyToID("_ShadowsExtraData");

            public static int CoverageMaskWorldData = Shader.PropertyToID("_MaskWorldData");
            public static int BlendSrc = Shader.PropertyToID("_BlendSrc");
            public static int BlendDst = Shader.PropertyToID("_BlendDst");

            public static int BoundsData = Shader.PropertyToID("_BoundsData");

            public const string SKW_CLOUDS_THICK = "_CLOUDS_THICK";
            public const string SKW_CLOUDS_ANISOTROPY = "_CLOUDS_ANISOTROPY";
            public const string SKW_SHADOWS_3D = "_SHADOWS_3D";
            public const string SKW_SHADOWS_3D_HQ = "_SHADOWS_3D_HQ";
            public const string SKW_COVERAGE_MASK = "_SHADOWS_COVERAGE_MASK";
            public const string SKW_COVERAGE_MASK_DEBUG = "_SHADOWS_COVERAGE_MASK_DEBUG";
            public const string SKW_ORTHOGRAPHIC = "_ORTHOGRAPHIC";
            public const string SKW_ORTHOGRAPHIC_OVERLAY = "_ORTHOGRAPHIC_OVERLAY";
            public const string SKW_PRESERVE_DIRECTIONAL_SHADOWS = "_PRESERVE_DIRECTIONAL_SHADOWS";
            public const string SKW_BOUNDS = "_BOUNDS";
            public const string SKW_RECEIVE = "_SSCS_RECEIVE";
            public const string SKW_SHOW_CLOUDS = "_SSCS_SHOW_CLOUDS";
            public const string SKW_SHOW_SHADOWS = "_SSCS_SHOW_SHADOWS";
        }


        Material matShadows, matClouds;
        Vector4 windOffset1;
        static readonly List<CloudShadows> s_ActiveInstances = new List<CloudShadows>();

        public static bool HasActiveInstances => s_ActiveInstances.Count > 0;

        [NonSerialized]
        public Color32[] maskColors;
        [NonSerialized]
        bool needMaskUpdate;

        private void OnValidate() {
            UpdateMaterialProperties();
            SyncFeatureKeywords();
        }

        private void OnEnable() {
            if (!s_ActiveInstances.Contains(this)) {
                s_ActiveInstances.Add(this);
            }

            EnsureSunLight();
            UpdateMaterialProperties();
            SyncFeatureKeywords();
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            Camera.onPreCull += OnPreCullBuiltIn;
            Camera.onPostRender += OnPostRenderBuiltIn;
        }

        private void OnDisable() {
            s_ActiveInstances.Remove(this);
            if (profile != null) {
                profile.onSettingsChanged -= UpdateMaterialProperties;
            }
            Shader.DisableKeyword(ShaderParams.SKW_RECEIVE);
            Shader.DisableKeyword(ShaderParams.SKW_SHOW_CLOUDS);
            Shader.DisableKeyword(ShaderParams.SKW_SHOW_SHADOWS);
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            Camera.onPreCull -= OnPreCullBuiltIn;
            Camera.onPostRender -= OnPostRenderBuiltIn;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam) {
            RenderForCamera(cam, afterCamera: false);
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam) {
            RenderForCamera(cam, afterCamera: true);
        }

        private void OnPreCullBuiltIn(Camera cam) {
            if (GraphicsSettings.currentRenderPipeline != null) {
                return;
            }
            RenderForCamera(cam, afterCamera: false);
        }

        private void OnPostRenderBuiltIn(Camera cam) {
            if (GraphicsSettings.currentRenderPipeline != null) {
                return;
            }
            RenderForCamera(cam, afterCamera: true);
        }

        internal static void RenderAllForCamera(Camera cam, CommandBuffer cmd) {
            for (int i = 0; i < s_ActiveInstances.Count; i++) {
                s_ActiveInstances[i].RenderForCamera(cam, afterCamera: cam.orthographic);
            }
        }

        void RenderForCamera(Camera cam, bool afterCamera) {
            if (profile == null) return;
            if (cam.targetTexture != null && cam.targetTexture.format == RenderTextureFormat.Depth) return;
            if ((cameraLayerMask & (1 << cam.gameObject.layer)) == 0) return;

            // Perspective: draw during beginCameraRendering. Orthographic: draw clouds after the camera finishes.
            if (cam.orthographic != afterCamera) {
                return;
            }

            cam.depthTextureMode |= DepthTextureMode.Depth;

            float distance = cam.farClipPlane;
            Vector3 tl = cam.ViewportToWorldPoint(new Vector3(0, 1, distance));
            Vector3 br = cam.ViewportToWorldPoint(new Vector3(1, 0, distance));
            Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1, 1, distance));
            Vector3 p = (tl + br) * 0.5f;
            float tltr_dist = Vector3.Distance(tl, tr);
            float trbr_dist = Vector3.Distance(tr, br);
            Vector3 s = new Vector3(tltr_dist, trbr_dist, 1f);
            Matrix4x4 m = Matrix4x4.TRS(p, cam.transform.rotation, s);

            UpdateMaterialProperties();
            SyncFeatureKeywords();

            Bounds globalBounds = new Bounds(p, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
            if (shadowApplication == CloudShadowApplication.ScreenSpace
                && profile.showShadows
                && profile.shadowCoverage > 0
                && matShadows != null) {
                RenderParams rp = new RenderParams(matShadows) {
                    camera = cam,
                    layer = gameObject.layer,
                    worldBounds = globalBounds
                };
                RenderEffect(cam, matShadows, ref m, ref rp, isCloudsPass: false);
            }
            if (profile.showClouds && profile.coverage > 0 && matClouds != null) {
                RenderParams rp = new RenderParams(matClouds) {
                    camera = cam,
                    layer = gameObject.layer,
                    worldBounds = globalBounds
                };
                RenderEffect(cam, matClouds, ref m, ref rp, isCloudsPass: true);
            }
        }

        void EnsureSunLight() {
            if (sun != null) {
                return;
            }

            Light[] lights = Misc.FindObjectsOfType<Light>();
            foreach (Light light in lights) {
                if (light.type == LightType.Directional) {
                    sun = light;
                    break;
                }
            }
        }

        void RenderEffect(Camera cam, Material mat, ref Matrix4x4 m, ref RenderParams rp, bool isCloudsPass) {

            if (mat == null) return;

            EnsureSunLight();
            if (sun != null) {
                Shader.SetGlobalVector(ShaderParams.LightDir, sun.transform.forward);
            } else {
                Shader.SetGlobalVector(ShaderParams.LightDir, Vector3.down);
            }

            if (cam.orthographic) {
                mat.EnableKeyword(ShaderParams.SKW_ORTHOGRAPHIC);
                if (isCloudsPass) {
                    mat.EnableKeyword(ShaderParams.SKW_ORTHOGRAPHIC_OVERLAY);
                } else {
                    mat.DisableKeyword(ShaderParams.SKW_ORTHOGRAPHIC_OVERLAY);
                }
            } else {
                mat.DisableKeyword(ShaderParams.SKW_ORTHOGRAPHIC);
                mat.DisableKeyword(ShaderParams.SKW_ORTHOGRAPHIC_OVERLAY);
            }

            Vector3 windSpeed = profile.windSpeed;
            float angle = Mathf.Sin(Time.time * 0.2f) * 0.4f;
            float cosTheta = Mathf.Cos(angle);
            float sinTheta = Mathf.Sin(angle);
            windSpeed.x = windSpeed.x * cosTheta - windSpeed.z * sinTheta;
            windSpeed.z = windSpeed.x * sinTheta + windSpeed.z * cosTheta;
            windSpeed.x *= 1f + angle;
            windSpeed.z *= 1f + angle;

            float dt = Application.isPlaying ? Time.deltaTime * 10f : 0f;
            windOffset1.x += windSpeed.x * dt;
            windOffset1.z += windSpeed.z * dt;
            windOffset1.x %= 10000000f;
            windOffset1.z %= 10000000f;
            windOffset1.y = 1.0f - profile.coverage;
            windOffset1.w = 1f + profile.contrast;
            Shader.SetGlobalVector(ShaderParams.Layer1CloudsData2, windOffset1);

            Graphics.RenderMesh(rp, fullscreenMesh, 0, m);
        }

        void SyncGlobalLightAndWind(bool animateWind) {
            if (profile == null) {
                return;
            }

            EnsureSunLight();
            if (sun != null) {
                Shader.SetGlobalVector(ShaderParams.LightDir, sun.transform.forward);
            } else {
                Shader.SetGlobalVector(ShaderParams.LightDir, Vector3.down);
            }

            if (animateWind) {
                Vector3 windSpeed = profile.windSpeed;
                float angle = Mathf.Sin(Time.time * 0.2f) * 0.4f;
                float cosTheta = Mathf.Cos(angle);
                float sinTheta = Mathf.Sin(angle);
                windSpeed.x = windSpeed.x * cosTheta - windSpeed.z * sinTheta;
                windSpeed.z = windSpeed.x * sinTheta + windSpeed.z * cosTheta;
                windSpeed.x *= 1f + angle;
                windSpeed.z *= 1f + angle;

                float dt = Application.isPlaying ? Time.deltaTime * 10f : 0f;
                windOffset1.x += windSpeed.x * dt;
                windOffset1.z += windSpeed.z * dt;
                windOffset1.x %= 10000000f;
                windOffset1.z %= 10000000f;
            }

            windOffset1.y = 1.0f - profile.coverage;
            windOffset1.w = 1f + profile.contrast;
            Shader.SetGlobalVector(ShaderParams.Layer1CloudsData2, windOffset1);
        }

        static Mesh _fullScreenMesh;

        Mesh fullscreenMesh {
            get {
                if (_fullScreenMesh != null) {
                    return _fullScreenMesh;
                }
                Mesh val = new Mesh();
                _fullScreenMesh = val;
                _fullScreenMesh.SetVertices(new List<Vector3> {
            new Vector3 (-0.5f, -0.5f, 0f),
            new Vector3 (-0.5f, 0.5f, 0f),
            new Vector3 (0.5f, -0.5f, 0f),
            new Vector3 (0.5f, 0.5f, 0f)
        });
                _fullScreenMesh.SetIndices(new int[6] { 0, 1, 2, 2, 1, 3 }, (MeshTopology)0, 0, false);
                _fullScreenMesh.UploadMeshData(true);
                return _fullScreenMesh;
            }
        }

        #region Gizmos

        void OnDrawGizmosSelected() {
            if (coverageMask && maskEditorEnabled) {
                Vector3 pos = coverageMaskWorldCenter;
                pos.y = -10;
                Vector3 size = new Vector3(coverageMaskWorldSize.x, 0.1f, coverageMaskWorldSize.z);
                Gizmos.color = Color.cyan;
                for (int k = 0; k < 5; k++) {
                    Gizmos.DrawWireCube(pos, size);
                    pos.y += 0.5f;
                    Gizmos.DrawWireCube(pos, size);
                    pos.y += 0.5f;
                }
            }
        }

        #endregion


        public void UpdateMaterialProperties() {
            EnsureMaterials();
            if (profile == null || matShadows == null) {
                return;
            }

            UpdateMaterialProperties(matShadows);
            if (matClouds != null) {
                UpdateMaterialProperties(matClouds);
            }

            SyncGlobalShaderProperties();
            SyncGlobalShaderKeywords();
            SyncGlobalLightAndWind(false);
            SyncFeatureKeywords();

            matShadows.SetInt(ShaderParams.BlendSrc, (int)BlendMode.Zero);
            matShadows.SetInt(ShaderParams.BlendDst, (int)BlendMode.SrcColor);
            matShadows.renderQueue = renderQueue;

            matShadows.DisableKeyword(ShaderParams.SKW_COVERAGE_MASK);
            matShadows.DisableKeyword(ShaderParams.SKW_COVERAGE_MASK_DEBUG);
            if (coverageMask && coverageMaskTexture != null) {
                if (needMaskUpdate) {
                    UpdateMaskTexture();
                }
                matShadows.SetTexture(ShaderParams.CoverageMaskTex, coverageMaskTexture);
                matShadows.SetVector(ShaderParams.CoverageMaskWorldData, new Vector4(coverageMaskWorldSize.x, coverageMaskWorldCenter.x, coverageMaskWorldSize.z, coverageMaskWorldCenter.z));

                if (maskEditorEnabled) {
                    matShadows.EnableKeyword(ShaderParams.SKW_COVERAGE_MASK_DEBUG);
                    matShadows.SetInt(ShaderParams.BlendSrc, (int)BlendMode.SrcAlpha);
                    matShadows.SetInt(ShaderParams.BlendDst, (int)BlendMode.OneMinusSrcAlpha);
                } else {
                    matShadows.EnableKeyword(ShaderParams.SKW_COVERAGE_MASK);
                }
            }

            if (matClouds != null) {
                matClouds.renderQueue = renderQueue + 1;
            }
        }

        void EnsureMaterials() {
            if (matShadows == null) {
                Shader shadowsShader = Shader.Find(ShaderParams.ShadowsShaderName);
                if (shadowsShader != null) {
                    matShadows = new Material(shadowsShader);
                }
            }

            if (matClouds == null) {
                Shader cloudsShader = Shader.Find(ShaderParams.CloudsShaderName);
                if (cloudsShader == null) {
                    Debug.LogError("[CloudShadows] Missing shader: Hidden/Kronnect/SSCS/ScreenSpaceClouds");
                } else {
                    matClouds = new Material(cloudsShader);
                }
            }
        }

        public void UpdateMaterialProperties(Material mat) {
            if (profile == null || mat == null) return;

            profile.onSettingsChanged -= UpdateMaterialProperties;
            profile.onSettingsChanged += UpdateMaterialProperties;

            mat.SetColor(ShaderParams.LightColor, profile.sunColor);
            mat.SetColor(ShaderParams.ShadowColor, profile.shadowTintColor);
            mat.SetColor(ShaderParams.CloudsLightColor, profile.cloudsLightColor);
            mat.SetColor(ShaderParams.CloudsDarkColor, profile.cloudsDarkColor);
            mat.SetVector(ShaderParams.Layer1CloudsData, new Vector4(profile.scale * 0.001f, profile.edgeNoise, profile.edgeAnimationSpeed, profile.shadowOpacity));
            mat.SetVector(ShaderParams.Layer1CloudsData3, new Vector4(profile.cloudsAmbientLight, profile.cloudsOpacity, profile.distanceFade * profile.distanceFade, profile.cloudsThickness));
            mat.SetVector(ShaderParams.Layer1CloudsData4, new Vector4(profile.cloudsLightIntensity, profile.cloudsAnisotropy, profile.normalBias, profile.cloudsBumpMapScale * profile.scale * 0.001f));
            mat.SetTexture(ShaderParams.CloudsTex, profile.cloudsTexture);
            mat.SetTexture(ShaderParams.Clouds2Tex, profile.clouds2Texture);
            mat.SetTexture(ShaderParams.CloudsBumpMap, profile.cloudsBumpMap);
            mat.SetVector(ShaderParams.Offsets, new Vector4(profile.offset.x, profile.altitude, profile.offset.y, profile.noiseMultiplier));
            mat.SetVector(ShaderParams.ShadowsExtraData, new Vector4(1f - profile.shadowCoverage, profile.shadowMinAltitude, profile.preserveDirectionalShadows, 0));

            if (profile.cloudsThickness > 0) {
                mat.EnableKeyword(ShaderParams.SKW_CLOUDS_THICK);
            } else {
                mat.DisableKeyword(ShaderParams.SKW_CLOUDS_THICK);
            }

            if (profile.cloudsAnisotropy > 0 && !profile.topDownOnly) {
                mat.EnableKeyword(ShaderParams.SKW_CLOUDS_ANISOTROPY);
            } else {
                mat.DisableKeyword(ShaderParams.SKW_CLOUDS_ANISOTROPY);
            }

            if (profile.topDownOnly) {
                mat.DisableKeyword(ShaderParams.SKW_SHADOWS_3D);
                mat.DisableKeyword(ShaderParams.SKW_SHADOWS_3D_HQ);
            } else if (profile.improvedNormalReconstruction) {
                mat.DisableKeyword(ShaderParams.SKW_SHADOWS_3D);
                mat.EnableKeyword(ShaderParams.SKW_SHADOWS_3D_HQ);
            } else {
                mat.DisableKeyword(ShaderParams.SKW_SHADOWS_3D_HQ);
                mat.EnableKeyword(ShaderParams.SKW_SHADOWS_3D);
            }

            if (profile.preserveDirectionalShadows > 0) {
                mat.EnableKeyword(ShaderParams.SKW_PRESERVE_DIRECTIONAL_SHADOWS);
            } else {
                mat.DisableKeyword(ShaderParams.SKW_PRESERVE_DIRECTIONAL_SHADOWS);
            }

            if (useCustomBounds) {
                mat.EnableKeyword(ShaderParams.SKW_BOUNDS);
                mat.SetVector(ShaderParams.BoundsData, new Vector4(bounds.center.x, bounds.center.z, bounds.size.x, bounds.size.z));
            } else {
                mat.DisableKeyword(ShaderParams.SKW_BOUNDS);
            }

        }

        void SyncGlobalShaderProperties() {
            if (profile == null) {
                return;
            }

            Shader.SetGlobalColor(ShaderParams.LightColor, profile.sunColor);
            Shader.SetGlobalColor(ShaderParams.ShadowColor, profile.shadowTintColor);
            Shader.SetGlobalColor(ShaderParams.CloudsLightColor, profile.cloudsLightColor);
            Shader.SetGlobalColor(ShaderParams.CloudsDarkColor, profile.cloudsDarkColor);
            Shader.SetGlobalVector(ShaderParams.Layer1CloudsData, new Vector4(profile.scale * 0.001f, profile.edgeNoise, profile.edgeAnimationSpeed, profile.shadowOpacity));
            Shader.SetGlobalVector(ShaderParams.Layer1CloudsData3, new Vector4(profile.cloudsAmbientLight, profile.cloudsOpacity, profile.distanceFade * profile.distanceFade, profile.cloudsThickness));
            Shader.SetGlobalVector(ShaderParams.Layer1CloudsData4, new Vector4(profile.cloudsLightIntensity, profile.cloudsAnisotropy, profile.normalBias, profile.cloudsBumpMapScale * profile.scale * 0.001f));
            Shader.SetGlobalTexture(ShaderParams.CloudsTex, profile.cloudsTexture);
            Shader.SetGlobalTexture(ShaderParams.Clouds2Tex, profile.clouds2Texture);
            Shader.SetGlobalTexture(ShaderParams.CloudsBumpMap, profile.cloudsBumpMap);
            Shader.SetGlobalVector(ShaderParams.Offsets, new Vector4(profile.offset.x, profile.altitude, profile.offset.y, profile.noiseMultiplier));
            Shader.SetGlobalVector(ShaderParams.ShadowsExtraData, new Vector4(1f - profile.shadowCoverage, profile.shadowMinAltitude, profile.preserveDirectionalShadows, 0));

            if (coverageMask && coverageMaskTexture != null) {
                Shader.SetGlobalTexture(ShaderParams.CoverageMaskTex, coverageMaskTexture);
                Shader.SetGlobalVector(ShaderParams.CoverageMaskWorldData, new Vector4(coverageMaskWorldSize.x, coverageMaskWorldCenter.x, coverageMaskWorldSize.z, coverageMaskWorldCenter.z));
            }

            if (useCustomBounds) {
                Shader.SetGlobalVector(ShaderParams.BoundsData, new Vector4(bounds.center.x, bounds.center.z, bounds.size.x, bounds.size.z));
            }
        }

        void SyncGlobalShaderKeywords() {
            if (profile == null) {
                return;
            }

            SetGlobalKeyword(ShaderParams.SKW_CLOUDS_THICK, profile.cloudsThickness > 0);
            SetGlobalKeyword(ShaderParams.SKW_CLOUDS_ANISOTROPY, profile.cloudsAnisotropy > 0 && !profile.topDownOnly);

            if (profile.topDownOnly) {
                Shader.DisableKeyword(ShaderParams.SKW_SHADOWS_3D);
                Shader.DisableKeyword(ShaderParams.SKW_SHADOWS_3D_HQ);
            } else if (profile.improvedNormalReconstruction) {
                Shader.DisableKeyword(ShaderParams.SKW_SHADOWS_3D);
                Shader.EnableKeyword(ShaderParams.SKW_SHADOWS_3D_HQ);
            } else {
                Shader.DisableKeyword(ShaderParams.SKW_SHADOWS_3D_HQ);
                Shader.EnableKeyword(ShaderParams.SKW_SHADOWS_3D);
            }

            SetGlobalKeyword(ShaderParams.SKW_PRESERVE_DIRECTIONAL_SHADOWS, profile.preserveDirectionalShadows > 0);
            SetGlobalKeyword(ShaderParams.SKW_BOUNDS, useCustomBounds);

            if (coverageMask && coverageMaskTexture != null && !maskEditorEnabled) {
                Shader.EnableKeyword(ShaderParams.SKW_COVERAGE_MASK);
                Shader.DisableKeyword(ShaderParams.SKW_COVERAGE_MASK_DEBUG);
            } else if (coverageMask && coverageMaskTexture != null && maskEditorEnabled) {
                Shader.DisableKeyword(ShaderParams.SKW_COVERAGE_MASK);
                Shader.EnableKeyword(ShaderParams.SKW_COVERAGE_MASK_DEBUG);
            } else {
                Shader.DisableKeyword(ShaderParams.SKW_COVERAGE_MASK);
                Shader.DisableKeyword(ShaderParams.SKW_COVERAGE_MASK_DEBUG);
            }
        }

        void SyncFeatureKeywords() {
            if (!isActiveAndEnabled || profile == null) {
                Shader.DisableKeyword(ShaderParams.SKW_RECEIVE);
                Shader.DisableKeyword(ShaderParams.SKW_SHOW_CLOUDS);
                Shader.DisableKeyword(ShaderParams.SKW_SHOW_SHADOWS);
                return;
            }

            bool showShadows = profile.showShadows && profile.shadowCoverage > 0f;
            bool showClouds = profile.showClouds && profile.coverage > 0f;

            SetGlobalKeyword(ShaderParams.SKW_SHOW_SHADOWS, showShadows);
            SetGlobalKeyword(ShaderParams.SKW_SHOW_CLOUDS, showClouds);

            bool shaderIntegrationShadows = showShadows
                && shadowApplication == CloudShadowApplication.ShaderIntegration;
            SetGlobalKeyword(ShaderParams.SKW_RECEIVE, shaderIntegrationShadows);
        }

        static void SetGlobalKeyword(string keyword, bool enabled) {
            if (enabled) {
                Shader.EnableKeyword(keyword);
            } else {
                Shader.DisableKeyword(keyword);
            }
        }


        void CheckMaskColorsArray() {
            int length = coverageMaskTexture.width * coverageMaskTexture.height;
            if (maskColors == null || maskColors.Length != length) {
                maskColors = coverageMaskTexture.GetPixels32();
            }
        }

        void CheckMaskTexture() {
            if (coverageMaskTexture != null) {
                CheckMaskColorsArray();
                return;
            }

            int res = Mathf.Clamp(maskTextureResolution, 256, 8192);
            coverageMask = true;
            coverageMaskTexture = new Texture2D(res, res, TextureFormat.Alpha8, false, true);
            coverageMaskTexture.wrapMode = TextureWrapMode.Clamp;

            MaskClear(255);

            UpdateMaterialProperties();
        }

        /// <summary>
        /// Paints or clears shadows on mask
        /// </summary>
        /// <param name="pos">Position in world space</param>
        /// <param name="value">Opacity of the shadows being painted</param>
        public void MaskPaint(Vector3 pos, byte value, float brushWidth, float brushOpacity = 1f, float brushFuzziness = 0f) {

            CheckMaskTexture();
            int th = coverageMaskTexture.height;
            int tw = coverageMaskTexture.width;

            if (Application.isPlaying) {
                System.Threading.Tasks.Task.Run(() => MaskPaintThread(pos, value, brushWidth, tw, th, brushOpacity, brushFuzziness));
            } else {
                MaskPaintThread(pos, value, brushWidth, tw, th, brushOpacity, brushFuzziness);
            }
        }

        void MaskPaintThread(Vector3 pos, byte value, float brushWidth, int tw, int th, float brushOpacity = 1f, float brushFuzziness = 0f) {

            // Get texture location
            float x = (pos.x - coverageMaskWorldCenter.x) / coverageMaskWorldSize.x + 0.5f;
            float z = (pos.z - coverageMaskWorldCenter.z) / coverageMaskWorldSize.z + 0.5f;
            int tx = Mathf.Clamp((int)(x * tw), 0, tw - 1);
            int ty = Mathf.Clamp((int)(z * th), 0, th - 1);

            // Prepare brush data
            int brushSize = Mathf.FloorToInt(tw * brushWidth / coverageMaskWorldSize.x);
            float opacity = 1f - brushOpacity;
            double fuzziness = 1.1 - brushFuzziness;
            byte colort = (byte)(value * (1f - opacity));
            float radiusSqr = brushSize * brushSize;
            // Paint!
            int j0 = ty - brushSize;
            j0 = Mathf.Clamp(j0, 1, th);
            int j1 = ty + brushSize;
            j1 = Mathf.Clamp(j1, 1, th);
            int k0 = tx - brushSize;
            k0 = Mathf.Clamp(k0, 1, tw);
            int k1 = tx + brushSize;
            k1 = Mathf.Clamp(k1, 1, tw);
            System.Random rnd = new System.Random();
            if (brushFuzziness > 0) {
                for (int j = j0; j < j1; j++) {
                    int jj = j * tw;
                    int dj = (j - ty) * (j - ty);
                    for (int k = k0; k < k1; k++) {
                        int distSqr = dj + (k - tx) * (k - tx);
                        float op = distSqr / radiusSqr;
                        double threshold = rnd.NextDouble();
                        if (op <= 1f && threshold * op < fuzziness) {
                            maskColors[jj + k].r = (byte)(colort + maskColors[jj + k].r * opacity);
                        }
                    }
                }
            } else {
                for (int j = j0; j < j1; j++) {
                    int jj = j * tw;
                    int dj = (j - ty) * (j - ty);
                    for (int k = k0; k < k1; k++) {
                        int distSqr = dj + (k - tx) * (k - tx);
                        if (distSqr <= radiusSqr) {
                            maskColors[jj + k].r = (byte)(colort + maskColors[jj + k].r * opacity);
                        }
                    }
                }
            }

            needMaskUpdate = true;
        }

        /// <summary>
        /// Fills the mask texture with a constant value
        /// </summary>
        public void MaskClear(byte value) {

            CheckMaskTexture();

            Color32 opaque = new Color32(value, 255, 255, 255);
            int length = maskColors.Length;
            for (int k = 0; k < length; k++) {
                maskColors[k] = opaque;
            }

            needMaskUpdate = true;
        }


        /// <summary>
        /// Fills an area with shadows equal to the object shape
        /// </summary>
        public void MaskFillArea(GameObject go, byte value, float opacity = 1f, float border = 0f) {
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer == null) return;
            MaskFillArea(meshRenderer, value, opacity, border);
        }

        readonly List<ushort> indices = new List<ushort>();
        readonly List<Vector3> vertices = new List<Vector3>();
        Mesh lastMesh;

        /// <summary>
        /// Fills an area with shadows equal to the object shape
        /// </summary>
        /// <param name="meshRenderer">Mesh renderer of the object</param>
        public void MaskFillArea(MeshRenderer meshRenderer, byte value, float opacity = 1f, float border = 0f) {

            CheckMaskTexture();

            if (meshRenderer == null) return;

            Bounds bounds = meshRenderer.bounds;

            Vector3 worldSize = coverageMaskWorldSize;
            Vector3 worldCenter = coverageMaskWorldCenter;
            Vector3 worldPosition = bounds.center;

            float tx = (worldPosition.x - worldCenter.x) / worldSize.x + 0.5f;
            if (tx < 0 || tx > 1f)
                return;
            float tz = (worldPosition.z - worldCenter.z) / worldSize.z + 0.5f;
            if (tz < 0 || tz > 1f)
                return;

            // Get triangle info
            MeshFilter mf = meshRenderer.GetComponent<MeshFilter>();
            if (mf == null) {
                Debug.LogError("No MeshFilter found on this object.");
                return;
            }

            Mesh mesh = mf.sharedMesh;
            if (mesh == null) {
                Debug.LogError("No Mesh found on this object.");
                return;
            }
            if (mf.sharedMesh.GetTopology(0) != MeshTopology.Triangles) {
                Debug.LogError("Only triangle topology is supported by this tool.");
                return;
            }
            if (lastMesh != mesh) {
                mesh.GetTriangles(indices, 0);
                mesh.GetVertices(vertices);
                lastMesh = mesh;
            }
            int indicesLength = indices.Count;
            Vector2[] triangles = new Vector2[indicesLength];
            Transform t = meshRenderer.transform;

            Vector2 v0, v1, v2;
            if (border > 0) {
                for (int k = 0; k < indicesLength; k += 3) {
                    Vector3 w0 = t.TransformPoint(vertices[indices[k]]);
                    Vector3 w1 = t.TransformPoint(vertices[indices[k + 1]]);
                    Vector3 w2 = t.TransformPoint(vertices[indices[k + 2]]);
                    v0.x = w0.x; v0.y = w0.z;
                    v1.x = w1.x; v1.y = w1.z;
                    v2.x = w2.x; v2.y = w2.z;
                    Vector2 c = (v0 + v1 + v2) / 3f;
                    v0 += (v0 - c).normalized * border;
                    v1 += (v1 - c).normalized * border;
                    v2 += (v2 - c).normalized * border;
                    triangles[k] = v0;
                    triangles[k + 1] = v1;
                    triangles[k + 2] = v2;
                }

            } else {
                for (int k = 0; k < indicesLength; k += 3) {
                    Vector3 w0 = t.TransformPoint(vertices[indices[k]]);
                    Vector3 w1 = t.TransformPoint(vertices[indices[k + 1]]);
                    Vector3 w2 = t.TransformPoint(vertices[indices[k + 2]]);
                    v0.x = w0.x; v0.y = w0.z;
                    v1.x = w1.x; v1.y = w1.z;
                    v2.x = w2.x; v2.y = w2.z;
                    triangles[k] = v0;
                    triangles[k + 1] = v1;
                    triangles[k + 2] = v2;
                }
            }

            if (Application.isPlaying) {
                System.Threading.Tasks.Task.Run(() => MaskFillAreaThread(value, opacity, triangles, indicesLength, bounds, tx, tz, worldCenter, worldSize));
            } else {
                MaskFillAreaThread(value, opacity, triangles, indicesLength, bounds, tx, tz, worldCenter, worldSize);
            }
        }

        void MaskFillAreaThread(byte value, float opacity, Vector2[] triangles, int indicesLength, Bounds bounds, float tx, float tz, Vector3 worldCenter, Vector3 worldSize) {

            int res = maskTextureResolution;
            int tw = res;
            int th = res;
            int px = (int)(tx * tw);
            int pz = (int)(tz * th);
            float trz = bounds.extents.z / worldSize.z;
            float trx = bounds.extents.x / worldSize.x;
            int deltaz = (int)(th * trz);
            int deltax = (int)(tw * trx);

            int r0 = pz - deltaz;
            if (r0 < 1) r0 = 1; else if (r0 >= th) r0 = th - 1;
            int r1 = pz + deltaz;
            if (r1 < 1) r1 = 1; else if (r1 >= th) r1 = th - 1;
            int c0 = px - deltax;
            if (c0 < 1) c0 = 1; else if (c0 >= tw) c0 = tw - 1;
            int c1 = px + deltax;
            if (c1 < 1) c1 = 1; else if (c1 >= tw) c1 = tw - 1;

            Vector2 wpos;

            int index = 0;
            Vector2 v0 = triangles[index];
            Vector2 v1 = triangles[index + 1];
            Vector2 v2 = triangles[index + 2];

            for (int z = r0; z <= r1; z++) {
                int zz = z * res;
                wpos.y = (((z + 0.5f) / res) - 0.5f) * worldSize.z + worldCenter.z;
                for (int x = c0; x <= c1; x++) {
                    wpos.x = (((x + 0.5f) / res) - 0.5f) * worldSize.x + worldCenter.x;

                    // Check if any triangle contains this position
                    if (opacity >= 1f) {
                        for (int i = 0; i < indicesLength; i += 3) {
                            if (PointInTriangle(wpos, v0, v1, v2)) {
                                maskColors[zz + x].r = value;
                                break;
                            } else {
                                index += 3;
                                index %= indicesLength;
                                v0 = triangles[index];
                                v1 = triangles[index + 1];
                                v2 = triangles[index + 2];
                            }
                        }
                    } else {
                        for (int i = 0; i < indicesLength; i += 3) {
                            if (PointInTriangle(wpos, v0, v1, v2)) {
                                byte v = (byte)(value * opacity + maskColors[zz + x].r * (1f - opacity));
                                maskColors[zz + x].r = v;
                                break;
                            } else {
                                index += 3;
                                index %= indicesLength;
                                v0 = triangles[index];
                                v1 = triangles[index + 1];
                                v2 = triangles[index + 2];
                            }
                        }
                    }
                }
            }

            needMaskUpdate = true;
        }


        float Sign(Vector2 p1, Vector2 p2, Vector2 p3) {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3) {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, v1, v2);
            d2 = Sign(pt, v2, v3);
            d3 = Sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        void UpdateMaskTexture() {
            if (coverageMaskTexture != null && maskColors != null) {
                int length = coverageMaskTexture.width * coverageMaskTexture.height;
                if (length == maskColors.Length) {
                    coverageMaskTexture.SetPixels32(maskColors);
                    coverageMaskTexture.Apply(false);
                    needMaskUpdate = false;
                }
            }
        }


        /// <summary>
        /// Gets bounds in world space
        /// </summary>
        public Bounds GetBounds() {
            Bounds bounds = this.bounds;
            return bounds;
        }


        /// <summary>
        /// Sets bounds in world space
        /// </summary>
        public void SetBounds(Bounds bounds) {
            this.bounds = bounds;
            UpdateMaterialProperties();
        }

    }

}

