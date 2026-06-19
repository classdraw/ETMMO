using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XEngine.Hud {
    //sprite对象  兼容ngui对象
    public class UISpriteInfo 
    {
        public string m_sName = "UnitySprite";

        public Rect m_rOuter = new Rect(0f,0f,1f,1f);//外框精灵实际大小(在纹理的像素坐标)
        public Rect m_rInner = new Rect(0f,0f,1f,1f);//内框，用来填充模式像素坐标，必须在外框之内

        public bool m_bRotated = false;

        public float m_fPaddingLeft = 0f;   // 用来做精灵图层选择时扩展选择框范围的东东，没有实际意义
        public float m_fPaddingRight = 0f;
        public float m_fPaddingTop = 0f;
        public float m_fPaddingBottom = 0f;

        // 下面是扩展属性
        public int m_iNameID;   // 精灵ID
        public int m_iAtlasID;  // 材质ID
        public string m_sAtlasName;  // 对应的材质名字

        public bool hasPadding { get { return m_fPaddingLeft != 0f || m_fPaddingRight != 0f || m_fPaddingTop != 0f || m_fPaddingBottom != 0f; } }

        public UISpriteInfo Clone() {
            UISpriteInfo p = new UISpriteInfo();
            //p.Copy(this);
            return p;
        }

        public void Copy(UISpriteInfo src) {
            m_sName = src.m_sName.Clone() as string;
            m_rOuter = new Rect(src.m_rOuter.xMin, src.m_rOuter.yMin,src.m_rOuter.width,src.m_rOuter.height);
            m_rInner = new Rect(src.m_rInner.xMin, src.m_rInner.yMin, src.m_rInner.width, src.m_rInner.height);

            m_bRotated = src.m_bRotated;
            m_fPaddingLeft = src.m_fPaddingLeft;
            m_fPaddingRight = src.m_fPaddingRight;
            m_fPaddingTop = src.m_fPaddingTop;
            m_fPaddingBottom = src.m_fPaddingBottom;

            m_iNameID = src.m_iNameID;
            m_iAtlasID = src.m_iAtlasID;
            m_sAtlasName = src.m_sAtlasName.Clone() as string;



        }

        public void Serialize(ref CSerialize ar) {
            ar.ReadWriteValue(ref m_sName);
            ar.ReadWriteValue(ref m_rOuter);
            ar.ReadWriteValue(ref m_rInner);
            ar.ReadWriteValue(ref m_bRotated);
            ar.ReadWriteValue(ref m_fPaddingLeft);
            ar.ReadWriteValue(ref m_fPaddingRight);
            ar.ReadWriteValue(ref m_fPaddingTop);
            ar.ReadWriteValue(ref m_fPaddingBottom);
            ar.ReadWriteValue(ref m_sAtlasName);
            if (ar.GetVersion() >= 1)
            {
                ar.ReadWriteValue(ref m_iNameID);
                ar.ReadWriteValue(ref m_iAtlasID);
            }
        }

        public void SerializeToTxt(ref SerializeText ar)
        {
            ar.ReadWriteValue("name", ref m_sName);
            ar.ReadWriteValue("outer", ref m_rOuter);
            ar.ReadWriteValue("inner", ref m_rInner);
            ar.ReadWriteValue("rotated", ref m_bRotated);
            ar.ReadWriteValue("paddingLeft", ref m_fPaddingLeft);
            ar.ReadWriteValue("paddingRight", ref m_fPaddingRight);
            ar.ReadWriteValue("paddingTop", ref m_fPaddingTop);
            ar.ReadWriteValue("paddingBottom", ref m_fPaddingBottom);
            ar.ReadWriteValue("AtlasName", ref m_sAtlasName);
            if (ar.GetVersion() >= 1)
            {
                ar.ReadWriteValue("NameID", ref m_iNameID);
                ar.ReadWriteValue("AtlasID", ref m_iAtlasID);
            }
        }
    }


    //改良后读取自定义图集的图片
    public class UISpriteSimple {
        public int m_iIndex;//在里面的序号 唯一id
        public string m_sName;
        public Rect m_kRect;//宽高等矩形数据
        public Vector4 m_kUV;//uv
        public Vector4 m_kBorder;//边框 一般没啥用 生成图集都是0,0,0,0
        public int m_iAtlasID=1;//默认图集资源
        public void Copy(UISpriteSimple spriteSimple) { 
            m_sName=spriteSimple.m_sName;
            m_kRect=spriteSimple.m_kRect;
            m_kUV=spriteSimple.m_kUV;
            m_kBorder=spriteSimple.m_kBorder;
            m_iAtlasID=spriteSimple.m_iAtlasID;
        }

        public UISpriteSimple Clone()
        {
            UISpriteSimple p = new UISpriteSimple();
            p.Copy(this);
            return p;
        }

    }

}
