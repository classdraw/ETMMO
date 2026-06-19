using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.Rendering;
using XEngine.Utilities;

namespace XEngine.Hud {
    public class HudRender
    {
        BetterList<HudMesh> m_kMeshList = new BetterList<HudMesh>();// 所有的
        BetterList<HudMesh> m_kValidList = new BetterList<HudMesh>();// 当前有效的
        HudMesh m_kMeshFont;
        HudMesh m_kCurFontMesh;
        public bool m_bMeshDirty;
        readonly bool m_bUseNumberShader;



        public HudRender(bool useNumberShader = false)
        {
            m_bUseNumberShader = useNumberShader;
        }

        public HudMesh QueryMesh(int atlasId) {

            //先从当前有效mesh里面找
            for (int i = m_kValidList.size - 1; i >= 0; i--) {
                if (m_kValidList[i].AtlasId == atlasId) {
                    return m_kValidList[i];
                }
            }
            //从所有的里面找
            for (int i = m_kMeshList.size - 1; i >= 0; i--) {
                if (m_kMeshList[i].AtlasId == atlasId) {//丢入有效的里面
                    m_kValidList.Add(m_kMeshList[i]);
                    m_kMeshList[i].SetAtlasId(atlasId, m_bUseNumberShader);
                    m_bMeshDirty = true;
                    return m_kMeshList[i];
                }
            }
            //都没有找到新建一个
            HudMesh hudMesh = new HudMesh();
            hudMesh.SetAtlasId(atlasId, m_bUseNumberShader);
            m_kMeshList.Add(hudMesh);
            m_kValidList.Add(hudMesh);
            m_bMeshDirty = true;
            return hudMesh;
        }
        //独立的字体mesh
        public HudMesh FontMesh()
        {
            if (m_kCurFontMesh != null)
                return m_kCurFontMesh;
            if (m_kMeshFont == null)
            {
                m_kMeshFont = new HudMesh();
                m_kMeshList.Add(m_kMeshFont);
            }
            m_kCurFontMesh = m_kMeshFont;
            m_kValidList.Add(m_kMeshFont);
            m_bMeshDirty = true;

            UIFont uiFont = HudTitleBase.GetHudTitleFont();
            m_kMeshFont.SetFont(uiFont);
            return m_kMeshFont;
        }

        public void OnChangeFont(UIFont uiFont) {
            if (m_kMeshFont != null) {
                m_kMeshFont.SetFont(uiFont);
            }

        }

        public void Release() {
            for (int i = 0; i < m_kMeshList.size; ++i)
            {
                m_kMeshList[i].Release();
                m_kMeshList[i] = null;
            }
            m_kMeshFont = null;
            m_kCurFontMesh = null;
            m_kMeshList.Clear();
            m_kValidList.Clear();

        }

        // 功能：快速清模型顶点
        public void FastClearVertex() {
            m_kCurFontMesh = null;
            for (int i = m_kValidList.size - 1; i >= 0; i--) {
                HudMesh mesh = m_kValidList[i];
                mesh.FastClearVertex();
            }
            m_kValidList.Clear();

        }

        //功能：更新模型顶点(没帧更新)
        public void FillMesh() {
            for (int i = m_kValidList.size - 1; i >= 0; i--) {
                HudMesh mesh = m_kValidList[i];
                if (mesh.IsDirty()) {
                    int nOldSpriteNumb = mesh.OldSpriteNumb;
                    mesh.UpdateLogic();
                    int nCurSpriteNumb = mesh.SpriteNumb;
                    if (nOldSpriteNumb != 0 && nCurSpriteNumb == 0)
                        m_bMeshDirty = true;
                    else if (nOldSpriteNumb == 0 && nCurSpriteNumb != 0)
                        m_bMeshDirty = true;
                    if (nCurSpriteNumb == 0)
                    {
                        m_kValidList.RemoveAt(i);
                        if (m_kMeshFont == mesh)
                        {
                            m_kCurFontMesh = null;
                        }
                        else
                        {
                            mesh.ClearAllVertex();
                        }
                    }
                }
            }
        }

        public void OnCancelRender() {
            m_bMeshDirty = false;
        }
        public void LogInfo(){
            int spriteCount = 0;
            int fontCount = 0;
            foreach (var kvp in m_kValidList) {
                if (kvp.SpriteNumb > 0 && kvp.AtlasId != 0) {
                    Debug.LogError("__" + kvp.m_kVerts.size);
                    spriteCount++;
                } else if (kvp.SpriteNumb > 0 && kvp.AtlasId == 0) {
                    fontCount++;
                }
            }
            Log.Debug(spriteCount + "___"+ fontCount);
        }
        public void RenderTo(CommandBuffer cmdBuffer){
            m_bMeshDirty=false;
            if(m_kValidList.size==0){return;}
            Matrix4x4 matWorld=Matrix4x4.identity;
            for (int i = 0, nSize = m_kValidList.size; i < nSize; ++i)
            {
                HudMesh mesh = m_kValidList[i];
                if (mesh.SpriteNumb > 0 && mesh.AtlasId != 0)
                {
                    cmdBuffer.DrawMesh(mesh.m_kMesh, matWorld, mesh.m_kMaterial);
                }
            }

            for (int i = 0, nSize = m_kValidList.size; i < nSize; ++i)
            {
                HudMesh mesh = m_kValidList[i];
                if (mesh.SpriteNumb > 0 && mesh.AtlasId == 0)
                {
                    cmdBuffer.DrawMesh(mesh.m_kMesh, matWorld, mesh.m_kMaterial);
                }
            }
        }
    }
}
