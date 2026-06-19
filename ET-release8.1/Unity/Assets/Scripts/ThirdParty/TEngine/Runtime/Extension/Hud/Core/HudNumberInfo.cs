using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XEngine.Hud {
    public class HudNumberInfo
    {
        public Transform m_kTran;
        public HudNumberInfo m_kNext;
        public Enum_NumberRender_Type m_eNumberRenderType;
        public Vector3 m_vWorldPos;//世界坐标
        public Vector2 m_vScreenPos; // 屏幕坐标
        public Vector2 m_vInitOffset; // 初始化位移
        public Vector2 m_vMove;  // 位移
        public float m_fAniScale;//动画缩放
        public float m_fScale;//计算后缩放的值
        public float m_fAlpha;

        public float m_fRotate;//当前旋转
        public int m_iRotateIndex = -1;

        public BetterList<HudVertex> m_kSprites=new BetterList<HudVertex>();
        public int m_iWidth = 0;
        public int m_iHeight=0;
        public int m_iSpriteGap=0;
        public float m_fStartTime = 0.0f;
        public bool m_bStop = false;
        public Color m_kColor = Color.white;


        public void Reset() {
            m_kTran = null;
            m_vWorldPos = Vector3.zero;
            m_vInitOffset = Vector3.zero;

            m_vMove = Vector3.zero;
            m_fScale = 1.0f;
            m_fAniScale = 1.0f;
            m_fAlpha = 1.0f;
            
            m_iWidth= 0;
            m_iHeight= 0;
            m_iSpriteGap= 0;
            m_bStop= false;
            m_fRotate= 0.0f;
            m_iRotateIndex = -1;
            m_kColor = Color.white;
           // m_kNumberRenderType=Hudnum
        }

        public void ApplyVertexColor()
        {
            Color32 c = m_kColor;
            for (int i = m_kSprites.size - 1; i >= 0; --i)
            {
                HudVertex v = m_kSprites[i];
                v.m_vColorLU = c;
                v.m_vColorLD = c;
                v.m_vColorRU = c;
                v.m_vColorRD = c;
            }
        }

        public void ReleaseVertex() {
            for (int i= m_kSprites.size - 1; i>=0 ;i--) { 
                HudVertex hudVertex=m_kSprites[i];
                if (hudVertex.m_kHudMesh!=null) {
                    hudVertex.m_kHudMesh.EraseHudVertex(hudVertex);
                }
                hudVertex.m_kHudMesh = null;
                HudVertex.ReleaseVertex(hudVertex);
                m_kSprites[i] = null;
            }

            m_kSprites.Clear();
        }

        //更新屏幕位置
        public bool UpdateScreenPos(ref HudAnimeAttribute attribute) {
            if (attribute.ScreenAlign) {
                return false;
            }
            if (m_kTran != null)
            {
                Vector3 vPos = m_kTran.position;
                Vector3 v = vPos - m_vWorldPos;
                if (v.x * v.x + v.y * v.y + v.z * v.z > 0.00001f)//有变化
                {
                    Camera caMain = HudTitleRender.GetInstance().GetMainCamera();
                    if (caMain != null)
                    {
                        m_vWorldPos = vPos;
                        m_vScreenPos = caMain.WorldToScreenPoint(vPos);
                        Vector3 vCameraPos = caMain.transform.position;
                        CaleCameraScale(vCameraPos);
                        return true;
                    }
                }
            }
            return false;
        }

        //计算相机缩放
        public void CaleCameraScale(Vector3 cameraPos)
        {
            /*var pos=m_vWorldPos;
            float nearDistance=HudBoardSetting.GetInstance().CameraNearDist;
            float farDistance=HudBoardSetting.GetInstance().CameraFarDist;
            float minScale=HudBoardSetting.GetInstance().m_fTitleScaleMin;
            float maxScale=HudBoardSetting.GetInstance().m_fTitleScaleMax;
            float dis=Vector3.Distance(m_vWorldPos,cameraPos);//自己坐标和相机的距离
            float ratio=Mathf.Clamp01((dis - nearDistance) / (farDistance - nearDistance));//比例
            float scale=minScale * ratio + (1.0f - ratio) * maxScale;//近到远差值
            m_fScale=1f/scale;


            Debug.LogError(nearDistance+"__"+farDistance+"  "+minScale+">>"+maxScale+"___"+ratio);*/

            var pos = m_vWorldPos;
            float nearDistance = HudBoardSetting.GetInstance().CameraNearDist;
            float farDistance = HudBoardSetting.GetInstance().CameraFarDist;
            float minScale = HudBoardSetting.GetInstance().m_fTitleScaleMin;
            float maxScale = HudBoardSetting.GetInstance().m_fTitleScaleMax;
            float dis = Vector3.Distance(m_vWorldPos, cameraPos);//自己坐标和相机的距离
            if (dis <= nearDistance)
            {
                dis = nearDistance;
            }
            if (dis >= farDistance)
            {
                dis = farDistance;
            }
            float ratio = Mathf.Clamp01((dis - nearDistance) / (farDistance - nearDistance));
            m_fScale = Mathf.Lerp(maxScale, minScale, ratio);
        }

        public void PushSprite(float y,int spriteIndex) {
            HudVertex node = HudVertex.QueryVertex();
            node.m_vWorldPos = m_vWorldPos;
            node.m_vScenePos = m_vScreenPos;
            node.m_vOffset.Set(m_iWidth,y);
            node.m_iSpriteIndex = spriteIndex;
            node.InitSprite();
            m_kSprites.Add(node);
            m_iWidth += node.m_iWidth + m_iSpriteGap;
            if (m_iHeight<node.m_iHeight) { 
                m_iHeight = node.m_iHeight;
            }

        }

        public void MakeLeft() {
            m_iWidth -= m_iSpriteGap;
            MoveAll(0f);
        }

        public void MakeCenter()
        {
            m_iWidth -= m_iSpriteGap;
            float fHalfW = m_iWidth * 0.5f;
            MoveAll(fHalfW);
        }

        public void MakeRight()
        {
            m_iWidth -= m_iSpriteGap;
            float fMoveX = m_iWidth;
            MoveAll(fMoveX);
        }
        //所有顶点偏移
        public void MoveAll(float moveX) {
            float halfH = m_iHeight * 0.5f;
            int size = m_kSprites.size;
            for (int i=0;i<=size-1 ;i++) {
                float fh = m_kSprites[i].m_iHeight;
                m_kSprites[i].m_vOffset.x -= moveX;
                m_kSprites[i].m_vOffset.y -= fh * 0.5f - halfH;

            }
        }
    }

}
