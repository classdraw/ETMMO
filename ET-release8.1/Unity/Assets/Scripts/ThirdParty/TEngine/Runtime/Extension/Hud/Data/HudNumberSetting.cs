using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XEngine.Hud {

    public class HudNumberData {
        public int m_iFirstSpriteIndex = -1;//开头的图片
        public int m_iAddSpriteIndex = -1;//+号
        public int m_iSubSpriteIndex = -1;//-号
        public int[] m_iNumbers = new int[10];//数字的id 0~9
        public Vector2 m_OffsetFirstSprite;

        public void Init(string startNumStr, string addStr, string subStr, string firstStr)
        {
            Init(startNumStr, addStr, subStr, firstStr, Vector2.zero);
        }

        public void Init(string startNumStr, string addStr, string subStr, string firstStr, Vector2 offsetFirstSprite) {
            m_iFirstSpriteIndex = -1;
            m_iAddSpriteIndex = -1;
            m_iSubSpriteIndex = -1;
            m_OffsetFirstSprite = offsetFirstSprite;

            if (!string.IsNullOrEmpty(startNumStr)) {
                for (int i=0; i<=9;i++) {
                    string numStr = startNumStr + i;

                    int index = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetSpriteIndexByName(numStr);
                    m_iNumbers[i] = index;
                    //Debug.Log(index);
                
                }
            }

            if (!string.IsNullOrEmpty(addStr)) {
                int index = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetSpriteIndexByName(addStr);
                m_iAddSpriteIndex = index;
            }

            if (!string.IsNullOrEmpty(subStr))
            {
                int index = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetSpriteIndexByName(subStr);
                m_iSubSpriteIndex = index;
            }

            if (!string.IsNullOrEmpty(firstStr))
            {
                int index = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetSpriteIndexByName(firstStr);
                m_iFirstSpriteIndex = index;
            }
        }
    }
}
