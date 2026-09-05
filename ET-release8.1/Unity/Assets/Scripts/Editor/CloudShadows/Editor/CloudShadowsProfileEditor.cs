using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace SSCS {
    [CustomEditor(typeof(CloudShadowsProfile))]
    public class CloudShadowsProfileEditor : Editor {

        SerializedProperty showClouds, sunColor, cloudsLightColor, cloudsDarkColor, cloudsAmbientLight, cloudsLightIntensity, cloudsAnisotropy, cloudsThickness, cloudsOpacity, distanceFade;
        SerializedProperty showShadows, shadowCoverage, shadowTintColor, topDownOnly, improvedNormalReconstruction, normalBias, shadowMinAltitude, preserveDirectionalShadows;

        SerializedProperty altitude, scale, offset, coverage, shadowOpacity, contrast, edgeNoise, edgeAnimationSpeed, windSpeed;
        SerializedProperty cloudsTexture, clouds2Texture, cloudsBumpMap;
        SerializedProperty cloudsBumpMapScale, noiseMultiplier;

        private void OnEnable() {
            try {
                showClouds = serializedObject.FindProperty("showClouds");
                sunColor = serializedObject.FindProperty("sunColor");
                cloudsLightColor = serializedObject.FindProperty("cloudsLightColor");
                cloudsDarkColor = serializedObject.FindProperty("cloudsDarkColor");
                cloudsAmbientLight = serializedObject.FindProperty("cloudsAmbientLight");
                cloudsLightIntensity = serializedObject.FindProperty("cloudsLightIntensity");
                cloudsAnisotropy = serializedObject.FindProperty("cloudsAnisotropy");
                cloudsThickness = serializedObject.FindProperty("cloudsThickness");
                cloudsOpacity = serializedObject.FindProperty("cloudsOpacity");
                distanceFade = serializedObject.FindProperty("distanceFade");

                showShadows = serializedObject.FindProperty("showShadows");
                shadowCoverage = serializedObject.FindProperty("shadowCoverage");
                shadowTintColor = serializedObject.FindProperty("shadowTintColor");
                shadowOpacity = serializedObject.FindProperty("shadowOpacity");
                topDownOnly = serializedObject.FindProperty("topDownOnly");
                improvedNormalReconstruction = serializedObject.FindProperty("improvedNormalReconstruction");
                normalBias = serializedObject.FindProperty("normalBias");
                shadowMinAltitude = serializedObject.FindProperty("shadowMinAltitude");
                preserveDirectionalShadows = serializedObject.FindProperty("preserveDirectionalShadows");

                altitude = serializedObject.FindProperty("altitude");
                scale = serializedObject.FindProperty("scale");
                offset = serializedObject.FindProperty("offset");
                coverage = serializedObject.FindProperty("coverage");
                contrast = serializedObject.FindProperty("contrast");
                edgeNoise = serializedObject.FindProperty("edgeNoise");
                edgeAnimationSpeed = serializedObject.FindProperty("edgeAnimationSpeed");
                windSpeed = serializedObject.FindProperty("windSpeed");
                cloudsTexture = serializedObject.FindProperty("cloudsTexture");
                clouds2Texture = serializedObject.FindProperty("clouds2Texture");
                cloudsBumpMap = serializedObject.FindProperty("cloudsBumpMap");
                cloudsBumpMapScale = serializedObject.FindProperty("cloudsBumpMapScale");
                noiseMultiplier = serializedObject.FindProperty("noiseMultiplier");
            } catch { }
        }


        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(showClouds);
            if (showClouds.boolValue) {
                EditorGUILayout.PropertyField(coverage);
                EditorGUILayout.PropertyField(cloudsThickness, new GUIContent("Thickness"));
                EditorGUILayout.PropertyField(cloudsOpacity, new GUIContent("Opacity"));
                EditorGUILayout.PropertyField(cloudsLightColor, new GUIContent("Low Density Color"));
                EditorGUILayout.PropertyField(cloudsDarkColor, new GUIContent("High Density Color"));
                EditorGUILayout.PropertyField(sunColor, new GUIContent("Sun Color"));
                EditorGUILayout.PropertyField(cloudsLightIntensity, new GUIContent("Light Intensity"));
                EditorGUILayout.PropertyField(cloudsAnisotropy, new GUIContent("Anisotropy"));
                EditorGUILayout.PropertyField(cloudsAmbientLight, new GUIContent("Ambient Light"));
            }

            EditorGUILayout.PropertyField(showShadows);
            if (showShadows.boolValue) {
                EditorGUILayout.PropertyField(shadowCoverage, new GUIContent("Coverage"));
                EditorGUILayout.PropertyField(shadowMinAltitude, new GUIContent("Min Altitude"));
                EditorGUILayout.PropertyField(shadowOpacity, new GUIContent("Opacity"));
                EditorGUILayout.PropertyField(shadowTintColor, new GUIContent("Tint Color"));
                if (GraphicsSettings.currentRenderPipeline != null) {
                    EditorGUILayout.PropertyField(preserveDirectionalShadows, new GUIContent("Preserve Directional Shadows (only URP)"));
                }
            }

            EditorGUILayout.PropertyField(altitude);
            EditorGUILayout.PropertyField(topDownOnly);
            if (!topDownOnly.boolValue) {
                EditorGUILayout.PropertyField(improvedNormalReconstruction);
                EditorGUILayout.PropertyField(normalBias);
            }
            EditorGUILayout.PropertyField(scale);
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(distanceFade);
            EditorGUILayout.PropertyField(contrast);
            EditorGUILayout.PropertyField(edgeNoise);
            EditorGUILayout.PropertyField(edgeAnimationSpeed);
            EditorGUILayout.PropertyField(windSpeed);
            EditorGUILayout.PropertyField(cloudsTexture, new GUIContent("Clouds Texture 1"));
            EditorGUILayout.PropertyField(clouds2Texture, new GUIContent("Clouds Texture 2"));
            EditorGUILayout.PropertyField(cloudsBumpMap, new GUIContent("Clouds Bump Map"));
            EditorGUILayout.PropertyField(cloudsBumpMapScale, new GUIContent("Bump Map Scale"));
            EditorGUILayout.PropertyField(noiseMultiplier, new GUIContent("Noise Multiplier"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}