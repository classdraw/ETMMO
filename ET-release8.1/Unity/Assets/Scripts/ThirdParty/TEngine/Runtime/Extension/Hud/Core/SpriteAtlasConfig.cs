using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace XEngine.Hud
{

    [CreateAssetMenu(fileName = "SpriteAtlasConfig", menuName = "Scriptable Objects/SpriteAtlasConfig", order = 4)]
    public class SpriteAtlasConfig : ScriptableObject
    {
        public string m_sHpBg;//血量背景
        public string m_sHpGreen;//血量绿色
        public string m_sHpBlue;//血蓝色
        public string m_sHpRed;//血红色


        public int m_iWidth;
        public int m_iHeight;
        public Texture2D m_kTexture;
        public RenderPassEvent m_eRenderPassEvent= RenderPassEvent.AfterRenderingTransparents;
        public List<SpriteInfo> m_kSprites=new List<SpriteInfo>();

        public int GetSpriteIndexByName(string name) {
            foreach(var val in m_kSprites) {
                if (val.m_sSpriteName.Equals(name)) {
                    return val.m_iIndex;
                }
            }

            return -1;
        }
        #region 快速获取某个图片id
        public int m_iHpBgId=-1;
        public int GetHpBgId() {
            if (m_iHpBgId>=0) { 
                return m_iHpBgId;
            }
            m_iHpBgId = GetSpriteIndexByName(m_sHpBg);
            return m_iHpBgId;
        }
        public int m_iHpRedId = -1;
        public int GetHpRedId()
        {
            if (m_iHpRedId >= 0)
            {
                return m_iHpRedId;
            }
            m_iHpRedId = GetSpriteIndexByName(m_sHpRed);
            return m_iHpRedId;
        }

        public int m_iHpGreenId = -1;
        public int GetHpGreenId()
        {
            if (m_iHpGreenId >= 0)
            {
                return m_iHpGreenId;
            }
            m_iHpGreenId = GetSpriteIndexByName(m_sHpGreen);
            return m_iHpGreenId;
        }

        public int m_iHpBlueId = -1;
        public int GetHpBlueId()
        {
            if (m_iHpBlueId >= 0)
            {
                return m_iHpBlueId;
            }
            m_iHpBlueId = GetSpriteIndexByName(m_sHpBlue);
            return m_iHpBlueId;
        }
        #endregion
    }

    [Serializable]
    public class SpriteInfo {
        public int m_iIndex;
        public string m_sSpriteName;
        public Rect m_kRect;
        public Vector4 m_kUV;
        //  public Vector4 m_kUVLD;//���½�  xy��x  zw��y
        //   public Vector4 m_kUVRU;//���Ͻ� xy��x  zw��y
        public Vector4 m_kBorder;//Ŀǰû�õ�


    }
}

