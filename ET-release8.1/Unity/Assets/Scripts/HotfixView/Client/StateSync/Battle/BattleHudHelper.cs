using UnityEngine;
using XEngine.Hud;

namespace ET.Client
{
    public static class BattleHudHelper
    {
        public static void ShowBattleResultNumber(Unit target, long damage, bool isCrit)
        {
            if (target == null || target.IsDisposed || damage == 0)
            {
                return;
            }

            GameObjectComponent gameObjectComponent = target.GetComponent<GameObjectComponent>();
            Transform transform = gameObjectComponent?.GameObject?.transform;
            if (transform == null)
            {
                return;
            }

            Enum_NumberRender_Type renderType;
            if (damage < 0)
            {
                renderType = isCrit ? Enum_NumberRender_Type.HUD_SHOW_HP_Crit : Enum_NumberRender_Type.HUD_SHOW_HP_HURT;
            }
            else
            {
                renderType = Enum_NumberRender_Type.HUD_SHOW_HP_ADD;
            }

            HudNumberRender.GetInstance().ShowHurtNumber(transform, renderType, (int)damage, true);
        }
    }
}
