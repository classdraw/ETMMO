using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XEngine.Hud {
    //单个纹理材质
    public class UITextureInfo
    {
        public string m_sAtlasName = "InputName";//材质名称

        public string m_sTextureName = "";//纹理名称
        public string m_sShaderName = "";//shader名称
        public Material m_kMaterial;//材质
        public Texture m_kMainAlphaTex;//主贴图通道图

        public int m_iAtlasId = 0;
        Coordinates m_kCoordinates = Coordinates.Pixels;

        // Size in pixels for the sake of MakePixelPerfect functions.
        int m_iPixelSize = 1;

        // Whether the atlas is using a pre-multiplied alpha material. -1 = not checked. 0 = no. 1 = yes.
        int m_iPMA = -1;

        int m_iTexWidth = 1;   // 纹理的宽度
        int m_iTexHeight = 1;  // 纹理的高度

        bool m_bCanLOD = false; // 是不是可以LOD缩放

        //临时变量
        public int m_iRef;  // 引用计数
        public float m_fReleaseTime; // 释放时间
        public int m_iVersion;     // 当前修改的版本号(有修改就改变)
        public void CopyFromSetting(UITextureInfo from) {
            m_kCoordinates = from.m_kCoordinates;
            m_iPixelSize = from.m_iPixelSize;
            m_iPMA = from.m_iPMA;
        }

        public Texture MainAlphaTexture { get { return m_kMainAlphaTex != null ? m_kMainAlphaTex : MainTexture; } }
        public Texture MainTexture { get { if (m_kMaterial != null) { return m_kMaterial.mainTexture; }return null; } }


        public void SetTextureSizeByMaterial(Material mat) {
            Texture tex = mat != null ? mat.mainTexture : null;
            SetTextureSizeByTexture(tex);
            if (mat!=null&&mat.shader!=null) {
                m_sShaderName = mat.shader.name;
            }
        }

        public void SetTextureSizeByTexture(Texture tex) {
            if (tex != null)
            {
                m_iTexWidth = tex.width;
                m_iTexHeight = tex.height;
            }
            else {
                m_iTexWidth = m_iTexHeight = 1;
            }
        }

        //图片宽度
        public int TexWidth { get { return m_iTexWidth; } }
        //图片高度
        public int TexHeight { get { return m_iTexHeight; } }

        public Coordinates Coordinates { get { return m_kCoordinates; }set { m_kCoordinates = value; } }

        public int PixelSize { get { return m_iPixelSize; }set { m_iPixelSize = value; } }

        //预乘alpha
        public bool PremultipliedAlpha {
            get {
                if (m_iPMA==-1) {
                    Material mat = m_kMaterial;
                    m_iPMA = (mat != null && mat.shader != null && mat.shader.name.Contains("Premultiplied")) ? 1 : 0;
                }
                return m_iPMA==1;
            }
        }

        public bool IsCanLOD() {
            return m_bCanLOD;
        }

        public void SetLODFlag(bool canLOD) {
            m_bCanLOD = canLOD;
        }

        //调整资源
        public void AdjustAtlas(UITextureInfo other) {
            m_sAtlasName = other.m_sAtlasName;
            m_sTextureName = other.m_sTextureName;
            m_iAtlasId = other.m_iAtlasId;
            m_sShaderName = other.m_sShaderName;
            m_iPixelSize = other.m_iPixelSize;
            m_kCoordinates = other.m_kCoordinates;
            m_iTexWidth = other.m_iTexWidth;
            m_iTexHeight = other.m_iTexHeight;
            m_bCanLOD = other.m_bCanLOD;
        }

        public void Serialize(ref CSerialize ar) {
            int coordVal = (int)m_kCoordinates;
            ar.ReadWriteValue(ref m_sAtlasName);
            ar.ReadWriteValue(ref m_sTextureName);
            ar.ReadWriteValue(ref coordVal);
            ar.ReadWriteValue(ref m_iPixelSize);
            m_kCoordinates = coordVal == (int)Coordinates.Pixels ? Coordinates.Pixels : Coordinates.TexCoords;
            ar.ReadWriteValue(ref m_iTexWidth);
            ar.ReadWriteValue(ref m_iTexHeight);

            if (ar.GetVersion()>=1) {
                ar.ReadWriteValue(ref m_iAtlasId);
            }
            if (ar.GetVersion() >= 2)
            {
                ar.ReadWriteValue(ref m_sShaderName);
            }
            if (ar.GetVersion() >= 4)
            {
                ar.ReadWriteValue(ref m_bCanLOD);
            }
        }

        public void SerializeToTxt(ref SerializeText ar) {
            int coordVal = (int)m_kCoordinates;
            ar.ReadWriteValue("AtlasName", ref m_sAtlasName);
            ar.ReadWriteValue("TexName", ref m_sTextureName);
            ar.ReadWriteValue("Coordinates", ref coordVal);
            ar.ReadWriteValue("PixelSize", ref m_iPixelSize);
            m_kCoordinates = coordVal == (int)Coordinates.Pixels ? Coordinates.Pixels : Coordinates.TexCoords;
            ar.ReadWriteValue("texWidth", ref m_iTexWidth);
            ar.ReadWriteValue("texHeight", ref m_iTexHeight);

            if (ar.GetVersion() >= 1)
            {
                ar.ReadWriteValue("AtlasID", ref m_iAtlasId);
            }
            if (ar.GetVersion() >= 2)
            {
                ar.ReadWriteValue("ShaderName", ref m_sShaderName);
            }
            if (ar.GetVersion() >= 4)
            {
                ar.ReadWriteValue("CanScale", ref m_bCanLOD);
            }
        }
    }
    public enum Coordinates
    {
        Pixels,
        TexCoords,
    }

}

