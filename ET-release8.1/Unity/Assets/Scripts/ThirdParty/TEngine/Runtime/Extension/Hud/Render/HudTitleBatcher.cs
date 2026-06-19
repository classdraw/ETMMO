using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XEngine.Hud {
    //Hud批次处理
    public class HudTitleBatcher 
    {
        public BetterList<HudTitleInfo> m_kValidTitles=new BetterList<HudTitleInfo>();
        public HudRender m_kMeshRender=new HudRender();

        public bool m_bNeedSort = false;
        public bool m_bMove = false;
        public bool m_bStatic = false;
        private bool m_bRebuildMesh = false;
        private bool m_bHaveNullTitle = false;
        private int m_iSortVersion = 0;
        private int m_iMaxSortCount = 0;

        //按照相机距离排序
        private void SortByDis() {
            m_bNeedSort = false;
            bool change = true;
            HudTitleInfo[] buffer = m_kValidTitles.buffer;
            int size = m_kValidTitles.size;
            HudTitleInfo temp = null;
            int changeCount = 0;
            while (change) {
                change = false;
                for (int i=1;i<size ;i++) {
                    if (buffer[i - 1].m_fDistToCamera < buffer[i].m_fDistToCamera) { 
                        temp= buffer[i];
                        buffer[i]=buffer[i - 1];
                        buffer[i-1]=temp;
                        change= true;
                        changeCount++;
                    }
                }
            
            }
            if (changeCount>0) {
                if (m_iMaxSortCount<changeCount) { 
                    m_iMaxSortCount=changeCount;
                }
                //改过顺序后按照距离排序
                for (int i=m_kValidTitles.size-1;i>=0 ;i--) { 
                    HudTitleInfo title=m_kValidTitles[i];
                    if (title.m_iBatcherIndex<i) { 
                        m_bRebuildMesh = true;
                    }
                    title.m_iBatcherIndex=i;
                }


            }
        }

        private void PrepareRebuild() {
            m_kMeshRender.FastClearVertex();
            int size = m_kValidTitles.size;
            for (int i=0; i<size;i++) { 
                HudTitleInfo titleInfo = m_kValidTitles[i];
                titleInfo.m_bInitHudMesh = false;
                titleInfo.PrepareRebuildMesh();
            }
        }
        private void InitTitleHudMesh(HudTitleInfo title) {
            //初始化mesh
            if (!title.m_bInitHudMesh&&!title.m_bNeedHide) {
                title.m_bInitHudMesh = true;
                int size = title.m_kSprites.size;
                for (int i=0; i<size;i++) { 
                    HudVertex v=title.m_kSprites[i];
                    if (v.m_kHudMesh==null) {
                        if (v.m_iAtlasID != 0)//ͼƬ
                        {
                            v.m_kHudMesh=m_kMeshRender.QueryMesh(v.m_iAtlasID);
                        }
                        else { //字体mesh
                            v.m_kHudMesh = m_kMeshRender.FontMesh();
                        }
                        v.m_kHudMesh.PushHudVertex(v);
                    }
                }//for

            }
        }

        public void UpdateLogic(bool cameraDirty,Vector3 cameraPos) {
            if (m_bHaveNullTitle) {//清除null节点
                m_bHaveNullTitle = false;
                m_kValidTitles.ClearNullItem();
            }

            //更新位置信息
            int size = m_kValidTitles.size;
            for (int i=size-1;i>=0 ;i--) { 
                HudTitleInfo title = m_kValidTitles[i];
                title.m_iBatcherIndex = i;
                if (title.m_kTran!=null) {
                    title.ApplyMove(cameraDirty,cameraPos);//相机移动 自身动态计算 会改变move 如果自身有移动脏数据
                }
            }

            if (m_bMove) { 
                m_bMove = false;
                m_iSortVersion++;
            }

            if (m_bNeedSort||m_iSortVersion>10) {
                bool needSort = m_bNeedSort;
                int sortVersion = m_iSortVersion;
                m_bMove = false;
                m_bNeedSort=false;
                m_iSortVersion = 0;

                SortByDis();
                if (m_bRebuildMesh) {
                    //if(m_bStatic)
                    //    Debug.LogError("Static Need PrepareRebuild, NeedSort=" + needSort + ", Version=" + sortVersion);
                    //else
                    //     Debug.LogError("Dynamic Need PrepareRebuild, NeedSort=" + needSort + ", Version=" + sortVersion);
                    m_bRebuildMesh = false;
                    PrepareRebuild();
                }
            }
            size=m_kValidTitles.size;
            for (int i=0; i<size;i++) { 
                HudTitleInfo titleInfo = m_kValidTitles[i];
                titleInfo.m_iBatcherIndex = i;
                if (!titleInfo.m_bNeedHide&&!titleInfo.m_bInitHudMesh) {//需要显示 并且mesh没有生成 
                    InitTitleHudMesh(titleInfo);
                }
            }

            m_kMeshRender.FillMesh();
        }


        public void PushTitle(HudTitleInfo title) {
            title.m_iBatcherIndex = m_kValidTitles.size;
            m_kValidTitles.Add(title);
            m_bNeedSort = true;
        }

        public void SwitchPushTitle(HudTitleInfo title) { 
            title.m_iBatcherIndex=m_kValidTitles.size;
            m_kValidTitles.Add(title);
            m_bNeedSort = true;
            InitTitleHudMesh(title);
        }
        public void EraseTitle(HudTitleInfo title) {
            int index = title.m_iBatcherIndex;
            title.EraseSpriteFromMesh();
            if (index>=0&&index<m_kValidTitles.size) {
                if (m_kValidTitles[index] != null && m_kValidTitles[index]==title) { 
                    m_bHaveNullTitle = true;//移除这个节点
                    m_kValidTitles[index]=null;
                    return;
                }
            }

            for (int i=m_kValidTitles.size-1; i>=0;i--) {
                if (m_kValidTitles[i] != null && m_kValidTitles[index]==title) { 
                    m_bHaveNullTitle=true;//移除这个节点
                    m_kValidTitles[index] = null;
                    return;
                }
            }
        }
    }
}
