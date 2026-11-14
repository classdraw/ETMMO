using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 客户端会话级图标图集：挂在 Root <see cref="Scene"/>，<see cref="ClientPreloadHelper.PreloadOnStartupAsync"/> 索引后进游戏常驻；
    /// UI 通过 <see cref="IconAtlasComponentSystem.TryGetIconSprite"/> 读缓存即可。
    /// </summary>
    [ComponentOf(typeof(Scene))]
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
