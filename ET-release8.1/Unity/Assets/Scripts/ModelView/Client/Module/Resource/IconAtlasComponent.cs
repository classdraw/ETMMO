using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI 通用图标图集（物品、技能等）：挂在 <see cref="UI"/> 上；
    /// 关界面时销毁组件级 Sprite 名字缓存，YooAsset 图集句柄由 <see cref="ResourcesLoaderComponent"/> 复用。
    /// </summary>
    [ComponentOf(typeof(UI))]
    public class IconAtlasComponent : Entity, IAwake, IDestroy
    {
        public const string AtlasLocation = "Assets/Bundles/Atlas/IconAtlas.spriteatlasv2";
        public const string AtlasShortLocation = "IconAtlas.spriteatlasv2";
        public const string SingleSpriteFolder = "Assets/Res/UI/Icons_Atlas";
        public const string LogTag = "IconAtlas";

        public Dictionary<string, Sprite> SpriteCache;
        public bool SpritesIndexed;
        public bool Indexing;
        public bool LoadFailed;
    }
}
