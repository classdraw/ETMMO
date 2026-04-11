using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// ReferenceCollector「角色绑点收集」允许的 key 白名单（工程内资源，可入库版本管理）。
    /// </summary>
    [CreateAssetMenu(fileName = "AvatarBindKeyAllowlist", menuName = "ET/角色绑点 Key 白名单", order = 500)]
    public class AvatarBindKeyAllowlistAsset : ScriptableObject
    {
        [Tooltip("角色绑点收集：仅当挂 SpriteRenderer 的物体名与本列表某项完全一致（区分大小写）时才会绑定，一项一行。")]
        public List<string> keys = new List<string>();

        [Tooltip("预制体处理工具窗口内白名单列表区域高度（像素）。")]
        [Range(100f, 520f)]
        public float editorListScrollHeight = 180f;
    }
}
