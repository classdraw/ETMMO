using ET;
using UnityEditor;
using UnityEngine;

namespace ET.Editor.Frame2D
{
    [CustomEditor(typeof(FrameAnimBoneConfig))]
    public class FrameAnimBoneConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty frameCountProp;
        private SerializedProperty frameMeshFramesProp;
        private SerializedProperty boneTracksProp;

        private int selectedTrackIndex;
        private int selectedFrameIndex;

        private void OnEnable()
        {
            frameCountProp = serializedObject.FindProperty("frameCount");
            frameMeshFramesProp = serializedObject.FindProperty("frameMeshFrames");
            boneTracksProp = serializedObject.FindProperty("boneTracks");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Frame Anim Bone Config", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(frameCountProp, new GUIContent("Frame Count"));

            int frameCount = Mathf.Max(1, frameCountProp.intValue);
            SyncAllTrackFrameCounts(frameCount);
            SyncMeshFrameCounts(frameCount);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Graphics Mesh（每帧）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "与骨骼同级：Graphics 节点的 localPosition(X/Z) 与 localScale(X/Z)，骨骼数据互不影响。",
                MessageType.None);
            DrawMeshFrameList(frameCount);

            EditorGUILayout.Space(6);
            DrawTrackToolbar(frameCount);

            EditorGUILayout.Space(6);
            DrawBoneTrackList(frameCount);

            if (boneTracksProp.arraySize > 0)
            {
                selectedTrackIndex = Mathf.Clamp(selectedTrackIndex, 0, boneTracksProp.arraySize - 1);
                EditorGUILayout.Space(8);
                DrawSelectedTrackEditor(frameCount);
            }
            else
            {
                EditorGUILayout.HelpBox("点击「添加骨骼」开始配置。每个骨骼可独立设置各帧 localPosition。", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTrackToolbar(int frameCount)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("添加骨骼", GUILayout.Height(24)))
                {
                    ShowAddBoneMenu(frameCount);
                }

                using (new EditorGUI.DisabledScope(boneTracksProp.arraySize == 0))
                {
                    if (GUILayout.Button("删除当前骨骼", GUILayout.Height(24)))
                    {
                        boneTracksProp.DeleteArrayElementAtIndex(selectedTrackIndex);
                        selectedTrackIndex = Mathf.Clamp(selectedTrackIndex, 0, Mathf.Max(0, boneTracksProp.arraySize - 1));
                    }
                }
            }
        }

        private void ShowAddBoneMenu(int frameCount)
        {
            GenericMenu menu = new GenericMenu();
            foreach (BindBoneTypeEditor bone in System.Enum.GetValues(typeof(BindBoneTypeEditor)))
            {
                if (HasTrack(bone))
                {
                    menu.AddDisabledItem(new GUIContent(bone.ToString()));
                    continue;
                }

                BindBoneTypeEditor captured = bone;
                menu.AddItem(new GUIContent(bone.ToString()), false, () =>
                {
                    AddTrack(captured, frameCount);
                });
            }

            menu.ShowAsContext();
        }

        private bool HasTrack(BindBoneTypeEditor boneType)
        {
            for (int i = 0; i < boneTracksProp.arraySize; i++)
            {
                SerializedProperty trackProp = boneTracksProp.GetArrayElementAtIndex(i);
                SerializedProperty boneProp = trackProp.FindPropertyRelative("boneType");
                if (boneProp != null && boneProp.enumValueIndex == (int)FrameAnimBoneTypeConverter.ToRuntime(boneType))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddTrack(BindBoneTypeEditor boneType, int frameCount)
        {
            boneTracksProp.InsertArrayElementAtIndex(boneTracksProp.arraySize);
            SerializedProperty trackProp = boneTracksProp.GetArrayElementAtIndex(boneTracksProp.arraySize - 1);
            trackProp.FindPropertyRelative("boneType").enumValueIndex = (int)FrameAnimBoneTypeConverter.ToRuntime(boneType);
            SerializedProperty positionsProp = trackProp.FindPropertyRelative("framePositions");
            positionsProp.ClearArray();
            for (int i = 0; i < frameCount; i++)
            {
                positionsProp.InsertArrayElementAtIndex(i);
            }

            selectedTrackIndex = boneTracksProp.arraySize - 1;
            selectedFrameIndex = 0;
        }

        private void DrawBoneTrackList(int frameCount)
        {
            EditorGUILayout.LabelField("已配置骨骼", EditorStyles.miniBoldLabel);
            if (boneTracksProp.arraySize == 0)
            {
                return;
            }

            for (int i = 0; i < boneTracksProp.arraySize; i++)
            {
                SerializedProperty trackProp = boneTracksProp.GetArrayElementAtIndex(i);
                SerializedProperty boneProp = trackProp.FindPropertyRelative("boneType");
                string label = boneProp != null ? ((FrameAnimBindBoneType)boneProp.enumValueIndex).ToString() : $"Track {i}";
                if (GUILayout.Toggle(selectedTrackIndex == i, label, "MiniButton"))
                {
                    selectedTrackIndex = i;
                    selectedFrameIndex = Mathf.Clamp(selectedFrameIndex, 0, frameCount - 1);
                }
            }
        }

        private void DrawSelectedTrackEditor(int frameCount)
        {
            SerializedProperty trackProp = boneTracksProp.GetArrayElementAtIndex(selectedTrackIndex);
            SerializedProperty boneProp = trackProp.FindPropertyRelative("boneType");
            SerializedProperty positionsProp = trackProp.FindPropertyRelative("framePositions");

            string boneName = boneProp != null ? ((FrameAnimBindBoneType)boneProp.enumValueIndex).ToString() : "Bone";
            EditorGUILayout.LabelField($"编辑骨骼: {boneName}", EditorStyles.boldLabel);

            selectedFrameIndex = Mathf.Clamp(selectedFrameIndex, 0, frameCount - 1);
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < frameCount; i++)
                {
                    GUIStyle style = i == selectedFrameIndex ? "Button" : "MiniButton";
                    if (GUILayout.Button($"Frame {i}", style, GUILayout.Height(22)))
                    {
                        selectedFrameIndex = i;
                    }
                }
            }

            SerializedProperty positionProp = positionsProp.GetArrayElementAtIndex(selectedFrameIndex).FindPropertyRelative("localPosition");
            Vector3 pos = positionProp.vector3Value;
            EditorGUI.BeginChangeCheck();
            Vector2 xz = EditorGUILayout.Vector2Field($"Frame {selectedFrameIndex} 骨骼 localPosition (X, Z)", new Vector2(pos.x, pos.z));
            if (EditorGUI.EndChangeCheck())
            {
                positionProp.vector3Value = new Vector3(xz.x, 0f, xz.y);
            }

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("清零当前帧"))
                {
                    positionProp.vector3Value = Vector3.zero;
                }

                if (GUILayout.Button("清零全部帧"))
                {
                    for (int i = 0; i < positionsProp.arraySize; i++)
                    {
                        positionsProp.GetArrayElementAtIndex(i).FindPropertyRelative("localPosition").vector3Value = Vector3.zero;
                    }
                }

                if (GUILayout.Button("复制当前帧到全部"))
                {
                    Vector3 value = positionProp.vector3Value;
                    for (int i = 0; i < positionsProp.arraySize; i++)
                    {
                        positionsProp.GetArrayElementAtIndex(i).FindPropertyRelative("localPosition").vector3Value = value;
                    }
                }
            }
        }

        private void DrawMeshFrameList(int frameCount)
        {
            if (frameMeshFramesProp == null)
            {
                return;
            }

            selectedFrameIndex = Mathf.Clamp(selectedFrameIndex, 0, frameCount - 1);
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < frameCount; i++)
                {
                    if (GUILayout.Toggle(selectedFrameIndex == i, $"Frame {i}", selectedFrameIndex == i ? "Button" : "MiniButton", GUILayout.Height(22)))
                    {
                        selectedFrameIndex = i;
                    }
                }
            }

            if (frameMeshFramesProp.arraySize > selectedFrameIndex)
            {
                SerializedProperty meshProp = frameMeshFramesProp.GetArrayElementAtIndex(selectedFrameIndex);
                SerializedProperty offsetProp = meshProp.FindPropertyRelative("localPositionXZ");
                SerializedProperty scaleProp = meshProp.FindPropertyRelative("localScaleXZ");
                EditorGUILayout.PropertyField(offsetProp, new GUIContent($"Frame {selectedFrameIndex} Graphics 偏移 (X, Z)"));
                EditorGUILayout.PropertyField(scaleProp, new GUIContent($"Frame {selectedFrameIndex} Graphics 缩放 (X, Z)"));
            }
        }

        private void SyncMeshFrameCounts(int frameCount)
        {
            if (frameMeshFramesProp == null)
            {
                return;
            }

            while (frameMeshFramesProp.arraySize < frameCount)
            {
                frameMeshFramesProp.InsertArrayElementAtIndex(frameMeshFramesProp.arraySize);
            }

            while (frameMeshFramesProp.arraySize > frameCount)
            {
                frameMeshFramesProp.DeleteArrayElementAtIndex(frameMeshFramesProp.arraySize - 1);
            }
        }

        private void SyncAllTrackFrameCounts(int frameCount)
        {
            for (int i = 0; i < boneTracksProp.arraySize; i++)
            {
                SerializedProperty positionsProp = boneTracksProp.GetArrayElementAtIndex(i).FindPropertyRelative("framePositions");
                while (positionsProp.arraySize < frameCount)
                {
                    positionsProp.InsertArrayElementAtIndex(positionsProp.arraySize);
                }

                while (positionsProp.arraySize > frameCount)
                {
                    positionsProp.DeleteArrayElementAtIndex(positionsProp.arraySize - 1);
                }
            }
        }
    }

    public static class FrameAnimBoneConfigAssetMenu
    {
        private const string DefaultFolder = "Assets/Bundles/ScriptObject/FrameBone";

        [MenuItem("Tools/Frame2D/Create Frame Anim Bone Config")]
        public static void CreateAsset()
        {
            EnsureFolder(DefaultFolder);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{DefaultFolder}/FrameAnimBoneConfig.asset");
            FrameAnimBoneConfig config = ScriptableObject.CreateInstance<FrameAnimBoneConfig>();
            config.frameCount = 4;
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(config);
            Selection.activeObject = config;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Bundles"))
            {
                AssetDatabase.CreateFolder("Assets", "Bundles");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Bundles/ScriptObject"))
            {
                AssetDatabase.CreateFolder("Assets/Bundles", "ScriptObject");
            }

            AssetDatabase.CreateFolder("Assets/Bundles/ScriptObject", "FrameBone");
        }
    }
}
