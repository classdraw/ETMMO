#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;

namespace ET.Tilemap
{
    /// <summary>
    /// Tile Palette / Grid 绘制时，在 Scene 视图鼠标旁显示当前格子的整数 X、Y（取 WorldToCell 的格子坐标）。
    /// </summary>
    [InitializeOnLoad]
    internal static class TilePaletteCoordinateOverlay
    {
        private static GUIStyle s_LabelStyle;

        static TilePaletteCoordinateOverlay()
        {
            SceneView.duringSceneGui += OnDuringSceneGui;
        }

        private static void EnsureStyle()
        {
            if (s_LabelStyle != null)
            {
                return;
            }

            s_LabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.UpperLeft
            };
            s_LabelStyle.normal.textColor = Color.yellow;
        }

        /// <summary>
        /// 不依赖 GridPaintingState.gridLayout（部分 Unity / 包版本无此成员），从当前绘制目标解析 Grid。
        /// </summary>
        private static GridLayout GetActivePaintGridLayout()
        {
            GameObject paintTarget = GridPaintingState.scenePaintTarget;
            if (paintTarget == null)
            {
                return null;
            }

            Grid grid = paintTarget.GetComponentInParent<Grid>();
            return grid;
        }

        private static void OnDuringSceneGui(SceneView sceneView)
        {
            GridLayout gridLayout = GetActivePaintGridLayout();
            if (gridLayout == null)
            {
                return;
            }

            Event e = Event.current;
            if (e == null)
            {
                return;
            }

            if (e.type != EventType.Repaint && e.type != EventType.MouseMove && e.type != EventType.MouseDrag)
            {
                return;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Transform tr = gridLayout.transform;
            Plane plane = new Plane(tr.forward, tr.position);
            if (!plane.Raycast(ray, out float distance))
            {
                return;
            }

            Vector3 world = ray.GetPoint(distance);
            Vector3Int cell = gridLayout.WorldToCell(world);
            EnsureStyle();

            Handles.BeginGUI();
            try
            {
                Vector2 mp = e.mousePosition;
                Rect rect = new Rect(mp.x + 14f, mp.y + 10f, 220f, 24f);
                GUI.Label(rect, $"X: {cell.x}  Y: {cell.y}", s_LabelStyle);
            }
            finally
            {
                Handles.EndGUI();
            }

            if (e.type == EventType.MouseMove)
            {
                sceneView.Repaint();
            }
        }
    }
}
#endif
