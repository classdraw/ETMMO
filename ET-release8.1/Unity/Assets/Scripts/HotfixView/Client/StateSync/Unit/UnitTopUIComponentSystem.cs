using UnityEngine;
using XEngine.Hud;

namespace ET.Client
{
    [EntitySystemOf(typeof(UnitTopUIComponent))]
    [FriendOf(typeof(UnitTopUIComponent))]
    public static partial class UnitTopUIComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UnitTopUIComponent self)
        {
            var gameObject = self.GetParent<Unit>().GetComponent<GameObjectComponent>().GameObject;
            self.HudInfoObj = new GameObject("HudInfoObj");
            self.HudInfoObj.transform.parent = gameObject.transform;
            self.HudInfoObj.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
            self.HudInfoScript=self.HudInfoObj.AddComponent<HudInfo>();

            var unit = self.GetParent<Unit>();
            if (unit.IsMonster())
            {
                self.HudInfoScript.DisplayMonster(unit.Name);
            }
            else
            {
                self.HudInfoScript.DisplayPlayer(unit.Name);
            }

            self.RefreshHpBar();
        }
        [EntitySystem]
        private static void Destroy(this UnitTopUIComponent self)
        {
            self.HudInfoScript?.Release();
            if (self.HudInfoObj!=null)
            {
                GameObject.Destroy(self.HudInfoObj);
            }
            self.HudInfoObj = null;
            self.HudInfoScript = null;
        }

        public static void RefreshHpBar(this UnitTopUIComponent self)
        {
            if (self == null || self.IsDisposed || self.HudInfoScript == null)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            self.HudInfoScript.RefreshHpLv(unit.GetHpLv());
        }
    }
}

