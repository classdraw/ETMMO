using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;


namespace ET.Client
{
    [EntitySystemOf(typeof(UIBagComponent))]
    [FriendOf(typeof(UIBagComponent))]
    public static partial class UIBagComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIBagComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this UIBagComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this UIBagComponent self)
        {
        }
    }
}