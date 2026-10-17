using System.Collections.Generic;
using System.Text;
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
        private const string PixelMaterialSuffix = "Pixel";
        private const string PixelMaterialsFolder = "Assets/Bundles/Materials/Pixel";

        [MenuItem("Tools/场景编辑操作", false, 52)]
        public static void Open()
        {
            var window = GetWindow<SceneEditToolsWindow>(true, "场景编辑操作", true);
            window.minSize = new Vector2(320, 280);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "隐藏/显示碰撞：当前已加载场景中所有 Collider（含 MeshCollider、BoxCollider 等）会切换 enabled；\n"
                + "另外对名字为 Cube（不区分大小写），或父节点名为 Collider / Colliders（不区分大小写）的节点切换 SetActive。",
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

            EditorGUILayout.Space(12);
            EditorGUILayout.HelpBox(
                "选中 Hierarchy 中的预制体实例，或 Project 中的预制体资源：遍历其下所有 Renderer，"
                + "将材质「原名」替换为工程内同名 Pixel 材质（例：Baker's_house → Baker's_housePixel）。"
                + "已是 *Pixel 或找不到对应 .mat 的槽位会跳过。",
                MessageType.Info);
            EditorGUILayout.Space(4);

            if (GUILayout.Button("选中预制体：材质替换为 Pixel", GUILayout.Height(34)))
            {
                ReplaceSelectedPrefabMaterialsWithPixel();
            }
        }

        private static void SetCollisionVisible(bool visible)
        {
            List<Collider> colliders = CollectAllCollidersInLoadedScenes();
            List<GameObject> proxyTargets = CollectCollisionObjects();
            if (colliders.Count == 0 && proxyTargets.Count == 0)
            {
                EditorUtility.DisplayDialog("场景编辑操作", "当前已加载场景中没有碰撞体或碰撞代理节点。", "确定");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            string undoName = visible ? "显示碰撞" : "隐藏碰撞";
            var dirtyScenes = new HashSet<int>();
            int colliderChanged = 0;
            int proxyChanged = 0;

            foreach (Collider collider in colliders)
            {
                if (collider == null || collider.enabled == visible)
                {
                    continue;
                }

                Undo.RecordObject(collider, undoName);
                collider.enabled = visible;
                EditorUtility.SetDirty(collider);
                colliderChanged++;
                if (collider.gameObject.scene.IsValid())
                {
                    dirtyScenes.Add(collider.gameObject.scene.handle);
                }
            }

            foreach (GameObject go in proxyTargets)
            {
                if (go == null || go.activeSelf == visible)
                {
                    continue;
                }

                Undo.RecordObject(go, undoName);
                go.SetActive(visible);
                EditorUtility.SetDirty(go);
                proxyChanged++;
                if (go.scene.IsValid())
                {
                    dirtyScenes.Add(go.scene.handle);
                }
            }

            Undo.SetCurrentGroupName(undoName);
            Undo.CollapseUndoOperations(undoGroup);
            MarkScenesDirty(dirtyScenes);
            EditorUtility.DisplayDialog("场景编辑操作",
                $"{(visible ? "已启用" : "已禁用")} Collider：{colliderChanged} 个\n"
                + $"{(visible ? "已显示" : "已隐藏")} 碰撞代理节点：{proxyChanged} 个",
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

        private static void ReplaceSelectedPrefabMaterialsWithPixel()
        {
            GameObject selected = Selection.activeGameObject;
                    UnityEngine.Object activeObject = Selection.activeObject;

            if (selected == null && activeObject is GameObject goAsset && PrefabUtility.IsPartOfPrefabAsset(goAsset))
            {
                selected = goAsset;
            }

            if (selected == null)
            {
                EditorUtility.DisplayDialog("场景编辑操作", "请在 Hierarchy 或 Project 中选中一个预制体（或其根节点）。", "确定");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(selected);
            bool isPrefabAsset = !string.IsNullOrEmpty(prefabPath) && PrefabUtility.IsPartOfPrefabAsset(selected);

            if (isPrefabAsset)
            {
                GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    ReplaceRendererMaterialsWithPixel(prefabContents, out int slots, out int renderers, out List<string> missing);
                    PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
                    ShowPixelReplaceResult(slots, renderers, missing, prefabPath, savedPrefab: true);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabContents);
                }

                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            const string undoName = "材质替换为 Pixel";
            ReplaceRendererMaterialsWithPixel(selected, out int replacedSlots, out int affectedRenderers, out List<string> missingNames);
            Undo.SetCurrentGroupName(undoName);
            Undo.CollapseUndoOperations(undoGroup);

            var dirtyScenes = new HashSet<int>();
            if (selected.scene.IsValid())
            {
                dirtyScenes.Add(selected.scene.handle);
            }

            MarkScenesDirty(dirtyScenes);
            PrefabUtility.RecordPrefabInstancePropertyModifications(selected);
            ShowPixelReplaceResult(replacedSlots, affectedRenderers, missingNames, selected.name, savedPrefab: false);
        }

        private static void ShowPixelReplaceResult(
            int replacedSlots,
            int affectedRenderers,
            List<string> missingNames,
            string targetName,
            bool savedPrefab)
        {
            var message = new StringBuilder();
            message.AppendLine($"目标：{targetName}");
            message.AppendLine($"已修改 Renderer：{affectedRenderers} 个");
            message.AppendLine($"已替换材质槽：{replacedSlots} 个");
            if (savedPrefab)
            {
                message.AppendLine("（已写回预制体资源）");
            }

            if (missingNames != null && missingNames.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("未找到 Pixel 材质（已跳过）：");
                int showMax = 12;
                for (int i = 0; i < missingNames.Count && i < showMax; i++)
                {
                    message.AppendLine("· " + missingNames[i]);
                }

                if (missingNames.Count > showMax)
                {
                    message.AppendLine($"... 另有 {missingNames.Count - showMax} 个");
                }
            }

            EditorUtility.DisplayDialog("场景编辑操作", message.ToString(), "确定");
        }

        private static void ReplaceRendererMaterialsWithPixel(
            GameObject root,
            out int replacedSlots,
            out int affectedRenderers,
            out List<string> missingPixelMaterials)
        {
            replacedSlots = 0;
            affectedRenderers = 0;
            missingPixelMaterials = new List<string>();
            var missingSet = new HashSet<string>();
            var pixelCache = new Dictionary<string, Material>();

            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                bool rendererChanged = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material source = materials[i];
                    if (source == null)
                    {
                        continue;
                    }

                    string baseName = GetMaterialBaseName(source);
                    if (string.IsNullOrEmpty(baseName)
                        || baseName.EndsWith(PixelMaterialSuffix, System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string pixelName = baseName + PixelMaterialSuffix;
                    if (!pixelCache.TryGetValue(pixelName, out Material pixelMaterial))
                    {
                        pixelMaterial = FindPixelMaterialByName(pixelName);
                        pixelCache[pixelName] = pixelMaterial;
                    }

                    if (pixelMaterial == null)
                    {
                        if (missingSet.Add(pixelName))
                        {
                            missingPixelMaterials.Add(pixelName);
                        }

                        continue;
                    }

                    if (materials[i] == pixelMaterial)
                    {
                        continue;
                    }

                    materials[i] = pixelMaterial;
                    rendererChanged = true;
                    replacedSlots++;
                }

                if (!rendererChanged)
                {
                    continue;
                }

                Undo.RecordObject(renderer, "材质替换为 Pixel");
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
                affectedRenderers++;
            }
        }

        private static string GetMaterialBaseName(Material material)
        {
            string name = material.name;
            const string instanceSuffix = " (Instance)";
            if (name.EndsWith(instanceSuffix, System.StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - instanceSuffix.Length);
            }

            return name;
        }

        private static Material FindPixelMaterialByName(string pixelMaterialName)
        {
            string inPixelFolder = $"{PixelMaterialsFolder}/{pixelMaterialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(inPixelFolder);
            if (mat != null)
            {
                return mat;
            }

            string[] guids = AssetDatabase.FindAssets($"{pixelMaterialName} t:Material");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material candidate = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (candidate != null && candidate.name == pixelMaterialName)
                {
                    return candidate;
                }
            }

            return null;
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

        private static List<Collider> CollectAllCollidersInLoadedScenes()
        {
            var result = new List<Collider>();
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

                    Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
                    foreach (Collider collider in colliders)
                    {
                        if (collider == null || !visited.Add(collider.GetInstanceID()))
                        {
                            continue;
                        }

                        result.Add(collider);
                    }
                }
            }

            return result;
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
            if (parent == null)
            {
                return false;
            }

            string parentName = parent.name;
            return string.Equals(parentName, "Collider", System.StringComparison.OrdinalIgnoreCase)
                   || string.Equals(parentName, "Colliders", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
