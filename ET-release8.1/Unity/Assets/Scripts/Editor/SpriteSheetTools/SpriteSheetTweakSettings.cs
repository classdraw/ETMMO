using UnityEngine;

namespace ET
{
    /// <summary>
    /// 序列帧微调页的编辑器设置：图集、行列、每帧偏移、目标尺寸、预览锚点。
    /// </summary>
    public class SpriteSheetTweakSettings : ScriptableObject
    {
        public Texture2D spriteSheet;
        public int rows = 1;
        public int columns = 1;
        public int targetWidth;
        public int targetHeight;
        public Vector2[] offsets;
        public Vector2 anchor;
    }
}
