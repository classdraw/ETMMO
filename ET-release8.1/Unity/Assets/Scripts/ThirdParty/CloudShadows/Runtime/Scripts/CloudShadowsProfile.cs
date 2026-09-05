using UnityEngine;

namespace SSCS {

    public delegate void OnSettingsChanged();

    [HelpURL("https://kronnect.com/guides-category/cloud-shadows-fx/")]
    [CreateAssetMenu(fileName = "CloudShadowsProfile.asset", menuName = "Clouds Shadows Profile", order = 103)]
    public class CloudShadowsProfile : ScriptableObject {

        [Header("Clouds")]
        public bool showClouds;
        [Range(0, 1)]
        public float coverage = 0.35f;
        [ColorUsage(showAlpha: false)] public Color sunColor = new Color(1f, 0.9568f, 0.8392f);
        [ColorUsage(showAlpha: false)] public Color cloudsLightColor = new Color(0.94f, 0.94f, 1f);
        [ColorUsage(showAlpha: false)] public Color cloudsDarkColor = new Color(0.7f, 0.7f, 0.78f);
        [Tooltip("Light reflectivity intensity")]
        [Range(0, 1)]
        public float cloudsLightIntensity = 1f;
        [Tooltip("Intensity of the Sun light coming through the clouds. Only appreciated when viewing the clouds from the ground towards the Sun.")]
        public float cloudsAnisotropy = 16;
        [Tooltip("Minimum light intensity applied to the clouds")]
        [Range(0, 1)]
        public float cloudsAmbientLight = 0.002f;
        [Range(0, 50)]
        public float cloudsThickness;
        [Range(0, 1)]
        public float cloudsOpacity = 0.8f;
        [Tooltip("Fades clouds and shadows on distance to the horizon")]
        public float distanceFade = 2500f;

        [Header("Shadows")]
        public bool showShadows = true;
        [Range(0, 1)]
        public float shadowCoverage = 0.35f;
        [ColorUsage(showAlpha: false)]
        public Color shadowTintColor = Color.black;
        [Range(0, 2)]
        public float shadowOpacity = 0.8f;
        [Tooltip("Enable if shadows will be visible only from a top-down perspective. If disabled, additional checks will be performed to avoid shadows on ceilings")]
        public bool topDownOnly = true;
        [Tooltip("Enable to use an improved normal reconstruction algorithm which reduces halo around geometry edges")]
        public bool improvedNormalReconstruction;
        [Tooltip("Affects the intensity of shadows on wals and ceilings")]
        [Range(-0.5f, 0.5f)]
        public float normalBias = 0.3f;
        [Tooltip("Use this setting to prevent cloud shadows rendered under certain altitude, for example, under water level.")]
        public float shadowMinAltitude = -10000;
        [Tooltip("Reduces shadow intensity based on diretional shadow presence (only in URP)")]
        [Range(0, 1)]
        public float preserveDirectionalShadows;

        [Header("General")]
        public float altitude = 200;
        public float scale = 0.8f;
        public Vector2 offset = new Vector2(0.3f, 0);
        [Range(0, 10)]
        public float contrast = 2f;
        [Range(0, 1)]
        public float edgeNoise = 0.2f;
        public float edgeAnimationSpeed = 4f;
        public Vector3 windSpeed = new Vector3(0.1f, 0, 0.2f);
        public Texture2D clouds2Texture;
        public Texture2D cloudsTexture;
        public Texture2D cloudsBumpMap;
        public float cloudsBumpMapScale = 1f;
        public float noiseMultiplier = 2f;

        public event OnSettingsChanged onSettingsChanged;

        private void OnEnable() {
            ValidateSettings();
        }

        private void OnValidate() {
            ValidateSettings();
        }

        void ValidateSettings() {
            edgeNoise = Mathf.Max(0, edgeNoise);
            distanceFade = Mathf.Max(0, distanceFade);
            cloudsAnisotropy = Mathf.Max(1, cloudsAnisotropy);
            cloudsBumpMapScale = Mathf.Max(0, cloudsBumpMapScale);

            if (cloudsTexture == null) {
                cloudsTexture = Resources.Load<Texture2D>("SSCS/Textures/Clouds1Tex");
            }

            if (clouds2Texture == null) {
                clouds2Texture = cloudsTexture;
            }

            if (cloudsBumpMap == null) {
                cloudsBumpMap = Resources.Load<Texture2D>("SSCS/Textures/Clouds1TexBump");
            }

            if (onSettingsChanged != null) {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () => {
                    try {
                        onSettingsChanged();
                        UnityEditor.EditorApplication.delayCall += () => UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    } catch { }
                };
#else
                onSettingsChanged();
#endif
            }

        }


    }

}