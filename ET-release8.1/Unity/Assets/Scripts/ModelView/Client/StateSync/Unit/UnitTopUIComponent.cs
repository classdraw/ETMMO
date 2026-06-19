using UnityEngine;
using XEngine.Hud;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class UnitTopUIComponent:Entity,IAwake,IDestroy
    {
        public GameObject HudInfoObj;
        public HudInfo HudInfoScript;
    }
}

