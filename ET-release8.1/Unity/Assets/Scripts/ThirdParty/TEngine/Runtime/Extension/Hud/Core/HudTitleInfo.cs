using UnityEngine;

namespace XEngine.Hud
{
    public class HudTitleLine
    {
        public string m_sText;//文本内容
        public string m_sValidText;//
        public float m_fWidth;
        public int m_iHeight;
        public Enum_HudTitleType m_eHudTitleType;
        public int m_iColorIndex;
        public int m_iSpriteId;
        public int m_iStart;
        public int m_iEnd;
        public int m_iLine;
    }



    public class HudTitleInfo : HudTitleBase
    {
        public Transform m_kTran;
        public float m_fDistToCamera = 0.0f;//离相机的距离 用来排序
        public bool m_bInitHudMesh = false;

        public bool m_bNeedHide = false;
        private bool m_bDirty = false;
        public bool m_bIsMain = false;

        HudTitleLine[] m_kTitleLines = new HudTitleLine[(int)Enum_HudTitleType.Count];
        int m_iTitleNumber;
        float m_fLineOffsetY;
        float m_fCurLineHeight;
        float m_fCurLineWidth; // 当前行的宽度（只有居中的才统计)
        int m_iStartLineIndex;
        int m_iBloodIndex;
        int m_iBloodSpriteId;
        Enum_HudBloodType m_eHudBloodType;
        int m_iLines;
        int m_iMeridianIndex;//头顶冲脉title下标
        int m_iMeridianNumber;//头顶冲脉数字

        //batcher

        public int m_iBatcherIndex;//
        float m_fLastMoveTime = 0f;//最后一次移动时间
        // 功能：重置头顶文本的UV坐标
        // 说明：因为退出游戏时，把UI的模型都给释放了，所以重新登陆时，需要重新设置一下UV坐标
        public void RebuildFontUI()
        {
            CharacterInfo tempCharInfo = new CharacterInfo();
            RebuildCharUV(ref tempCharInfo);
        }
        private void RebuildCharUV(ref CharacterInfo characterInfo)
        {
            if (m_kSprites.size == 0)
            {
                return;
            }
            UIFont uiFont = GetHudTitleFont();
            int start = 0;
            int end = 0;
            for (int i = 0; i < m_iTitleNumber; i++)
            {
                HudTitleLine titleLine = m_kTitleLines[i];
                if (string.IsNullOrEmpty(titleLine.m_sValidText))
                {
                    continue;
                }
                uiFont.PrepareQueryText(titleLine.m_sValidText);
                start = titleLine.m_iStart;
                end = titleLine.m_iEnd;

                for (; start < end; start++)
                {
                    HudVertex v = m_kSprites[start];
                    if (v.m_iAtlasID == 0)
                    {
                        uiFont.GetCharacterInfo(v.m_cChar, ref characterInfo);
                        v.RebuildCharUV(characterInfo);
                        if (v.m_kHudMesh != null)
                        {
                            v.m_kHudMesh.VertexDirty();
                        }
                    }
                }
            }
        }



        public void ApplyMove(bool cameraDirty, Vector3 cameraPos)
        {
            Camera cameraMain = HudTitleRender.GetInstance().GetMainCamera();
            Vector3 pos = m_kTran.position;
            Vector3 scale = m_kTran.localScale;
            pos.y += (m_fOffsetY + HudBoardSetting.GetInstance().m_fTitleOffsetY) * scale.y;

            if (!cameraDirty && !m_bDirty)
            {
                //判断位置 是否需要重新计算  tran目标 worldPos 算出来的位置
                float dx = pos.x - m_vWorldPos.x;
                float dz = pos.z - m_vWorldPos.z;
                float dy = pos.y - m_vWorldPos.y;

                bool isDirty = !m_bInitHudMesh;
                if (dx * dx + dz * dz + dy * dy > 0.00001f)
                {
                    m_bDirty = true;
                }
                if (!m_bDirty)
                {
                    return;
                }

                if (m_kBatcher != null)
                {
                    m_kBatcher.m_bMove = true;
                }
                m_fLastMoveTime = UnityEngine.Time.time;

            }

            m_bDirty = false;
            m_vWorldPos = pos;
            m_vScreenPos = cameraMain.WorldToScreenPoint(pos);

            m_fDistToCamera = Vector3.Distance(cameraPos, m_vWorldPos);
            if (m_bIsMain)
            {
                m_fDistToCamera -= 1000f;//主角丢前面
            }
            this.CaleCameraScale(cameraPos);//计算缩放    
            OnChangeScreenPos();
        }

        public void RebuildForEditor()
        {
            Transform tf = m_kTran;
            Vector3 vPos = m_vWorldPos;
            Vector2 vScreenPos = m_vScreenPos;
            Enum_HudBloodType nBloodType = m_eHudBloodType;

            float fPos = HudBoardSetting.Instance.m_fTestBloodPos;
            HudTitleLine[] titles = new HudTitleLine[m_iTitleNumber];
            int nNumb = m_iTitleNumber;
            for (int i = 0; i < m_iTitleNumber; ++i)
            {
                titles[i] = new HudTitleLine();
                titles[i].m_sText = m_kTitleLines[i].m_sText;
                titles[i].m_eHudTitleType = m_kTitleLines[i].m_eHudTitleType;
                titles[i].m_iColorIndex = m_kTitleLines[i].m_iColorIndex;
                titles[i].m_iLine = m_kTitleLines[i].m_iLine;
                titles[i].m_iSpriteId = m_kTitleLines[i].m_iSpriteId;
            }
            Clear();
            m_kTran = tf;
            m_vWorldPos = vPos;
            m_vScreenPos = vScreenPos;
            m_eHudBloodType = nBloodType;
            int nOldLine = -1;
            bool bStartLine = false;
            for (int i = 0; i < nNumb; ++i)
            {
                HudTitleLine title = titles[i];
                if (nOldLine != title.m_iLine)
                {
                    if (bStartLine)
                    {
                        EndTitle();
                    }
                    bStartLine = true;
                    BeginTitle();
                }
                if (title.m_eHudTitleType == Enum_HudTitleType.Blood)
                {
                    PushBlood(nBloodType, fPos);
                }
                else if (title.m_iSpriteId != 0)
                {
                    PushIcon(title.m_eHudTitleType, title.m_iSpriteId);
                }

                else
                {
                    PushTitle(title.m_sText, title.m_eHudTitleType, title.m_iColorIndex);

                }
                //  
                nOldLine = title.m_iLine;
            }
            if (bStartLine)
            {
                EndTitle();
            }
        }


        public void BeginTitle()
        {
            m_fCurLineHeight = 0;
            m_fCurLineWidth = 0;
        }

        public void EndTitle()
        {
            m_iLines++;
            Align();
        }

        public void OnRelease()
        {
            Clear();
            m_bIsMain = false;
            m_bNeedHide = false;
            m_iMeridianIndex = 0;
            m_iMeridianNumber = 0;
            m_kTran = null;
        }
        public void SetOffsetY(float offsetY)
        {
            if (Mathf.Abs(m_fOffsetY - offsetY) > 0.0001f)
            {
                m_bDirty = true;
            }
            m_fOffsetY = offsetY;
        }
        public void Clear()
        {
            ClearSprite();
            m_fLineOffsetY = 0;
            m_fCurLineHeight = 0;
            m_fCurLineWidth = 0;
            m_iStartLineIndex = 0;
            m_iTitleNumber = 0;
            m_bInitHudMesh = false;
            m_iBatcherIndex = 0;
            m_iBloodSpriteId = 0;
            m_eHudBloodType = Enum_HudBloodType.Blood_None;
            m_iLines = 0;
            m_iMeridianIndex = 0;
            m_iMeridianNumber = 0;
        }


        private void Align()
        {
            int lineGap = 0;
            //先让Y轴居中
            float offsetY = m_fLineOffsetY + m_fCurLineHeight * 0.5f;
            for (int i = m_iStartLineIndex; i < m_iTitleNumber; i++)
            {
                HudTitleLine title = m_kTitleLines[i];
                int iType = (int)title.m_eHudTitleType;
                var titleSet = HudBoardSetting.GetInstance().TitleSets[iType];
                var attr = titleSet.GetTitle(title.m_iColorIndex);
                if (attr.m_eAlignType == Enum_HudAlignType.Align_Right)
                {
                    OffsetXY(title.m_iStart, title.m_iEnd, m_fCurLineWidth * 0.5f + attr.m_iCharGap, offsetY);
                }
                else if (attr.m_eAlignType == Enum_HudAlignType.Align_Left)
                {
                    OffsetXY(title.m_iStart, title.m_iEnd, -m_fCurLineWidth * 0.5f - title.m_fWidth - attr.m_iCharGap, offsetY);
                }
                else
                {
                    OffsetXY(title.m_iStart, title.m_iEnd, title.m_fWidth * -0.5f, offsetY);
                }

                if (lineGap < attr.m_iLineGap)
                {
                    lineGap = attr.m_iLineGap;
                }
            }

            m_fLineOffsetY += m_fCurLineHeight + lineGap;
            m_iStartLineIndex = m_iTitleNumber;
        }

        // 必须先有居中的  内容设置
        public void PushTitle(string text, Enum_HudTitleType hudTitleType, int colorIndex)
        {
            if (m_iTitleNumber >= m_kTitleLines.Length)
            {
                return;
            }
            UIFont font = GetHudTitleFont();
            CharacterInfo tempCharInfo = new CharacterInfo();
            HudTitleAttribute attr = HudBoardSetting.GetInstance().TitleSets[(int)hudTitleType].GetTitle(colorIndex);
            int lineGap = attr.m_iLineGap;
            int charGap = attr.m_iCharGap;
            

            HudTextParse.GetInstance().ParseText(text);
            font.PrepareQueryText(HudTextParse.GetInstance().m_sText);

            Title_Effect_Type style = attr.m_eEffectType;
            int spriteCount = HudTextParse.GetInstance().m_iSpriteCount;
            HudCharInfo[] sprites = HudTextParse.GetInstance().m_kSprites;

            char ch;
            float shadowX = attr.m_iOffsetX;
            float shadowY = attr.m_iOffsetY;
            float fx = 0f;
            Color32 colorLeftUp = attr.m_kColorLeftUp;
            Color32 colorLeftDown = attr.m_kColorLeftDown;
            Color32 colorRightUp = attr.m_kColorRightUp;
            Color32 colorRightDown = attr.m_kColorRightDown;

            Color32 colorShadow = attr.m_kColorShadow;

            Color32 colorCustom;

            int start = m_kSprites.size;
            int height = font.GetFontHeight();

            int fontH = attr.m_iHeight;
            int fontOffsetY = attr.m_iFontOffsetY;
            int iY = 0;
            for (int i = 0; i < spriteCount; ++i)
            {
                if (sprites[i].m_bChar)
                {
                    ch = sprites[i].m_kChar;
                    font.GetCharacterInfo(ch, ref tempCharInfo);
                    iY = (tempCharInfo.glyphHeight - fontH) / 2 + fontOffsetY;

                    if (style != Title_Effect_Type.None)
                    {
                        PushShadow(ref tempCharInfo, ch, fx, iY, colorShadow, shadowX, shadowY);
                        if (style == Title_Effect_Type.Outline)
                        {
                            PushShadow(ref tempCharInfo, ch, fx, iY, colorShadow, shadowX, -shadowY);
                            PushShadow(ref tempCharInfo, ch, fx, iY, colorShadow, -shadowX, shadowY);
                            PushShadow(ref tempCharInfo, ch, fx, iY, colorShadow, -shadowX, -shadowY);
                        }
                    }
                    if (sprites[i].m_bCustomColor)
                    {
                        colorCustom = sprites[i].m_kCustomColor;
                        HudVertex node = PushChar(ref tempCharInfo, ch, fx, iY, colorCustom, colorCustom, colorCustom, colorCustom);
                        fx += node.m_iWidth + charGap;
                    }
                    else
                    {
                        HudVertex node = PushChar(ref tempCharInfo, ch, fx, iY, colorLeftUp, colorLeftDown, colorRightUp, colorRightDown);
                        fx += node.m_iWidth + charGap;
                    }
                }
                else
                {
                    // 图片
                    if (sprites[i].m_kCharType == UIFontUnitType.UnitType_Icon)
                    {
                        HudVertex node = PushSprite(sprites[i].m_iSpriteId, sprites[i].m_iSpriteWidth, sprites[i].m_iSpriteHeight, fx, attr.m_iSpriteOffsetY);
                        fx += node.m_iWidth + charGap;
                        if (height < node.m_iHeight - attr.m_iSpriteReduceHeight)//高度给个限制 不能小于一定值
                            height = node.m_iHeight - attr.m_iSpriteReduceHeight;
                    }
                }
            }


            if (m_kTitleLines[m_iTitleNumber] == null)
                m_kTitleLines[m_iTitleNumber] = new HudTitleLine();
            m_kTitleLines[m_iTitleNumber].m_eHudTitleType = hudTitleType;
            m_kTitleLines[m_iTitleNumber].m_fWidth = fx - charGap;
            m_kTitleLines[m_iTitleNumber].m_iHeight = height;
            m_kTitleLines[m_iTitleNumber].m_iStart = start;
            m_kTitleLines[m_iTitleNumber].m_iEnd = m_kSprites.size;
            m_kTitleLines[m_iTitleNumber].m_sText = text;
            m_kTitleLines[m_iTitleNumber].m_sValidText = HudTextParse.GetInstance().m_sText;
            m_kTitleLines[m_iTitleNumber].m_iColorIndex = colorIndex;
            m_kTitleLines[m_iTitleNumber].m_iLine = m_iLines;
            m_kTitleLines[m_iTitleNumber].m_iSpriteId = 0;

            if (attr.m_eAlignType == Enum_HudAlignType.Align_Center)
            {
                m_fCurLineWidth = fx - charGap;
            }
            if (height < attr.m_iLockMaxHeight && attr.m_iLockMaxHeight > 0)//最小值限制
                height = attr.m_iLockMaxHeight;
            if (m_fCurLineHeight < height)
                m_fCurLineHeight = height;
            ++m_iTitleNumber;


        }

        public void PushIcon(Enum_HudTitleType hudTitleType, int spriteIndex)
        {
            var attr = HudBoardSetting.GetInstance().TitleSets[(int)hudTitleType].GetTitle(0);
            var spriteSimple = HudAtlasManager.GetInstance().GetSpriteSimpleByIndex(spriteIndex);

            int width = 0;
            int height = 0;
            int start = m_kSprites.size;
            if (spriteSimple != null)
            {
                width = (int)spriteSimple.m_kRect.width;
                height = (int)spriteSimple.m_kRect.height;
                PushSprite(spriteIndex, width, height, 0f, attr.m_iSpriteOffsetY);
            }
            if (m_kTitleLines[m_iTitleNumber] == null)
                m_kTitleLines[m_iTitleNumber] = new HudTitleLine();

            m_kTitleLines[m_iTitleNumber].m_eHudTitleType = hudTitleType;
            m_kTitleLines[m_iTitleNumber].m_fWidth = width;
            m_kTitleLines[m_iTitleNumber].m_iHeight = height;
            m_kTitleLines[m_iTitleNumber].m_iStart = start;
            m_kTitleLines[m_iTitleNumber].m_iEnd = m_kSprites.size;
            m_kTitleLines[m_iTitleNumber].m_sText = string.Empty;
            m_kTitleLines[m_iTitleNumber].m_iColorIndex = 0;

            ++m_iTitleNumber;
            //XLogger.LogError(width+"__"+height);
        }

        //设置血条 血条类型以及百分比
        public void PushBlood(Enum_HudBloodType hudBloodType, float bloodLv)
        {
            if (m_iTitleNumber>=m_kTitleLines.Length) {
                return;
            }
            int bkWidth = HudBoardSetting.GetInstance().m_iBloodBkWidth;
            int bkHeight = HudBoardSetting.GetInstance().m_iBloodBkHeight;
            int start = m_kSprites.size;
            int bloodWidth = HudBoardSetting.GetInstance().m_iBloodWidth;
            int bloodHeight=HudBoardSetting.GetInstance().m_iBloodHeight;



            int bloodSpriteId = -1;
            if (hudBloodType == Enum_HudBloodType.Blood_Red)
            {
                bloodSpriteId = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetHpRedId();
            }
            else if (hudBloodType == Enum_HudBloodType.Blood_Green)
            {
                bloodSpriteId = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetHpGreenId();
            }
            else {
                bloodSpriteId = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetHpBlueId();
            }


            PushSprite(HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetHpBgId(), bkWidth,bkHeight, ( bloodWidth- bkWidth) * 0.5f, 0f);
            PushSliceTitleNew(bloodSpriteId, bloodWidth, bloodHeight, 0.0f, 0.0f, bloodLv);
            //PushSprite(HudBoardSetting.Instance.m_nBloodBk, nBkWidth, nBkHeight, (nBloodWidth - nBkWidth) * 0.5f, 0);// (nBkHeight - nHeight) * 0.5f);
            //PushSprite(7,);
            //PushSprite();
            if (m_kTitleLines[m_iTitleNumber] == null)
                m_kTitleLines[m_iTitleNumber] = new HudTitleLine();
            m_kTitleLines[m_iTitleNumber].m_eHudTitleType = Enum_HudTitleType.Blood;
            m_kTitleLines[m_iTitleNumber].m_fWidth = bloodWidth;
            m_kTitleLines[m_iTitleNumber].m_iHeight = bloodHeight;
            m_kTitleLines[m_iTitleNumber].m_iStart = start;
            m_kTitleLines[m_iTitleNumber].m_iEnd = m_kSprites.size;
            m_kTitleLines[m_iTitleNumber].m_sText = string.Empty;
            m_kTitleLines[m_iTitleNumber].m_iColorIndex = 0;

            m_eHudBloodType = hudBloodType;
            m_iBloodIndex = m_iTitleNumber;
            m_fCurLineWidth = bloodWidth;
            m_fCurLineHeight = bloodHeight;
            ++m_iTitleNumber;
        }

        // 功能：设置血条的进度（百分比)
        public void SetBloodPos(float bloodLv)
        {
            // 跳过背景
            if (m_iBloodIndex >= 0 && m_iBloodIndex < m_iTitleNumber)
            {
                int bloodSpriteId = -1;
                if (m_eHudBloodType == Enum_HudBloodType.Blood_Red)
                {
                    bloodSpriteId = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetHpRedId();
                }
                else if (m_eHudBloodType == Enum_HudBloodType.Blood_Green)
                {
                    bloodSpriteId = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetHpGreenId();
                }
                else
                {
                    bloodSpriteId = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.GetHpBlueId();
                }
                int bloodWidth = HudBoardSetting.GetInstance().m_iBloodWidth;
                int bloodHeight = HudBoardSetting.GetInstance().m_iBloodHeight;
                SliceTitleNew(bloodSpriteId, bloodWidth, bloodHeight, m_kTitleLines[m_iBloodIndex].m_iStart+1, bloodLv);//为啥+1 因为 初始是bg
            }
        }
        //清除自身所有mesh顶点
        public void EraseSpriteFromMesh()
        {
            if (!m_bInitHudMesh)
            {
                return;
            }
            m_bInitHudMesh = false;
            for (int i = m_kSprites.size - 1; i >= 0; i--)
            {
                HudVertex v = m_kSprites[i];
                if (v.m_kHudMesh != null)
                {
                    v.m_kHudMesh.EraseHudVertex(v);
                }
                v.m_kHudMesh = null;
            }
        }

        //显示title
        public void ShowTitle()
        {
            if (m_bNeedHide)
            {
                m_bNeedHide = false;
                // if (m_bInitHudMesh) { 
                //     EraseSpriteFromMesh();
                //  }
            }
        }

        //隐藏title
        public void HideTitle()
        {
            if (!m_bNeedHide)
            {
                m_bNeedHide = true;
                if (m_bInitHudMesh)
                {
                    EraseSpriteFromMesh();//隐藏清除mesh
                }
            }
        }
        //缩放后处理
        public void OnScale()
        {
            m_bDirty = true;
        }


    }
}
