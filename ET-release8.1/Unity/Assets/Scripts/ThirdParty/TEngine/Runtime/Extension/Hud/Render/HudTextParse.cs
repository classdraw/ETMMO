using System.Collections.Generic;
using UnityEngine;
using XEngine.Utilities;

namespace XEngine.Hud {


    public class HudTextParse:Singleton<HudTextParse>
    {
        public HudCharInfo[] m_kSprites;
        public short[] m_kLineHeight;
        public int m_iSpriteCount;
        public string m_sText;
        char[] m_kValidChars;
        int m_iCharCount;
        bool m_bCharDirty=false;
        BetterList<Color> m_kColors;
        //List<UIFontc>
        List<UIFontCustomObject> m_kCustomObjs;

        protected override void Init()
        {
            m_kSprites = new HudCharInfo[100];//���128���ַ�
            m_kValidChars=new char[100];
            m_kLineHeight = new short[100];
            m_kColors=new BetterList<Color>();
            m_kCustomObjs=new List<UIFontCustomObject>();
        }
        //自动增加2倍
        private void AutoGrow() {
            int size = m_kSprites.Length * 2;
            HudCharInfo[] sprites=new HudCharInfo[size];
            char[] chars=new char[size];
            for (int i=0;i<m_iSpriteCount ;i++) {
                sprites[i] = m_kSprites[i];
            }

            for (int i=0; i<m_iCharCount;i++) {
                chars[i] = m_kValidChars[i];
            }

            m_kSprites=sprites;
            m_kValidChars = chars;
        }

        //增加字符
        private void PushChar(char ch,int charPos) {
            if (m_iSpriteCount>=m_kSprites.Length||m_iSpriteCount>=m_kValidChars.Length) {
                AutoGrow();
            }

            int colorSize = m_kColors.size;

            m_kSprites[m_iSpriteCount].m_kCharType = UIFontUnitType.UnitType_Char;
            m_kSprites[m_iSpriteCount].m_bChar = true;
            m_kSprites[m_iSpriteCount].m_kChar = ch;
            m_kSprites[m_iSpriteCount].m_bCustomColor = colorSize > 0;
            m_kSprites[m_iSpriteCount].m_iSpriteId = 0;
            if (colorSize>0) {
                m_kSprites[m_iSpriteCount].m_kCustomColor = m_kColors[colorSize-1];
            }

            m_iSpriteCount++;

            if (ch!='\n') {
                m_kValidChars[m_iCharCount++] = ch;
            }
        }

        void PushLinkChar(char ch, int nCharPos, int nObjIndex)
        {
            m_kSprites[m_iSpriteCount].m_bChar = true;
            m_kSprites[m_iSpriteCount].m_kCharType = UIFontUnitType.UnitType_Link;
            m_kSprites[m_iSpriteCount].m_kChar = ch;
            if (m_kColors.size > 0)
            {
                m_kSprites[m_iSpriteCount].m_bCustomColor = true;
                m_kSprites[m_iSpriteCount].m_kCustomColor = m_kColors[m_kColors.size - 1];
            }
            else
                m_kSprites[m_iSpriteCount].m_bCustomColor = false;
            m_kSprites[m_iSpriteCount].m_iSpriteId = 0;

            m_kValidChars[m_iCharCount++] = ch;
        }

        void PushLink(string szLink, string szCustomDesc, int nCharPos, UIHyperlinkType linkType)
        {
            UIFontCustomObject obj = new UIFontCustomObject();
            obj.m_eUIFontUnitType = UIFontUnitType.UnitType_Link;
            obj.m_sLink = szLink;
            obj.m_sCustomDesc = szCustomDesc;
            obj.m_eHyperlinkType = linkType;
            m_kCustomObjs.Add(obj);
            int nObjIndex = m_kCustomObjs.Count - 1;

            for (int i = 0; i < szLink.Length; ++i)
            {
                PushLinkChar(szLink[i], nCharPos, nObjIndex);
            }
        }
        //增加换行
        private void PushEnter(char ch,int charPos) {
            m_kSprites[m_iSpriteCount].m_kCharType = UIFontUnitType.UnitType_Enter;
            m_kSprites[m_iSpriteCount].m_bChar = false;
            m_kSprites[m_iSpriteCount].m_kChar = ch;
            m_kSprites[m_iSpriteCount].m_bCustomColor = false;
            m_kSprites[m_iSpriteCount].m_iSpriteId = 0;
            m_iSpriteCount++;
        }

        int GetX16(int iStart) {
            int nValue = 0;
            char ch = m_sText[iStart];
            if (ch >= '0' && ch <= '9')
                nValue = ch - '0';
            else if (ch >= 'a' && ch <= 'f')
                nValue = ch - 'a' + 10;
            else if (ch >= 'A' && ch <= 'F')
                nValue = ch - 'A' + 10;

            nValue *= 16;
            ch = m_sText[iStart + 1];
            if (ch >= '0' && ch <= '9')
                nValue += ch - '0';
            else if (ch >= 'a' && ch <= 'f')
                nValue += ch - 'a' + 10;
            else if (ch >= 'A' && ch <= 'F')
                nValue += ch - 'A' + 10;

            return nValue;
        }

        private bool IsColorARGB(string text,int start) {
            int length = text.Length;
            if (start + 16 > length)
            {
                return false;
            }
            if (text[start + 2] != '#'
                || text[start + 15] != ']')
            {
                return false;
            }

            //检查不是合法数字
            for (int i = 0; i < 12; ++i)
            {
                char ch = text[start + 3 + i];
                if (ch < '0' || ch > '9')
                {
                    return false;
                }
            }
            return true;
        }

        bool IsColorRGB(string szText, int iStart)
        {
            int length = szText.Length;
            if (iStart + 13 > length)
            {
                return false;
            }
            if (szText[iStart + 2] != '#'
                || szText[iStart + 12] != ']')
            {
                return false;
            }

            //  检查不是合法数字
            for (int i = 0; i < 9; ++i)
            {
                char ch = szText[iStart + 3 + i];
                if (ch < '0' || ch > '9')
                {
                    return false;
                }
            }
            return true;
        }

        int GetColorValue(string szText, int iStart)
        {
            int nValue = szText[iStart] - '0';
            nValue *= 10;
            nValue += szText[iStart + 1] - '0';
            nValue *= 10;
            nValue += szText[iStart + 2] - '0';
            if (nValue < 0)
                nValue = 0;
            if (nValue > 255)
                nValue = 255;
            return nValue;
        }

        void AnylseColorARGB(ref int iStart) {
            int nA = GetColorValue(m_sText, iStart + 3);
            int nR = GetColorValue(m_sText, iStart + 6);
            int nG = GetColorValue(m_sText, iStart + 9);
            int nB = GetColorValue(m_sText, iStart + 12);
            Color32 c = new Color32((byte)nR, (byte)nG, (byte)nB, (byte)nA);
            m_kColors.Add(c);
            iStart += 15;
        }

        bool TryParseOldColor(ref int iStart)
        {
            //[rrggbb][-]
            int length = m_sText.Length;
            if (iStart + 3 > length)
            {
                return false;
            }
            if (m_sText[iStart + 1] == '-'
                && m_sText[iStart + 2] == ']')
            {
                if (m_kColors.size > 0)
                {
                    m_kColors.Pop();
                }
                iStart += 2;
                return true;
            }

            if (iStart + 8 > length)
                return false;
            if (m_sText[iStart + 7] != ']')
                return false;

            for (int i = 0; i < 6; ++i)
            {
                char ch = m_sText[iStart + i + 1];
                if (ch >= '0' && ch <= '9')
                    continue;
                if (ch >= 'a' && ch <= 'f')
                    continue;
                if (ch >= 'A' && ch <= 'F')
                    continue;
                return false;
            }

            int nR = GetX16(iStart + 1);
            int nG = GetX16(iStart + 3);
            int nB = GetX16(iStart + 5);

            Color32 c = new Color32((byte)nR, (byte)nG, (byte)nB, (byte)255);
            m_kColors.Add(c);
            iStart += 7;
            return true;
        }
        bool TryParseOldColor(string szText, ref int iStart)
        {
            string szOld = m_sText;
            m_sText = szText;
            bool bSuc = TryParseOldColor(ref iStart);
            m_sText = szOld;
            return bSuc;
        }
        // 功能：分析图标
        void ParseIconName(ref int iStart)
        {
            // [5#iconname]
            int nEnd = m_sText.IndexOf(']', iStart + 3);
            if (nEnd == -1)
            {
                ++iStart;
                PushChar('[', iStart - 1);
                return;
            }
            string szIconName = m_sText.Substring(iStart + 3, nEnd - iStart - 3);
            int nIconID = HudAtlasManager.GetInstance().SpriteNameToId(szIconName);
            PushIcon(nIconID, 0, 0, iStart);
            iStart = nEnd;
        }

        //功能：分析结束码
        private void ParseEndCode(ref int iStart) {
            int length = m_sText.Length;
            // [0#]
            if (iStart + 4 > length)
            {
                ++iStart;
                PushChar('[', iStart - 1);
                return;
            }
            // 合法的
            if (m_sText[iStart + 2] == '#'
                && m_sText[iStart + 3] == ']')
            {
                if (m_kColors.size > 0)
                {
                    m_kColors.Pop();
                }
                iStart += 3;
                return;
            }
            ++iStart;
            PushChar('[', iStart - 1); // 这是不合法的字符
        }
        // 功能：分析颜色码
        bool TryParseColorARGB(ref int iStart)
        {
            // [1#AAARRRGGGBBB]
            if (IsColorARGB(m_sText, iStart))
            {
                AnylseColorARGB(ref iStart);
                return true;
            }
            else
            {
                ++iStart;
                PushChar('[', iStart - 1);
                return false;
            }
        }

        // 功能：分析颜色码
        bool ParseColorRGB(ref int iStart)
        {
            // [1#AAARRRGGGBBB]
            if (IsColorRGB(m_sText, iStart))
            {
                int nR = GetColorValue(m_sText, iStart + 3);
                int nG = GetColorValue(m_sText, iStart + 6);
                int nB = GetColorValue(m_sText, iStart + 9);
                Color32 c = new Color32((byte)nR, (byte)nG, (byte)nB, (byte)255);
                m_kColors.Add(c);
                iStart += 12;
                return true;
            }
            else
            {
                ++iStart;
                PushChar('[', iStart - 1);
                return false;
            }
        }
        bool ParseColorRGB(string szText, ref int iStart)
        {
            // [1#AAARRRGGGBBB]
            if (IsColorRGB(m_sText, iStart))
            {
                int nR = GetColorValue(m_sText, iStart + 3);
                int nG = GetColorValue(m_sText, iStart + 6);
                int nB = GetColorValue(m_sText, iStart + 9);
                Color32 c = new Color32((byte)nR, (byte)nG, (byte)nB, (byte)255);
                m_kColors.Add(c);
                iStart += 12;
                return true;
            }
            else
            {
                ++iStart;
                PushChar('[', iStart - 1);
                return false;
            }
        }

        void ParseLinkColorARGB(string szText, ref int iStart)
        {
            string szOld = m_sText;
            m_sText = szText;
            AnylseColorARGB(ref iStart);
            m_sText = szOld;
        }
        // 功能：扫描超连接字符，并去除颜色码
        void ScaleLinkColor(string szLink, string szCustomDesc, int nCharPos)
        {
            UIHyperlinkType linkType = UIHyperlinkType.None;
            if (!string.IsNullOrEmpty(szCustomDesc) && szCustomDesc.Length > 2)
            {
                char chFirst = '0';
                if (szCustomDesc[1] == ':')
                {
                    chFirst = szCustomDesc[0];
                    szCustomDesc = szCustomDesc.Substring(2);
                    if (chFirst == 'u' || chFirst == 'U')
                        linkType = UIHyperlinkType.Underline;
                    else if (chFirst == 'f' || chFirst == 'F')
                        linkType = UIHyperlinkType.Underline_Flash;
                }
            }

            int nIndex = szLink.IndexOf('[');
            if (nIndex == -1)
            {
                PushLink(szLink, szCustomDesc, nCharPos, linkType);
                return;
            }

            // 只支持一个颜色码，不支持多组
            int nLen = szLink.Length;
            int[] NewCharPos = new int[nLen];
            char[] szNewLink = new char[nLen];
            UIFontUnit[] fontUnit = new UIFontUnit[nLen];
            int nNewLen = 0;

            for (int i = 0; i < nLen; ++i)
            {
                char chType = szLink[i];
                if (chType == '[')
                {
                    if (i + 3 > nLen
                        || szLink[i + 2] != '#')
                    {
                        // 兼容旧的颜色码  [ff0000] [-]
                        if (TryParseOldColor(szLink, ref i))
                            continue;
                    }
                    if (i + 2 <= nLen)
                    {
                        bool bValidFalgs = false;
                        switch (szLink[i + 1])
                        {
                            case '0': // 结束符 [0#]
                                {
                                    if (szLink[i + 2] == '#')
                                    {
                                        bValidFalgs = true;
                                        m_kColors.Pop();
                                        i += 3;
                                    }
                                }
                                break;
                            case '1': // [1#ARGB
                                {
                                    bValidFalgs = IsColorARGB(szLink, i);
                                    if (bValidFalgs)
                                        ParseLinkColorARGB(szLink, ref i);
                                }
                                break;
                            case '2': // [2#RGB
                                {
                                    bValidFalgs = ParseColorRGB(szLink, ref i);
                                }
                                break;
                            case '-':  // [-] 旧的结束码
                                {
                                    if (szLink[i + 2] == ']')
                                    {
                                        bValidFalgs = true;
                                        m_kColors.Pop();
                                        i += 2;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        if (bValidFalgs)
                        {
                            continue;
                        }
                    }
                }
                PushLinkChar(chType, nCharPos, m_kCustomObjs.Count);
            }

            UIFontCustomObject obj = new UIFontCustomObject();
            obj.m_eUIFontUnitType = UIFontUnitType.UnitType_Link;
            obj.m_sLink = new string(szNewLink, 0, nNewLen);
            obj.m_sCustomDesc = szCustomDesc;
            obj.m_eHyperlinkType = linkType;
            m_kCustomObjs.Add(obj);
        }
        // 功能：分析一个超连接
        void ParseLink(ref int iStart)
        {
            // [7#链接字符;自定义字符串]
            string szLink = string.Empty;
            string szCustomDesc = string.Empty;
            int nLinkStart = iStart + 3;
            int nCustomStart = m_sText.IndexOf(';', nLinkStart);
            int nEnd = iStart;
            if (nCustomStart != -1)
            {
                ++nCustomStart;
                nEnd = m_sText.IndexOf(']', nCustomStart);
                if (nEnd == -1)
                {
                    ++iStart;
                    PushChar('[', iStart - 1);
                    return;
                }
                szLink = m_sText.Substring(nLinkStart, nCustomStart - nLinkStart - 1);
                szCustomDesc = m_sText.Substring(nCustomStart, nEnd - nCustomStart);
            }
            else
            {
                nEnd = m_sText.IndexOf(']', nLinkStart);
                if (nEnd == -1)
                {
                    ++iStart;
                    PushChar('[', iStart - 1);
                    return;
                }
                szLink = m_sText.Substring(nLinkStart, nEnd - nLinkStart);
            }
            ScaleLinkColor(szLink, szCustomDesc, iStart);
            iStart = nEnd;
        }
        // 功能：分析数字 
        bool ParseNumb(ref int nIconID, ref int iStart, char chEnd1, char chEnd2 = '\0')
        {
            nIconID = 0;
            int length = m_sText.Length;
            for (; iStart < length; ++iStart)
            {
                char ch = m_sText[iStart];
                if (ch == chEnd1 || ch == chEnd2)
                {
                    return true;
                }
                if (ch >= '0' && ch <= '9')
                {
                    nIconID *= 10;
                    nIconID += ch - '0';
                }
                else
                {
                    // 不合法的
                    break;
                }
            }
            return false;
        }

        //增加一个空格
        void PushSpace(int nW, int nH, int nCharPos)
        {
            m_kSprites[m_iSpriteCount].m_bChar = false;
            m_kSprites[m_iSpriteCount].m_kCharType = UIFontUnitType.UnitType_Space;
            m_kSprites[m_iSpriteCount].m_kChar = '\0';
            m_kSprites[m_iSpriteCount].m_iSpriteId = 0;
            m_kSprites[m_iSpriteCount].m_iSpriteWidth = (short)nW;
            m_kSprites[m_iSpriteCount].m_iSpriteHeight = (short)nH;
            m_iSpriteCount++;
        }

        int ParseMulNumb(int[] aNumb, ref int iStart, char chEnd, char chTab)
        {
            for (int i = 0; i < aNumb.Length; ++i)
            {
                aNumb[i] = 0;
            }
            int nCount = 0;
            int length = m_sText.Length;
            bool bValidNumb = false;
            for (; iStart < length; ++iStart)
            {
                char ch = m_sText[iStart];
                if (ch == chEnd)
                {
                    if (nCount == 0 && bValidNumb)
                        ++nCount;
                    return nCount;
                }
                if (ch >= '0' && ch <= '9')
                {
                    if (nCount < aNumb.Length)
                    {
                        bValidNumb = true;
                        aNumb[nCount] *= 10;
                        aNumb[nCount] += ch - '0';
                    }
                }
                else if (ch == chTab)
                {
                    ++nCount;
                }
                else
                {
                    // 不合法的
                    break;
                }
            }
            return 0;  // 没有结束符的都不合法
        }
        void PushGif(int nGifID, int nWidth, int nHeight, int nCharPos)
        {
            m_kSprites[m_iSpriteCount].m_bChar = false;
            m_kSprites[m_iSpriteCount].m_kCharType = UIFontUnitType.UnitType_Gif;
            m_kSprites[m_iSpriteCount].m_bCustomColor = false;
            m_kSprites[m_iSpriteCount].m_iSpriteId = nGifID;
            m_kSprites[m_iSpriteCount].m_iSpriteWidth = (short)nWidth;
            m_kSprites[m_iSpriteCount].m_iSpriteHeight = (short)nHeight;

            m_iSpriteCount++;
        }

        void PushIcon(int nIconID, int nWidth, int nHeight, int nCharPos)
        {
            m_kSprites[m_iSpriteCount].m_bChar = false;
            m_kSprites[m_iSpriteCount].m_kCharType = UIFontUnitType.UnitType_Icon;
            m_kSprites[m_iSpriteCount].m_bCustomColor = false;
            m_kSprites[m_iSpriteCount].m_iSpriteId = nIconID;
            m_kSprites[m_iSpriteCount].m_iSpriteWidth = (short)nWidth;
            m_kSprites[m_iSpriteCount].m_iSpriteHeight = (short)nHeight;

            m_iSpriteCount++;
        }

        // 功能：分析图标ID
        void ParseIocnID(ref int iStart)
        {
            // [4#xxxx]
            int length = m_sText.Length;
            int nBakStart = iStart;
            iStart += 3;
            int[] aValueNumb = { 0, 0, 0 };
            int nCount = ParseMulNumb(aValueNumb, ref iStart, ']', ';');
            if (nCount > 0)
            {
                PushIcon(aValueNumb[0], aValueNumb[1], aValueNumb[2], nBakStart);
                return;
            }
            iStart = nBakStart + 1;
            PushChar('[', iStart - 1);
        }
        // 功能：分析一个动画
        void ParseGifName(ref int iStart)
        {
            // [6#ani_name]
            int nEnd = m_sText.IndexOf(']', iStart + 3);
            if (nEnd == -1)
            {
                ++iStart;
                PushChar('[', iStart - 1);
                return;
            }
            int nPos = iStart + 3;

            int[] aValueNumb = { 0, 0, 0 };
            int nCount = ParseMulNumb(aValueNumb, ref nPos, ']', ';');
            PushGif(aValueNumb[0], aValueNumb[1], aValueNumb[2], iStart);
            //string szGifName = m_szText.Substring(iStart + 3, nEnd - iStart - 3);
            iStart = nEnd;
        }

        private void ParseSpace(ref int iStart) {
            // [8#www-hhh]
            int nBakStart = iStart;
            int nW = 0, nH = 0;
            if (!ParseNumb(ref nW, ref iStart, '-'))
            {
                iStart = nBakStart + 1;
                PushChar('[', iStart - 1);
                return;
            }
            if (!ParseNumb(ref nH, ref iStart, ']'))
            {
                iStart = nBakStart + 1;
                PushChar('[', iStart - 1);
                return;
            }
            PushSpace(nW, nH, nBakStart);
        }

        void ParseObject(ref int iStart)
        {
            m_bCharDirty = true;

            int length = m_sText.Length;
            if (iStart + 3 > length
                || m_sText[iStart + 2] != '#')
            {
                // 兼容旧的颜色码
                // [ff0000] [-]
                if (!TryParseOldColor(ref iStart))
                    PushChar('[', iStart);
                return;
            }
            char chType = m_sText[iStart + 1];
            switch (chType)
            {
                case '[':
                    ++iStart;
                    PushChar('[', iStart - 1);
                    return;
                case '0':  // [0#]结束码
                    ParseEndCode(ref iStart);
                    break;
                case '1': // [1#AAARRRGGGBBB] 颜色码 ARGB
                    TryParseColorARGB(ref iStart);
                    break;
                case '2': // [2#RRRGGGBBB] 颜色码 RGB
                    ParseColorRGB(ref iStart);
                    break;
                case '3': // 暂时不支持的字符
                    ++iStart;
                    PushChar('[', iStart - 1);
                    break;
                case '4': // [4#xxxx]  图片ID
                    ParseIocnID(ref iStart);
                    break;
                case '5': // [5#iconname] 图片名字
                    ParseIconName(ref iStart);
                    break;
                case '6': // [6#ani_name] 动画的名字
                    ParseGifName(ref iStart);
                    break;
                case '7': // [7#链接字符;自定义字符串]
                    ParseLink(ref iStart);
                    break;
                case '8': // [8#www-hhh] 任意大小的空格
                    ParseSpace(ref iStart);
                    break;
                case '9':
                    break;
                default:
                    ++iStart;
                    PushChar('[', iStart - 1);
                    break;
            }
        }

        //解析字符串方法 分析颜色 文本 以及特殊类型
        public void ParseText(string szText)
        {
            m_iSpriteCount = 0;
            m_iCharCount = 0;
            m_sText = szText;
            m_kColors.Clear();
            m_kCustomObjs.Clear();
            if (string.IsNullOrEmpty(szText))
                return;
            m_bCharDirty = false;
            int length = szText.Length;
            for (int i = 0; i < length; ++i)
            {
                char ch = m_sText[i];
                switch (ch)
                {
                    case '[':
                        ParseObject(ref i);
                        break;
                    case '\n':
                        PushEnter(ch, i);
                        break;
                    case '\\':
                        {
                            if (i + 1 < length)
                            {
                                if (m_sText[i + 1] == 'n')
                                {
                                    PushChar('\n', i);
                                    ++i;
                                }
                                else if (m_sText[i + 1] == '\\')
                                {
                                    PushChar('\\', i);
                                    ++i;
                                }
                                else
                                {
                                    PushChar(ch, i);
                                }
                            }
                            else
                            {
                                PushChar(ch, i);
                            }
                        }
                        break;
                    case '\0': // 不可见字符(结束符，不能显示)
                        break;
                    default:
                        {
                            PushChar(ch, i);
                        }
                        break;
                }
            }
            if (m_bCharDirty)
            {
                if (m_iCharCount == 0)
                    m_sText = string.Empty;
                else
                    m_sText = new string(m_kValidChars, 0, m_iCharCount);
            }
        }
    }

}
