using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class RoleTextureComponent : Entity, IAwake, IDestroy
    {
        public string RoleTextureFolderPath = "Assets/Bundles/ScriptObject";
        public string RoleTextureFilePrefix = "FrameRoleTexture_";

        public Dictionary<FrameRolePartType, FrameRoleTextureConfig> RoleTextureConfigs = new();
    }
}
