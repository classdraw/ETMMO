using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEngine.Hud;

namespace XEngine.Hud {
    public class HudObject : MonoBehaviour
    {
        [SerializeField]
        public SpriteAtlasConfig m_kConfig;

        [SerializeField]
        public HudAniSetting m_kHudAniSetting;

        [SerializeField]
        public UIFont m_kFont;

    }
}

