using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XEngine.Utilities;

namespace XEngine.Hud
{
    public class HudNumberRender : Singleton<HudNumberRender>
    {
        HudNumberInfo m_kInvalid;
        HudNumberInfo m_kValidList;


        HudRender m_kMeshRender = new HudRender(useNumberShader: true);
        private bool m_bMeshDirty=false;
        private Dictionary<Enum_NumberRender_Type, HudNumberData> m_kNumberSetting = new Dictionary<Enum_NumberRender_Type, HudNumberData>();
        private bool m_bPause = false;
        private float m_fCurrentDuration = 0f;
        Camera m_kOldCamera;
        Camera m_kRenderCamera;
        CommandBuffer m_kCmdBuffer;
        bool m_bAddCommandBuffer = false;
        bool m_bOpenUI = false;
        bool m_bOldOpenUI = false;

        public bool m_bStart = false;
        //屏幕缩放
        bool m_bCalcCameraScale = false;
        bool m_bCaleScreenScale = false;
        float m_fScreenScaleX = 1.0f;
        float m_fScreenScaleY = 1.0f;

        //logic tick
        bool m_bAddUpdateLogic = false;
        float m_fLastUpdateLogicTime = 0.0f;

        //二级面板处理
        bool m_bStartDark = false;
        bool m_bOldStartDark = false;
        float m_fStartDarkTime = 0.0f;
        float m_fDarkTime = 0.0f;

        BetterList<int> m_tempNumb = new BetterList<int>();
        public void Build() {

            //手动添加跳字样式
            m_kNumberSetting.Clear();
            HudNumberData info = new HudNumberData();
            info.Init("TNum", "TNumAdd", "TNumSub", null, Vector2.zero);
            m_kNumberSetting.Add(Enum_NumberRender_Type.HUD_SHOW_HP_HURT,info);

            info=new HudNumberData();
            info.Init("GNum", "GreenAdd", "GreenSub", null, Vector2.zero);
            m_kNumberSetting.Add(Enum_NumberRender_Type.HUD_SHOW_HP_ADD, info);

            info=new HudNumberData();
            info.Init("CNum",null,null,"AxeT1", Vector2.zero);
            m_kNumberSetting.Add(Enum_NumberRender_Type.HUD_SHOW_TIP_NUM, info);

            info = new HudNumberData();
            info.Init("TNum", "TNumAdd", "TNumSub","2006",new Vector2(0f,30f));
            m_kNumberSetting.Add(Enum_NumberRender_Type.HUD_SHOW_HP_Crit, info);

            m_fCurrentDuration = HudBoardSetting.GetInstance().m_fDurationTime;
        }

        private HudNumberInfo QueryHudNumber(Enum_NumberRender_Type numberRender) {
            int index = (int)numberRender;
            HudNumberInfo node = m_kInvalid;
            if (node!=null) { 
                m_kInvalid=node.m_kNext;
                node.m_kNext=null;
            }

            if (node==null) {
                node = new HudNumberInfo();
                node.m_eNumberRenderType = numberRender;
            }
            return node;
        }

        private void ReleaseHudNumber(HudNumberInfo node) {
            if (node!=null) {
                node.m_kNext = m_kInvalid;
                m_kInvalid = node;
            }
        }
        public CommandBuffer GetCmdBuffer()
        {
            return m_kCmdBuffer;
        }
        public void EnterGame() {
            if (m_bStart)
            {
                return;
            }
            m_bStart = true;
        }
        public void LeaveGame() {
            if (!m_bStart)
            {
                return;
            }
            CleanCurrentNumber();
            CleanAllMeshRender();
            ReleaseCmmmandBuffer();
            m_bStart =false;
        }

        // 功能：开始NPC对话
        public void OnOpenUI()
        {
            // 需要隐藏所有的文字
            m_bOpenUI = true;

            ReleaseCmmmandBuffer();
        }

        // 功能：结束NPC对话
        public void OnCloseUI()
        {
            m_bOpenUI = false;
        }

        public void OnStartScreenDark(float fTime)
        {
            m_bStartDark = true;
            m_fStartDarkTime = UnityEngine.Time.time;
            m_fDarkTime = fTime;
            ReleaseCmmmandBuffer();
        }
        public void OnEndScreenDark()
        {
            m_bStartDark = false;
        }

        void CleanCurrentNumber()
        {
            while (m_kValidList != null)
            {
                HudNumberInfo pDel = m_kValidList;
                m_kValidList = m_kValidList.m_kNext;
                pDel.m_kNext = null;
                OnErase(pDel);
            }
            m_bMeshDirty = true;
        }


        private void OnPush(HudNumberInfo node) {
            for (int i=0; i<node.m_kSprites.size;i++) {
                HudMesh hudMesh = m_kMeshRender.QueryMesh(node.m_kSprites[i].m_iAtlasID);
                node.m_kSprites[i].m_kHudMesh = hudMesh;
                hudMesh.PushHudVertex(node.m_kSprites[i]);
            }
        }

        private void OnErase(HudNumberInfo node) {
            node.ReleaseVertex();
            ReleaseHudNumber (node);
        }

        private void FillMeshRender() {
            if (m_kCmdBuffer == null)
            {
                m_kCmdBuffer = new CommandBuffer();
                m_kCmdBuffer.name = "NumberCmd";
            }
            m_kMeshRender.FillMesh();
            if (m_kMeshRender.m_bMeshDirty) { 
                m_bMeshDirty = true;
            }

            if (!m_bMeshDirty) { 
                Camera camera = HudTitleRender.GetInstance().GetMainCamera();
                if (camera!=m_kOldCamera)
                {
                    m_kOldCamera=camera;
                    m_bMeshDirty=true;
                }
            }

            if (m_bMeshDirty) {
                m_bMeshDirty = false;
                if (m_kRenderCamera != null) {
                    m_kRenderCamera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, m_kCmdBuffer);// 挂在AfterForwardAlpha可以与物体做遮挡
                    m_kRenderCamera = null;
                    m_bAddCommandBuffer = false;
                }
                Camera cameraMain=HudTitleRender.GetInstance().GetMainCamera();
                m_kCmdBuffer.Clear();
                if (cameraMain==null||m_bOpenUI) {
                    return;
                }
                m_kMeshRender.RenderTo(m_kCmdBuffer);
                if (m_kCmdBuffer.sizeInBytes>0&&cameraMain!=null) {
                    //m_kRenderCamera = cameraMain;
                   // cameraMain.AddCommandBuffer(CameraEvent.AfterSkybox, m_kCmdBuffer);  // 挂在AfterForwardAlpha可以与物体做遮挡
                    m_bAddCommandBuffer = true;
                }
            }
        }

        private void CalcScreenScale() {
            m_bCaleScreenScale = true;
            m_fScreenScaleX = Screen.width / HudBoardSetting.GetInstance().m_fAllWidth; ;
            m_fScreenScaleY = Screen.height / HudBoardSetting.GetInstance().m_fAllHeight;
            m_fScreenScaleX *= HudMesh.s_fNumberScale;
            m_fScreenScaleY *= HudMesh.s_fNumberScale;
        }

        public void Tick() {

            UpdateLogic();
        }

        private void UpdateLogic() {
            if (!m_bAddUpdateLogic) {
                return;
            }
            //计算屏幕缩放
            CalcScreenScale();
            HudNumberInfo node = m_kValidList;
            HudNumberInfo last = m_kValidList;
            while (node!=null) {
                PlayAnimation(node, false);
                if (node.m_bStop) {
                    HudNumberInfo del = node;
                    if (node == m_kValidList)
                    {
                        m_kValidList = m_kValidList.m_kNext;//栈顶一处
                        last = m_kValidList;
                    }
                    else {
                        last.m_kNext = node.m_kNext;//中间的移除 node没了
                    }

                    node = node.m_kNext;
                    OnErase(del);
                    continue;

                }

                last = node;
                node=node.m_kNext;
            }//while

            if (m_kValidList==null) { 
                m_bMeshDirty = true;
            }

            // 处理二级面板开启
            // 屏幕变黑后的处理
            if (m_bStartDark != m_bOldStartDark)
            {
                m_bOldStartDark = m_bStartDark;
                m_bMeshDirty = true;
            }
            else if (m_bStartDark) {
                if (m_fStartDarkTime + m_fDarkTime < UnityEngine.Time.time)
                    m_bStartDark = false;//时间到了 自动结束
            }

            bool bOpenUI = m_bOpenUI || m_bStartDark || m_bStart;
            if (m_bOldOpenUI != bOpenUI)
            {
                m_bOldOpenUI = bOpenUI;
                m_bMeshDirty = true;
            }
            FillMeshRender();
            if (m_kValidList == null)//跳字全没了
            {
                if (m_fLastUpdateLogicTime + 5.0f < UnityEngine.Time.time)
                {
                    m_bAddUpdateLogic = false;
                }
                CleanAllMeshRender();
            }
            else
            {
                m_fLastUpdateLogicTime = UnityEngine.Time.time;
            }
        }

        public void ShowHurtNumber(Transform tran, Enum_NumberRender_Type numberRenderType, int number, bool showHead, bool isCrit = false, int rotateIndex = -1, Vector2 initOffset = default(Vector2))
        {
            if (m_bOpenUI|| m_bStartDark||!m_bStart) {
                return;
            }
            Vector3 vPos=tran.position;
            int index=(int)numberRenderType;
            Camera cameraMain = HudTitleRender.GetInstance().GetMainCamera();
            if (cameraMain==null) {
                return;
            }
            if (!m_bAddUpdateLogic) { 
                m_bAddUpdateLogic=true;
                m_bMeshDirty=true;
            }
            HudAnimeAttribute attr=HudBoardSetting.GetInstance().NumberAttributes[index];
            HudNumberInfo node = QueryHudNumber(numberRenderType);
            node.m_eNumberRenderType = numberRenderType;
            node.m_kNext = m_kValidList;
            m_kValidList = node;

            bool showAdd = false;
            bool showSub = false;
            Color vertexColor = Color.white;
            if (number < 0)
            {
                showSub = true;
                number = -number;
                vertexColor = isCrit ? attr.HurtCritColor : attr.HurtColor;
            }
            else
            {
                showAdd = number > 0;
            }

            node.Reset();
            node.m_kColor = vertexColor;
            // 初始化
            node.m_iSpriteGap = attr.SpriteGap;
            node.m_fStartTime = UnityEngine.Time.time;
            node.m_kTran = tran;
            node.m_vWorldPos = vPos;
            node.m_iRotateIndex = rotateIndex;
            node.m_vInitOffset = initOffset;
            Align(cameraMain,node,attr,vPos);


            float y = 0f;
            var setting=m_kNumberSetting[numberRenderType];
            int colorStartIndex = 0;
            if (showHead&&setting.m_iFirstSpriteIndex!=-1) {
                node.PushSprite(y, setting.m_iFirstSpriteIndex, setting.m_OffsetFirstSprite);
                colorStartIndex = node.m_kSprites.size;
            }
            bool haveNumber = true;
            //某些枚举没有数字 比如miss 抵抗等
            if (haveNumber) {
                if (showAdd&&setting.m_iAddSpriteIndex!=-1) {
                    node.PushSprite(y,setting.m_iAddSpriteIndex);//加号
                }else if (showSub && setting.m_iSubSpriteIndex != -1)
                {
                    node.PushSprite(y, setting.m_iSubSpriteIndex);//减号
                }

                //数字拆解
                m_tempNumb.Clear();
                int nn = 0;
                do {
                    nn = number % 10;
                    number /= 10;
                    m_tempNumb.Add(nn);
                } while (number>0);

                //翻转
                //m_tempNumb.Reverse();
                for (int i = m_tempNumb.size-1; i >=0; --i)
                {
                    nn = m_tempNumb[i];
                    //Debug.LogError(nn);
                    node.PushSprite(y, setting.m_iNumbers[nn]);
                }
                // 居中处理吧
                switch (attr.AlignType)
                {
                    case Enum_HudAlignType.Align_Right:
                        node.MakeRight();
                        break;
                    case Enum_HudAlignType.Align_Center:
                        node.MakeCenter();
                        break;
                    default:
                        node.MakeLeft();
                        break;
                }
                // 申请纹理
                OnPush(node);
                node.ApplyVertexColor(colorStartIndex);

                if (!m_bCaleScreenScale)
                {
                    CalcScreenScale();
                }
                PlayAnimation(node, true);
            }
        }


        private void Align(Camera cameraMain,HudNumberInfo node,HudAnimeAttribute attr,Vector3 vPos) {
            if (cameraMain != null)
            {
                // 如果按屏幕对齐
                if (attr.ScreenAlign)
                {
                    Vector3 v1 = cameraMain.WorldToScreenPoint(vPos);
                    v1.x = attr.OffsetX;
                    v1.y = attr.OffsetY;
                    float fScaleX = (float)Screen.width / HudBoardSetting.GetInstance().m_fAllWidth;
                    float fScaleY = (float)Screen.height / HudBoardSetting.GetInstance().m_fAllHeight;

                    if (attr.ScreenAlignType == Enum_HudAlignType.Align_Left)
                    {
                        v1.x = attr.OffsetX;
                        v1.y = attr.OffsetY;
                    }
                    else if (attr.ScreenAlignType == Enum_HudAlignType.Align_Right)
                    {
                        v1.x = HudBoardSetting.GetInstance().m_fAllWidth - attr.OffsetX;
                        v1.y = attr.OffsetY;
                    }
                    else
                    {
                        v1.x = Screen.width / 2.0f + attr.OffsetX;
                        v1.y = attr.OffsetY;
                    }
                    v1.x *= fScaleX;
                    v1.y *= fScaleY;

                    node.m_vScreenPos = v1;
                    vPos = cameraMain.ScreenToWorldPoint(v1);
                    node.m_vWorldPos = vPos;

                    Vector3 vCameraPos = cameraMain.transform.position;
                    node.CaleCameraScale(vCameraPos);
                }
                else
                {
                    node.m_vScreenPos = cameraMain.WorldToScreenPoint(vPos);
                    Vector3 vCameraPos = cameraMain.transform.position;
                    node.CaleCameraScale(vCameraPos);
                }
            }
        }


        // 功能：清除所有的模型渲染
        void CleanAllMeshRender()
        {
            m_kMeshRender.FastClearVertex(); // CleanAllVertex
            ReleaseCmmmandBuffer();
        }
        void ReleaseCmmmandBuffer()
        {
            if (m_bAddCommandBuffer)
            {
                m_bAddCommandBuffer = false;
                if (m_kRenderCamera != null)
                    m_kRenderCamera.RemoveCommandBuffer(CameraEvent.AfterSkybox, m_kCmdBuffer);  // 挂在AfterForwardAlpha可以与物体做遮挡
                m_kRenderCamera = null;
                m_kCmdBuffer.Clear();
            }
        }

        //执行数字动画效果
        private void PlayAnimation(HudNumberInfo node, bool first)
        {

            // int numberCount = (int)Enum_NumberRender_Type.HUD_SHOW_NUMBER;
            int index = (int)node.m_eNumberRenderType;
            float currentDuration = UnityEngine.Time.time - node.m_fStartTime;
            if (m_bPause)
            {
                currentDuration = 0f;
            }

            HudAnimeAttribute attr = HudBoardSetting.GetInstance().NumberAttributes[index];
            bool isDirty = false;
            float fAlpha = attr.AlphaCurve.Evaluate(currentDuration);
            float scale = attr.ScaleCurve.Evaluate(currentDuration);
            float pos = attr.MoveCurve.Evaluate(currentDuration);
            float rotate = 0f;
            if (node.m_iRotateIndex == 0)
            {
                rotate = attr.RotateCurve.Evaluate(currentDuration);
            }
            else if (node.m_iRotateIndex == 1)
            {
                rotate = attr.RotateCurve1.Evaluate(currentDuration);
            }


            float oldAlpha = node.m_fAlpha;
            float oldScale = node.m_fAniScale;
            float oldMoveX = node.m_vMove.x;
            float oldMoveY = node.m_vMove.y;
            float oldRotate = node.m_fRotate;
            node.m_fAlpha = fAlpha;
            node.m_fAniScale = scale;
            node.m_fRotate = rotate;
            if (attr.ScreenAlign)
            {
                node.m_vMove.x = 0.0f;
                node.m_vMove.y = pos * m_fScreenScaleY;
            }
            else
            {
                node.m_vMove.x = attr.OffsetX * m_fScreenScaleX;
                node.m_vMove.y = (attr.OffsetY + pos) * m_fScreenScaleY;
            }

            node.m_bStop = currentDuration > m_fCurrentDuration;
            if (m_bPause)
            {
                node.m_bStop = false;
            }

            int nAlpha = (int)(fAlpha * 255.0f + 0.5f);
            if (nAlpha < 0)
                nAlpha = 0;
            if (nAlpha > 255)
                nAlpha = 255;
            byte alpha = (byte)nAlpha;
            if (!first)//第一次必定是dirty
                isDirty = node.UpdateScreenPos(ref attr);
            else
                isDirty = true;

            if (!isDirty)
            {
                if (Mathf.Abs(oldAlpha - node.m_fAlpha) > 0.0001f)
                    isDirty = true;
                if (!isDirty && Mathf.Abs(oldScale - node.m_fAniScale) > 0.0001f)
                    isDirty = true;
                if (!isDirty && Mathf.Abs(oldMoveX - node.m_vMove.x) > 0.0001f)
                    isDirty = true;
                if (!isDirty && Mathf.Abs(oldMoveY - node.m_vMove.y) > 0.0001f)
                    isDirty = true;
                if (!isDirty && Mathf.Abs(oldRotate - node.m_fRotate) > 0.0001f)//旋转变化
                    isDirty = true;
            }

            if (!isDirty)
            {
                return;
            }


            var finalScale = scale * node.m_fScale;//曲线缩放*节点相机缩放*node默认缩放

            // 更新顶点数据
            Vector2 vScreenPos = node.m_vScreenPos;
            Vector2 vMove = node.m_vMove + node.m_vInitOffset;
            for (int i = node.m_kSprites.size - 1; i >= 0; --i)
            {
                HudVertex v = node.m_kSprites[i];
                v.m_vMove = vMove;
                v.m_vWorldPos = node.m_vWorldPos;
                v.m_vScenePos = vScreenPos;
                v.m_fScale = finalScale;
                v.m_fRotate = node.m_fRotate;
                v.m_vColorLU.a = alpha;
                v.m_vColorRU.a = alpha;
                v.m_vColorLD.a = alpha;
                v.m_vColorRD.a = alpha;

                v.m_kHudMesh.VertexDirty();
            }
        }
    }

}
