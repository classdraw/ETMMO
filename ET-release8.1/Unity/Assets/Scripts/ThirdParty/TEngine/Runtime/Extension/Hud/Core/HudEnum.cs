using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XEngine.Hud{

    public enum UIFontUnitType
    {
        UnitType_Char,  // 字符
        UnitType_Icon,  // 图标
        UnitType_Gif,   // 动画
        UnitType_Link,  // 连接
        UnitType_Space, // 占位符
        UnitType_Enter, // 换行符
    }
    //hud对齐方式
    public enum Enum_HudAlignType:byte { 
        Align_Left,//左对齐
        Align_Center,//居中
        Align_Right//右对齐
    }

    //血条类型
    public enum Enum_HudBloodType : byte
    { 
        Blood_None=0,
        Blood_Red=1,
        Blood_Green=2,
        Blood_Blue=3//队友血条
    }

    //title类型
    public enum Enum_HudTitleType : byte
    {
        PlayerTitle=0,//名称
        //PlayerPrestige,//声望
        PlayerCorp,//工会
        //PlayerDesignation,//职务
        MonsterName,//怪物名称
        ItemName,//物品名称
        PetName,//宠物名称
        Blood,//血条
        //PKFlag,//pl标识
        HeadIcon,//NPC头顶标记
        Count
    }

    //数字渲染的类型
    public enum Enum_NumberRender_Type : byte
    { 
        HUD_SHOW_HP_HURT=0,
        HUD_SHOW_HP_ADD=1,
        //添加新的数字类型
        HUD_SHOW_TIP_NUM=2,

        //项目内用的
        HUD_SHOW_HP_HURT_NEW=3,//新的受伤
        
        HUD_SHOW_NUMBER
    }

    //hud动画数据属性
    [System.Serializable]
    public struct HudAnimeAttribute {
        public AnimationCurve AlphaCurve;//透明度曲线
        public AnimationCurve ScaleCurve;//缩放曲线
        public AnimationCurve MoveCurve;//移动曲线

        public AnimationCurve MoveCurve1;//移动曲线
        public AnimationCurve RotateCurve;//旋转曲线1
        public AnimationCurve RotateCurve1;//旋转曲线2

        public float OffsetX;
        public float OffsetY;
        public float GapTime;
        public int SpriteGap;//图片间隔
        public Enum_HudAlignType AlignType;//对齐类型
        public bool ScreenAlign;//是否按照屏幕对齐
        public Enum_HudAlignType ScreenAlignType;//屏幕对齐类型

        public Color HurtColor;//受伤颜色
        public Color HurtCritColor;//受伤暴击颜色

    }

    public enum Title_Effect_Type : byte
    { 
        None,
        Shadow,
        Outline
    }
    //标题属性 
    [System.Serializable]
    public struct HudTitleAttribute {
        public Title_Effect_Type m_eEffectType;//效果
        public Color32 m_kColorShadow;//阴影颜色

        public int m_iOffsetX;//x偏移
        public int m_iOffsetY;//y偏移

        //四个顶点色
        public Color m_kColorLeftUp;
        public Color m_kColorLeftDown;
        public Color m_kColorRightUp;
        public Color m_kColorRightDown;

        public int m_iCharGap;//字符间隙
        public int m_iLineGap;//线间隙
        public int m_iHeight;//高度

        public Enum_HudAlignType m_eAlignType;//对齐方式
        public int m_iLockMaxHeight;//锁定最大高度
        public int m_iSpriteReduceHeight;//图片缩减高度
        public int m_iSpriteOffsetX;//图片左右移动距离
        public int m_iSpriteOffsetY;//图片上下移动距离
        public int m_iFontOffsetY;//文本上下移动距离
    }
    //title数据set
    public class HudTitleLabelSet{
        public HudTitleAttribute[] m_kData;
        public HudTitleLabelSet(HudTitleAttribute attr){
            m_kData=new HudTitleAttribute[1];
            m_kData[0]=attr;
        }

        public HudTitleLabelSet(HudTitleAttribute[] attrArray){
            m_kData=attrArray;
        }

        public HudTitleAttribute GetTitle(int index)
        {
            if (index < 0)
                index = 0;
            else if (index >= m_kData.Length)
                index = m_kData.Length - 1;
            return m_kData[index];
        }
    }

    public struct HudCharInfo {
        public UIFontUnitType m_kCharType;
        public bool m_bChar;
        public bool m_bCustomColor;
        public char m_kChar;//字符
        public int m_iSpriteId;//图片id
        public short m_iSpriteWidth;
        public short m_iSpriteHeight;
        public Color32 m_kCustomColor;
        public int m_iX;
        public int m_iY;
        public int m_iLine;
        public int m_iLineHeight;//当前行高
    }


    public class UIFontCustomObject111 {
        public UIFontUnitType m_eUIFontUnitType;
        public bool m_bIsChar;
        public bool m_bIsCustomColor;
        public char m_kChar;
        public int m_iSpriteId;
        public short m_iSpriteWidth;
        public short m_iSpriteHeight;
        public int m_iX;
        public int m_iY;
        public int m_iLine;
        public int m_iLineHeight;

    }
    public enum UIHyperlinkType
    {
        None,    // 没有下划线
        Underline, // 有下划线
        Underline_Flash, // 下划线闪烁
    }

    public class UIFontCustomObject
    {
        public UIFontUnitType m_eUIFontUnitType;  // 
        public int m_iIconID;          // 图标ID或动画ID
        public string m_sLink;
        public string m_sCustomDesc;  // 上层自定义的信息
        public UIHyperlinkType m_eHyperlinkType = UIHyperlinkType.None; // 超连接的类型
    }

    public class UIFontUnit
    {
        public int m_iX;     // 显示的相对坐标X
        public int m_iY;     // 显示的相对坐标Y
        public int m_iWidth;
        public int m_iHeight;
        public int m_iLineHeight;
        public UIFontUnitType m_eUIFontUnitType; // 类型
        public int m_iIconID;   // 图标ID或动画ID
        public int m_iObjIndex; // 对象索引
        public int m_iRow;      // 当前所在的行
        public int m_iCharPos;  // 字符串的位置
        public char m_kChar;
        public Color32 m_kColor1;
        public Color32 m_kColor2;
        public Color32 m_kColor3;
        public Color32 m_kColor4;
        public bool m_bCustomColor;
        public bool m_bZoom;

        public int right { get { return m_iX + m_iWidth; } }
        public int bottom { get { return m_iY + m_iHeight; } }
        public int midX { get { return m_iX + m_iWidth / 2; } }
    }
}

