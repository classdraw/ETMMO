using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using UnityEditor.IMGUI.Controls;

namespace SSCS {
    [CustomEditor(typeof(CloudShadows))]
    public class CloudShadowsEditor : Editor {

        SerializedProperty sun, cameraLayerMask, renderQueue, shadowApplication;
        SerializedProperty profile;
        SerializedProperty coverageMask, coverageMaskTexture, coverageMaskWorldSize, coverageMaskWorldCenter;
        SerializedProperty maskEditorEnabled, maskTextureResolution, maskBrushMode, maskBrushWidth, maskBrushFuzziness, maskBrushOpacity;
        SerializedProperty useCustomBounds, bounds;

        CloudShadowsProfile cachedProfile;
        Editor cachedProfileEditor;
        static GUIStyle boxStyle;
        CloudShadows cs;
        bool mouseIsDown;
        readonly TextureImporterSettings settings = new TextureImporterSettings();

        private void OnEnable() {
            sun = serializedObject.FindProperty("sun");
            cameraLayerMask = serializedObject.FindProperty("cameraLayerMask");
            renderQueue = serializedObject.FindProperty("renderQueue");
            shadowApplication = serializedObject.FindProperty("shadowApplication");
            profile = serializedObject.FindProperty("profile");

            coverageMask = serializedObject.FindProperty("coverageMask");
            coverageMaskTexture = serializedObject.FindProperty("coverageMaskTexture");
            coverageMaskWorldSize = serializedObject.FindProperty("coverageMaskWorldSize");
            coverageMaskWorldCenter = serializedObject.FindProperty("coverageMaskWorldCenter");

            maskEditorEnabled = serializedObject.FindProperty("maskEditorEnabled");
            maskTextureResolution = serializedObject.FindProperty("maskTextureResolution");
            maskBrushMode = serializedObject.FindProperty("maskBrushMode");
            maskBrushWidth = serializedObject.FindProperty("maskBrushWidth");
            maskBrushFuzziness = serializedObject.FindProperty("maskBrushFuzziness");
            maskBrushOpacity = serializedObject.FindProperty("maskBrushOpacity");

            useCustomBounds = serializedObject.FindProperty("useCustomBounds");
            bounds = serializedObject.FindProperty("bounds");

            cs = (CloudShadows)target;
        }

        private void OnSceneGUI() {
            ManageBounds();
            ManageMask();
        }


        public override void OnInspectorGUI() {

            RenderPipelineAsset pipe = GraphicsSettings.currentRenderPipeline;
            if (pipe != null) {
                bool depthTexture = false;
                System.Reflection.PropertyInfo supportsCameraDepthTexture = pipe.GetType().GetProperty("supportsCameraDepthTexture");
                if (supportsCameraDepthTexture != null) {
                    depthTexture = (bool)supportsCameraDepthTexture.GetValue(pipe);
                }
                if (!depthTexture) {
                    EditorGUILayout.HelpBox("Depth texture must be enabled in the URP asset.", MessageType.Error);
                    if (GUILayout.Button("Go to URP asset")) {
                        Selection.activeObject = pipe;
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.Separator();
                }
            }

            if (boxStyle == null) {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.padding = new RectOffset(15, 10, 5, 5);
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(sun);
            EditorGUILayout.PropertyField(cameraLayerMask);
            EditorGUILayout.PropertyField(renderQueue);
            EditorGUILayout.PropertyField(shadowApplication, new GUIContent("Shadow Application",
                "Shader Integration: recommended for transparent tilemaps. Screen Space: fullscreen multiply after the scene (needs depth texture)."));

            EditorGUILayout.PropertyField(profile);

            if (profile.objectReferenceValue != null) {
                if (cachedProfile != profile.objectReferenceValue) {
                    cachedProfile = null;
                }
                if (cachedProfile == null) {
                    cachedProfile = (CloudShadowsProfile)profile.objectReferenceValue;
                    cachedProfileEditor = CreateEditor(profile.objectReferenceValue);
                }

                // Drawing the profile editor
                EditorGUILayout.BeginVertical(boxStyle);
                cachedProfileEditor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            } else {
                EditorGUILayout.HelpBox("Create or assign a cloud shadows profile.", MessageType.Info);
                if (GUILayout.Button("New Profile")) {
                    CreateProfile();
                }
            }

            EditorGUILayout.PropertyField(useCustomBounds, new GUIContent("Use Custom Bounds", "Limit cloud shadows to a specific area defined by a center and size."));
            if (useCustomBounds.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(bounds);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(coverageMask, new GUIContent("Enable Coverage Mask", "Uses red channel of a custom texture to determine if shadows can render at that position. A value less than 1, means no shadows at that position."));
            if (coverageMask.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(coverageMaskTexture, new GUIContent("Mask Texture (A)", "Coverage mask. A value of alpha less than 1 means no shadows."));

                EditorGUI.BeginChangeCheck();
                Texture2D tex = (Texture2D)coverageMaskTexture.objectReferenceValue;
                CheckCoverageTextureImportSettings(tex);
                if (tex != null) {
                    if (EditorGUI.EndChangeCheck()) {
                        if (tex.isReadable) {
                            cs.maskColors = tex.GetPixels32();
                            maskTextureResolution.intValue = tex.width;
                        }
                    }
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Texture Size", tex.width.ToString());
                    EditorGUILayout.LabelField("Texture Path", AssetDatabase.GetAssetPath(tex));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(coverageMaskWorldSize, new GUIContent("World Size", "Mapping of the texture against the world in world units. Usually this should match terrain size."));
                EditorGUILayout.PropertyField(coverageMaskWorldCenter, new GUIContent("World Center", "Mapping of the texture center against the world in world units. Use this as an offset to apply coverage mask over a certain area."));

                EditorGUILayout.PropertyField(maskEditorEnabled, new GUIContent("Enable Mask Editor", "Activates terrain brush to paint/remove shadows at custom locations."));
                if (maskEditorEnabled.boolValue) {
                    if (!coverageMask.boolValue) {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.LabelField("Coverage Mask feature is disabled. Enable it?");
                        if (GUILayout.Button("Enable Coverage Mask")) coverageMask.boolValue = true;
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Separator();
                        GUI.enabled = false;
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.PropertyField(maskTextureResolution, new GUIContent("Resolution", "Resolution of the mask texture. Higher resolution allows more detail but it can be slower."));
                    if (GUILayout.Button("Create New Mask Texture")) {
                        if (EditorUtility.DisplayDialog("Create Mask Texture", "A texture asset will be created with a size of " + maskTextureResolution.intValue + "x" + maskTextureResolution.intValue + ".\n\nContinue?", "Ok", "Cancel")) {
                            CreateNewMaskTexture();
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Separator();
                    EditorGUILayout.PropertyField(maskBrushMode, new GUIContent("Brush Mode", "Select brush operation mode."));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(maskBrushWidth, new GUIContent("Width", "Width of the editor brush."));
                    EditorGUILayout.PropertyField(maskBrushFuzziness, new GUIContent("Fuzziness", "Solid vs spray brush."));
                    EditorGUILayout.PropertyField(maskBrushOpacity, new GUIContent("Opacity", "Stroke opacity."));
                    EditorGUILayout.BeginHorizontal();
                    if (tex == null) GUI.enabled = false;
                    if (GUILayout.Button("Fill Mask")) {
                        FillMaskTexture(255);
                    }
                    if (GUILayout.Button("Clear Mask")) {
                        FillMaskTexture(0);
                    }
                    EditorGUILayout.EndHorizontal();
                    GUI.enabled = true;
                    EditorGUILayout.Separator();
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        #region Profile utils

        void CreateProfile() {

            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets)) {
                path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path)) {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }
            CloudShadowsProfile pf = CreateInstance<CloudShadowsProfile>();
            pf.name = "New Cloud Shadows Profile";
            string fullFileName = path + "/" + pf.name + ".asset";
            fullFileName = AssetDatabase.GenerateUniqueAssetPath(fullFileName);
            AssetDatabase.CreateAsset(pf, fullFileName);
            AssetDatabase.SaveAssets();
            profile.objectReferenceValue = pf;
            EditorGUIUtility.PingObject(pf);
        }

        #endregion

        #region Mask utils


        private void ManageMask() {
            Event e = Event.current;
            if (cs == null || !maskEditorEnabled.boolValue || e == null) return;

            Camera sceneCamera = null;
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) sceneCamera = sceneView.camera;
            if (sceneCamera == null) return;

            Vector2 mousePos = Event.current.mousePosition;
            if (mousePos.x < 0 || mousePos.x > sceneCamera.pixelWidth || mousePos.y < 0 || mousePos.y > sceneCamera.pixelHeight) return;

            Selection.activeGameObject = cs.gameObject;
            cs.UpdateMaterialProperties();

            if (e.type == EventType.KeyDown && e.shift) {
                if (e.keyCode == KeyCode.X) {
                    cs.maskBrushMode = MaskTextureBrushMode.RemoveShadows;
                    e.Use();
                    return;
                } else if (e.keyCode == KeyCode.C) {
                    cs.maskBrushMode = MaskTextureBrushMode.RestoreShadows;
                    e.Use();
                    return;
                }
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo)) {
                float handleSize = HandleUtility.GetHandleSize(hitInfo.point);
                Handles.color = cs.maskBrushMode == MaskTextureBrushMode.RestoreShadows ? new Color(0.2f, 0.2f, 1f, 0.5f) : new Color(0, 0, 0, 0.5f); ;
                Handles.SphereHandleCap(0, hitInfo.point, Quaternion.identity, handleSize, EventType.Repaint);
                HandleUtility.Repaint();

                if (e.isMouse && e.button == 0) {
                    var controlID = GUIUtility.GetControlID(FocusType.Passive);
                    var eventType = e.GetTypeForControl(controlID);

                    if (eventType == EventType.MouseDown) {
                        GUIUtility.hotControl = controlID;
                        mouseIsDown = true;
                        PaintOnMaskPosition(hitInfo.point);
                    } else if (eventType == EventType.MouseUp) {
                        GUIUtility.hotControl = controlID;
                        mouseIsDown = false;
                    }

                    if (mouseIsDown && eventType == EventType.MouseDrag) {
                        GUIUtility.hotControl = controlID;
                        PaintOnMaskPosition(hitInfo.point);
                    }
                }
            }
        }



        public void CheckCoverageTextureImportSettings(Texture2D tex) {
            if (Application.isPlaying || tex == null)
                return;
            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path))
                return;
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null)
                return;
            imp.ReadTextureSettings(settings);
            if (settings.textureType != TextureImporterType.SingleChannel || settings.singleChannelComponent != TextureImporterSingleChannelComponent.Alpha || settings.sRGBTexture || settings.wrapMode != TextureWrapMode.Clamp || settings.filterMode != FilterMode.Bilinear) {
                EditorGUILayout.HelpBox("Texture has invalid import settings.", MessageType.Warning);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Fix Texture Import Settings", GUILayout.Width(200))) {
                    settings.textureType = TextureImporterType.SingleChannel;
                    settings.singleChannelComponent = TextureImporterSingleChannelComponent.Alpha;
                    settings.sRGBTexture = false;
                    settings.wrapMode = TextureWrapMode.Clamp;
                    settings.filterMode = FilterMode.Bilinear;
                    settings.alphaSource = TextureImporterAlphaSource.FromInput;
                    settings.alphaIsTransparency = true;
                    settings.readable = true;
                    settings.aniso = 0;
                    imp.SetTextureSettings(settings);
                    imp.SaveAndReimport();
                    cs.UpdateMaterialProperties();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
            }
        }

        private void CreateNewMaskTexture() {
            int res = Mathf.Clamp(maskTextureResolution.intValue, 256, 8192);
            Texture2D tex = new Texture2D(res, res, TextureFormat.R8, false, true);
            tex.wrapMode = TextureWrapMode.Clamp;
            coverageMaskTexture.objectReferenceValue = tex;
            coverageMask.boolValue = true;
            serializedObject.ApplyModifiedProperties();
            FillMaskTexture(255);
            AssetDatabase.CreateAsset(tex, "Assets/ShadowMaskTexture.asset");
            AssetDatabase.SaveAssets();
        }

        private void PaintOnMaskPosition(Vector3 pos) {
            // Get texture location
            Texture2D currentMaskTexture = (Texture2D)coverageMaskTexture.objectReferenceValue;
            if (currentMaskTexture == null) {
                EditorUtility.DisplayDialog("Mask Editor", "Create or assign a coverage mask texture in the inspector before painting!", "Ok");
                return;
            }
            byte value = maskBrushMode.intValue == (int)MaskTextureBrushMode.RestoreShadows ? (byte)255 : (byte)0;
            cs.MaskPaint(pos, value, maskBrushWidth.floatValue, maskBrushOpacity.floatValue * 0.2f, maskBrushFuzziness.floatValue);
            EditorUtility.SetDirty(currentMaskTexture);
        }

        void FillMaskTexture(byte value) {
            cs.MaskClear(value);
            Texture2D currentMaskTexture = (Texture2D)coverageMaskTexture.objectReferenceValue;
            EditorUtility.SetDirty(currentMaskTexture);
        }


        #endregion

        #region Bounds Management

        private readonly BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        protected virtual void ManageBounds() {
            if (!cs.useCustomBounds) return;

            Bounds bounds = cs.GetBounds();
            m_BoundsHandle.center = bounds.center;
            m_BoundsHandle.size = bounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck()) {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(cs, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                cs.SetBounds(newBounds);
            }
        }

        #endregion

    }

}