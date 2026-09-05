using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 序列帧图集合并、切割、背景色去除、微调预览与边框。
    /// </summary>
    public class SpriteSheetToolsWindow : EditorWindow
    {
        private static readonly string[] TabNames = { "合并", "切割", "背景色去除", "序列帧微调", "边框", "2D贴图制作", "图片改色" };
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".tga", ".bmp" };

        private int selectedTab;
        private Vector2 combineScrollPosition;
        private Vector2 sliceScrollPosition;

        private string folderPath;
        private int cellWidth = 64;
        private int cellHeight = 64;
        private int combinePadRows;
        private int combinePadColumns;
        private Color combinePadColor = Color.clear;
        private readonly List<string> rowInputs = new List<string> { "" };
        private readonly List<Rect> rowDropRects = new List<Rect>();
        private Rect folderDropRect;
        private Rect addRowDropRect;

        private Texture2D sourceTexture;
        private string sourcePath;
        private int columns = 4;
        private int rows = 4;

        private Vector2 backgroundScrollPosition;
        private Texture2D backgroundSourceTexture;
        private Texture2D backgroundPreviewTexture;
        private string backgroundSourcePath;
        private Color keyColor = new Color(0f, 1f, 0f, 1f);
        private float tolerance = 30f;
        private int erosion;
        private int edgeSmooth = 1;
        private Rect backgroundPreviewRect;
        private bool backgroundPreviewRectValid;

        private Texture2D tweakTexture;
        private string tweakPath;
        private bool tweakOwnsTexture;
        private int tweakColumns = 4;
        private int tweakRows = 4;
        private bool[] tweakSelected = System.Array.Empty<bool>();
        private Vector2[] tweakOffsets = System.Array.Empty<Vector2>();
        private Vector2 tweakListScroll;
        private Vector2 tweakScrollPosition;
        private float tweakInterval = 0.1f;
        private int tweakPlayIndex;
        private double tweakLastFrameTime;
        private Vector2 tweakAnchor;
        private int tweakTargetWidth;
        private int tweakTargetHeight;
        private bool tweakTargetSizeInitialized;
        private SpriteSheetTweakSettings tweakLoadedSettings;

        private Texture2D borderSourceTexture;
        private Texture2D borderPreviewTexture;
        private string borderSourcePath;
        private bool borderOwnsTexture;
        private int borderSize = 1;
        private Color borderColor = new Color(1f, 1f, 1f, 1f);
        private Vector2 borderScroll;
        private Vector2 twoDTextureScrollPosition;
        private Vector2 imageRecolorScrollPosition;

        [SerializeField] private TwoDTextureMakerTab twoDTextureMakerTab = new TwoDTextureMakerTab();
        [SerializeField] private ImageRecolorTab imageRecolorTab = new ImageRecolorTab();

        [MenuItem("Tools/序列帧", false, 53)]
        public static void Open()
        {
            var window = GetWindow<SpriteSheetToolsWindow>(true, "序列帧", true);
            window.minSize = new Vector2(620, 520);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            selectedTab = GUILayout.Toolbar(selectedTab, TabNames);
            EditorGUILayout.Space(8);

            switch (selectedTab)
            {
                case 0:
                    DrawCombineTab();
                    break;
                case 1:
                    DrawSliceTab();
                    break;
                case 2:
                    DrawBackgroundTab();
                    break;
                case 3:
                    DrawTweakTab();
                    break;
                case 4:
                    DrawBorderTab();
                    break;
                case 5:
                    DrawTwoDTextureTab();
                    break;
                case 6:
                    DrawImageRecolorTab();
                    break;
            }
        }

        private void OnEnable()
        {
            EditorApplication.update += UpdateTweakPreviewPlayback;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateTweakPreviewPlayback;
            ClearBackgroundPreview();
            DestroyOwnedTexture(ref backgroundSourceTexture);
            ClearTweakTexture();
            ClearBorderTextures();
            imageRecolorTab.ClearPreview();
        }

        private void DrawCombineTab()
        {
            combineScrollPosition = EditorGUILayout.BeginScrollView(combineScrollPosition);

            EditorGUILayout.HelpBox(
                "指定单帧宽高，按行填写文件名（逗号分隔），将文件夹中的单帧图合并为一张图集。可拖入文件夹，或把多张图拖到某一行 / 底部区域自动填入。补行、补列会在现有行列基础上向下方 / 右侧扩展空格子，并用补全颜色填充。",
                MessageType.Info);
            EditorGUILayout.Space(6);

            folderDropRect = DrawDropBox(string.IsNullOrEmpty(folderPath) ? "拖拽文件夹到此处" : folderPath, 60f);
            EditorGUILayout.Space(8);

            cellWidth = EditorGUILayout.IntField("单张宽度", cellWidth);
            cellHeight = EditorGUILayout.IntField("单张高度", cellHeight);

            EditorGUILayout.Space(4);
            combinePadRows = Mathf.Max(0, EditorGUILayout.IntField("补行", combinePadRows));
            combinePadColumns = Mathf.Max(0, EditorGUILayout.IntField("补列", combinePadColumns));
            combinePadColor = EditorGUILayout.ColorField(
                new GUIContent("补全颜色"),
                combinePadColor,
                true,
                true,
                false);
            DrawCombineOutputSizeHint();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("行列表（可手动输入，或拖拽多张图片自动填充）", EditorStyles.boldLabel);

            rowDropRects.Clear();
            for (int i = 0; i < rowInputs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                rowInputs[i] = EditorGUILayout.TextField($"第 {i + 1} 行", rowInputs[i]);
                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    rowInputs.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
                rowDropRects.Add(GUILayoutUtility.GetLastRect());
            }

            if (GUILayout.Button("+ 增加一行"))
            {
                rowInputs.Add("");
            }

            addRowDropRect = DrawDropBox("拖拽多张图片到此处，自动新增一行（如 idle_0,idle_1）", 36f);

            EditorGUILayout.Space(12);

            bool canCombine = CanCombine(out string validationMessage);
            using (new EditorGUI.DisabledScope(!canCombine))
            {
                if (GUILayout.Button("生成合并图片", GUILayout.Height(34)))
                {
                    CombineAndSave();
                }
            }

            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
            HandleCombineDragAndDrop();
        }

        private void DrawSliceTab()
        {
            sliceScrollPosition = EditorGUILayout.BeginScrollView(sliceScrollPosition);

            EditorGUILayout.HelpBox(
                "将一张行列图集按横向、纵向数量切成单帧 PNG，保存到源图同目录。支持拖入 Project 中的纹理或外部图片文件。",
                MessageType.Info);
            EditorGUILayout.Space(6);

            DrawDropBox(
                string.IsNullOrEmpty(sourcePath) ? "拖拽图片到此处" : Path.GetFileName(sourcePath),
                60f);

            EditorGUILayout.Space(8);
            columns = EditorGUILayout.IntField("横向数量 (X)", columns);
            rows = EditorGUILayout.IntField("纵向数量 (Y)", rows);

            EditorGUILayout.Space(12);

            bool canSlice = CanSlice(out string validationMessage);
            using (new EditorGUI.DisabledScope(!canSlice))
            {
                if (GUILayout.Button("切割并保存", GUILayout.Height(34)))
                {
                    SliceAndSave();
                }
            }

            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }

            if (!string.IsNullOrEmpty(sourcePath))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("源文件", sourcePath, EditorStyles.wordWrappedLabel);
            }

            if (sourceTexture != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField($"尺寸: {sourceTexture.width} x {sourceTexture.height}");
                float maxPreview = 256f;
                float scale = Mathf.Min(maxPreview / sourceTexture.width, maxPreview / sourceTexture.height, 1f);
                Rect previewRect = GUILayoutUtility.GetRect(
                    sourceTexture.width * scale,
                    sourceTexture.height * scale,
                    GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(previewRect, sourceTexture);
            }

            EditorGUILayout.EndScrollView();
            HandleSliceDragAndDrop();
        }

        private void DrawBackgroundTab()
        {
            backgroundScrollPosition = EditorGUILayout.BeginScrollView(backgroundScrollPosition);
            backgroundPreviewRectValid = false;

            EditorGUILayout.HelpBox(
                "点击图片预览可吸取颜色。容差越大，去除范围越大；侵蚀会向内收缩不透明区域；边缘平滑对透明边缘做羽化。处理后保存为 原名_nobg.png。",
                MessageType.Info);
            EditorGUILayout.Space(6);

            DrawDropBox(
                string.IsNullOrEmpty(backgroundSourcePath) ? "拖拽图片到此处" : Path.GetFileName(backgroundSourcePath),
                60f);

            EditorGUILayout.Space(8);
            keyColor = EditorGUILayout.ColorField("目标颜色", keyColor);
            tolerance = EditorGUILayout.Slider("容差", tolerance, 0f, 255f);
            erosion = EditorGUILayout.IntSlider("侵蚀", erosion, 0, 20);
            edgeSmooth = EditorGUILayout.IntSlider("边缘平滑", edgeSmooth, 0, 20);

            EditorGUILayout.Space(12);

            using (new EditorGUI.DisabledScope(backgroundSourceTexture == null))
            {
                if (GUILayout.Button("去除背景并保存", GUILayout.Height(34)))
                {
                    ProcessBackgroundAndSave();
                }
            }

            if (backgroundSourceTexture == null)
            {
                EditorGUILayout.HelpBox("请先拖入要处理的图片。", MessageType.Warning);
            }

            if (!string.IsNullOrEmpty(backgroundSourcePath))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("源文件", backgroundSourcePath, EditorStyles.wordWrappedLabel);
            }

            DrawBackgroundPreview();

            EditorGUILayout.EndScrollView();
            HandleBackgroundDragAndDrop();
            HandleBackgroundPreviewClick();
        }

        private void DrawTweakTab()
        {
            tweakScrollPosition = EditorGUILayout.BeginScrollView(tweakScrollPosition);

            EditorGUILayout.HelpBox(
                "拖入图集并设置行列后，勾选一帧或多帧，下方预览会按间隔循环播放选中帧。命名为 _行_列（左上为 0,0）。每帧可设 X/Y 像素偏移（+X 向右，+Y 向上）。\n" +
                "目标宽高小于原图单帧时，从原图单帧中心裁切；预览左侧为原图单帧（黄框为裁切区域），右侧为目标图，锚点设置在目标图预览上。保存时按目标宽高与行列生成新图集。",
                MessageType.Info);
            EditorGUILayout.Space(6);

            DrawDropBox(
                string.IsNullOrEmpty(tweakPath) ? "拖拽图片到此处" : Path.GetFileName(tweakPath),
                60f);

            EditorGUILayout.Space(8);
            EditorGUI.BeginChangeCheck();
            tweakColumns = Mathf.Max(1, EditorGUILayout.IntField("横向数量 (列)", tweakColumns));
            tweakRows = Mathf.Max(1, EditorGUILayout.IntField("纵向数量 (行)", tweakRows));
            if (EditorGUI.EndChangeCheck())
            {
                EnsureTweakSelection(true);
                EnsureTweakTargetSize(true);
            }

            if (tweakTexture != null)
            {
                GetTweakSourceCellSize(out int sourceCellWidth, out int sourceCellHeight);
                EditorGUILayout.LabelField(
                    $"原图单帧尺寸: {sourceCellWidth} x {sourceCellHeight}（图集 {tweakTexture.width} x {tweakTexture.height}）",
                    EditorStyles.miniLabel);

                EditorGUI.BeginChangeCheck();
                tweakTargetWidth = Mathf.Max(1, EditorGUILayout.IntField("目标图片宽度", tweakTargetWidth));
                tweakTargetHeight = Mathf.Max(1, EditorGUILayout.IntField("目标图片高度", tweakTargetHeight));
                if (EditorGUI.EndChangeCheck())
                {
                    tweakTargetWidth = Mathf.Min(tweakTargetWidth, sourceCellWidth);
                    tweakTargetHeight = Mathf.Min(tweakTargetHeight, sourceCellHeight);
                    ClampTweakAnchorToTarget();
                }

                if (tweakTargetWidth > sourceCellWidth || tweakTargetHeight > sourceCellHeight)
                {
                    EditorGUILayout.HelpBox(
                        $"目标尺寸不能超过原图单帧 {sourceCellWidth} x {sourceCellHeight}。",
                        MessageType.Warning);
                }
            }
            else
            {
                tweakTargetWidth = Mathf.Max(1, EditorGUILayout.IntField("目标图片宽度", tweakTargetWidth));
                tweakTargetHeight = Mathf.Max(1, EditorGUILayout.IntField("目标图片高度", tweakTargetHeight));
            }

            tweakInterval = EditorGUILayout.Slider("播放间隔 (秒)", tweakInterval, 0.02f, 2f);

            EditorGUILayout.Space(8);
            DrawTweakSettingsBar();

            if (tweakTexture == null)
            {
                EditorGUILayout.HelpBox("请先拖入一张序列帧图集。", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                HandleTweakDragAndDrop();
                return;
            }

            EnsureTweakSelection(false);
            EnsureTweakTargetSize(false);
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选", GUILayout.Width(72)))
            {
                SetAllTweakSelected(true);
            }

            if (GUILayout.Button("全不选", GUILayout.Width(72)))
            {
                SetAllTweakSelected(false);
            }

            EditorGUILayout.LabelField($"共 {tweakRows * tweakColumns} 帧，已选 {CountTweakSelected()} 帧");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            tweakListScroll = EditorGUILayout.BeginScrollView(tweakListScroll, GUILayout.MinHeight(160f));
            for (int row = 0; row < tweakRows; row++)
            {
                for (int column = 0; column < tweakColumns; column++)
                {
                    int index = row * tweakColumns + column;
                    EditorGUILayout.BeginHorizontal();
                    tweakSelected[index] = EditorGUILayout.Toggle(tweakSelected[index], GUILayout.Width(18));
                    Rect thumbRect = GUILayoutUtility.GetRect(40f, 40f, GUILayout.Width(40f), GUILayout.Height(40f));
                    DrawTweakCell(thumbRect, row, column, tweakOffsets[index]);
                    EditorGUILayout.LabelField($"_{row}_{column}", GUILayout.Width(56));
                    EditorGUILayout.LabelField("X偏移", GUILayout.Width(36));
                    float offsetX = DrawStepFloatField(tweakOffsets[index].x, 0.01f, 48f);
                    EditorGUILayout.LabelField("Y偏移", GUILayout.Width(36));
                    float offsetY = DrawStepFloatField(tweakOffsets[index].y, 0.01f, 48f);
                    tweakOffsets[index] = new Vector2(offsetX, offsetY);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("预览", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("锚点 X", GUILayout.Width(48));
            tweakAnchor.x = DrawStepFloatField(tweakAnchor.x, 0.01f, 56f);
            EditorGUILayout.LabelField("锚点 Y", GUILayout.Width(48));
            tweakAnchor.y = DrawStepFloatField(tweakAnchor.y, 0.01f, 56f);
            EditorGUILayout.LabelField(
                $"像素，相对目标图左下角（+X 右，+Y 上），{tweakTargetWidth}x{tweakTargetHeight}",
                EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            Rect previewRowRect = GUILayoutUtility.GetRect(0f, 220f, GUILayout.ExpandWidth(true));
            float halfWidth = (previewRowRect.width - 8f) * 0.5f;
            Rect originalPreviewRect = new Rect(previewRowRect.x, previewRowRect.y, halfWidth, previewRowRect.height);
            Rect targetPreviewRect = new Rect(previewRowRect.x + halfWidth + 8f, previewRowRect.y, halfWidth, previewRowRect.height);
            DrawTweakOriginalPreview(originalPreviewRect);
            DrawTweakTargetPreview(targetPreviewRect);

            EditorGUILayout.EndScrollView();
            HandleTweakDragAndDrop();
        }

        private static float DrawStepFloatField(float value, float step, float fieldWidth)
        {
            float result = EditorGUILayout.FloatField(value, GUILayout.Width(fieldWidth));
            if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(18)))
            {
                result = Mathf.Round((result - step) / step) * step;
            }

            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(18)))
            {
                result = Mathf.Round((result + step) / step) * step;
            }

            return result;
        }

        private void DrawBorderTab()
        {
            borderScroll = EditorGUILayout.BeginScrollView(borderScroll);
            EditorGUILayout.HelpBox(
                "拖入图片后设置边框像素和颜色。会用指定颜色覆盖原图最外一圈像素，图片宽高不变；颜色 Alpha 可设为 0（把边缘做成透明）。保存会覆盖原图。",
                MessageType.Info);
            EditorGUILayout.Space(6);

            DrawDropBox(
                string.IsNullOrEmpty(borderSourcePath) ? "拖拽图片到此处" : Path.GetFileName(borderSourcePath),
                60f);

            EditorGUILayout.Space(8);
            EditorGUI.BeginChangeCheck();
            borderSize = Mathf.Max(0, EditorGUILayout.IntField("边框像素", borderSize));
            borderColor = EditorGUILayout.ColorField(new GUIContent("边框颜色"), borderColor, true, true, false);
            if (EditorGUI.EndChangeCheck())
            {
                RebuildBorderPreview();
            }

            EditorGUILayout.LabelField($"当前 Alpha: {borderColor.a:0.###}（0 为全透明）");

            EditorGUILayout.Space(12);
            using (new EditorGUI.DisabledScope(borderSourceTexture == null || borderSize < 0 || string.IsNullOrEmpty(borderSourcePath)))
            {
                if (GUILayout.Button("保存图片", GUILayout.Height(34)))
                {
                    SaveBorderImage();
                }
            }

            if (borderSourceTexture == null)
            {
                EditorGUILayout.HelpBox("请先拖入一张图片。", MessageType.Warning);
            }
            else if (!string.IsNullOrEmpty(borderSourcePath))
            {
                EditorGUILayout.LabelField("源文件", borderSourcePath, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField(
                    $"尺寸保持 {borderSourceTexture.width}x{borderSourceTexture.height}，覆盖外圈 {borderSize} 像素");
            }

            Texture2D display = borderPreviewTexture != null ? borderPreviewTexture : borderSourceTexture;
            if (display != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField(borderPreviewTexture != null ? "处理后预览" : "原图预览");
                float maxPreview = 320f;
                float scale = Mathf.Min(maxPreview / display.width, maxPreview / display.height, 1f);
                Rect previewRect = GUILayoutUtility.GetRect(
                    display.width * scale,
                    display.height * scale,
                    GUILayout.ExpandWidth(false));
                EditorGUI.DrawTextureTransparent(previewRect, display);
            }

            EditorGUILayout.EndScrollView();
            HandleBorderDragAndDrop();
        }

        private void DrawTwoDTextureTab()
        {
            twoDTextureScrollPosition = EditorGUILayout.BeginScrollView(twoDTextureScrollPosition);
            twoDTextureMakerTab.OnGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawImageRecolorTab()
        {
            imageRecolorScrollPosition = EditorGUILayout.BeginScrollView(imageRecolorScrollPosition);
            imageRecolorTab.OnGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawTweakSettingsBar()
        {
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!CanSaveTweakSettings(out _)))
            {
                if (GUILayout.Button("保存设置", GUILayout.Height(28)))
                {
                    SaveTweakSettings();
                }
            }

            using (new EditorGUI.DisabledScope(!CanSaveTweakOffsetImage(out _)))
            {
                if (GUILayout.Button("保存图片", GUILayout.Height(28)))
                {
                    SaveTweakOffsetImage();
                }
            }

            if (GUILayout.Button("加载设置", GUILayout.Height(28)))
            {
                LoadTweakSettings();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            tweakLoadedSettings = (SpriteSheetTweakSettings)EditorGUILayout.ObjectField(
                "微调设置",
                tweakLoadedSettings,
                typeof(SpriteSheetTweakSettings),
                false);
            if (EditorGUI.EndChangeCheck() && tweakLoadedSettings != null)
            {
                ApplyTweakSettings(tweakLoadedSettings);
            }
        }

        private static Rect DrawDropBox(string label, float height)
        {
            Rect dropArea = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, label);
            return dropArea;
        }

        private void HandleCombineDragAndDrop()
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            if (IsFolderDrag())
            {
                if (!folderDropRect.Contains(evt.mousePosition))
                {
                    return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    TryLoadFolderFromDrag();
                }

                evt.Use();
                Repaint();
                return;
            }

            if (!IsImageDrag())
            {
                return;
            }

            int rowIndex = -1;
            for (int i = 0; i < rowDropRects.Count; i++)
            {
                if (rowDropRects[i].Contains(evt.mousePosition))
                {
                    rowIndex = i;
                    break;
                }
            }

            bool overAddRow = addRowDropRect.Contains(evt.mousePosition);
            if (rowIndex < 0 && !overAddRow)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                ApplyImageDragToRow(rowIndex, overAddRow);
            }

            evt.Use();
            Repaint();
        }

        private void HandleSliceDragAndDrop()
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                TryLoadSliceFromDrag();
            }

            evt.Use();
            Repaint();
        }

        private void HandleBackgroundDragAndDrop()
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                TryLoadBackgroundFromDrag();
            }

            evt.Use();
            Repaint();
        }

        private void HandleTweakDragAndDrop()
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                TryLoadTweakFromDrag();
            }

            evt.Use();
            Repaint();
        }

        private void HandleBorderDragAndDrop()
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                TryLoadBorderFromDrag();
            }

            evt.Use();
            Repaint();
        }

        private void ApplyImageDragToRow(int rowIndex, bool addNewRow)
        {
            List<string> names = GetImageNamesFromDrag();
            if (names.Count == 0)
            {
                return;
            }

            TrySetFolderFromDraggedFiles();
            string joined = string.Join(",", names);
            if (addNewRow || rowIndex < 0)
            {
                rowInputs.Add(joined);
            }
            else
            {
                rowInputs[rowIndex] = joined;
            }
        }

        private static bool IsFolderDrag()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path))
                {
                    return true;
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (Directory.Exists(ToAbsolutePath(path)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsImageDrag()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D)
                {
                    return true;
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (IsImageFile(path))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsImageFile(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".bmp";
        }

        private static List<string> GetImageNamesFromDrag()
        {
            var names = new List<string>();
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrEmpty(path))
                    {
                        names.Add(Path.GetFileNameWithoutExtension(path));
                    }
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (IsImageFile(path))
                {
                    names.Add(Path.GetFileNameWithoutExtension(path));
                }
            }

            names = names.Distinct().ToList();
            names.Sort(CompareSpriteSheetNames);
            return names;
        }

        private static int CompareSpriteSheetNames(string left, string right)
        {
            ParseTrailingParenNumber(left, out _, out int leftNumber);
            ParseTrailingParenNumber(right, out _, out int rightNumber);

            bool leftHasNumber = leftNumber >= 0;
            bool rightHasNumber = rightNumber >= 0;
            if (leftHasNumber && rightHasNumber)
            {
                int numberCompare = leftNumber.CompareTo(rightNumber);
                if (numberCompare != 0)
                {
                    return numberCompare;
                }
            }
            else if (leftHasNumber != rightHasNumber)
            {
                return leftHasNumber ? 1 : -1;
            }

            return EditorUtility.NaturalCompare(left ?? string.Empty, right ?? string.Empty);
        }

        private static void ParseTrailingParenNumber(string name, out string prefix, out int number)
        {
            prefix = name ?? string.Empty;
            number = -1;
            if (string.IsNullOrEmpty(name) || name[name.Length - 1] != ')')
            {
                return;
            }

            int open = name.LastIndexOf('(');
            if (open < 0 || open >= name.Length - 2)
            {
                return;
            }

            string inner = name.Substring(open + 1, name.Length - open - 2);
            if (!int.TryParse(inner, out number))
            {
                number = -1;
                return;
            }

            prefix = name.Substring(0, open).TrimEnd();
        }

        private void TrySetFolderFromDraggedFiles()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrEmpty(path))
                    {
                        folderPath = ToAbsolutePath(Path.GetDirectoryName(path));
                        return;
                    }
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (IsImageFile(path))
                {
                    folderPath = ToAbsolutePath(Path.GetDirectoryName(path));
                    return;
                }
            }
        }

        private void TryLoadFolderFromDrag()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path))
                {
                    folderPath = ToAbsolutePath(path);
                    Repaint();
                    return;
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                string absolute = ToAbsolutePath(path);
                if (Directory.Exists(absolute))
                {
                    folderPath = absolute;
                    Repaint();
                    return;
                }
            }
        }

        private void TryLoadSliceFromDrag()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is UnityEngine.Texture2D tex)
                {
                    sourceTexture = tex;
                    sourcePath = AssetDatabase.GetAssetPath(tex);
                    Repaint();
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
            if (!File.Exists(path))
            {
                return;
            }

            byte[] data = File.ReadAllBytes(path);
            if (sourceTexture == null || !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(sourceTexture)))
            {
                sourceTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            }

            if (!sourceTexture.LoadImage(data))
            {
                EditorUtility.DisplayDialog("序列帧", "无法加载图片文件。", "确定");
                return;
            }

            sourcePath = path;
            Repaint();
        }

        private static string ToAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (Path.IsPathRooted(path))
            {
                return Path.GetFullPath(path);
            }

            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        private void DrawCombineOutputSizeHint()
        {
            List<string[]> parsedRows = ParseRows();
            if (parsedRows.Count == 0)
            {
                return;
            }

            int columnCount = parsedRows.Max(r => r.Length);
            int rowCount = parsedRows.Count;
            int outputColumns = columnCount + combinePadColumns;
            int outputRows = rowCount + combinePadRows;
            int totalWidth = outputColumns * cellWidth;
            int totalHeight = outputRows * cellHeight;
            EditorGUILayout.LabelField(
                $"当前内容 {columnCount} 列 x {rowCount} 行，补全后 {outputColumns} 列 x {outputRows} 行，输出 {totalWidth} x {totalHeight}",
                EditorStyles.miniLabel);
        }

        private bool CanCombine(out string validationMessage)
        {
            validationMessage = string.Empty;
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                validationMessage = "请先拖入有效的图片文件夹。";
                return false;
            }

            if (cellWidth <= 0 || cellHeight <= 0)
            {
                validationMessage = "单张宽度和高度必须大于 0。";
                return false;
            }

            if (rowInputs.Count == 0 || !rowInputs.Any(r => !string.IsNullOrWhiteSpace(r)))
            {
                validationMessage = "请至少填写一行文件名。";
                return false;
            }

            return true;
        }

        private bool CanSlice(out string validationMessage)
        {
            validationMessage = string.Empty;
            if (sourceTexture == null || string.IsNullOrEmpty(sourcePath))
            {
                validationMessage = "请先拖入要切割的图片。";
                return false;
            }

            if (columns <= 0 || rows <= 0)
            {
                validationMessage = "横向、纵向数量必须大于 0。";
                return false;
            }

            return true;
        }

        private void CombineAndSave()
        {
            List<string[]> parsedRows = ParseRows();
            if (parsedRows.Count == 0)
            {
                EditorUtility.DisplayDialog("序列帧", "请至少填写一行文件名。", "确定");
                return;
            }

            int columnCount = parsedRows.Max(r => r.Length);
            int rowCount = parsedRows.Count;
            int outputColumnCount = columnCount + combinePadColumns;
            int outputRowCount = rowCount + combinePadRows;
            int totalWidth = outputColumnCount * cellWidth;
            int totalHeight = outputRowCount * cellHeight;

            Texture2D result = new Texture2D(totalWidth, totalHeight, TextureFormat.RGBA32, false);
            Color[] fill = new Color[totalWidth * totalHeight];
            for (int i = 0; i < fill.Length; i++)
            {
                fill[i] = combinePadColor;
            }

            result.SetPixels(fill);

            var loadedTextures = new List<Texture2D>();
            var missingFiles = new List<string>();
            bool cancelled = false;
            int outputCellCount = outputColumnCount * outputRowCount;

            try
            {
                for (int row = 0; row < rowCount && !cancelled; row++)
                {
                    string[] names = parsedRows[row];
                    for (int col = 0; col < names.Length; col++)
                    {
                        string fileName = names[col];
                        if (string.IsNullOrWhiteSpace(fileName))
                        {
                            continue;
                        }

                        float progress = (float)(row * columnCount + col) / Mathf.Max(outputCellCount, 1);
                        if (EditorUtility.DisplayCancelableProgressBar("合并中", $"正在处理: {fileName}", progress))
                        {
                            cancelled = true;
                            break;
                        }

                        Texture2D source = LoadImageFromFolder(folderPath, fileName.Trim());
                        if (source == null)
                        {
                            missingFiles.Add(fileName);
                            continue;
                        }

                        loadedTextures.Add(source);
                        int destX = col * cellWidth;
                        int destY = (outputRowCount - 1 - row) * cellHeight;
                        BlitToRegion(result, source, destX, destY, cellWidth, cellHeight);
                    }
                }

                if (cancelled)
                {
                    return;
                }

                result.Apply();

                string outputPath = Path.Combine(folderPath, "combined.png");
                File.WriteAllBytes(outputPath, result.EncodeToPNG());

                if (folderPath.Replace('\\', '/').Contains("/Assets"))
                {
                    AssetDatabase.Refresh();
                }

                string message = $"已生成: {outputPath}\n尺寸: {totalWidth} x {totalHeight}（{outputColumnCount} 列 x {outputRowCount} 行）";
                if (missingFiles.Count > 0)
                {
                    message += $"\n\n未找到 {missingFiles.Count} 个文件:\n" + string.Join("\n", missingFiles);
                }

                EditorUtility.DisplayDialog(missingFiles.Count > 0 ? "完成（部分缺失）" : "完成", message, "确定");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                DestroyImmediate(result);
                foreach (Texture2D tex in loadedTextures)
                {
                    DestroyImmediate(tex);
                }
            }
        }

        private List<string[]> ParseRows()
        {
            var parsed = new List<string[]>();
            foreach (string input in rowInputs)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                string[] names = input.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
                System.Array.Sort(names, CompareSpriteSheetNames);
                parsed.Add(names);
            }

            return parsed;
        }

        private static Texture2D LoadImageFromFolder(string folder, string nameWithoutExt)
        {
            foreach (string ext in ImageExtensions)
            {
                string path = Path.Combine(folder, nameWithoutExt + ext);
                if (File.Exists(path))
                {
                    return LoadTextureFromFile(path);
                }
            }

            foreach (string file in Directory.GetFiles(folder))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (string.Equals(fileName, nameWithoutExt, System.StringComparison.OrdinalIgnoreCase))
                {
                    return LoadTextureFromFile(file);
                }
            }

            return null;
        }

        private static Texture2D LoadTextureFromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
            {
                DestroyImmediate(tex);
                return null;
            }

            return tex;
        }

        private static void BlitToRegion(Texture2D target, Texture2D source, int destX, int destY, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D scaled = new Texture2D(width, height, TextureFormat.RGBA32, false);
            scaled.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            scaled.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            target.SetPixels(destX, destY, width, height, scaled.GetPixels());
            DestroyImmediate(scaled);
        }

        private void SliceAndSave()
        {
            Texture2D readable = GetReadableTexture(sourceTexture);
            if (readable == null)
            {
                EditorUtility.DisplayDialog("序列帧", "无法读取图片数据。", "确定");
                return;
            }

            int cellW = readable.width / columns;
            int cellH = readable.height / rows;
            if (cellW <= 0 || cellH <= 0)
            {
                DestroyImmediate(readable);
                EditorUtility.DisplayDialog("序列帧", "横向或纵向数量过大，无法切割。", "确定");
                return;
            }

            if (readable.width % columns != 0 || readable.height % rows != 0)
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "尺寸未整除",
                    $"图片尺寸 {readable.width}x{readable.height} 无法被 {columns}x{rows} 整除。\n" +
                    $"将按 {cellW}x{cellH} 切割，边缘多余像素会被丢弃。是否继续？",
                    "继续", "取消");
                if (!proceed)
                {
                    DestroyImmediate(readable);
                    return;
                }
            }

            string directory = Path.GetDirectoryName(sourcePath);
            string baseName = Path.GetFileNameWithoutExtension(sourcePath);
            int total = columns * rows;
            int saved = 0;
            bool cancelled = false;

            try
            {
                for (int y = 0; y < rows && !cancelled; y++)
                {
                    for (int x = 0; x < columns; x++)
                    {
                        float progress = (float)saved / total;
                        if (EditorUtility.DisplayCancelableProgressBar(
                                "切割中",
                                $"正在保存 {baseName}_{y}_{x}.png",
                                progress))
                        {
                            cancelled = true;
                            break;
                        }

                        Color[] pixels = readable.GetPixels(
                            x * cellW,
                            readable.height - (y + 1) * cellH,
                            cellW,
                            cellH);

                        Texture2D slice = new Texture2D(cellW, cellH, TextureFormat.RGBA32, false);
                        slice.SetPixels(pixels);
                        slice.Apply();

                        string outputPath = Path.Combine(directory, $"{baseName}_{y}_{x}.png");
                        File.WriteAllBytes(outputPath, slice.EncodeToPNG());
                        DestroyImmediate(slice);
                        saved++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                DestroyImmediate(readable);
            }

            if (!string.IsNullOrEmpty(directory) && directory.Replace('\\', '/').Contains("/Assets"))
            {
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("完成", $"已切割并保存 {saved} 张图片到:\n{directory}", "确定");
        }

        private static Texture2D GetReadableTexture(Texture2D source)
        {
            if (source == null)
            {
                return null;
            }

            RenderTexture tmp = RenderTexture.GetTemporary(
                source.width, source.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(source, tmp);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = tmp;

            Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readable.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(tmp);
            return readable;
        }

        private void DrawBackgroundPreview()
        {
            Texture2D display = backgroundPreviewTexture != null ? backgroundPreviewTexture : backgroundSourceTexture;
            if (display == null)
            {
                return;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(backgroundPreviewTexture != null ? "处理后预览" : "原图预览（点击吸取颜色）");

            float maxPreview = 320f;
            float scale = Mathf.Min(maxPreview / display.width, maxPreview / display.height, 1f);
            backgroundPreviewRect = GUILayoutUtility.GetRect(
                display.width * scale,
                display.height * scale,
                GUILayout.ExpandWidth(false));
            backgroundPreviewRectValid = true;
            EditorGUI.DrawPreviewTexture(backgroundPreviewRect, display);
        }

        private void HandleBackgroundPreviewClick()
        {
            if (backgroundSourceTexture == null || !backgroundPreviewRectValid)
            {
                return;
            }

            Event evt = Event.current;
            if (evt.type != EventType.MouseDown || evt.button != 0)
            {
                return;
            }

            if (!backgroundPreviewRect.Contains(evt.mousePosition))
            {
                return;
            }

            float u = (evt.mousePosition.x - backgroundPreviewRect.x) / backgroundPreviewRect.width;
            float v = 1f - (evt.mousePosition.y - backgroundPreviewRect.y) / backgroundPreviewRect.height;
            int x = Mathf.Clamp(Mathf.FloorToInt(u * backgroundSourceTexture.width), 0, backgroundSourceTexture.width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(v * backgroundSourceTexture.height), 0, backgroundSourceTexture.height - 1);

            keyColor = backgroundSourceTexture.GetPixel(x, y);
            keyColor.a = 1f;
            evt.Use();
            Repaint();
        }

        private void TryLoadBackgroundFromDrag()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D tex)
                {
                    LoadBackgroundTexture(tex, AssetDatabase.GetAssetPath(tex));
                    return;
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (IsImageFile(path))
                {
                    LoadBackgroundExternalImage(path);
                    return;
                }
            }
        }

        private void LoadBackgroundTexture(Texture2D tex, string path)
        {
            ClearBackgroundPreview();
            DestroyOwnedTexture(ref backgroundSourceTexture);
            backgroundSourceTexture = GetReadableTexture(tex);
            backgroundSourcePath = string.IsNullOrEmpty(path) ? null : ToAbsolutePath(path);
            Repaint();
        }

        private void LoadBackgroundExternalImage(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            byte[] data = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
            {
                DestroyImmediate(tex);
                EditorUtility.DisplayDialog("序列帧", "无法加载图片文件。", "确定");
                return;
            }

            LoadBackgroundTexture(tex, path);
            DestroyImmediate(tex);
        }

        private void ProcessBackgroundAndSave()
        {
            if (backgroundSourceTexture == null)
            {
                return;
            }

            Texture2D readable = GetReadableTexture(backgroundSourceTexture);
            if (readable == null)
            {
                EditorUtility.DisplayDialog("序列帧", "无法读取图片数据。", "确定");
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("处理中", "正在去除背景...", 0.2f);
                Texture2D result = RemoveBackground(readable, keyColor, tolerance, erosion, edgeSmooth);
                ClearBackgroundPreview();
                backgroundPreviewTexture = result;

                if (string.IsNullOrEmpty(backgroundSourcePath))
                {
                    EditorUtility.DisplayDialog("完成", "背景已去除。预览已更新（未指定源文件路径，未保存到磁盘）。", "确定");
                    return;
                }

                string directory = Path.GetDirectoryName(backgroundSourcePath);
                string baseName = Path.GetFileNameWithoutExtension(backgroundSourcePath);
                string outputPath = Path.Combine(directory, baseName + "_nobg.png");
                File.WriteAllBytes(outputPath, result.EncodeToPNG());

                if (!string.IsNullOrEmpty(directory) &&
                    (directory.Replace('\\', '/').Contains("/Assets") ||
                     ToAbsolutePath(directory).Replace('\\', '/').Contains("/Assets")))
                {
                    AssetDatabase.Refresh();
                }

                EditorUtility.DisplayDialog("完成", $"已保存:\n{outputPath}", "确定");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                DestroyImmediate(readable);
            }
        }

        private static Texture2D RemoveBackground(Texture2D source, Color targetColor, float colorTolerance, int erosionPasses, int smoothRadius)
        {
            int width = source.width;
            int height = source.height;
            Color[] src = source.GetPixels();
            float[] alpha = new float[src.Length];
            float threshold = colorTolerance / 255f;

            for (int i = 0; i < src.Length; i++)
            {
                Color p = src[i];
                float diff = Mathf.Max(
                    Mathf.Abs(p.r - targetColor.r),
                    Mathf.Abs(p.g - targetColor.g),
                    Mathf.Abs(p.b - targetColor.b));
                alpha[i] = diff <= threshold ? 0f : p.a;
            }

            if (erosionPasses > 0)
            {
                alpha = ErodeAlpha(alpha, width, height, erosionPasses);
            }

            if (smoothRadius > 0)
            {
                alpha = BlurAlpha(alpha, width, height, smoothRadius);
            }

            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var dst = new Color[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = src[i];
                dst[i].a = alpha[i];
            }

            result.SetPixels(dst);
            result.Apply();
            return result;
        }

        private static float[] ErodeAlpha(float[] alpha, int width, int height, int iterations)
        {
            float[] current = alpha;
            var temp = new float[alpha.Length];

            for (int pass = 0; pass < iterations; pass++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        float min = current[idx];
                        for (int oy = -1; oy <= 1; oy++)
                        {
                            for (int ox = -1; ox <= 1; ox++)
                            {
                                int nx = x + ox;
                                int ny = y + oy;
                                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                                {
                                    min = 0f;
                                    continue;
                                }

                                min = Mathf.Min(min, current[ny * width + nx]);
                            }
                        }

                        temp[idx] = min;
                    }
                }

                float[] swap = current;
                current = temp;
                temp = swap;
            }

            if (current != alpha)
            {
                return current;
            }

            var copy = new float[alpha.Length];
            System.Array.Copy(alpha, copy, alpha.Length);
            return copy;
        }

        private static float[] BlurAlpha(float[] alpha, int width, int height, int radius)
        {
            float[] result = new float[alpha.Length];
            int size = radius * 2 + 1;
            float inv = 1f / (size * size);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sum = 0f;
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int nx = Mathf.Clamp(x + kx, 0, width - 1);
                            int ny = Mathf.Clamp(y + ky, 0, height - 1);
                            sum += alpha[ny * width + nx];
                        }
                    }

                    result[y * width + x] = sum * inv;
                }
            }

            return result;
        }

        private void ClearBackgroundPreview()
        {
            DestroyOwnedTexture(ref backgroundPreviewTexture);
        }

        private static void DestroyOwnedTexture(ref Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            DestroyImmediate(texture);
            texture = null;
        }

        private void TryLoadTweakFromDrag()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D tex)
                {
                    SetTweakTexture(tex, AssetDatabase.GetAssetPath(tex), false);
                    return;
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (IsImageFile(path))
                {
                    LoadTweakExternalImage(path);
                    return;
                }
            }
        }

        private void LoadTweakExternalImage(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            byte[] data = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
            {
                DestroyImmediate(tex);
                EditorUtility.DisplayDialog("序列帧", "无法加载图片文件。", "确定");
                return;
            }

            SetTweakTexture(tex, path, true);
        }

        private void SetTweakTexture(Texture2D tex, string path, bool ownsTexture, bool resetTargetSize = true)
        {
            ClearTweakTexture();
            tweakTexture = tex;
            tweakPath = string.IsNullOrEmpty(path) ? null : path;
            tweakOwnsTexture = ownsTexture;
            if (resetTargetSize)
            {
                tweakTargetSizeInitialized = false;
            }

            EnsureTweakSelection(true);
            EnsureTweakTargetSize(resetTargetSize);
            tweakPlayIndex = 0;
            tweakLastFrameTime = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void ClearTweakTexture()
        {
            if (tweakOwnsTexture)
            {
                DestroyOwnedTexture(ref tweakTexture);
            }
            else
            {
                tweakTexture = null;
            }

            tweakOwnsTexture = false;
            tweakPath = null;
        }

        private void EnsureTweakSelection(bool reset)
        {
            int count = Mathf.Max(1, tweakRows) * Mathf.Max(1, tweakColumns);
            if (!reset && tweakSelected != null && tweakSelected.Length == count
                && tweakOffsets != null && tweakOffsets.Length == count)
            {
                return;
            }

            tweakSelected = new bool[count];
            tweakOffsets = new Vector2[count];
            tweakPlayIndex = 0;
        }

        private void SetAllTweakSelected(bool selected)
        {
            EnsureTweakSelection(false);
            for (int i = 0; i < tweakSelected.Length; i++)
            {
                tweakSelected[i] = selected;
            }

            tweakPlayIndex = 0;
        }

        private bool CanSaveTweakSettings(out string message)
        {
            message = string.Empty;
            if (tweakTexture == null)
            {
                message = "请先拖入一张序列帧图集。";
                return false;
            }

            if (!TryGetTweakAssetDirectory(out _, out _))
            {
                message = "当前图片不在 Project 的 Assets 目录下，无法保存 ScriptableObject。";
                return false;
            }

            return true;
        }

        private void SaveTweakSettings()
        {
            if (!CanSaveTweakSettings(out string message))
            {
                EditorUtility.DisplayDialog("序列帧", message, "确定");
                return;
            }

            if (!TryGetTweakAssetDirectory(out string directoryAssetPath, out string pngName))
            {
                EditorUtility.DisplayDialog("序列帧", "无法解析当前图片的 Assets 路径。", "确定");
                return;
            }

            EnsureTweakSelection(false);
            string assetPath = $"{directoryAssetPath}/{pngName}_tweak.asset";
            SpriteSheetTweakSettings settings = AssetDatabase.LoadAssetAtPath<SpriteSheetTweakSettings>(assetPath);
            bool isNew = settings == null;
            if (isNew)
            {
                settings = CreateInstance<SpriteSheetTweakSettings>();
                AssetDatabase.CreateAsset(settings, assetPath);
            }

            Texture2D projectTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetTweakTextureAssetPath());
            settings.spriteSheet = projectTexture != null ? projectTexture : tweakTexture;
            settings.rows = tweakRows;
            settings.columns = tweakColumns;
            settings.targetWidth = tweakTargetWidth;
            settings.targetHeight = tweakTargetHeight;
            settings.offsets = (Vector2[])tweakOffsets.Clone();
            settings.anchor = tweakAnchor;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            tweakLoadedSettings = settings;
            EditorGUIUtility.PingObject(settings);
            Selection.activeObject = settings;
            EditorUtility.DisplayDialog("序列帧", isNew ? $"已保存:\n{assetPath}" : $"已覆盖保存:\n{assetPath}", "确定");
        }

        private bool CanSaveTweakOffsetImage(out string message)
        {
            message = string.Empty;
            if (tweakTexture == null)
            {
                message = "请先拖入一张序列帧图集。";
                return false;
            }

            if (string.IsNullOrEmpty(tweakPath))
            {
                message = "当前图片没有文件路径，无法保存。";
                return false;
            }

            return true;
        }

        private void SaveTweakOffsetImage()
        {
            if (!CanSaveTweakOffsetImage(out string message))
            {
                EditorUtility.DisplayDialog("序列帧", message, "确定");
                return;
            }

            Texture2D readable = GetReadableTexture(tweakTexture);
            if (readable == null)
            {
                EditorUtility.DisplayDialog("序列帧", "无法读取图片数据。", "确定");
                return;
            }

            EnsureTweakSelection(false);
            EnsureTweakTargetSize(false);
            int columns = Mathf.Max(1, tweakColumns);
            int rows = Mathf.Max(1, tweakRows);
            GetTweakSourceCellSize(out int sourceCellWidth, out int sourceCellHeight);
            int targetCellWidth = tweakTargetWidth;
            int targetCellHeight = tweakTargetHeight;
            if (sourceCellWidth <= 0 || sourceCellHeight <= 0)
            {
                DestroyImmediate(readable);
                EditorUtility.DisplayDialog("序列帧", "横向或纵向数量过大，无法导出。", "确定");
                return;
            }

            if (targetCellWidth > sourceCellWidth || targetCellHeight > sourceCellHeight)
            {
                DestroyImmediate(readable);
                EditorUtility.DisplayDialog("序列帧", "目标宽高不能超过原图单帧尺寸。", "确定");
                return;
            }

            int outputWidth = targetCellWidth * columns;
            int outputHeight = targetCellHeight * rows;
            Texture2D result = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false);
            Color[] clear = new Color[outputWidth * outputHeight];
            result.SetPixels(clear);

            int total = rows * columns;
            bool cancelled = false;
            try
            {
                for (int row = 0; row < rows && !cancelled; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        int index = row * columns + column;
                        float progress = (float)index / total;
                        if (EditorUtility.DisplayCancelableProgressBar(
                                "保存图片",
                                $"正在处理 _{row}_{column}",
                                progress))
                        {
                            cancelled = true;
                            break;
                        }

                        int srcX = column * sourceCellWidth;
                        int srcY = (rows - 1 - row) * sourceCellHeight;
                        Color[] cellPixels = readable.GetPixels(srcX, srcY, sourceCellWidth, sourceCellHeight);
                        Vector2 offset = GetTweakOffset(index);
                        int offsetX = Mathf.RoundToInt(offset.x);
                        int offsetY = Mathf.RoundToInt(offset.y);
                        Color[] shiftedCell = ShiftCellPixels(cellPixels, sourceCellWidth, sourceCellHeight, offsetX, offsetY);
                        Color[] outputCell = CropCenterCellPixels(
                            shiftedCell, sourceCellWidth, sourceCellHeight, targetCellWidth, targetCellHeight);
                        int destX = column * targetCellWidth;
                        int destY = (rows - 1 - row) * targetCellHeight;
                        result.SetPixels(destX, destY, targetCellWidth, targetCellHeight, outputCell);
                    }
                }

                if (cancelled)
                {
                    return;
                }

                result.Apply();
                string directory = Path.GetDirectoryName(ToAbsolutePath(tweakPath));
                string baseName = Path.GetFileNameWithoutExtension(tweakPath);
                string outputPath = Path.Combine(directory, baseName + "_offset.png");
                File.WriteAllBytes(outputPath, result.EncodeToPNG());

                if (!string.IsNullOrEmpty(directory) &&
                    (directory.Replace('\\', '/').Contains("/Assets") ||
                     ToAbsolutePath(directory).Replace('\\', '/').Contains("/Assets")))
                {
                    AssetDatabase.Refresh();
                }

                EditorUtility.DisplayDialog(
                    "完成",
                    $"已保存:\n{outputPath}\n尺寸: {result.width} x {result.height}，单帧 {targetCellWidth}x{targetCellHeight}，网格 {rows}x{columns}",
                    "确定");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                DestroyImmediate(readable);
                DestroyImmediate(result);
            }
        }

        private static Color[] ShiftCellPixels(Color[] source, int width, int height, int offsetX, int offsetY)
        {
            var output = new Color[width * height];
            if (offsetX == 0 && offsetY == 0)
            {
                System.Array.Copy(source, output, source.Length);
                return output;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int destX = x + offsetX;
                    int destY = y + offsetY;
                    if (destX < 0 || destY < 0 || destX >= width || destY >= height)
                    {
                        continue;
                    }

                    output[destY * width + destX] = source[y * width + x];
                }
            }

            return output;
        }

        private void LoadTweakSettings()
        {
            SpriteSheetTweakSettings settings = tweakLoadedSettings;
            if (settings == null)
            {
                string startDir = Application.dataPath;
                if (TryGetTweakAssetDirectory(out string directoryAssetPath, out _))
                {
                    startDir = ToAbsolutePath(directoryAssetPath);
                }

                string selectedPath = EditorUtility.OpenFilePanel("加载微调设置", startDir, "asset");
                if (string.IsNullOrEmpty(selectedPath))
                {
                    return;
                }

                if (!TryGetAssetPath(selectedPath, out string assetPath))
                {
                    EditorUtility.DisplayDialog("序列帧", "请选择 Project 中 Assets 目录下的设置文件。", "确定");
                    return;
                }

                settings = AssetDatabase.LoadAssetAtPath<SpriteSheetTweakSettings>(assetPath);
                if (settings == null)
                {
                    EditorUtility.DisplayDialog("序列帧", "所选文件不是序列帧微调设置。", "确定");
                    return;
                }
            }

            ApplyTweakSettings(settings);
        }

        private void ApplyTweakSettings(SpriteSheetTweakSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            tweakLoadedSettings = settings;
            tweakRows = Mathf.Max(1, settings.rows);
            tweakColumns = Mathf.Max(1, settings.columns);
            tweakAnchor = settings.anchor;
            tweakTargetSizeInitialized = settings.targetWidth > 0 && settings.targetHeight > 0;
            if (tweakTargetSizeInitialized)
            {
                tweakTargetWidth = settings.targetWidth;
                tweakTargetHeight = settings.targetHeight;
            }

            Texture2D texture = settings.spriteSheet;
            string texturePath = texture != null ? AssetDatabase.GetAssetPath(texture) : null;
            if (texture == null)
            {
                EnsureTweakTargetSize(!tweakTargetSizeInitialized);
                EditorUtility.DisplayDialog("序列帧", "设置里没有图片引用，已加载行列、目标尺寸、偏移和锚点。", "确定");
            }
            else
            {
                SetTweakTexture(texture, texturePath, false, false);
            }

            EnsureTweakSelection(true);
            EnsureTweakTargetSize(!tweakTargetSizeInitialized);
            if (settings.offsets != null)
            {
                int copyCount = Mathf.Min(tweakOffsets.Length, settings.offsets.Length);
                for (int i = 0; i < copyCount; i++)
                {
                    tweakOffsets[i] = settings.offsets[i];
                }
            }

            tweakPlayIndex = 0;
            Repaint();
        }

        private string GetTweakTextureAssetPath()
        {
            if (TryGetAssetPath(tweakPath, out string assetPath))
            {
                return assetPath;
            }

            return tweakTexture != null ? AssetDatabase.GetAssetPath(tweakTexture) : string.Empty;
        }

        private bool TryGetTweakAssetDirectory(out string directoryAssetPath, out string pngName)
        {
            directoryAssetPath = null;
            pngName = null;
            string textureAssetPath = GetTweakTextureAssetPath();
            if (string.IsNullOrEmpty(textureAssetPath) || !TryGetAssetPath(textureAssetPath, out textureAssetPath))
            {
                return false;
            }

            directoryAssetPath = Path.GetDirectoryName(textureAssetPath)?.Replace('\\', '/');
            pngName = Path.GetFileNameWithoutExtension(textureAssetPath);
            return !string.IsNullOrEmpty(directoryAssetPath) && !string.IsNullOrEmpty(pngName);
        }

        private static bool TryGetAssetPath(string path, out string assetPath)
        {
            assetPath = null;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            path = path.Replace('\\', '/');
            if (path.StartsWith("Assets/"))
            {
                assetPath = path;
                return true;
            }

            string dataPath = Application.dataPath.Replace('\\', '/');
            if (path.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase))
            {
                assetPath = "Assets" + path.Substring(dataPath.Length);
                return true;
            }

            return false;
        }

        private int CountTweakSelected()
        {
            if (tweakSelected == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < tweakSelected.Length; i++)
            {
                if (tweakSelected[i])
                {
                    count++;
                }
            }

            return count;
        }

        private int GetTweakSelectedIndexAt(int playIndex)
        {
            if (tweakSelected == null || tweakSelected.Length == 0)
            {
                return -1;
            }

            int selectedCount = CountTweakSelected();
            if (selectedCount <= 0)
            {
                return -1;
            }

            int target = playIndex % selectedCount;
            int seen = 0;
            for (int i = 0; i < tweakSelected.Length; i++)
            {
                if (!tweakSelected[i])
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

        private void DrawTweakCell(Rect rect, int row, int column, Vector2 pixelOffset)
        {
            if (tweakTexture == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1f));
            GUI.BeginClip(rect);
            Rect drawRect = new Rect(0f, 0f, rect.width, rect.height);
            ApplyTweakPixelOffset(ref drawRect, pixelOffset, rect.width, rect.height);
            GUI.DrawTextureWithTexCoords(drawRect, tweakTexture, GetTweakCellUvNormalized(row, column), true);
            GUI.EndClip();
        }

        private Rect GetTweakCellUvNormalized(int row, int column)
        {
            float width = 1f / Mathf.Max(tweakColumns, 1);
            float height = 1f / Mathf.Max(tweakRows, 1);
            return new Rect(column * width, (tweakRows - 1 - row) * height, width, height);
        }

        private void ApplyTweakPixelOffset(ref Rect drawRect, Vector2 pixelOffset, float viewWidth, float viewHeight)
        {
            if (tweakTexture == null)
            {
                return;
            }

            float cellWidth = tweakTexture.width / (float)Mathf.Max(tweakColumns, 1);
            float cellHeight = tweakTexture.height / (float)Mathf.Max(tweakRows, 1);
            drawRect.x += pixelOffset.x * (viewWidth / Mathf.Max(cellWidth, 0.0001f));
            drawRect.y -= pixelOffset.y * (viewHeight / Mathf.Max(cellHeight, 0.0001f));
        }

        private Vector2 GetTweakOffset(int frameIndex)
        {
            if (tweakOffsets == null || frameIndex < 0 || frameIndex >= tweakOffsets.Length)
            {
                return Vector2.zero;
            }

            return tweakOffsets[frameIndex];
        }

        private void GetTweakSourceCellSize(out int cellWidth, out int cellHeight)
        {
            if (tweakTexture == null)
            {
                cellWidth = 0;
                cellHeight = 0;
                return;
            }

            cellWidth = tweakTexture.width / Mathf.Max(1, tweakColumns);
            cellHeight = tweakTexture.height / Mathf.Max(1, tweakRows);
        }

        private void EnsureTweakTargetSize(bool resetToSource)
        {
            if (tweakTexture == null)
            {
                return;
            }

            GetTweakSourceCellSize(out int sourceCellWidth, out int sourceCellHeight);
            if (sourceCellWidth <= 0 || sourceCellHeight <= 0)
            {
                return;
            }

            if (resetToSource || !tweakTargetSizeInitialized || tweakTargetWidth <= 0 || tweakTargetHeight <= 0)
            {
                tweakTargetWidth = sourceCellWidth;
                tweakTargetHeight = sourceCellHeight;
                tweakTargetSizeInitialized = true;
            }
            else
            {
                tweakTargetWidth = Mathf.Clamp(tweakTargetWidth, 1, sourceCellWidth);
                tweakTargetHeight = Mathf.Clamp(tweakTargetHeight, 1, sourceCellHeight);
            }

            ClampTweakAnchorToTarget();
        }

        private void ClampTweakAnchorToTarget()
        {
            tweakAnchor.x = Mathf.Clamp(tweakAnchor.x, 0f, tweakTargetWidth);
            tweakAnchor.y = Mathf.Clamp(tweakAnchor.y, 0f, tweakTargetHeight);
        }

        private static Color[] CropCenterCellPixels(Color[] source, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            var output = new Color[targetWidth * targetHeight];
            int cropX = (sourceWidth - targetWidth) / 2;
            int cropY = (sourceHeight - targetHeight) / 2;

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    int srcX = cropX + x;
                    int srcY = cropY + y;
                    Color color = Color.clear;
                    if (srcX >= 0 && srcY >= 0 && srcX < sourceWidth && srcY < sourceHeight)
                    {
                        color = source[srcY * sourceWidth + srcX];
                    }

                    output[y * targetWidth + x] = color;
                }
            }

            return output;
        }

        private static Rect GetCenterCropRectInSource(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            float cropX = (sourceWidth - targetWidth) * 0.5f;
            float cropY = (sourceHeight - targetHeight) * 0.5f;
            return new Rect(cropX, cropY, targetWidth, targetHeight);
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

        private void DrawTweakOriginalPreview(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
            GUI.Label(new Rect(rect.x + 6f, rect.y + 4f, rect.width - 12f, 18f), "原图预览", EditorStyles.miniBoldLabel);

            int frameIndex = GetTweakSelectedIndexAt(tweakPlayIndex);
            if (frameIndex < 0 || tweakTexture == null)
            {
                GUI.Label(rect, "勾选序列帧后在此循环播放", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            int row = frameIndex / Mathf.Max(tweakColumns, 1);
            int column = frameIndex % Mathf.Max(tweakColumns, 1);
            Vector2 pixelOffset = GetTweakOffset(frameIndex);
            GetTweakSourceCellSize(out int sourceCellWidth, out int sourceCellHeight);
            float cellAspect = sourceCellWidth / (float)Mathf.Max(sourceCellHeight, 1);
            Rect contentRect = new Rect(rect.x + 4f, rect.y + 22f, rect.width - 8f, rect.height - 44f);
            Rect drawRect = FitRectWithAspect(contentRect, cellAspect);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(contentRect);
                Rect localDraw = drawRect;
                localDraw.x -= contentRect.x;
                localDraw.y -= contentRect.y;
                ApplyTweakPixelOffset(ref localDraw, pixelOffset, drawRect.width, drawRect.height);
                GUI.DrawTextureWithTexCoords(localDraw, tweakTexture, GetTweakCellUvNormalized(row, column), true);

                Rect cropRect = GetCenterCropRectInSource(sourceCellWidth, sourceCellHeight, tweakTargetWidth, tweakTargetHeight);
                float scaleX = drawRect.width / Mathf.Max(sourceCellWidth, 0.0001f);
                float scaleY = drawRect.height / Mathf.Max(sourceCellHeight, 0.0001f);
                Rect localCrop = new Rect(
                    localDraw.x + cropRect.x * scaleX,
                    localDraw.yMax - (cropRect.y + cropRect.height) * scaleY,
                    cropRect.width * scaleX,
                    cropRect.height * scaleY);
                DrawRectOutline(localCrop, new Color(1f, 0.85f, 0.2f, 1f), 2f);
                GUI.EndClip();
            }

            GUI.Label(
                new Rect(rect.x + 6f, rect.yMax - 20f, rect.width - 12f, 18f),
                $"_{row}_{column}  原图 {sourceCellWidth}x{sourceCellHeight}  X:{pixelOffset.x}  Y:{pixelOffset.y}");
        }

        private void DrawTweakTargetPreview(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
            GUI.Label(new Rect(rect.x + 6f, rect.y + 4f, rect.width - 12f, 18f), "目标图预览", EditorStyles.miniBoldLabel);

            int frameIndex = GetTweakSelectedIndexAt(tweakPlayIndex);
            if (frameIndex < 0 || tweakTexture == null)
            {
                GUI.Label(rect, "勾选序列帧后在此循环播放", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            int row = frameIndex / Mathf.Max(tweakColumns, 1);
            int column = frameIndex % Mathf.Max(tweakColumns, 1);
            Vector2 pixelOffset = GetTweakOffset(frameIndex);
            GetTweakSourceCellSize(out int sourceCellWidth, out int sourceCellHeight);
            float targetAspect = tweakTargetWidth / (float)Mathf.Max(tweakTargetHeight, 1);
            Rect contentRect = new Rect(rect.x + 4f, rect.y + 22f, rect.width - 8f, rect.height - 44f);
            Rect drawRect = FitRectWithAspect(contentRect, targetAspect);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(contentRect);
                Rect localDraw = drawRect;
                localDraw.x -= contentRect.x;
                localDraw.y -= contentRect.y;

                GUI.BeginClip(localDraw);
                float scaleX = localDraw.width / Mathf.Max(tweakTargetWidth, 0.0001f);
                float scaleY = localDraw.height / Mathf.Max(tweakTargetHeight, 0.0001f);
                float fullDrawWidth = sourceCellWidth * scaleX;
                float fullDrawHeight = sourceCellHeight * scaleY;
                Rect sourceDrawRect = new Rect(
                    -(sourceCellWidth - tweakTargetWidth) * 0.5f * scaleX,
                    -(sourceCellHeight - tweakTargetHeight) * 0.5f * scaleY,
                    fullDrawWidth,
                    fullDrawHeight);
                ApplyTweakPixelOffset(ref sourceDrawRect, pixelOffset, fullDrawWidth, fullDrawHeight);
                GUI.DrawTextureWithTexCoords(sourceDrawRect, tweakTexture, GetTweakCellUvNormalized(row, column), true);
                GUI.EndClip();
                GUI.EndClip();

                DrawRectOutline(drawRect, new Color(0.4f, 0.85f, 1f, 1f), 2f);
                DrawAnchorDot(GetTweakAnchorScreenPosition(drawRect));
            }

            GUI.Label(
                new Rect(rect.x + 6f, rect.yMax - 20f, rect.width - 12f, 18f),
                $"目标 {tweakTargetWidth}x{tweakTargetHeight}  锚点 ({tweakAnchor.x:0.##}, {tweakAnchor.y:0.##})");
        }

        private void UpdateTweakPreviewPlayback()
        {
            if (selectedTab != 3 || tweakTexture == null)
            {
                return;
            }

            int selectedCount = CountTweakSelected();
            if (selectedCount <= 0)
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float interval = Mathf.Max(tweakInterval, 0.02f);
            if (now - tweakLastFrameTime < interval)
            {
                return;
            }

            tweakLastFrameTime = now;
            tweakPlayIndex = (tweakPlayIndex + 1) % selectedCount;
            Repaint();
        }

        private Vector2 GetTweakAnchorScreenPosition(Rect frameRect)
        {
            float scaleX = frameRect.width / Mathf.Max(tweakTargetWidth, 0.0001f);
            float scaleY = frameRect.height / Mathf.Max(tweakTargetHeight, 0.0001f);
            return new Vector2(
                frameRect.x + tweakAnchor.x * scaleX,
                frameRect.yMax - tweakAnchor.y * scaleY);
        }

        private static void DrawAnchorDot(Vector2 center)
        {
            const float arm = 7f;
            const float thickness = 2f;
            EditorGUI.DrawRect(new Rect(center.x - arm, center.y - thickness * 0.5f, arm * 2f, thickness), Color.red);
            EditorGUI.DrawRect(new Rect(center.x - thickness * 0.5f, center.y - arm, thickness, arm * 2f), Color.red);

            const float size = 8f;
            EditorGUI.DrawRect(new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size), Color.red);
        }

        private static void DrawRectOutline(Rect rect, Color color, float thickness)
        {
            thickness = Mathf.Max(thickness, 1f);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private void TryLoadBorderFromDrag()
        {
            foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D tex)
                {
                    SetBorderTexture(tex, AssetDatabase.GetAssetPath(tex), false);
                    return;
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (IsImageFile(path))
                {
                    LoadBorderExternalImage(path);
                    return;
                }
            }
        }

        private void LoadBorderExternalImage(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            byte[] data = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
            {
                DestroyImmediate(tex);
                EditorUtility.DisplayDialog("序列帧", "无法加载图片文件。", "确定");
                return;
            }

            SetBorderTexture(tex, path, true);
        }

        private void SetBorderTexture(Texture2D tex, string path, bool ownsTexture)
        {
            ClearBorderTextures();
            borderSourceTexture = ownsTexture ? tex : GetReadableTexture(tex);
            borderOwnsTexture = true;
            borderSourcePath = string.IsNullOrEmpty(path) ? null : path;
            RebuildBorderPreview();
            Repaint();
        }

        private void ClearBorderTextures()
        {
            DestroyOwnedTexture(ref borderPreviewTexture);
            if (borderOwnsTexture)
            {
                DestroyOwnedTexture(ref borderSourceTexture);
            }
            else
            {
                borderSourceTexture = null;
            }

            borderOwnsTexture = false;
        }

        private void RebuildBorderPreview()
        {
            DestroyOwnedTexture(ref borderPreviewTexture);
            if (borderSourceTexture == null)
            {
                return;
            }

            borderPreviewTexture = CreateBorderedTexture(borderSourceTexture, borderSize, borderColor);
        }

        private void SaveBorderImage()
        {
            if (borderSourceTexture == null || string.IsNullOrEmpty(borderSourcePath))
            {
                EditorUtility.DisplayDialog("序列帧", "请先拖入一张图片。", "确定");
                return;
            }

            Texture2D result = CreateBorderedTexture(borderSourceTexture, borderSize, borderColor);
            if (result == null)
            {
                EditorUtility.DisplayDialog("序列帧", "生成失败。", "确定");
                return;
            }

            try
            {
                string outputPath = ToAbsolutePath(borderSourcePath);
                File.WriteAllBytes(outputPath, result.EncodeToPNG());

                if (outputPath.Replace('\\', '/').Contains("/Assets"))
                {
                    AssetDatabase.Refresh();
                }

                DestroyOwnedTexture(ref borderPreviewTexture);
                if (borderOwnsTexture)
                {
                    DestroyOwnedTexture(ref borderSourceTexture);
                }

                borderSourceTexture = result;
                borderOwnsTexture = true;
                result = null;
                EditorUtility.DisplayDialog("完成", $"已覆盖原图:\n{outputPath}\n尺寸: {borderSourceTexture.width} x {borderSourceTexture.height}", "确定");
            }
            finally
            {
                if (result != null)
                {
                    DestroyImmediate(result);
                }
            }
        }

        private static Texture2D CreateBorderedTexture(Texture2D source, int border, Color color)
        {
            if (source == null)
            {
                return null;
            }

            int width = source.width;
            int height = source.height;
            border = Mathf.Clamp(border, 0, Mathf.Max(Mathf.Min(width, height) / 2, 0));

            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = source.GetPixels();
            if (border > 0)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (x < border || y < border || x >= width - border || y >= height - border)
                        {
                            pixels[y * width + x] = color;
                        }
                    }
                }
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
    }
}
