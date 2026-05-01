using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ET;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ETEditor
{
    /// <summary>
    /// 从Unity的NavMesh组件里导出地图数据，供服务器来使用
    /// https://blog.csdn.net/huutu/article/details/52672505
    /// </summary>
    public class NavMeshExporter: Editor
    {
        public const byte VERSION = 1;

        private class Vert
        {
            public int id;
            public float x;
            public float y;
            public float z;

            public UnityEngine.Vector3 ToVector3()
            {
                return new UnityEngine.Vector3(x, y, z);
            }
        }

        private class Face
        {
            public int id;
            public int area;
            public float centerX;
            public float centerZ;
            public float normalX;
            public float normalZ;
            public double normalA;
            public double normalB;
            public double normalC;
            public double normalD;
            public uint sortValue;
            public List<Vert> verts = new List<Vert>();
        }

        private class Pair
        {
            public float centerX;
            public float centerZ;
            public float distance;
            public Face firstEdgeFace;
            public int firstEdgeIndex;
            public Face secondEdgeFace;
            public int secondEdgeIndex;
        }

        private static List<Vert> vertList = new List<Vert>();
        private static List<Face> faceList = new List<Face>();
        private static List<Pair> pairList = new List<Pair>();
        private static Dictionary<Vert, Face> vertFaceDict = new Dictionary<Vert, Face>();
        private static Dictionary<Vert, Dictionary<Vert, Pair>> vertPairDict = new Dictionary<Vert, Dictionary<Vert, Pair>>();
        private static Dictionary<float, Dictionary<float, Vert>> pointVertDict = new Dictionary<float, Dictionary<float, Vert>>();
        private static Dictionary<int, Vert> indexVertDict = new Dictionary<int, Vert>();
        private static string outputClientFolder = "../RecastNavMesh/Meshes/";
        private static string outputServerFolder = "../Config/RecastNavData/ExportedObj/";

        #region 菜单主函数
        [MenuItem("ET/NavMesh/ExportSceneObj", false, ETMenuItemPriority.NavMesh)]
        public static void ExportScene()
        {
            var triangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();
            if (triangulation.indices.Length < 3)
            {
                Debug.LogError($"NavMeshExporter ExportScene Error - 场景里没有需要被导出的物体，请先用NavMesh进行Bake。");
                return;
            }

            vertList.Clear();
            faceList.Clear();
            pairList.Clear();
            vertFaceDict.Clear();
            vertPairDict.Clear();
            pointVertDict.Clear();
            indexVertDict.Clear();
            InputVertices(triangulation.vertices);
            InputTriangles(triangulation.indices, triangulation.areas);
            IndexVertsAndFaces();
            //WriteFile();

            // 导出*_internal.Obj，仅供Unity编辑器自己查看
            //WriteUnityObjFile();
            // 导出Recast可用的*.Obj文件
            WriteRecastObjFile();
            // 拷贝Obj和Bytes文件到服务器目录下 TODO 暂不需要
            //CopyObjFiles();

            Debug.Log($"NavMesh Output Info - Vertices:[{vertList.Count}] - Faces:[{faceList.Count}]");
        }

        #endregion

        #region 导出Bytes

        private static void InputVertices(Vector3[] vertices)
        {
            for (int i = 0, n = vertices.Length - 1; i <= n; i++)
            {
                var point = vertices[i];
                var x = (float) Math.Round(point.x, 2);
                var y = (float) Math.Round(point.y, 2);
                var z = (float) Math.Round(point.z, 2);
                if (!pointVertDict.ContainsKey(x))
                {
                    pointVertDict.Add(x, new Dictionary<float, Vert>());
                }

                Vert vert;
                if (pointVertDict[x].ContainsKey(z))
                {
                    vert = pointVertDict[x][z];
                }
                else
                {
                    vert = new Vert();
                    vert.x = x;
                    vert.y = y;
                    vert.z = z;
                    pointVertDict[x][z] = vert;
                }

                indexVertDict.Add(i, vert);
            }
        }

        private static void InputTriangles(int[] indices, int[] areas)
        {
            Face face = null;
            var faceIndices = new HashSet<int>();
            for (int i = 0, n = areas.Length; i < n; i++)
            {
                var triangleIndexList = new int[3];
                var triangleVertList = new Vert[3];
                for (var j = 0; j < 3; j++)
                {
                    triangleIndexList[j] = indices[i * 3 + j];
                    triangleVertList[j] = indexVertDict[triangleIndexList[j]];
                }

                var vert0 = triangleVertList[0];
                var vert1 = triangleVertList[1];
                var vert2 = triangleVertList[2];
                if (vert0 == vert1 || vert1 == vert2 || vert2 == vert0)
                {
                    continue;
                }

                var newFace = true;
                var area = areas[i] >= 3? areas[i] - 2 : 0;
                if (face != null && face.area == area)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        if (faceIndices.Contains(triangleIndexList[j]))
                        {
                            newFace = false;
                            break;
                        }
                    }
                }

                if (newFace)
                {
                    if (face != null)
                    {
                        InitFace(face);
                        faceIndices.Clear();
                    }

                    face = new Face();
                    face.area = area;
                }

                double x1 = vert1.x - vert0.x;
                double y1 = vert1.y - vert0.y;
                double z1 = vert1.z - vert0.z;
                double x2 = vert2.x - vert0.x;
                double y2 = vert2.y - vert0.y;
                double z2 = vert2.z - vert0.z;
                double normalA = y1 * z2 - z1 * y2;
                double normalB = z1 * x2 - x1 * z2;
                double normalC = x1 * y2 - y1 * x2;
                if (normalB < -0.000001 || 0.000001 < normalB)
                {
                    var normalD = normalA + normalB + normalC;
                    if (normalD > face.normalD)
                    {
                        face.normalA = normalA;
                        face.normalB = normalB;
                        face.normalC = normalC;
                        face.normalD = normalD;
                    }
                }

                for (var j = 0; j < 3; j++)
                {
                    if (!faceIndices.Contains(triangleIndexList[j]))
                    {
                        faceIndices.Add(triangleIndexList[j]);
                        face.verts.Add(triangleVertList[j]);
                    }
                }
            }

            if (face != null)
            {
                InitFace(face);
            }

            foreach (var pair in pairList)
            {
                var firstFace = pair.firstEdgeFace;
                var secondFace = pair.secondEdgeFace;
                var firstDistance = GetDistance(firstFace.centerX - pair.centerX, firstFace.centerZ - pair.centerZ);
                var secondDistance = GetDistance(secondFace.centerX - pair.centerX, secondFace.centerZ - pair.centerZ);
                pair.distance = firstDistance + secondDistance;
            }
        }

        private static float GetDistance(float deltaX, float deltaZ)
        {
            return (float) Math.Round(Math.Sqrt((double) deltaX * (double) deltaX + (double) deltaZ * (double) deltaZ), 2);
        }

        private static void InitFace(Face face)
        {
            face.centerX = 0;
            face.centerZ = 0;
            var vertCount = face.verts.Count;
            foreach (var vert in face.verts)
            {
                face.centerX += vert.x;
                face.centerZ += vert.z;
                if (!vertFaceDict.ContainsKey(vert))
                {
                    vertFaceDict.Add(vert, face);
                    vertList.Add(vert);
                }
            }

            face.centerX /= vertCount;
            face.centerZ /= vertCount;
            if (face.normalB != 0)
            {
                face.normalX = (float) Math.Round(face.normalA / face.normalB, 6);
                face.normalZ = (float) Math.Round(face.normalC / face.normalB, 6);
            }

            for (int i = 0, n = vertCount - 1; i <= n; i++)
            {
                var firstVert = face.verts[i];
                var secondVert = face.verts[i == n? 0 : i + 1];
                if (!vertPairDict.ContainsKey(firstVert))
                {
                    vertPairDict.Add(firstVert, new Dictionary<Vert, Pair>());
                }

                if (!vertPairDict.ContainsKey(secondVert))
                {
                    vertPairDict.Add(secondVert, new Dictionary<Vert, Pair>());
                }

                if (!vertPairDict[secondVert].ContainsKey(firstVert))
                {
                    var pair = new Pair();
                    pair.firstEdgeFace = face;
                    pair.firstEdgeIndex = i;
                    vertPairDict[firstVert][secondVert] = pair;
                }
                else
                {
                    var pair = vertPairDict[secondVert][firstVert];
                    pair.centerX = (firstVert.x + secondVert.x) / 2;
                    pair.centerZ = (firstVert.z + secondVert.z) / 2;
                    pair.secondEdgeFace = face;
                    pair.secondEdgeIndex = i;
                    pairList.Add(pair);
                }
            }

            faceList.Add(face);
        }

        private static void IndexVertsAndFaces()
        {
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;
            foreach (var vert in vertList)
            {
                if (minX > vert.x)
                {
                    minX = vert.x;
                }

                if (maxX < vert.x)
                {
                    maxX = vert.x;
                }

                if (minZ > vert.z)
                {
                    minZ = vert.z;
                }

                if (maxZ < vert.x)
                {
                    maxZ = vert.x;
                }
            }

            var hilbertX = 65535f / (maxX - minX);
            var hilbertZ = 65535f / (maxZ - minZ);
            foreach (var face in faceList)
            {
                var X = (uint) Math.Round((face.centerX - minX) * hilbertX);
                var Z = (uint) Math.Round((face.centerZ - minZ) * hilbertZ);
                var a = X ^ Z;
                var b = 0xFFFF ^ a;
                var c = 0xFFFF ^ (X | Z);
                var d = X & (Z ^ 0xFFFF);
                var A = a | (b >> 1);
                var B = (a >> 1) ^ a;
                var C = ((c >> 1) ^ (b & (d >> 1))) ^ c;
                var D = ((a & (c >> 1)) ^ (d >> 1)) ^ d;
                a = A;
                b = B;
                c = C;
                d = D;
                A = (a & (a >> 2)) ^ (b & (b >> 2));
                B = (a & (b >> 2)) ^ (b & ((a ^ b) >> 2));
                C ^= (a & (c >> 2)) ^ (b & (d >> 2));
                D ^= (b & (c >> 2)) ^ ((a ^ b) & (d >> 2));
                a = A;
                b = B;
                c = C;
                d = D;
                A = (a & (a >> 4)) ^ (b & (b >> 4));
                B = (a & (b >> 4)) ^ (b & ((a ^ b) >> 4));
                C ^= (a & (c >> 4)) ^ (b & (d >> 4));
                D ^= (b & (c >> 4)) ^ ((a ^ b) & (d >> 4));
                a = A;
                b = B;
                c = C;
                d = D;
                C ^= (a & (c >> 8)) ^ (b & (d >> 8));
                D ^= (b & (c >> 8)) ^ ((a ^ b) & (d >> 8));
                C ^= C >> 1;
                D ^= D >> 1;
                c = X ^ Z;
                d = D | (0xFFFF ^ (c | C));
                c = (c | (c << 8)) & 0x00FF00FF;
                c = (c | (c << 4)) & 0x0F0F0F0F;
                c = (c | (c << 2)) & 0x33333333;
                c = (c | (c << 1)) & 0x55555555;
                d = (d | (d << 8)) & 0x00FF00FF;
                d = (d | (d << 4)) & 0x0F0F0F0F;
                d = (d | (d << 2)) & 0x33333333;
                d = (d | (d << 1)) & 0x55555555;
                face.sortValue = (d << 1) | c;
            }

            faceList.Sort(SortComparison);
            for (int i = 0, n = vertList.Count; i < n; i++)
            {
                vertList[i].id = i;
            }

            for (int i = 0, n = faceList.Count; i < n; i++)
            {
                faceList[i].id = i;
            }
        }

        private static int SortComparison(Face a, Face b)
        {
            return a.sortValue.CompareTo(b.sortValue);
        }

        private static void WriteFile()
        {
            if (!System.IO.Directory.Exists(outputClientFolder))
            {
                System.IO.Directory.CreateDirectory(outputClientFolder);
            }

            var path = outputClientFolder + SceneManager.GetActiveScene().name + ".bytes";
            var writer = new BinaryWriter(new FileStream(path, FileMode.Create));
            writer.Write('N');
            writer.Write('a');
            writer.Write('v');
            writer.Write('M');
            writer.Write('e');
            writer.Write('s');
            writer.Write('h');
            writer.Write(VERSION);
            writer.Write(vertList.Count);
            foreach (var vert in vertList)
            {
                writer.Write(vert.x);
                writer.Write(vert.y);
                writer.Write(vert.z);
            }

            writer.Write(faceList.Count);
            foreach (var face in faceList)
            {
                writer.Write(face.area);
                writer.Write(face.normalX);
                writer.Write(face.normalZ);
                writer.Write(face.verts.Count);
                foreach (var vert in face.verts)
                {
                    writer.Write(vert.id);
                }
            }

            writer.Write(pairList.Count);
            foreach (var pair in pairList)
            {
                writer.Write(pair.distance);
                writer.Write(pair.firstEdgeFace.id);
                writer.Write(pair.firstEdgeIndex);
                writer.Write(pair.secondEdgeFace.id);
                writer.Write(pair.secondEdgeIndex);
            }

            writer.Flush();
            writer.Close();
            AssetDatabase.Refresh();
        }

        #endregion

        #region 导出*_internal.Obj

        // 		————————————————
        // 版权声明：本文为CSDN博主「_Captain」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
        // 原文链接：https://blog.csdn.net/huutu/article/details/52672505

        private static void WriteUnityObjFile()
        {
            var path = outputClientFolder + SceneManager.GetActiveScene().name + "_internal.obj";
            StreamWriter tmpStreamWriter = new StreamWriter(path);

            NavMeshTriangulation tmpNavMeshTriangulation = UnityEngine.AI.NavMesh.CalculateTriangulation();

            //顶点
            for (int i = 0; i < tmpNavMeshTriangulation.vertices.Length; i++)
            {
                tmpStreamWriter.WriteLine("v  " + tmpNavMeshTriangulation.vertices[i].x + " " + tmpNavMeshTriangulation.vertices[i].y + " " +
                    tmpNavMeshTriangulation.vertices[i].z);
            }

            tmpStreamWriter.WriteLine("g pPlane1");

            //索引
            for (int i = 0; i < tmpNavMeshTriangulation.indices.Length;)
            {
                tmpStreamWriter.WriteLine("f " + (tmpNavMeshTriangulation.indices[i] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 1] + 1) + " " +
                    (tmpNavMeshTriangulation.indices[i + 2] + 1));
                i = i + 3;
            }

            tmpStreamWriter.Flush();
            tmpStreamWriter.Close();

            AssetDatabase.Refresh();
        }

        #endregion

        #region 导出Obj(Recast使用)

        // ————————————————
        // 版权声明：本文为CSDN博主「Rhett_Yuan」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
        // 原文链接：https://blog.csdn.net/rhett_yuan/article/details/79483387
        //          https://www.cnblogs.com/koshio0219/p/12195974.html
        //			http://wiki.unity3d.com/index.php?title=ObjExporter#EditorObjExporter.cs

        /// <summary>
        /// 将NavMesh里的所有物体导出成为RecastNavigation可以识别的Obj文件。July.11.2020. Liu Gang.
        /// </summary>
        private static void WriteRecastObjFile()
        {
            if (!System.IO.Directory.Exists(outputClientFolder))
            {
                System.IO.Directory.CreateDirectory(outputClientFolder);
            }

            var filename = SceneManager.GetActiveScene().name;
            var path = outputClientFolder + filename + ".obj";
            StreamWriter sw = new StreamWriter(path);

            Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

            var tempTerrainRoots = new List<GameObject>();
            List<MeshFilter> meshes = Collect(tempTerrainRoots);
            try
            {
                int count = 0;
                foreach (MeshFilter mf in meshes)
                {
                    sw.Write("mtllib ./" + filename + ".mtl\n");
                    string strMes = MeshToString(mf, materialList);
                    sw.Write(strMes);
                    EditorUtility.DisplayProgressBar("Exporting objects...", mf.name, count++ / (float) meshes.Count);
                }
            }
            finally
            {
                foreach (GameObject go in tempTerrainRoots)
                {
                    if (go != null)
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                    }
                }

                EditorUtility.ClearProgressBar();
            }

            sw.Flush();
            sw.Close();

            AssetDatabase.Refresh();
        }

        // Global containers for all active mesh/terrain tags
        public static List<MeshFilter> m_Meshes = new List<MeshFilter>();

        private static string NAVMESH_TAG = "NavMesh";

        private static int vertexOffset = 0;
        private static int normalOffset = 0;
        private static int uvOffset = 0;

        public struct ObjMaterial
        {
            public string name;
            public string textureName;
        }

        private static void Clear()
        {
            vertexOffset = 0;
            normalOffset = 0;
            uvOffset = 0;
        }

        private static Dictionary<string, ObjMaterial> PrepareFileWrite()
        {
            Clear();
            return new Dictionary<string, ObjMaterial>();
        }

        /// <param name="tempTerrainExportRoots">本方法为地形导出创建的临时根物体，调用方须在 finally 中 DestroyImmediate。</param>
        public static List<MeshFilter> Collect(List<GameObject> tempTerrainExportRoots)
        {
            List<MeshFilter> meshes = new List<MeshFilter>();
            if (tempTerrainExportRoots == null)
            {
                tempTerrainExportRoots = new List<GameObject>();
            }

            // 确定场景内必须有NAVMESH_TAG这个tag
            // ————————————————
            // 版权声明：本文为CSDN博主「懵懵爸爸」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
            // 原文链接：https://blog.csdn.net/ljason1993/article/details/80924723
            bool bFindTag = false;
            string[] strTags = UnityEditorInternal.InternalEditorUtility.tags;
            foreach (string tag in strTags)
            {
                if (tag == NAVMESH_TAG)
                {
                    bFindTag = true;
                    break;
                }
            }

            if (!bFindTag)
            {
                Debug.LogError($"NavMeshExporter Collect Error - 所有需要被NavMesh导出的物体的Tag必须是：[{NAVMESH_TAG}]，目前的项目里没有这个Tag。");
                return meshes;
            }

            MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();
            foreach (MeshFilter mf in meshFilters)
            {
                if (mf.gameObject.tag == NAVMESH_TAG)
                {
                    meshes.Add(mf);
                }
            }

            AppendTerrainColliderNavMeshes(meshes, tempTerrainExportRoots);

            if (meshes.Count == 0)
            {
                Debug.LogError(
                    $"NavMeshExporter Collect Error - 场景里没有需要被导出的物体：带 [{NAVMESH_TAG}] 的 MeshFilter，或同 Tag 的 TerrainCollider（需同物体有 Terrain + TerrainData）。");
            }

            return meshes;
        }

        /// <summary> 将带 NavMesh 标签的 TerrainCollider 转为高度图网格并挂到临时子物体上参与 Obj 导出。 </summary>
        private static void AppendTerrainColliderNavMeshes(List<MeshFilter> meshes, List<GameObject> tempTerrainExportRoots)
        {
            TerrainCollider[] terrainColliders = FindObjectsOfType<TerrainCollider>(true);
            foreach (TerrainCollider tc in terrainColliders)
            {
                if (tc == null || !tc.CompareTag(NAVMESH_TAG))
                {
                    continue;
                }

                Terrain terrain = tc.GetComponent<Terrain>();
                if (terrain == null || terrain.terrainData == null)
                {
                    Debug.LogWarning($"NavMeshExporter：TerrainCollider「{tc.gameObject.name}」无 Terrain 或 TerrainData，已跳过。");
                    continue;
                }

                Mesh mesh = BuildHeightmapMeshFromTerrainData(terrain.terrainData);
                if (mesh == null || mesh.vertexCount == 0)
                {
                    continue;
                }

                mesh.name = terrain.name + "_TerrainHeight";

                GameObject temp = new GameObject(terrain.name + "__NavTerrainExport");
                temp.hideFlags = HideFlags.HideAndDontSave;
                temp.transform.SetParent(terrain.transform, false);
                temp.transform.localPosition = Vector3.zero;
                temp.transform.localRotation = Quaternion.identity;
                temp.transform.localScale = Vector3.one;

                MeshFilter mf = temp.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;

                MeshRenderer mr = temp.AddComponent<MeshRenderer>();
                Material builtinMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                mr.sharedMaterial = builtinMat != null ? builtinMat : new Material(Shader.Find("Diffuse"));

                tempTerrainExportRoots.Add(temp);
                meshes.Add(mf);
            }
        }

        private static int GetTerrainHeightmapExportStep(int resolution)
        {
            const int maxSamplesPerAxis = 513;
            if (resolution <= maxSamplesPerAxis)
            {
                return 1;
            }

            return Mathf.Max(1, Mathf.CeilToInt((float)(resolution - 1) / (maxSamplesPerAxis - 1)));
        }

        private static List<int> BuildHeightmapAxisCoords(int resolution, int step)
        {
            var coords = new List<int>();
            if (resolution < 2)
            {
                return coords;
            }

            for (int i = 0; i < resolution; i += step)
            {
                coords.Add(i);
            }

            if (coords[coords.Count - 1] != resolution - 1)
            {
                coords.Add(resolution - 1);
            }

            return coords;
        }

        /// <summary> Terrain 本地空间网格：X/Z 覆盖 terrainData.size，Y 为高度图采样（与 TerrainCollider 一致）。 </summary>
        private static Mesh BuildHeightmapMeshFromTerrainData(TerrainData terrainData)
        {
            int dim = terrainData.heightmapResolution;
            if (dim < 2)
            {
                return null;
            }

            float sizeX = terrainData.size.x;
            float sizeY = terrainData.size.y;
            float sizeZ = terrainData.size.z;
            float[,] heights = terrainData.GetHeights(0, 0, dim, dim);
            int step = GetTerrainHeightmapExportStep(dim);
            List<int> cx = BuildHeightmapAxisCoords(dim, step);
            List<int> cz = BuildHeightmapAxisCoords(dim, step);
            int nx = cx.Count;
            int nz = cz.Count;

            var vertices = new List<Vector3>(nx * nz);
            for (int iz = 0; iz < nz; iz++)
            {
                int hz = cz[iz];
                float vz = (float)hz / (dim - 1) * sizeZ;
                for (int ix = 0; ix < nx; ix++)
                {
                    int hx = cx[ix];
                    float vx = (float)hx / (dim - 1) * sizeX;
                    float vy = heights[hz, hx] * sizeY;
                    vertices.Add(new Vector3(vx, vy, vz));
                }
            }

            var triangles = new List<int>((nx - 1) * (nz - 1) * 6);
            for (int iz = 0; iz < nz - 1; iz++)
            {
                for (int ix = 0; ix < nx - 1; ix++)
                {
                    int v00 = iz * nx + ix;
                    int v10 = v00 + 1;
                    int v01 = v00 + nx;
                    int v11 = v01 + 1;
                    triangles.Add(v00);
                    triangles.Add(v11);
                    triangles.Add(v10);
                    triangles.Add(v00);
                    triangles.Add(v01);
                    triangles.Add(v11);
                }
            }

            var mesh = new Mesh();
            mesh.indexFormat = vertices.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList)
        {
            Mesh m = mf.sharedMesh;
            if (m == null)
            {
                Debug.LogWarning($"NavMeshExporter MeshToString：「{mf.name}」无 sharedMesh，已跳过。");
                return string.Empty;
            }

            if (!EditorUtility.IsPersistent(m))
            {
                if (m.vertexCount > 0 && (m.normals == null || m.normals.Length != m.vertexCount))
                {
                    m.RecalculateNormals();
                }

                if (m.vertexCount > 0 && (m.uv == null || m.uv.Length != m.vertexCount))
                {
                    m.uv = new Vector2[m.vertexCount];
                }
            }

            Renderer rend = mf.GetComponent<Renderer>();
            Material[] mats = rend != null ? rend.sharedMaterials : null;
            if (mats == null || mats.Length == 0)
            {
                mats = new Material[] { null };
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("g ").Append(mf.name).Append("\n");
            // foreach(Vector3 v in m.vertices) {
            // 	sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,v.z));
            // }
            foreach (Vector3 lv in m.vertices)
            {
                Vector3 wv = mf.transform.TransformPoint(lv);
                //This is sort of ugly - inverting x-component since we're in
                //a different coordinate system than "everyone" is "used to".
                sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
            }

            sb.Append("\n");

            // foreach(Vector3 v in m.normals) {
            // 	sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
            // }
            foreach (Vector3 lv in m.normals)
            {
                Vector3 wv = mf.transform.TransformDirection(lv);
                sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
            }

            sb.Append("\n");

            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }

            int countMat = m.subMeshCount;
            if (mats.Length < countMat)
            {
                Debug.LogWarning($"NavMeshExporter MeshToString Error - 共享材质数量小于该物体的子物体数量 - {mats.Length} / {countMat}");
                countMat = mats.Length;
            }

            for (int material = 0; material < countMat; material++)
            {
                string nameMat = "null";
                Texture mainTexture = null;
                if (mats[material] != null)
                {
                    nameMat = mats[material].name;
                    mainTexture = mats[material].mainTexture;
                }

                sb.Append("\n");
                sb.Append("usemtl ").Append(nameMat).Append("\n");
                sb.Append("usemap ").Append(nameMat).Append("\n");

                //See if this material is already in the materiallist.
                try
                {
                    ObjMaterial objMaterial = new ObjMaterial();
                    objMaterial.name = nameMat;
                    if (mainTexture)
                    {
                        objMaterial.textureName = AssetDatabase.GetAssetPath(mainTexture);
                    }
                    else
                    {
                        objMaterial.textureName = null;
                    }

                    materialList.Add(objMaterial.name, objMaterial);
                }
                catch (ArgumentException)
                {
                    //Already in the dictionary
                }

                // int[] triangles = m.GetTriangles(material);
                // for (int i=0;i<triangles.Length;i+=3) {
                // 	sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
                // 		triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));
                // }
                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    //Because we inverted the x-component, we also needed to alter the triangle winding.
                    sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                        triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
                }
            }

            vertexOffset += m.vertices.Length;
            normalOffset += m.normals.Length;
            uvOffset += m.uv.Length;
            return sb.ToString();
        }

        #endregion

        #region 拷贝文件

        /// <summary>
        /// 把生成的Obj文件拷贝到服务器
        /// https://www.cnblogs.com/wangjianhui008/p/3234519.html
        /// </summary>
        private static void CopyObjFiles()
        {
            string sourceFolder = outputClientFolder;
            // *.bytes, *.obj, *_internal.obj文件不再拷贝到服务器的Config/Navmesh目录下，不再需要了，减少服务器数据文件的大小。Aug.27.2020. Liu Gang.
            //得到原文件根目录下的所有文件
            //	    {
            //		    string[] files = System.IO.Directory.GetFiles(sourceFolder);
            //		    foreach (string file in files)
            //		    {
            //			    string name = System.IO.Path.GetFileName(file);
            //			    // 仅拷贝bytes文件和obj文件，但是不包括文件名里包含“internal”字样的obj文件。
            //			    var ext = Path.GetExtension(file);
            //			    if (ext == ".bytes" || (ext == ".obj" && !file.Contains("_internal.")))
            //			    {
            //				    string dest = System.IO.Path.Combine(destFolder, name);
            //				    System.IO.File.Copy(file, dest, true); //复制文件
            //			    }
            //		    }
            //	    }

            // 拷贝到RecastDemo配置路径
            {
                string[] files = System.IO.Directory.GetFiles(sourceFolder);
                //如果目标路径不存在,则创建目标路径
                if (!System.IO.Directory.Exists(outputServerFolder))
                {
                    System.IO.Directory.CreateDirectory(outputServerFolder);
                }
                foreach (string file in files)
                {
                    string name = System.IO.Path.GetFileName(file);
                    // 仅拷贝bytes文件和obj文件，但是不包括文件名里包含“internal”字样的obj文件。
                    var ext = Path.GetExtension(file);
                    if (ext == ".obj" && !file.Contains("_internal."))
                    {
                        string dest = System.IO.Path.Combine(outputServerFolder, name);
                        System.IO.File.Copy(file, dest, true); //复制文件
                        UnityEngine.Debug.Log($"Recast：从{file}复制obj文件到{dest}成功");
                    }
                }
            }
        }

        #endregion
    }
}