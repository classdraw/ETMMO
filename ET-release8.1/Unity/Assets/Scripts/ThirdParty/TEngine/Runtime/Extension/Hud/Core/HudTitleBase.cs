using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XEngine.Hud{
    public class HudTitleBase
    {
        public Vector3 m_vWorldPos;//对象世界坐标
        public Vector2 m_vScreenPos;//屏幕坐标
        public float m_fScale=1.0f;//缩放
        public float m_fOffsetY;
        public BetterList<HudVertex> m_kSprites=new BetterList<HudVertex>();// 对应的图片顶点信息, 这个不要在外部设置
    


        public int m_iUUID = 0;

        public HudTitleBatcher m_kBatcher;//当前处理的批次
        public float m_fLastMoveTime = 0.0f; // 最后移动的时间
        public static UIFont GetHudTitleFont(){

            return HudBoardSetting.GetInstance().m_kFont;
        }

        protected void ClearSprite() {
            for (int i=m_kSprites.size-1 ; i>=0 ;i--) {
                HudVertex v = m_kSprites[i];
                if (v.m_kHudMesh!=null) {
                    v.m_kHudMesh.EraseHudVertex(v);
                }
                v.m_kHudMesh = null;
                HudVertex.ReleaseVertex(v);
                m_kSprites[i] = null;
            }

            m_kSprites.Clear();
        }

        //计算相机缩放
        public void CaleCameraScale (Vector3 cameraPos) {
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
            if (dis<=nearDistance) {
                dis = nearDistance;
            }
            if (dis>=farDistance) {
                dis = farDistance;
            }
            float ratio = Mathf.Clamp01((dis - nearDistance) / (farDistance - nearDistance));
            m_fScale = Mathf.Lerp(maxScale,minScale,ratio);
        }

        //改变屏幕坐标
        protected void OnChangeScreenPos(){
            for (int i = m_kSprites.size - 1; i >= 0; --i)
            {
                HudVertex v = m_kSprites[i];
                v.m_vScenePos = m_vScreenPos;
                v.m_fScale = m_fScale;
                v.m_vWorldPos = m_vWorldPos;
                if (v.m_kHudMesh != null)
                    v.m_kHudMesh.VertexDirty();
            }
        }
        //重新加载Mesh
        public void PrepareRebuildMesh(){
            for (int i = m_kSprites.size - 1; i >= 0; --i)
            {
                HudVertex v = m_kSprites[i];
                v.m_kHudMesh = null;
            }
        }

        protected void SetScale(float scale){
            for (int i = m_kSprites.size - 1; i >= 0; --i)
            {
                HudVertex v = m_kSprites[i];
                v.m_fScale = scale;
            }
        }

        protected void SliceFillNew(int spriteId, int width, int height, int start, float bloodLv) {
            UISpriteSimple sp = HudAtlasManager.GetInstance().GetSpriteSimpleByIndex(spriteId);
            if (sp == null)
            {
                return;
            }
            if (bloodLv < 0f)
            {
                bloodLv = 0f;
            }
            if (bloodLv > 1f)
            {
                bloodLv = 1f;
            }

            int bloodWidth = (int)(width * bloodLv + 0.5f);
            int atlasId = sp.m_iAtlasID;


        }

        //填充图片
        protected void SliceFill(int spriteId, int width, int height, int start, float bloodPos){
            UISpriteInfo sp=HudAtlasManager.GetInstance().GetSafeSpriteById(spriteId);
            if(sp==null){
                return;
            }
            if(bloodPos<0f){
                bloodPos=0f;
            }
            if(bloodPos>1f){
                bloodPos=1f;
            }
            int bloodWidth=(int)(width * bloodPos + 0.5f);
            int atlasId=sp.m_iAtlasID;
            Rect outUV=sp.m_rOuter;
            Rect innerUV=sp.m_rInner;
            //向上取整
            int outW=(int)(outUV.width+0.5f);
            int outH=(int)(outUV.height+0.5f);
            //边框
            int nW1 = (int)(innerUV.xMin - outUV.xMin + 0.5f);
            int nH1 = (int)(innerUV.yMin - outUV.yMin + 0.5f);
            int nW2 = (int)(outUV.xMax - innerUV.xMax + 0.5f);
            int nH2 = (int)(outUV.yMax - innerUV.yMax + 0.5f);
            UITextureInfo textureInfo=HudAtlasManager.GetInstance().GetTextureInfoById(atlasId);
            if(textureInfo!=null&&textureInfo.Coordinates==Coordinates.Pixels){
                outUV=HudVertex.ConvertToTexCoords(outUV,textureInfo.TexWidth,textureInfo.TexHeight);
                innerUV=HudVertex.ConvertToTexCoords(innerUV,textureInfo.TexWidth,textureInfo.TexHeight);
            }
            if (outW > 0 && nW1 + nW2 > bloodWidth)
            {
                nW1 = bloodWidth * nW1 / outW;
                nW2 = bloodWidth - nW1;
            }
            if (outH > 0 && nH1 + nH2 > height)
            {
                nH1 = height * nH1 / outH;
                nH2 = height - nH1;
            }

            int nMW = bloodWidth - nW1 - nW2;
            int nMH = height - nH1 - nH2;

            HudVertex v0 = m_kSprites[start];
            HudVertex v1 = m_kSprites[start + 1];
            HudVertex v2 = m_kSprites[start + 2];
            HudVertex v3 = m_kSprites[start + 3];
            HudVertex v4 = m_kSprites[start + 4];
            HudVertex v5 = m_kSprites[start + 5];
            HudVertex v6 = m_kSprites[start + 6];
            HudVertex v7 = m_kSprites[start + 7];
            HudVertex v8 = m_kSprites[start + 8];

            float fIn_xMin = innerUV.xMin;
            float fIn_xMax = innerUV.xMax;
            float fIn_yMin = innerUV.yMin;
            float fIn_yMax = innerUV.yMax;
            float fOu_xMin = outUV.xMin;
            float fOu_xMax = outUV.xMax;
            float fOu_yMin = outUV.yMin;
            float fOu_yMax = outUV.yMax;

            float fX2 = nW1 + nMW;
            float fY2 = nH2 + nMH;

            v0.SliceFill(nW1, nH2, 0f, 0f, fOu_xMin, fOu_yMin, fIn_xMin, fIn_yMin);
            v1.SliceFill(nMW, nH2, nW1, 0f, fIn_xMin, fOu_yMin, fIn_xMax, fIn_yMin);
            v2.SliceFill(nW2, nH2, fX2, 0f, fIn_xMax, fOu_yMin, fOu_xMax, fIn_yMin);

            v3.SliceFill(nW1, nMH, 0f, nH2, fOu_xMin, fIn_yMin, fIn_xMin, fIn_yMax);
            v4.SliceFill(nMW, nMH, nW1, nH2, fIn_xMin, fIn_yMin, fIn_xMax, fIn_yMax);
            v5.SliceFill(nW2, nMH, fX2, nH2, fIn_xMax, fIn_yMin, fOu_xMax, fIn_yMax);

            v6.SliceFill(nW1, nH1, 0f, fY2, fOu_xMin, fIn_yMax, fIn_xMin, fOu_yMax);
            v7.SliceFill(nMW, nH1, nW1, fY2, fIn_xMin, fIn_yMax, fIn_xMax, fOu_yMax);
            v8.SliceFill(nW2, nH1, fX2, fY2, fIn_xMax, fIn_yMax, fOu_xMax, fOu_yMax);

            v0.m_fScale = m_fScale;
            v1.m_fScale = m_fScale;
            v2.m_fScale = m_fScale;
            v3.m_fScale = m_fScale;
            v4.m_fScale = m_fScale;
            v5.m_fScale = m_fScale;
            v6.m_fScale = m_fScale;
            v7.m_fScale = m_fScale;
            v8.m_fScale = m_fScale;
        }

        
        protected HudVertex PushSprite(int spriteIndex, int width, int height, float fx, float fy){
            HudVertex node = HudVertex.QueryVertex();
            node.m_vWorldPos = m_vWorldPos;
            node.m_vScenePos = m_vScreenPos;
            node.m_iSpriteIndex = spriteIndex;
            node.m_vOffset.Set(fx, fy);
            node.m_vMove.Set(0f, 0f);
            node.InitSprite(width, height);
            node.m_fScale = m_fScale;

            m_kSprites.Add(node);
            return node;
        }

        protected HudVertex PushSliceTitleNew(int spriteIndex, int width, int height, float fx, float fy, float fBloodPos)
        {
            HudVertex node = HudVertex.QueryVertex();
            node.m_vWorldPos = m_vWorldPos;
            node.m_vScenePos = m_vScreenPos;
            node.m_iSpriteIndex = spriteIndex;
            node.m_vOffset.Set(fx, fy);
            node.m_vMove.Set(0f, 0f);
            node.InitSprite((int)(width* fBloodPos), height);
            node.m_fScale = m_fScale;

            m_kSprites.Add(node);
            return node;
        }
        protected HudVertex SliceTitleNew(int spriteIndex, int width, int height,int start, float fBloodPos)
        {
            HudVertex node = m_kSprites[start];
            node.InitSprite((int)(width * fBloodPos), height);
            node.m_fScale = m_fScale;
            if(node.m_kHudMesh!=null)
                node.m_kHudMesh.VertexDirty();
            return node;
        }
        protected void PushSliceTitle(int spriteIndex, int width, int height, float fx, float fy, float fBloodPos)
        {
            int start = m_kSprites.size;
            for (int i = 0; i < 9; ++i)
            {
                HudVertex node = HudVertex.QueryVertex();
                node.m_vWorldPos = m_vWorldPos;
                node.m_vScenePos = m_vScreenPos;
                node.m_iSpriteIndex = spriteIndex;
                node.m_iAtlasID = -1;
                node.m_fScale = m_fScale;
                node.m_vOffset.Set(fx, fy);
                node.m_vMove.Set(0f, 0f);
                node.m_iWidth = (short)width;
                node.m_iHeight = (short)height;
                m_kSprites.Add(node);
            }
            SliceFill(spriteIndex, width, height, start, fBloodPos);
        }

        protected HudVertex PushChar(ref CharacterInfo characterInfo, char ch, float fx, float fy, Color32 clrLeftUp, Color32 clrLeftDown, Color32 clrRightUp, Color32 clrRightDown){
            HudVertex node = HudVertex.QueryVertex();
            node.m_vWorldPos = m_vWorldPos;
            node.m_vScenePos = m_vScreenPos;
            node.m_cChar = ch;
            node.m_vOffset.Set(fx, fy);
            node.m_vMove.Set(0f, 0f);
            node.m_vColorLU = clrLeftUp;
            node.m_vColorLD = clrLeftDown;
            node.m_vColorRD = clrRightDown;
            node.m_vColorRU = clrRightUp;
            node.InitChar(characterInfo);
            node.m_fScale = m_fScale;
            m_kSprites.Add(node);

            return node;
        }

        protected HudVertex PushShadow(ref CharacterInfo tempCharInfo, char ch, float fx, float fy, Color32 clrShadow, float fMoveX, float fMoveY)
        {
            HudVertex node = HudVertex.QueryVertex();
            node.m_vWorldPos = m_vWorldPos;
            node.m_vScenePos = m_vScreenPos;
            node.m_cChar = ch;
            node.m_vOffset.Set(fx, fy);
            node.m_vMove.Set(fMoveX, fMoveY);
            node.m_vColorLU = clrShadow;
            node.m_vColorLD = clrShadow;
            node.m_vColorRD = clrShadow;
            node.m_vColorRU = clrShadow;
            node.InitChar(tempCharInfo);
            node.m_fScale = m_fScale;
            m_kSprites.Add(node);
            return node;
        }

        protected void OffsetXY(int nStart, int nEnd, float fOffsetX, float fOffsetY)
        {
            for (int i = nStart; i < nEnd; ++i)
            {
                HudVertex v = m_kSprites[i];
                v.m_vOffset.x += fOffsetX;
                v.m_vOffset.y += fOffsetY - v.m_iHeight * 0.5f;
            }
        }

        protected void Offset(int nStart, int nEnd, float fOffsetX, float fOffsetY)
        {
            for (int i = nStart; i < nEnd; ++i)
            {
                HudVertex v = m_kSprites[i];
                v.m_vOffset.x += fOffsetX;
                v.m_vOffset.y += fOffsetY;
            }
        }

        // 功能：下对齐
        // 参数：nStart, nEnd - 开始与结束的位置
        //       fHeight - 高度
        protected void AlignDown(int nStart, int nEnd, float fOffsetX, float fHeight)
        {
            for (int i = nStart; i < nEnd; ++i)
            {
                HudVertex v = m_kSprites[i];
                v.m_vOffset.x += fOffsetX;
                v.m_vOffset.y += fHeight - v.m_iHeight;
            }
        }
    }
}
