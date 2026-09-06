using System;
using System.Collections.Generic;
using System.IO;
using ET.Editor.Frame2D;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [Serializable]
    public sealed class FrameBoneExportBoneEntry
    {
        public FrameAnimBindBoneType boneType;
        public Vector3[] gridPositions = Array.Empty<Vector3>();
    }

    /// <summary>
    /// 序列帧窗口「序列帧骨骼数据导出」页签：拖入图集、勾选帧、逐帧编辑骨骼位置并导出。
    /// </summary>
    [Serializable]
    public sealed class FrameBoneExportTab
    {
        private const string ExportFolder = "Assets/Bundles/ScriptObject/FrameBone";
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".tga", ".bmp" };
        private const float PreviewHeight = 260f;
        private const float MarkerSize = 8f;
        private const float DropBoxHeight = 60f;

        [SerializeField] private Texture2D sourceTexture;
        [SerializeField] private string sourcePath;
        [SerializeField] private bool ownsTexture;
        [SerializeField] private int columns = 4;
        [SerializeField] private int rows = 4;
        [SerializeField] private int targetWidth;
        [SerializeField] private int targetHeight;
        [SerializeField] private bool targetSizeInitialized;
        [SerializeField] private float playInterval = 0.1f;
        [SerializeField] private bool[] frameSelected = Array.Empty<bool>();
        [SerializeField] private int editingSlot;
        [SerializeField] private int playSlot;
        [SerializeField] private bool isPlaying;
        [SerializeField] private int selectedBoneIndex;
        [SerializeField] private FrameAnimBoneConfig exportConfig;
        [SerializeField] private List<FrameBoneExportBoneEntry> bones = new List<FrameBoneExportBoneEntry>();
        [SerializeField] private Vector2 meshOffsetXZ = Vector2.zero;
        [SerializeField] private Vector2 meshScaleXZ = Vector2.one;
        [SerializeField] private float previewPixelsPerUnit = 100f;

        [NonSerialized] private Vector2 scrollPosition;
        [NonSerialized] private Vector2 frameListScroll;
        [NonSerialized] private Rect dropRect;
        [NonSerialized] private Rect previewRect;
        [NonSerialized] private Rect previewDrawRect;
        [NonSerialized] private double lastPlayTime;
        [NonSerialized] private bool isDraggingMarker;
        [NonSerialized] private int dragBoneIndex = -1;
        [NonSerialized] private EditorWindow hostWindow;

        public void SetHost(EditorWindow window)
        {
            hostWindow = window;
        }

        public void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.HelpBox(
                "拖入序列帧图集，勾选要导出的帧，编辑骨骼与 Graphics mesh 整体变换。\n" +
                "整体偏移/缩放对所有帧统一生效；骨骼与显示 mesh（Graphics）为同级节点。\n" +
                "预览区会实时反映 mesh 偏移/缩放；拖入已有 FrameAnimBoneConfig 作为导出目标，留空则导出时新建。",
                MessageType.Info);

            EditorGUILayout.Space(6f);
            dropRect = DrawDropBox(
                string.IsNullOrEmpty(sourcePath) ? "拖拽图片到此处" : Path.GetFileName(sourcePath),
                DropBoxHeight);

            EditorGUI.BeginChangeCheck();
            columns = Mathf.Max(1, EditorGUILayout.IntField("横向数量 (列)", columns));
            rows = Mathf.Max(1, EditorGUILayout.IntField("纵向数量 (行)", rows));
            if (EditorGUI.EndChangeCheck())
            {
                EnsureFrameSelection(true);
                EnsureTargetSize(true);
                EnsureBoneGridSize();
            }

            if (sourceTexture != null)
            {
                GetSourceCellSize(out int sourceCellWidth, out int sourceCellHeight);
                EditorGUILayout.LabelField(
                    $"原图单帧: {sourceCellWidth} x {sourceCellHeight}（图集 {sourceTexture.width} x {sourceTexture.height}）",
                    EditorStyles.miniLabel);

                EditorGUI.BeginChangeCheck();
                targetWidth = Mathf.Max(1, EditorGUILayout.IntField("目标图片宽度", targetWidth));
                targetHeight = Mathf.Max(1, EditorGUILayout.IntField("目标图片高度", targetHeight));
                if (EditorGUI.EndChangeCheck())
                {
                    targetWidth = Mathf.Min(targetWidth, sourceCellWidth);
                    targetHeight = Mathf.Min(targetHeight, sourceCellHeight);
                }
            }
            else
            {
                targetWidth = Mathf.Max(1, EditorGUILayout.IntField("目标图片宽度", targetWidth));
                targetHeight = Mathf.Max(1, EditorGUILayout.IntField("目标图片高度", targetHeight));
            }

            playInterval = EditorGUILayout.Slider("播放间隔 (秒)", playInterval, 0.02f, 2f);
            using (new EditorGUILayout.HorizontalScope())
            {
                exportConfig = (FrameAnimBoneConfig)EditorGUILayout.ObjectField(
                    "导出目标 (FrameAnimBoneConfig)",
                    exportConfig,
                    typeof(FrameAnimBoneConfig),
                    false);
                using (new EditorGUI.DisabledScope(!CanSyncFromConfig(out _)))
                {
                    if (GUILayout.Button("强制同步", GUILayout.Width(80f), GUILayout.Height(18f)))
                    {
                        ForceSyncFromConfig();
                    }
                }
            }
            if (exportConfig != null)
            {
                EditorGUILayout.LabelField($"将更新: {AssetDatabase.GetAssetPath(exportConfig)}", EditorStyles.miniLabel);
                int selectedCount = sourceTexture != null ? CountSelectedFrames() : 0;
                if (selectedCount > 0 && selectedCount != exportConfig.frameCount)
                {
                    EditorGUILayout.LabelField(
                        $"已选 {selectedCount} 帧，Config 为 {exportConfig.frameCount} 帧（一致时可强制同步）",
                        EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"未指定时将新建到: {ExportFolder}/", EditorStyles.miniLabel);
            }

            if (sourceTexture == null)
            {
                EditorGUILayout.HelpBox("请先拖入一张序列帧图集。", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                HandleDragAndDrop();
                return;
            }

            EnsureFrameSelection(false);
            EnsureTargetSize(false);
            EnsureBoneGridSize();

            EditorGUILayout.Space(6f);
            DrawFrameSelectionToolbar();
            DrawFrameList();

            EditorGUILayout.Space(8f);
            DrawBoneToolbar();
            DrawEditingFrameSelector();

            if (CountSelectedFrames() <= 0)
            {
                EditorGUILayout.HelpBox("请至少勾选一帧。", MessageType.Warning);
            }
            else
            {
                DrawPreviewArea();
                DrawMeshTransformInspector();
                DrawSelectedBoneInspector();
            }

            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(!CanExport(out _)))
            {
                if (GUILayout.Button("导出 FrameAnimBoneConfig", GUILayout.Height(32f)))
                {
                    ExportAsset();
                }
            }

            EditorGUILayout.EndScrollView();
            HandleDragAndDrop();
        }

        public void OnDisable()
        {
            ClearTexture();
        }

        public void UpdatePlayback()
        {
            if (!isPlaying || sourceTexture == null || CountSelectedFrames() <= 0)
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (now - lastPlayTime < Mathf.Max(playInterval, 0.02f))
            {
                return;
            }

            lastPlayTime = now;
            int selectedCount = CountSelectedFrames();
            playSlot = (playSlot + 1) % selectedCount;
            editingSlot = playSlot;
            RequestRepaint();
        }

        private void DrawFrameSelectionToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选", GUILayout.Width(72f)))
            {
                SetAllFramesSelected(true);
            }

            if (GUILayout.Button("全不选", GUILayout.Width(72f)))
            {
                SetAllFramesSelected(false);
            }

            EditorGUILayout.LabelField($"共 {rows * columns} 帧，已选 {CountSelectedFrames()} 帧");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFrameList()
        {
            frameListScroll = EditorGUILayout.BeginScrollView(frameListScroll, GUILayout.MinHeight(140f));
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int index = row * columns + column;
                    EditorGUILayout.BeginHorizontal();
                    frameSelected[index] = EditorGUILayout.Toggle(frameSelected[index], GUILayout.Width(18f));
                    Rect thumbRect = GUILayoutUtility.GetRect(36f, 36f, GUILayout.Width(36f), GUILayout.Height(36f));
                    DrawCellThumb(thumbRect, row, column);
                    EditorGUILayout.LabelField($"_{row}_{column}", GUILayout.Width(56f));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawBoneToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("添加骨骼", GUILayout.Height(24f)))
                {
                    ShowAddBoneMenu();
                }

                using (new EditorGUI.DisabledScope(bones.Count == 0))
                {
                    if (GUILayout.Button("删除当前骨骼", GUILayout.Height(24f)))
                    {
                        bones.RemoveAt(Mathf.Clamp(selectedBoneIndex, 0, bones.Count - 1));
                        selectedBoneIndex = Mathf.Clamp(selectedBoneIndex, 0, Mathf.Max(0, bones.Count - 1));
                    }
                }
            }

            if (bones.Count == 0)
            {
                EditorGUILayout.HelpBox("添加 Body / Head / Foot / LeftHand / RightHand 等骨骼后，在预览区拖动红点设置位置。", MessageType.None);
            }
        }

        private void DrawEditingFrameSelector()
        {
            int selectedCount = CountSelectedFrames();
            if (selectedCount <= 0)
            {
                return;
            }

            editingSlot = Mathf.Clamp(editingSlot, 0, selectedCount - 1);
            playSlot = Mathf.Clamp(playSlot, 0, selectedCount - 1);

            using (new EditorGUILayout.HorizontalScope())
            {
                isPlaying = GUILayout.Toggle(isPlaying, "播放已选帧", "Button", GUILayout.Height(22f));
                if (GUILayout.Button("停止", GUILayout.Width(56f), GUILayout.Height(22f)))
                {
                    isPlaying = false;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                for (int slot = 0; slot < selectedCount; slot++)
                {
                    int gridIndex = GetSelectedGridIndexAt(slot);
                    int row = gridIndex / Mathf.Max(columns, 1);
                    int col = gridIndex % Mathf.Max(columns, 1);
                    string label = $"_{row}_{col}";
                    if (GUILayout.Toggle(editingSlot == slot, label, editingSlot == slot ? "Button" : "MiniButton", GUILayout.Height(22f)))
                    {
                        if (editingSlot != slot)
                        {
                            editingSlot = slot;
                            isPlaying = false;
                        }
                    }
                }
            }
        }

        private void DrawPreviewArea()
        {
            int gridIndex = GetEditingGridIndex();
            if (gridIndex < 0)
            {
                return;
            }

            int row = gridIndex / Mathf.Max(columns, 1);
            int column = gridIndex % Mathf.Max(columns, 1);

            EditorGUILayout.LabelField("预览（当前编辑帧）", EditorStyles.boldLabel);
            previewPixelsPerUnit = Mathf.Max(1f, EditorGUILayout.FloatField("预览缩放 (像素/Unity单位)", previewPixelsPerUnit));
            previewRect = GUILayoutUtility.GetRect(10f, PreviewHeight, GUILayout.ExpandWidth(true));
            if (previewRect.width < 10f)
            {
                previewRect.width = Mathf.Max(EditorGUIUtility.currentViewWidth - 48f, 200f);
            }

            EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f, 1f));
            GUI.Label(new Rect(previewRect.x + 6f, previewRect.y + 4f, previewRect.width - 12f, 18f), "单帧预览 + 骨骼", EditorStyles.miniBoldLabel);

            Rect contentRect = new Rect(previewRect.x + 8f, previewRect.y + 24f, previewRect.width - 16f, previewRect.height - 36f);
            float aspect = targetWidth / (float)Mathf.Max(targetHeight, 1);
            previewDrawRect = FitRectWithAspect(contentRect, aspect);

            if (Event.current.type == EventType.Repaint)
            {
                DrawFrameTexture(previewDrawRect, row, column, gridIndex);
            }

            DrawBoneMarkers(previewDrawRect, gridIndex);

            GUI.Label(
                new Rect(previewRect.x + 6f, previewRect.yMax - 18f, previewRect.width - 12f, 16f),
                $"编辑 _{row}_{column}  |  目标 {targetWidth}x{targetHeight}  |  骨骼 {bones.Count}");
        }

        private void DrawFrameTexture(Rect drawRect, int row, int column, int gridIndex)
        {
            if (sourceTexture == null)
            {
                return;
            }

            Vector2 meshOffset = GetMeshOffsetXZ();
            Vector2 meshScale = GetMeshScaleXZ();
            float ppu = Mathf.Max(previewPixelsPerUnit, 0.0001f);

            GUI.BeginClip(drawRect);
            float scaleX = drawRect.width / Mathf.Max(targetWidth, 0.0001f);
            float scaleY = drawRect.height / Mathf.Max(targetHeight, 0.0001f);
            Vector2 center = new Vector2(drawRect.width * 0.5f, drawRect.height * 0.5f);
            float texW = targetWidth * scaleX * meshScale.x;
            float texH = targetHeight * scaleY * meshScale.y;
            Rect sourceDrawRect = new Rect(
                center.x - texW * 0.5f + meshOffset.x * ppu,
                center.y - texH * 0.5f - meshOffset.y * ppu,
                texW,
                texH);
            GUI.DrawTextureWithTexCoords(sourceDrawRect, sourceTexture, GetCellUv(row, column), true);
            GUI.EndClip();
        }

        private void DrawBoneMarkers(Rect drawRect, int gridIndex)
        {
            if (bones.Count == 0)
            {
                return;
            }

            Event evt = Event.current;
            bool isRepaint = evt.type == EventType.Repaint;

            if (isRepaint)
            {
                Handles.BeginGUI();
            }

            for (int i = 0; i < bones.Count; i++)
            {
                Vector3 pos = GetBoneGridPosition(i, gridIndex);
                Vector2 guiPos = LocalPositionToGui(drawRect, pos);
                bool isSelected = i == selectedBoneIndex;
                Rect markerRect = new Rect(guiPos.x - MarkerSize * 0.5f, guiPos.y - MarkerSize * 0.5f, MarkerSize, MarkerSize);

                if (isRepaint)
                {
                    Color color = isSelected ? new Color(1f, 0.15f, 0.15f, 1f) : new Color(1f, 0.35f, 0.35f, 0.9f);
                    EditorGUI.DrawRect(markerRect, color);
                    if (isSelected)
                    {
                        GUI.Label(new Rect(markerRect.x + MarkerSize, markerRect.y - 2f, 100f, 16f), bones[i].boneType.ToString(), EditorStyles.miniLabel);
                    }
                }

                if (evt.type == EventType.MouseDown && evt.button == 0 && markerRect.Contains(evt.mousePosition))
                {
                    selectedBoneIndex = i;
                    isDraggingMarker = true;
                    dragBoneIndex = i;
                    isPlaying = false;
                    evt.Use();
                }
            }

            if (isDraggingMarker && dragBoneIndex >= 0 && dragBoneIndex < bones.Count)
            {
                if (evt.type == EventType.MouseDrag && evt.button == 0)
                {
                    Vector3 bonePos = GuiToLocalPosition(drawRect, evt.mousePosition);
                    SetBoneGridPosition(dragBoneIndex, gridIndex, bonePos);
                    evt.Use();
                    RequestRepaint();
                }
                else if (evt.type == EventType.MouseUp && evt.button == 0)
                {
                    isDraggingMarker = false;
                    dragBoneIndex = -1;
                }
            }

            if (isRepaint)
            {
                Handles.EndGUI();
            }
        }

        private void DrawMeshTransformInspector()
        {
            if (GetEditingGridIndex() < 0)
            {
                return;
            }

            EditorGUILayout.LabelField("Graphics Mesh（全部帧）", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            meshOffsetXZ = EditorGUILayout.Vector2Field("整体偏移 (X, Z)", meshOffsetXZ);
            meshScaleXZ = EditorGUILayout.Vector2Field("整体缩放 (X, Z)", meshScaleXZ);
            if (EditorGUI.EndChangeCheck())
            {
                meshScaleXZ.x = meshScaleXZ.x <= 0f ? 1f : meshScaleXZ.x;
                meshScaleXZ.y = meshScaleXZ.y <= 0f ? 1f : meshScaleXZ.y;
                RequestRepaint();
            }

            EditorGUILayout.LabelField(
                "对应 Graphics 节点的 localPosition.x/z 与 localScale.x/z，所有帧统一使用",
                EditorStyles.miniLabel);
        }

        private void DrawSelectedBoneInspector()
        {
            if (bones.Count == 0)
            {
                return;
            }

            int gridIndex = GetEditingGridIndex();
            if (gridIndex < 0)
            {
                return;
            }

            selectedBoneIndex = Mathf.Clamp(selectedBoneIndex, 0, bones.Count - 1);

            string[] boneNames = new string[bones.Count];
            for (int i = 0; i < bones.Count; i++)
            {
                boneNames[i] = bones[i].boneType.ToString();
            }

            EditorGUILayout.LabelField("当前骨骼", EditorStyles.boldLabel);
            selectedBoneIndex = EditorGUILayout.Popup("骨骼", selectedBoneIndex, boneNames);

            FrameBoneExportBoneEntry entry = bones[selectedBoneIndex];
            Vector3 bonePos = GetBoneGridPosition(selectedBoneIndex, gridIndex);
            EditorGUI.BeginChangeCheck();
            Vector2 xz = EditorGUILayout.Vector2Field("localPosition (X, Z)", new Vector2(bonePos.x, bonePos.z));
            if (EditorGUI.EndChangeCheck())
            {
                SetBoneGridPosition(selectedBoneIndex, gridIndex, new Vector3(xz.x, 0f, xz.y));
            }

            EditorGUILayout.LabelField(
                $"{entry.boneType}  Frame _{gridIndex / Mathf.Max(columns, 1)}_{gridIndex % Mathf.Max(columns, 1)}: ({bonePos.x:0.###}, 0, {bonePos.z:0.###})",
                EditorStyles.miniLabel);
        }

        private void ShowAddBoneMenu()
        {
            GenericMenu menu = new GenericMenu();
            foreach (BindBoneTypeEditor bone in Enum.GetValues(typeof(BindBoneTypeEditor)))
            {
                FrameAnimBindBoneType runtimeBone = FrameAnimBoneTypeConverter.ToRuntime(bone);
                if (HasBone(runtimeBone))
                {
                    menu.AddDisabledItem(new GUIContent(bone.ToString()));
                    continue;
                }

                BindBoneTypeEditor captured = bone;
                menu.AddItem(new GUIContent(bone.ToString()), false, () => AddBone(captured));
            }

            menu.ShowAsContext();
        }

        private void AddBone(BindBoneTypeEditor boneType)
        {
            FrameAnimBindBoneType runtimeBone = FrameAnimBoneTypeConverter.ToRuntime(boneType);
            EnsureBoneGridSize();
            Vector3 defaultPos = Vector3.zero;

            FrameBoneExportBoneEntry entry = new FrameBoneExportBoneEntry
            {
                boneType = runtimeBone,
                gridPositions = new Vector3[frameSelected.Length],
            };

            for (int i = 0; i < entry.gridPositions.Length; i++)
            {
                entry.gridPositions[i] = defaultPos;
            }

            bones.Add(entry);
            selectedBoneIndex = bones.Count - 1;
        }

        private bool HasBone(FrameAnimBindBoneType boneType)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                if (bones[i].boneType == boneType)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3 GetBoneGridPosition(int boneIndex, int gridIndex)
        {
            FrameBoneExportBoneEntry entry = bones[boneIndex];
            if (entry.gridPositions == null || gridIndex < 0 || gridIndex >= entry.gridPositions.Length)
            {
                return Vector3.zero;
            }

            return entry.gridPositions[gridIndex];
        }

        private Vector2 GetMeshOffsetXZ()
        {
            return meshOffsetXZ;
        }

        private Vector2 GetMeshScaleXZ()
        {
            Vector2 scale = meshScaleXZ;
            if (scale.x <= 0f)
            {
                scale.x = 1f;
            }

            if (scale.y <= 0f)
            {
                scale.y = 1f;
            }

            return scale;
        }

        private void SetBoneGridPosition(int boneIndex, int gridIndex, Vector3 childPos)
        {
            if (boneIndex < 0 || boneIndex >= bones.Count || gridIndex < 0)
            {
                return;
            }

            bones[boneIndex].gridPositions[gridIndex] = new Vector3(childPos.x, 0f, childPos.z);
        }

        private Vector2 LocalPositionToGui(Rect drawRect, Vector3 localPos)
        {
            float ppu = Mathf.Max(previewPixelsPerUnit, 0.0001f);
            return drawRect.center + new Vector2(localPos.x * ppu, -localPos.z * ppu);
        }

        private Vector3 GuiToLocalPosition(Rect drawRect, Vector2 guiPos)
        {
            float ppu = Mathf.Max(previewPixelsPerUnit, 0.0001f);
            Vector2 delta = guiPos - drawRect.center;
            return new Vector3(delta.x / ppu, 0f, -delta.y / ppu);
        }

        private int GetEditingGridIndex()
        {
            return GetSelectedGridIndexAt(editingSlot);
        }

        private int GetSelectedGridIndexAt(int slot)
        {
            if (frameSelected == null || frameSelected.Length == 0)
            {
                return -1;
            }

            int selectedCount = CountSelectedFrames();
            if (selectedCount <= 0)
            {
                return -1;
            }

            int target = slot % selectedCount;
            int seen = 0;
            for (int i = 0; i < frameSelected.Length; i++)
            {
                if (!frameSelected[i])
                {
                    continue;
                }

                if (seen == target)
                {
                    return i;
                }

                seen++;
            }

            return -1;
        }

        private int CountSelectedFrames()
        {
            if (frameSelected == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < frameSelected.Length; i++)
            {
                if (frameSelected[i])
                {
                    count++;
                }
            }

            return count;
        }

        private void SetAllFramesSelected(bool selected)
        {
            EnsureFrameSelection(false);
            for (int i = 0; i < frameSelected.Length; i++)
            {
                frameSelected[i] = selected;
            }

            editingSlot = 0;
            playSlot = 0;
        }

        private void EnsureFrameSelection(bool reset)
        {
            int count = Mathf.Max(1, rows) * Mathf.Max(1, columns);
            if (!reset && frameSelected != null && frameSelected.Length == count)
            {
                return;
            }

            bool[] old = frameSelected;
            frameSelected = new bool[count];
            if (old != null)
            {
                int copy = Mathf.Min(old.Length, count);
                Array.Copy(old, frameSelected, copy);
            }

            editingSlot = 0;
            playSlot = 0;
        }

        private void EnsureBoneGridSize()
        {
            int count = Mathf.Max(1, rows) * Mathf.Max(1, columns);
            Vector3 defaultPos = Vector3.zero;

            for (int i = 0; i < bones.Count; i++)
            {
                FrameBoneExportBoneEntry entry = bones[i];
                if (entry.gridPositions == null || entry.gridPositions.Length != count)
                {
                    Vector3[] next = new Vector3[count];
                    int copy = entry.gridPositions != null ? Mathf.Min(entry.gridPositions.Length, count) : 0;
                    for (int j = 0; j < copy; j++)
                    {
                        next[j] = entry.gridPositions[j];
                    }

                    for (int j = copy; j < count; j++)
                    {
                        next[j] = defaultPos;
                    }

                    entry.gridPositions = next;
                }
            }
        }

        private void EnsureTargetSize(bool resetToSource)
        {
            if (sourceTexture == null)
            {
                return;
            }

            GetSourceCellSize(out int sourceCellWidth, out int sourceCellHeight);
            if (sourceCellWidth <= 0 || sourceCellHeight <= 0)
            {
                return;
            }

            if (resetToSource || !targetSizeInitialized || targetWidth <= 0 || targetHeight <= 0)
            {
                targetWidth = sourceCellWidth;
                targetHeight = sourceCellHeight;
                targetSizeInitialized = true;
            }
            else
            {
                targetWidth = Mathf.Clamp(targetWidth, 1, sourceCellWidth);
                targetHeight = Mathf.Clamp(targetHeight, 1, sourceCellHeight);
            }
        }

        private void GetSourceCellSize(out int cellWidth, out int cellHeight)
        {
            if (sourceTexture == null)
            {
                cellWidth = 0;
                cellHeight = 0;
                return;
            }

            cellWidth = sourceTexture.width / Mathf.Max(1, columns);
            cellHeight = sourceTexture.height / Mathf.Max(1, rows);
        }

        private Rect GetCellUv(int row, int column)
        {
            float width = 1f / Mathf.Max(columns, 1);
            float height = 1f / Mathf.Max(rows, 1);
            return new Rect(column * width, (rows - 1 - row) * height, width, height);
        }

        private void DrawCellThumb(Rect rect, int row, int column)
        {
            if (sourceTexture == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            GUI.DrawTextureWithTexCoords(rect, sourceTexture, GetCellUv(row, column), true);
        }

        private static Rect FitRectWithAspect(Rect container, float aspect)
        {
            float containerAspect = container.width / Mathf.Max(container.height, 0.0001f);
            Rect drawRect = container;
            if (aspect > containerAspect)
            {
                float height = container.width / aspect;
                drawRect.y += (container.height - height) * 0.5f;
                drawRect.height = height;
            }
            else
            {
                float width = container.height * aspect;
                drawRect.x += (container.width - width) * 0.5f;
                drawRect.width = width;
            }

            return drawRect;
        }

        private static Rect DrawDropBox(string label, float height)
        {
            Rect dropArea = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, label);
            return dropArea;
        }

        private void HandleDragAndDrop()
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            if (!dropRect.Contains(evt.mousePosition))
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                TryLoadFromDrag();
            }

            evt.Use();
            RequestRepaint();
        }

        private void TryLoadFromDrag()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D tex)
                {
                    SetTexture(tex, AssetDatabase.GetAssetPath(tex), false);
                    return;
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (IsImageFile(path))
                {
                    LoadExternalImage(path);
                    return;
                }
            }
        }

        private void LoadExternalImage(string path)
        {
            string absolute = ToAbsolutePath(path);
            if (!File.Exists(absolute))
            {
                return;
            }

            byte[] data = File.ReadAllBytes(absolute);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
            {
                UnityEngine.Object.DestroyImmediate(tex);
                EditorUtility.DisplayDialog("序列帧", "无法加载图片文件。", "确定");
                return;
            }

            SetTexture(tex, path, true);
        }

        private void SetTexture(Texture2D tex, string path, bool owns)
        {
            ClearTexture();
            sourceTexture = tex;
            sourcePath = string.IsNullOrEmpty(path) ? null : path;
            ownsTexture = owns;
            targetSizeInitialized = false;
            EnsureFrameSelection(true);
            EnsureTargetSize(true);
            EnsureBoneGridSize();
            editingSlot = 0;
            playSlot = 0;
            lastPlayTime = EditorApplication.timeSinceStartup;
            RequestRepaint();
        }

        private void ClearTexture()
        {
            if (ownsTexture && sourceTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(sourceTexture);
            }

            sourceTexture = null;
            sourcePath = null;
            ownsTexture = false;
        }

        private static bool IsImageFile(string path)
        {
            string ext = Path.GetExtension(path)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            for (int i = 0; i < ImageExtensions.Length; i++)
            {
                if (ImageExtensions[i] == ext)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ToAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        private bool CanSyncFromConfig(out string message)
        {
            message = string.Empty;
            if (exportConfig == null)
            {
                message = "请先指定 FrameAnimBoneConfig。";
                return false;
            }

            if (sourceTexture == null)
            {
                message = "请先拖入序列帧图集。";
                return false;
            }

            int selectedCount = CountSelectedFrames();
            if (selectedCount <= 0)
            {
                message = "请至少勾选一帧。";
                return false;
            }

            if (selectedCount != exportConfig.frameCount)
            {
                message = $"已选帧数 ({selectedCount}) 与 Config 帧数 ({exportConfig.frameCount}) 不一致。";
                return false;
            }

            return true;
        }

        private void ForceSyncFromConfig()
        {
            if (!CanSyncFromConfig(out string message))
            {
                EditorUtility.DisplayDialog("强制同步", message, "确定");
                return;
            }

            FrameAnimBoneConfig config = exportConfig;
            config.EnsureTrackFrameCount();

            if (config.frameMeshFrames != null && config.frameMeshFrames.Count > 0)
            {
                FrameAnimMeshFrameData mesh = config.frameMeshFrames[0];
                meshOffsetXZ = mesh.localPositionXZ;
                meshScaleXZ = mesh.localScaleXZ;
                if (meshScaleXZ.x <= 0f)
                {
                    meshScaleXZ.x = 1f;
                }

                if (meshScaleXZ.y <= 0f)
                {
                    meshScaleXZ.y = 1f;
                }
            }
            else
            {
                meshOffsetXZ = Vector2.zero;
                meshScaleXZ = Vector2.one;
            }

            bones.Clear();
            EnsureBoneGridSize();

            if (config.boneTracks != null)
            {
                for (int t = 0; t < config.boneTracks.Count; t++)
                {
                    FrameAnimBoneTrack track = config.boneTracks[t];
                    if (track?.framePositions == null)
                    {
                        continue;
                    }

                    FrameBoneExportBoneEntry entry = new FrameBoneExportBoneEntry
                    {
                        boneType = track.boneType,
                        gridPositions = new Vector3[frameSelected.Length],
                    };

                    for (int i = 0; i < entry.gridPositions.Length; i++)
                    {
                        entry.gridPositions[i] = Vector3.zero;
                    }

                    for (int slot = 0; slot < config.frameCount; slot++)
                    {
                        int gridIndex = GetSelectedGridIndexAt(slot);
                        if (gridIndex < 0)
                        {
                            continue;
                        }

                        Vector3 pos = slot < track.framePositions.Count
                            ? track.framePositions[slot].localPosition
                            : Vector3.zero;
                        entry.gridPositions[gridIndex] = new Vector3(pos.x, 0f, pos.z);
                    }

                    bones.Add(entry);
                }
            }

            selectedBoneIndex = Mathf.Clamp(selectedBoneIndex, 0, Mathf.Max(0, bones.Count - 1));
            editingSlot = 0;
            playSlot = 0;
            isPlaying = false;
            RequestRepaint();
            EditorUtility.DisplayDialog("强制同步", $"已从 Config 同步 {config.frameCount} 帧、{bones.Count} 个骨骼。", "确定");
        }

        private bool CanExport(out string message)
        {
            message = string.Empty;
            if (sourceTexture == null)
            {
                message = "请先拖入序列帧图集。";
                return false;
            }

            if (CountSelectedFrames() <= 0)
            {
                message = "请至少勾选一帧。";
                return false;
            }

            if (bones.Count == 0)
            {
                message = "请至少添加一个骨骼。";
                return false;
            }

            return true;
        }

        private void ExportAsset()
        {
            if (!CanExport(out string message))
            {
                EditorUtility.DisplayDialog("序列帧", message, "确定");
                return;
            }

            int exportFrameCount = CountSelectedFrames();
            FrameAnimBoneConfig config;
            string assetPath;
            bool isNew;

            if (exportConfig != null)
            {
                config = exportConfig;
                assetPath = AssetDatabase.GetAssetPath(config);
                isNew = false;
            }
            else
            {
                EnsureExportFolder();
                assetPath = AssetDatabase.GenerateUniqueAssetPath($"{ExportFolder}/FrameAnimBoneConfig.asset");
                config = ScriptableObject.CreateInstance<FrameAnimBoneConfig>();
                AssetDatabase.CreateAsset(config, assetPath);
                exportConfig = config;
                isNew = true;
            }

            config.frameCount = exportFrameCount;
            config.frameMeshFrames ??= new List<FrameAnimMeshFrameData>();
            config.frameMeshFrames.Clear();
            for (int slot = 0; slot < exportFrameCount; slot++)
            {
                Vector2 offset = GetMeshOffsetXZ();
                Vector2 scale = GetMeshScaleXZ();
                config.frameMeshFrames.Add(new FrameAnimMeshFrameData
                {
                    localPositionXZ = offset,
                    localScaleXZ = scale,
                });
            }

            config.boneTracks ??= new List<FrameAnimBoneTrack>();
            config.boneTracks.Clear();

            for (int i = 0; i < bones.Count; i++)
            {
                FrameBoneExportBoneEntry entry = bones[i];
                FrameAnimBoneTrack track = new FrameAnimBoneTrack
                {
                    boneType = entry.boneType,
                    framePositions = new List<FrameAnimBoneFrameData>(exportFrameCount),
                };

                for (int slot = 0; slot < exportFrameCount; slot++)
                {
                    int gridIndex = GetSelectedGridIndexAt(slot);
                    Vector3 bonePos = gridIndex >= 0 ? entry.gridPositions[gridIndex] : Vector3.zero;
                    bonePos.y = 0f;
                    track.framePositions.Add(new FrameAnimBoneFrameData { localPosition = bonePos });
                }

                config.boneTracks.Add(track);
            }

            config.EnsureTrackFrameCount();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(config);
            EditorUtility.DisplayDialog("序列帧", isNew ? $"已新建:\n{assetPath}" : $"已更新:\n{assetPath}", "确定");
        }

        private static void EnsureExportFolder()
        {
            if (AssetDatabase.IsValidFolder(ExportFolder))
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

        private void RequestRepaint()
        {
            hostWindow?.Repaint();
        }
    }
}
