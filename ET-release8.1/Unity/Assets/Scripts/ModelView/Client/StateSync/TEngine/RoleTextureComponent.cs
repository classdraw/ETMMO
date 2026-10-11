using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class RoleTextureComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 是否从 YooAsset 加载 FrameRoleTexture_* 配置（创角/分部件换装）。资源未进包或未做外显时保持 false。
        /// </summary>
        public bool LoadConfigOnInit;

        public string RoleTextureFolderPath = "Assets/Bundles/ScriptObject";
        public string RoleTextureFilePrefix = "FrameRoleTexture_";

        public Dictionary<FrameRolePartType, FrameRoleTextureConfig> RoleTextureConfigs = new();
    }
}
