using ET;
using UnityEditor;
using UnityEngine;

namespace ET.Editor.Frame2D
{
    [CustomEditor(typeof(FrameSheetAnimPlayer))]
    public class FrameSheetAnimPlayerEditor : UnityEditor.Editor
    {
        private FrameSheetAnimType previewAnim = FrameSheetAnimType.Idle;
        private FrameSheetFacing previewFacing = FrameSheetFacing.Down;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            SerializedProperty configProp = serializedObject.FindProperty("animConfig");
            SerializedProperty rendererProp = serializedObject.FindProperty("targetRenderer");
            bool canPreview = configProp.objectReferenceValue != null && rendererProp.objectReferenceValue != null;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!canPreview))
            {
                previewAnim = (FrameSheetAnimType)EditorGUILayout.EnumPopup("Animation", previewAnim);
                previewFacing = (FrameSheetFacing)EditorGUILayout.EnumPopup("Facing", previewFacing);

                if (GUILayout.Button("Play Preview", GUILayout.Height(28)))
                {
                    FrameSheetAnimPlayer player = (FrameSheetAnimPlayer)target;
                    if (!player.Play(previewAnim, previewFacing))
                    {
                        Debug.LogWarning($"FrameSheetAnimPlayer 预览失败: {previewAnim}, {previewFacing}", player);
                    }
                    else
                    {
                        EditorUtility.SetDirty(player);
                        SceneView.RepaintAll();
                    }
                }
            }

            if (!canPreview)
            {
                EditorGUILayout.HelpBox("Anim Config 与 Target Renderer 都不为空时，可在编辑器中预览。", MessageType.Info);
            }
        }
    }
}
