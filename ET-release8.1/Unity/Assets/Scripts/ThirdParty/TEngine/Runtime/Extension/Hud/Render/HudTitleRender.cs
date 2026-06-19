using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XEngine.Utilities;

namespace XEngine.Hud {
    public class HudTitleRender : Singleton<HudTitleRender>
    {
        //所有注册好的titleInfo
        Dictionary<int,HudTitleInfo> m_kHudTitles=new Dictionary<int, HudTitleInfo> ();
        BetterList<int> m_kDelayReleaseTitles=new BetterList<int> ();//准备销毁的

        private int m_iUUID=0;//唯一id 不断递增

        Camera m_kRenderCamera;
        Camera m_kOldCamera;
        CommandBuffer m_kCmdBuffer;
        bool m_bOpenUI = false; // NPC对话状态
        private bool m_bMeshDirty = false;

        private Transform m_kTranMain;//主角实体

        bool m_bInitFontCallback = false;

        private int m_iUpdateVersion = 0;//更新的版本
        private int m_iBaseUpdateVersion = 0;//更新到的版本
        private int m_iCameraUpdateVersion = 0;//相机更新的版本

        public int GetNextUUID() { 
            return ++m_iUUID;
        }

        private HudTitleBatcher m_kStaticBatcher=new HudTitleBatcher();//不动的 自己添加不动的
        private HudTitleBatcher m_kDynamicBatcher=new HudTitleBatcher();//会动的

        private Vector3 m_vLastCameraPos = Vector3.zero;//相机坐标保存
        private Vector3 m_vLastEulerAngles=Vector3.zero;//相机旋转保存
        private float m_fLastCheckMoveTime = 0;
        private bool m_bHideAllTitle = false;
        public bool m_bStart = false;
        public void Build()
        {
            m_kStaticBatcher.m_bStatic = true;
            m_kDynamicBatcher.m_bStatic = false;
            if (!m_bInitFontCallback) {
                m_bInitFontCallback = true;
                //var font=HudTitleInfo.GetHudTitleFont();
                //Font.textureRebuilt += OnAllFontChanged;
            }
            Log.Debug("HudTitleRender Init Success!!!");
        }

        public CommandBuffer GetCmdBuffer() { 
            return m_kCmdBuffer;
        }

        private void ReleaseCmdBuffer() {
            if (m_kCmdBuffer!=null) {
                if (m_kRenderCamera!=null) {
                    m_kRenderCamera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha,m_kCmdBuffer);
                }
                m_kCmdBuffer.Clear();
                m_kRenderCamera = null;
            }
        }

        #region 生命周期控制
        //开始npc对话
        private void OnOpenUI() { 
            m_bOpenUI = true;
            m_bMeshDirty=true;
        }
        //关闭npc对话框
        private void OnCloseUI() {
            m_bOpenUI=false;
            m_bMeshDirty = true;
        }

        public void ShowAllTitle() {
            HudBoardSetting.GetInstance().m_bHideAllTitle = false;
            m_bMeshDirty = true;
        }

        public void HideAllTitle() { 
            HudBoardSetting.GetInstance().m_bHideAllTitle=true;
            m_bMeshDirty = true;
        }

        public void EnterGame() {
            if (m_bStart)
            {
                return;
            }
            m_iUpdateVersion = 0;
            m_iBaseUpdateVersion = 0;
            m_iCameraUpdateVersion = 0;

            foreach (var v in m_kHudTitles) {
                HudTitleInfo title = v.Value;
                if (title.m_kBatcher==null)
                {
                    title.m_kBatcher = m_kStaticBatcher;
                    title.m_kBatcher.PushTitle(title);
                    title.RebuildFontUI();
                }
            }
            m_bStart = true;
        }

        public void LeaveGame() {
            if (!m_bStart)
            {
                return;
            }
            foreach (var v in m_kHudTitles)
            {
                HudTitleInfo title = v.Value;
                if (title.m_kBatcher != null)
                {
                    title.m_kBatcher.EraseTitle(title);
                    title.m_kBatcher = null;
                }
            }

            m_kDynamicBatcher.m_kMeshRender.Release();//mesh 字体 顶点清除
            m_kStaticBatcher.m_kMeshRender.Release();
            ReleaseCmdBuffer();
            m_bStart = false;
        }

        public Camera GetMainCamera()
        {
            return Camera.main;
        }
        #endregion

        #region 外部设置title内容
        //注册一个title
        public int RegisterTitle(Transform tf,float offsetY,bool isMain) {
            Camera cameraMain = Camera.main;
            //是否主角
            if (isMain) {
                m_kTranMain = tf;
            }

            Vector3 pos = tf.position;
            HudTitleInfo title=new HudTitleInfo();
            title.m_kTran = tf;
            title.m_bIsMain = isMain;
            title.m_vWorldPos = pos;
            title.m_fOffsetY = offsetY;
            if (cameraMain!=null) {
                pos.y += offsetY + HudBoardSetting.GetInstance().m_fTitleOffsetY;
                title.m_vScreenPos=cameraMain.WorldToScreenPoint(pos);
                title.CaleCameraScale(cameraMain.transform.position);//相机远近面计算缩放
            }

            int uuid = GetNextUUID();
            title.m_iUUID = uuid;
            m_kHudTitles.Add(uuid,title);


            if (isMain)
            {
                title.m_kBatcher = m_kStaticBatcher;
            }
            else { 
                title.m_kBatcher = m_kDynamicBatcher;
            }

            title.m_fLastMoveTime = UnityEngine.Time.time;


            title.m_kBatcher.PushTitle(title);

            //添加tick到lateupdate 这里去除
            return uuid;
        }

        //移除一个title
        public void ReleaseTitle(int titleUUID) {
            HudTitleInfo titleInfo;
            if (m_kHudTitles.TryGetValue(titleUUID,out titleInfo)) {
                if (titleInfo.m_iUUID!=titleUUID) {
                    Log.Debug("非法释放Title:"+titleUUID);
                    return;
                }
                if (titleInfo.m_bIsMain) {
                    this.m_kTranMain = null;
                }
                if (titleInfo.m_kBatcher != null)
                {
                    titleInfo.m_kBatcher.EraseTitle(titleInfo);//批处理移除自身
                    titleInfo.m_kBatcher = null;
                }
                titleInfo.OnRelease();
                m_kHudTitles.Remove(titleUUID);
            }
        }

        public void ApplySetting(HudAniSetting hudSetting)
        {
            foreach (var v in m_kHudTitles)
            {
                v.Value.RebuildForEditor();
            }
        }

        // 请不要在外部保存它
        public HudTitleInfo GetTitle(int titleUUID)
        {
            HudTitleInfo titleInfo;
            if (m_kHudTitles.TryGetValue(titleUUID, out titleInfo))
                return titleInfo;
            return null;
        }


        #endregion

        #region tick循环方法
        /*
        //字体改变回调
        void OnAllFontChanged(Font font)
        {
            if (font == null)
                return;
            UIFont uiFont = HudTitleInfo.GetHudTitleFont();
            if (font.GetInstanceID() != uiFont.dynamicFont.GetInstanceID())
            {
                return;
            }

            //m_kStaticBatcher.OnAllFontChanged(uiFont);
            //m_kDynamicBatcher.OnAllFontChanged(uiFont);

            // 头顶气泡
            //HUDTalk.HUDTalkRender.Instance.OnAllFontChanged(uiFont);
        }*/

        //相机变动处理
        public void UpdateCamera() {
            NextCameraUpdateVersion();
            if (m_iCameraUpdateVersion!=m_iBaseUpdateVersion) {
                BaseUpdateLogic(UnityEngine.Time.deltaTime);
                m_iCameraUpdateVersion = m_iBaseUpdateVersion;
            }
        }
        public void Tick() {
            NextUpdateVersion();
            if (m_iUpdateVersion!=m_iBaseUpdateVersion) {

                BaseUpdateLogic(UnityEngine.Time.deltaTime);
                m_iUpdateVersion = m_iBaseUpdateVersion;
            }
        }

        //每帧以及相机变动都需要处理
        private void BaseUpdateLogic(float deltaTime) {
            NextBaseUpdateVersion();
            Camera cameraMain = GetMainCamera();
            if (cameraMain==null) {
                return;
            }
            CheckCameraDirty();
        }


        private void CheckCameraDirty() {
            Camera cameraMain = GetMainCamera();
            Vector3 vCameraPos = cameraMain.transform.position;
            Vector3 vOffset = vCameraPos - m_vLastCameraPos;
            bool bCameraDirty = vOffset.x * vOffset.x + vOffset.y * vOffset.y + vOffset.z * vOffset.z > 0.000001f;//坐标有变化
            if (!bCameraDirty)
            {
                vOffset = cameraMain.transform.localEulerAngles - m_vLastEulerAngles;
                if (vOffset.x * vOffset.x + vOffset.y * vOffset.y + vOffset.z * vOffset.z > 0.000001f)
                    bCameraDirty = true;
            }

            bool bMeshDirty = m_bMeshDirty;
            if (cameraMain != m_kOldCamera)
            {
                m_kOldCamera = cameraMain;
                bCameraDirty = true;
                bMeshDirty = true;
            }

            if (bCameraDirty)
            {
                m_vLastCameraPos = vCameraPos;
                m_vLastEulerAngles = cameraMain.transform.localEulerAngles;
                m_fLastCheckMoveTime = UnityEngine.Time.time;
                float fScaleX = Screen.width / HudBoardSetting.GetInstance().m_fAllWidth;
                float fScaleY = Screen.height / HudBoardSetting.GetInstance().m_fAllHeight;
                float fScale = fScaleX > fScaleY ? fScaleX : fScaleY;
                HudMesh.s_fCameraScaleX = HudMesh.s_fCameraScale * fScale;
                HudMesh.s_fCameraScaleY = HudMesh.s_fCameraScale * fScale;
                CaleNumberScale(vCameraPos);
            }
            else {

                // 切换
                float fNow = UnityEngine.Time.time;
                if (m_fLastCheckMoveTime + 2.0f < fNow)
                {
                    m_fLastCheckMoveTime = fNow;
                    SwitchDynamieStatic(); // 这个不可以转换是什么鬼，有BUG
                }
            }


            if (m_bHideAllTitle != HudBoardSetting.GetInstance().m_bHideAllTitle)
            {
                m_bHideAllTitle = HudBoardSetting.GetInstance().m_bHideAllTitle;
                bMeshDirty = true;
            }

            // 静态批
            m_kStaticBatcher.UpdateLogic(bCameraDirty, vCameraPos);
            if (m_kStaticBatcher.m_kMeshRender.m_bMeshDirty) {
                bMeshDirty = true;
            }
                

            // 动态批
            m_kDynamicBatcher.UpdateLogic(bCameraDirty, vCameraPos);
            if (m_kDynamicBatcher.m_kMeshRender.m_bMeshDirty) {
                bMeshDirty = true;
            }


            //二级面板


            //屏幕后效处理

            if (m_bMeshDirty)
            {
                m_bMeshDirty = false;
                bMeshDirty = true;
            }

            if (bMeshDirty) {
                FillMeshRender();
            }
        }

        private void FillMeshRender() {
            Camera cameraMain = GetMainCamera();
            if (m_kCmdBuffer == null)
            {
                m_kCmdBuffer = new CommandBuffer();
                m_kCmdBuffer.name = "TitleCmd";
            }
            else {
                if (m_kRenderCamera!=null) {
                    m_kRenderCamera.RemoveCommandBuffer(CameraEvent.AfterSkybox, m_kCmdBuffer);  // 挂在AfterForwardAlpha可以与物体做遮挡
                }
            }
            m_kCmdBuffer.Clear();
            m_kRenderCamera=null;

            if (m_bOpenUI) {//其他一些效果也需要屏蔽
                return;
            }

            if (!m_bHideAllTitle)
            {
               // Debug.LogError("Dynamic!!!!");
                //m_kDynamicBatcher.m_kMeshRender.LogInfo();

                m_kDynamicBatcher.m_kMeshRender.RenderTo(m_kCmdBuffer);
                //Debug.LogError("Static!!!!");
                //m_kStaticBatcher.m_kMeshRender.LogInfo();
                m_kStaticBatcher.m_kMeshRender.RenderTo(m_kCmdBuffer);
            }
            else {
                m_kDynamicBatcher.m_kMeshRender.OnCancelRender();
                m_kStaticBatcher.m_kMeshRender.OnCancelRender();
            }

            if (m_kCmdBuffer.sizeInBytes > 0)
            {
                //m_kRenderCamera = cameraMain;
               // Debug.Log("0000000000000___" + cameraMain.commandBufferCount);
               // cameraMain.AddCommandBuffer(CameraEvent.AfterSkybox, m_kCmdBuffer);  // 挂在AfterForwardAlpha可以与物体做遮挡
               // Debug.Log("1111111111111___"+ cameraMain.commandBufferCount);
            }

        }
        void CaleNumberScale(Vector3 vCameraPos)
        {
            if (m_kTranMain != null)
            {
                Vector3 vPos = m_kTranMain.position;
                float m_nearDistance = HudBoardSetting.GetInstance().CameraNearDist;
                float m_farDistance = HudBoardSetting.GetInstance().CameraFarDist;
                float m_minScale = HudBoardSetting.GetInstance().m_fNumberScaleMin;
                float m_maxScale = HudBoardSetting.GetInstance().m_fNumberScaleMax;
                float dis = Vector3.Distance(vPos, vCameraPos);
                float ratio = Mathf.Clamp01((dis - m_nearDistance) / (m_farDistance - m_nearDistance));
                float fScale = m_minScale * ratio + (1.0f - ratio) * m_maxScale;
                HudMesh.s_fNumberScale = 1.0f / fScale;
            }
        }


        //静态动态转换 以及空对象销毁处理
        void SwitchDynamieStatic()
        {
            float fNow = UnityEngine.Time.time;
            m_fLastCheckMoveTime = fNow;
            Dictionary<int, HudTitleInfo>.Enumerator it = m_kHudTitles.GetEnumerator();
            while (it.MoveNext())
            {
                HudTitleInfo title = it.Current.Value;
                if (title.m_kTran == null)//没有目标删除
                {
                    m_kDelayReleaseTitles.Add(it.Current.Key);
                }
                if (title.m_bIsMain)
                    continue;
                if (title.m_kBatcher == null)//没有批次丢到静态处理
                {
                    title.m_kBatcher = m_kStaticBatcher;
                    title.m_kBatcher.PushTitle(title);
                    title.RebuildFontUI();
                }
                if (title.m_kBatcher == m_kStaticBatcher)
                {
                    // 动了
                    if (title.m_fLastMoveTime + 1.0f > fNow)
                    {
                        title.m_kBatcher.EraseTitle(title);
                        title.m_kBatcher = m_kDynamicBatcher;
                        title.m_kBatcher.SwitchPushTitle(title);
                        m_bMeshDirty = true;
                    }
                }
                else
                {
                    // 一秒钟不动就转静态批
                    if (title.m_fLastMoveTime + 1.0f < fNow )
                    {
                        title.m_kBatcher.EraseTitle(title);

                        title.m_kBatcher = m_kStaticBatcher;
                        title.m_kBatcher.SwitchPushTitle(title);
                        m_bMeshDirty = true;
                    }
                }
            }

            // 释放已经无效的
            for (int i = m_kDelayReleaseTitles.size - 1; i >= 0; --i)
            {
                ReleaseTitle(m_kDelayReleaseTitles[i]);
            }
            m_kDelayReleaseTitles.Clear();
        }

        private int NextUpdateVersion()
        {
            if (m_iUpdateVersion == int.MaxValue)
            {
                m_iUpdateVersion = 0;
            }
            else
            {
                m_iUpdateVersion++;
            }
            return m_iUpdateVersion;
        }

        private int NextBaseUpdateVersion()
        {
            if (m_iBaseUpdateVersion == int.MaxValue)
            {
                m_iBaseUpdateVersion = 0;
            }
            else
            {
                m_iBaseUpdateVersion++;
            }
            return m_iBaseUpdateVersion;
        }

        private int NextCameraUpdateVersion()
        {
            if (m_iCameraUpdateVersion == int.MaxValue)
            {
                m_iCameraUpdateVersion = 0;
            }
            else
            {
                m_iCameraUpdateVersion++;
            }
            return m_iCameraUpdateVersion;
        }

        #endregion
    }

}
