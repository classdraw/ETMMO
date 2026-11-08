using UnityEngine;

namespace ET
{
    /// <summary>
    /// Synty 模块化部件 Prefab：保存骨骼名，运行时绑定到角色 Root 骨架。
    /// </summary>
    [DisallowMultipleComponent]
    public class SyntySkinnedPartMetadata : MonoBehaviour
    {
        public string rootBoneName = "Hips";
        public string[] boneNames;
        public string slotPath;
    }
}
