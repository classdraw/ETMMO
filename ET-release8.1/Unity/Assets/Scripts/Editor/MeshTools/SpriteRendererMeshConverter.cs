using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class SpriteRendererMeshConverter
    {
        private const string SpriteShaderName = "Custom/SR_WorldSpriteTransparent";

        public struct ConvertResult
        {
            public bool Success;
            public string Message;
            public string OutputPrefabPath;
        }

        public static ConvertResult ConvertPrefab(DefaultAsset outputFolderAsset, UnityEngine.GameObject prefabAsset)
        {
            string outputRootFolder = AssetDatabase.GetAssetPath(outputFolderAsset);
            if (string.IsNullOrEmpty(outputRootFolder) || !AssetDatabase.IsValidFolder(outputRootFolder))
            {
                return Fail("请拖入 Project 中的输出文件夹。");
            }

            string sourcePrefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrEmpty(sourcePrefabPath) || !sourcePrefabPath.EndsWith(".prefab"))
            {
                return Fail("请拖入 Project 中的 Prefab 资源。");
            }

            if (PrefabUtility.GetPrefabAssetType(prefabAsset) == PrefabAssetType.NotAPrefab)
            {
                return Fail("所选对象不是 Prefab 资源。");
            }

            UnityEngine.Shader spriteShader = UnityEngine.Shader.Find(SpriteShaderName);
            if (spriteShader == null)
            {
                return Fail($"未找到 Shader：{SpriteShaderName}");
            }

            AssetDatabase.StartAssetEditing();
            try
            {
                ConvertResult result = ConvertPrefabAtPath(
                    sourcePrefabPath,
                    NormalizeAssetPath(outputRootFolder),
                    spriteShader);
                if (!result.Success)
                {
                    return result;
                }

                result.Message = $"已转换，输出：{result.OutputPrefabPath}";
                return result;
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static int CountSpriteRenderersInPrefab(UnityEngine.GameObject prefabAsset)
        {
            string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrEmpty(prefabPath))
            {
                return 0;
            }

            UnityEngine.GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                return CountValidSpriteRenderers(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static ConvertResult ConvertPrefabAtPath(
            string sourcePrefabPath,
            string outputRootFolder,
            UnityEngine.Shader spriteShader)
        {
            if (PrefabUtility.GetPrefabAssetType(AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(sourcePrefabPath))
                == PrefabAssetType.NotAPrefab)
            {
                return Fail("不是 Prefab 资源。");
            }

            string prefabName = Path.GetFileNameWithoutExtension(sourcePrefabPath);
            string outputDir = NormalizeAssetPath($"{outputRootFolder}/{prefabName}");
            EnsureFolderExists(outputDir);

            string outputPrefabPath = AssetDatabase.GenerateUniqueAssetPath($"{outputDir}/{prefabName}_mesh.prefab");

            UnityEngine.GameObject root = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
            try
            {
                UnityEngine.SpriteRenderer[] spriteRenderers = root.GetComponentsInChildren<UnityEngine.SpriteRenderer>(true);
                if (spriteRenderers.Length == 0)
                {
                    return Fail("未找到 SpriteRenderer 组件。");
                }

                int convertedCount = 0;
                foreach (UnityEngine.SpriteRenderer spriteRenderer in spriteRenderers)
                {
                    if (ConvertSpriteRendererNode(spriteRenderer, spriteShader, outputDir, prefabName))
                    {
                        convertedCount++;
                    }
                }

                if (convertedCount == 0)
                {
                    return Fail("未找到带 Sprite 的 SpriteRenderer。");
                }

                CleanupPrefabHierarchy(root);

                PrefabUtility.SaveAsPrefabAsset(root, outputPrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return new ConvertResult
                {
                    Success = true,
                    OutputPrefabPath = outputPrefabPath,
                    Message = $"输出：{outputPrefabPath}",
                };
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static int CountValidSpriteRenderers(UnityEngine.GameObject root)
        {
            int count = 0;
            UnityEngine.SpriteRenderer[] spriteRenderers = root.GetComponentsInChildren<UnityEngine.SpriteRenderer>(true);
            foreach (UnityEngine.SpriteRenderer spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer.sprite != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ConvertSpriteRendererNode(
            UnityEngine.SpriteRenderer spriteRenderer,
            UnityEngine.Shader spriteShader,
            string outputDir,
            string prefabBaseName)
        {
            UnityEngine.Sprite sprite = spriteRenderer.sprite;
            if (sprite == null)
            {
                return false;
            }

            UnityEngine.GameObject go = spriteRenderer.gameObject;
            UnityEngine.Transform transform = go.transform;

            UnityEngine.Color color = spriteRenderer.color;
            bool flipX = spriteRenderer.flipX;
            bool flipY = spriteRenderer.flipY;

            UnityEngine.Vector3 originalScale = transform.localScale;
            UnityEngine.Vector3 originalPosition = transform.localPosition;
            UnityEngine.Quaternion originalRotation = transform.localRotation;

            string assetBaseName = SanitizeFileName($"{prefabBaseName}_{go.name}");
            UnityEngine.Material material = CreateAndSaveSpriteMaterial(outputDir, assetBaseName, spriteShader, sprite, color);

            UnityEngine.Mesh mesh = BuildSpriteQuadMesh(sprite, flipX, flipY);
            string meshPath = AssetDatabase.GenerateUniqueAssetPath($"{outputDir}/{assetBaseName}.asset");
            mesh.name = Path.GetFileNameWithoutExtension(meshPath);
            AssetDatabase.CreateAsset(mesh, meshPath);

            UnityEngine.Object.DestroyImmediate(spriteRenderer);

            UnityEngine.MeshFilter meshFilter = go.GetComponent<UnityEngine.MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = go.AddComponent<UnityEngine.MeshFilter>();
            }

            meshFilter.sharedMesh = mesh;

            UnityEngine.MeshRenderer meshRenderer = go.GetComponent<UnityEngine.MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = go.AddComponent<UnityEngine.MeshRenderer>();
            }

            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = true;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            ApplyOriginalTransform(transform, originalPosition, originalRotation, originalScale);

            return true;
        }

        private static UnityEngine.Mesh BuildSpriteQuadMesh(UnityEngine.Sprite sprite, bool flipX, bool flipY)
        {
            UnityEngine.Bounds bounds = sprite.bounds;
            float minX = bounds.min.x;
            float maxX = bounds.max.x;
            float minY = bounds.min.y;
            float maxY = bounds.max.y;

            if (flipX)
            {
                float temp = minX;
                minX = -maxX;
                maxX = -temp;
            }

            if (flipY)
            {
                float temp = minY;
                minY = -maxY;
                maxY = -temp;
            }

            // Quad 在 XZ 平面，三角面绕序保证法线朝 +Y。
            var vertices = new[]
            {
                new UnityEngine.Vector3(minX, 0f, minY),
                new UnityEngine.Vector3(maxX, 0f, minY),
                new UnityEngine.Vector3(maxX, 0f, maxY),
                new UnityEngine.Vector3(minX, 0f, maxY),
            };

            var triangles = new[] { 0, 2, 1, 0, 3, 2 };
            var uvs = new[]
            {
                new UnityEngine.Vector2(0f, 0f),
                new UnityEngine.Vector2(1f, 0f),
                new UnityEngine.Vector2(1f, 1f),
                new UnityEngine.Vector2(0f, 1f),
            };
            var normals = new[]
            {
                UnityEngine.Vector3.up,
                UnityEngine.Vector3.up,
                UnityEngine.Vector3.up,
                UnityEngine.Vector3.up,
            };

            var mesh = new UnityEngine.Mesh { name = sprite.name };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static void ApplyOriginalTransform(
            UnityEngine.Transform transform,
            UnityEngine.Vector3 originalPosition,
            UnityEngine.Quaternion originalRotation,
            UnityEngine.Vector3 originalScale)
        {
            transform.localPosition = new UnityEngine.Vector3(
                originalPosition.x,
                originalPosition.z,
                originalPosition.y);
            transform.localRotation = originalRotation;
            transform.localScale = originalScale;
        }

        private static void CleanupPrefabHierarchy(UnityEngine.GameObject root)
        {
            UnityEngine.Transform[] transforms = root.GetComponentsInChildren<UnityEngine.Transform>(true);
            foreach (UnityEngine.Transform nodeTransform in transforms)
            {
                CleanupConvertedNode(nodeTransform.gameObject);
            }
        }

        private static void CleanupConvertedNode(UnityEngine.GameObject go)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null)
                {
                    continue;
                }

                if (component is UnityEngine.Transform)
                {
                    continue;
                }

                if (component is UnityEngine.MeshFilter)
                {
                    continue;
                }

                if (component is UnityEngine.MeshRenderer)
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(component);
            }
        }

        private static UnityEngine.Material CreateAndSaveSpriteMaterial(
            string outputDir,
            string materialName,
            UnityEngine.Shader spriteShader,
            UnityEngine.Sprite sprite,
            UnityEngine.Color color)
        {
            string materialPath = AssetDatabase.GenerateUniqueAssetPath($"{outputDir}/{materialName}.mat");
            UnityEngine.Material material = new UnityEngine.Material(spriteShader);
            ApplySpriteTexture(material, sprite, color);
            material.name = Path.GetFileNameWithoutExtension(materialPath);
            AssetDatabase.CreateAsset(material, materialPath);
            return material;
        }

        private static void ApplySpriteTexture(UnityEngine.Material material, UnityEngine.Sprite sprite, UnityEngine.Color color)
        {
            UnityEngine.Texture2D texture = sprite.texture;
            UnityEngine.Rect textureRect = sprite.textureRect;
            UnityEngine.Vector2 textureSize = new UnityEngine.Vector2(texture.width, texture.height);
            UnityEngine.Vector2 offset = new UnityEngine.Vector2(
                textureRect.x / textureSize.x,
                textureRect.y / textureSize.y);
            UnityEngine.Vector2 scale = new UnityEngine.Vector2(
                textureRect.width / textureSize.x,
                textureRect.height / textureSize.y);

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTextureOffset("_BaseMap", offset);
                material.SetTextureScale("_BaseMap", scale);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_OcclusionFade"))
            {
                material.SetFloat("_OcclusionFade", 1f);
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, '_');
            }

            return string.IsNullOrWhiteSpace(name) ? "Material" : name;
        }

        private static string NormalizeAssetPath(string path)
        {
            return path?.Replace('\\', '/').Trim() ?? string.Empty;
        }

        private static void EnsureFolderExists(string assetFolderPath)
        {
            assetFolderPath = NormalizeAssetPath(assetFolderPath);
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            string parentPath = Path.GetDirectoryName(assetFolderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(assetFolderPath);
            if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(folderName))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parentPath))
            {
                EnsureFolderExists(parentPath);
            }

            AssetDatabase.CreateFolder(parentPath, folderName);
        }

        private static ConvertResult Fail(string message)
        {
            return new ConvertResult
            {
                Success = false,
                Message = message,
            };
        }
    }
}
