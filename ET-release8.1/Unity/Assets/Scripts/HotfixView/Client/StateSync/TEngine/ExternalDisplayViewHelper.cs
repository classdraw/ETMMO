using GameLogic;
using UnityEngine;

namespace ET.Client
{
    public static class ExternalDisplayViewHelper
    {
        public static void ApplyToUnit(Scene scene, Unit unit)
        {
            if (scene == null || unit == null || string.IsNullOrEmpty(unit.BaseExternalDisplay))
            {
                return;
            }

            if (!ExternalDisplayHelper.TryParseExternalDisplayString(unit.BaseExternalDisplay, out ExternalDisplayAppearance appearance))
            {
                return;
            }

            GameObjectComponent gameObjectComponent = unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent?.GameObject == null)
            {
                return;
            }

            RoleTextureComponent roleTex = scene.Root().GetComponent<RoleTextureComponent>();
            if (roleTex == null)
            {
                return;
            }

            FrameSheetAnimPlayer player = gameObjectComponent.GameObject.GetComponentInChildren<FrameSheetAnimPlayer>();
            if (player != null)
            {
                UILoginRolePreviewHelper.ApplyPreview(player, roleTex, appearance);
            }
        }
    }
}
