using UnityEditor;
using UnityEngine;

namespace ET.Editor.Frame2D
{
    public static class FrameSheetAnimTypeEditorUtil
    {
        private static readonly int[] Ids =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
        };

        private static readonly string[] Labels =
        {
            "None",
            "Idle",
            "Stand",
            "Move",
            "Archery",
            "Cast",
            "Attack1",
            "Attack2",
            "Hit",
            "Death",
        };

        public static string GetName(int animTypeId)
        {
            for (int i = 0; i < Ids.Length; i++)
            {
                if (Ids[i] == animTypeId)
                {
                    return Labels[i];
                }
            }

            return animTypeId.ToString();
        }

        public static int DrawPopup(GUIContent label, int animTypeId)
        {
            int selectedIndex = GetIndex(animTypeId);
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, Labels);
            return Ids[selectedIndex];
        }

        public static int DrawPopup(int animTypeId)
        {
            return DrawPopup(GUIContent.none, animTypeId);
        }

        private static int GetIndex(int animTypeId)
        {
            for (int i = 0; i < Ids.Length; i++)
            {
                if (Ids[i] == animTypeId)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
