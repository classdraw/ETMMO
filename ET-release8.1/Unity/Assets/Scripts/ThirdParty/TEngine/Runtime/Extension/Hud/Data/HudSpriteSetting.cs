using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XEngine.Utilities;

namespace XEngine.Hud {
    public class HudSpriteSetting : MonoBehaviour
    {
        public int m_iHeadSpriteIndex=-1;//开头的图片
        public int m_iAddSpriteIndex = -1;//+号
        public int m_iSubSpriteIndex = -1;//-号
        public int[] m_iNumbers = new int[10];//数字的id
    }

}
