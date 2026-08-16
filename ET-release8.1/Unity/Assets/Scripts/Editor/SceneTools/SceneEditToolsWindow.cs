using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 场景编辑辅助：批量显隐碰撞节点。
    /// </summary>
    public class SceneEditToolsWindow : EditorWindow
    {
        private const string NavMeshTag = "NavMesh";

        [MenuItem("Tools/场景编辑操作", false, 52)]
        public static void Open()
        {
            var window = GetWindow<SceneEditToolsWindow>(true, "场景编辑操作", true);
            window.minSize = new Vector2(320, 200);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "处理当前已加载场景中：名字为 Cube（不区分大小写），或父节点名字为 Collider（不区分大小写）的节点。",
                MessageType.Info);
            EditorGUILayout.Space(8);

            if (GUILayout.Button("点击隐藏碰撞", GUILayout.Height(34)))
            {
                SetCollisionVisible(false);
            }

            if (GUILayout.Button("点击显示碰撞", GUILayout.Height(34)))
            {
                SetCollisionVisible(true);
            }

            if (GUILayout.Button("Tag 改为 NavMesh", GUILayout.Height(34)))
            {
                SetCollisionTagToNavMesh();
            }
        }

        private static void SetCollisionVisible(bool visible)
        {
            List<GameObject> targets = CollectCollisionObjects();
            if (targets.Count == 0)
            {
                EditorUtility.DisplayDialog("场景编辑操作", "当前已加载场景中没有匹配的碰撞节点。", "确定");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            string undoName = visible ? "显示碰撞" : "隐藏碰撞";
            var dirtyScenes = new HashSet<int>();

            foreach (GameObject go in targets)
            {
                if (go == null || go.activeSelf == visible)
                {
                    continue;
                }

                Undo.RecordObject(go, undoName);
                go.SetActive(visible);
                EditorUtility.SetDirty(go);
                if (go.scene.IsValid())
                {
                    dirtyScenes.Add(go.scene.handle);
                }
            }

            Undo.SetCurrentGroupName(undoName);
            Undo.CollapseUndoOperations(undoGroup);
            MarkScenesDirty(dirtyScenes);
            EditorUtility.DisplayDialog("场景编辑操作",
                $"{(visible ? "已显示" : "已隐藏")} {targets.Count} 个碰撞节点。",
                "确定");
        }

        private static void SetCollisionTagToNavMesh()
        {
            List<GameObject> targets = CollectCollisionObjects();
            if (targets.Count == 0)
            {
                EditorUtility.DisplayDialog("场景编辑操作", "当前已加载场景中没有匹配的碰撞节点。", "确定");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            const string undoName = "碰撞节点 Tag → NavMesh";
            var dirtyScenes = new HashSet<int>();
            int tagChanged = 0;

            try
            {
                foreach (GameObject go in targets)
                {
                    if (go == null || go.CompareTag(NavMeshTag))
                    {
                        continue;
                    }

                    Undo.RecordObject(go, undoName);
                    go.tag = NavMeshTag;
                    EditorUtility.SetDirty(go);
                    tagChanged++;
                    if (go.scene.IsValid())
                    {
                        dirtyScenes.Add(go.scene.handle);
                    }
                }
            }
            catch (UnityException e)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                EditorUtility.DisplayDialog("场景编辑操作",
                    $"设置 Tag 失败。请确认 Project Settings → Tags and Layers 中存在「{NavMeshTag}」标签。\n{e.Message}",
                    "确定");
                return;
            }

            Undo.SetCurrentGroupName(undoName);
            Undo.CollapseUndoOperations(undoGroup);
            MarkScenesDirty(dirtyScenes);
            EditorUtility.DisplayDialog("场景编辑操作",
                $"匹配节点：{targets.Count} 个\nTag 已改为「{NavMeshTag}」：{tagChanged} 个",
                "确定");
        }

        private static void MarkScenesDirty(HashSet<int> dirtyScenes)
        {
            if (dirtyScenes.Count == 0)
            {
                return;
            }

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.IsValid() && dirtyScenes.Contains(scene.handle))
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }

        private static List<GameObject> CollectCollisionObjects()
        {
            var result = new List<GameObject>();
            var visited = new HashSet<int>();

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                foreach (GameObject root in roots)
                {
                    if (root == null)
                    {
                        continue;
                    }

                    Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in transforms)
                    {
                        if (t == null || !IsCollisionObject(t))
                        {
                            continue;
                        }

                        GameObject go = t.gameObject;
                        if (!visited.Add(go.GetInstanceID()))
                        {
                            continue;
                        }

                        result.Add(go);
                    }
                }
            }

            return result;
        }

        private static bool IsCollisionObject(Transform t)
        {
            if (string.Equals(t.name, "Cube", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            Transform parent = t.parent;
            return parent != null &&
                   string.Equals(parent.name, "Collider", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
