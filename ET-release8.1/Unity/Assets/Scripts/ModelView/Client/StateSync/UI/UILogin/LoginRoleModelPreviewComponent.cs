using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public enum LoginRolePreviewSlot
    {
        Left = 0,
        Right = 1,
    }

    /// <summary>
    /// 创角 UI 角色模型预览：按职业+性别缓存实例与已加载 prefab，不重复加载、不卸载。
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class LoginRoleModelPreviewComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<string, GameObject> PreviewInstances = new();
        public HashSet<string> LoadedPrefabPaths = new();
        public string LeftActiveKey;
        public string RightActiveKey;
    }
}
