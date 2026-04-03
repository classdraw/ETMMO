using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class TUIComponent: Entity, IAwake
    {
        // 常量定义
        public const int LAYER_DEEP = 2000; 
        public const int WINDOW_DEEP = 100;
        public const int WINDOW_HIDE_LAYER = 2; // Ignore Raycast
        public const int WINDOW_SHOW_LAYER = 5; // UI

        public string UIRootPath = $"Assets/Bundles/Tools/UIRoot.prefab";
        
        // 核心字段
        public Transform InstanceRoot = null;          // UI根节点变换组件

        public Camera UICamera;
        
        public Transform UIRoot;

        public List<EntityRef<TUIWindow>> Windows = new List<EntityRef<TUIWindow>>();
    }
}