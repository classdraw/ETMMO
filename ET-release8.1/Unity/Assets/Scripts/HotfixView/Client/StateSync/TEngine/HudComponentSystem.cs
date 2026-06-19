using UnityEngine;
using XEngine.Hud;

namespace ET.Client
{
    [FriendOf(typeof(HudComponent))]
    [EntitySystemOf(typeof(HudComponent))]
    public static partial class HudComponentSystem
    {
        [EntitySystem]
        private static void Awake(this HudComponent self)
        {
            HudFacade.Instance.Build();
            HudFacade.Instance.EnterGame();
        }
        
        [EntitySystem]
        private static void Destroy(this HudComponent self)
        {
            HudFacade.Instance.LeaveGame();
        }
        
        

    }
}

