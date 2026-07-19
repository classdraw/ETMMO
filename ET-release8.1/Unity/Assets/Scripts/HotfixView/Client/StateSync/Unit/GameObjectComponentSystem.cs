using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(GameObjectComponent))]
    [FriendOf(typeof(GameObjectComponent))]
    [FriendOf(typeof(ReUseComponent))]
    public static partial class GameObjectComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this GameObjectComponent self)
        {
            if (self.GameObject == null)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            ReUseComponent reUseComponent = unit.GetComponent<ReUseComponent>();
            if (reUseComponent != null && !string.IsNullOrEmpty(reUseComponent.PoolKey))
            {
                EffectHelper.ReturnEffect(unit.Scene(), reUseComponent.PoolKey, self.GameObject);
            }
            else
            {
                UnityEngine.Object.Destroy(self.GameObject);
            }

            self.ReleaseGameObject();
        }

        [EntitySystem]
        private static void Awake(this GameObjectComponent self)
        {
        }
    }
}
