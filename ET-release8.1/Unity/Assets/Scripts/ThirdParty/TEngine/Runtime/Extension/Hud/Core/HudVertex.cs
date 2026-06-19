using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XEngine.Hud{
    public class HudVertex
    {
        HudVertex m_kNext;
        int m_iId;
        static HudVertex s_InvalidList;
        static int s_iInvalidCount=0;
        static int s_iVertexId=0;

        //取得一个顶点
        public static HudVertex QueryVertex(){
            HudVertex p=s_InvalidList;
            if(p!=null){
                s_InvalidList=p.m_kNext;
                s_iInvalidCount--;
                p.m_kNext=null;
            }else{
                p=new HudVertex();
                p.m_iId=++s_iVertexId;
            }
            return p;
        }

        public static void ReleaseVertex(HudVertex p){
            if(p!=null){
                // p.hudGif=null;
                p.m_kNext= s_InvalidList;
                s_InvalidList=p;
                s_iInvalidCount++;
            }
        }

        public int ID{
            get{
                return m_iId;
            }
        }

        public Vector2 m_vVertexRU;//右上角
        public Vector2 m_vVertexRD;//右下角
        public Vector2 m_vVertexLD;//左下角
        public Vector2 m_vVertexLU;//左上角

        public Vector2 m_vUVRD;//右上角
        public Vector2 m_vUVRU;//右上角
        public Vector2 m_vUVLU;//左上角
        public Vector2 m_vUVLD;//左下角

        public Color32 m_vColorRD;//右下
        public Color32 m_vColorRU;//右上
        public Color32 m_vColorLU;//左上
        public Color32 m_vColorLD;//左下

        public Vector3 m_vWorldPos;//世界坐标
        public Vector2 m_vScenePos;//屏幕坐标

        public Vector2 m_vOffset;//本地偏移
        public Vector2 m_vMove;//当前移动量  (变化值)
        public float m_fScale;//当前缩放
        public float m_fRotate = 0f;//当前旋转

        public int m_iSpriteIndex; // 图片在图集中序号
        public int m_iAtlasID;  // 图集ID
        public char m_cChar;       // 字
        public short m_iWidth;    // 图片或文字的宽度
        public short m_iHeight;   // 图片或文字的高度
        public HudMesh m_kHudMesh;
        public int m_iHudVertexIndex;//只能hudmesh修改 切记
        // public hudGif hudGif;

        public static int GetCharWidth(CharacterInfo charInfo){
            int width=charInfo.glyphWidth;
            if(charInfo.maxX>width){
                if(charInfo.minX<0){
                    return charInfo.maxX-charInfo.minX;
                }else{
                    return charInfo.maxX+charInfo.minX;
                }
            }
            return width;
        }

        public void InitChar(CharacterInfo charInfo){
            m_iWidth=(short)GetCharWidth(charInfo);
            m_iHeight=(short)charInfo.glyphHeight;
            m_fScale=1f;
            m_iAtlasID=0;

            m_vVertexRU.Set(charInfo.maxX,charInfo.minY);//右上角
            m_vVertexRD.Set(charInfo.maxX,charInfo.maxY);//右下角
            m_vVertexLU.Set(charInfo.minX,charInfo.minY);//左上角
            m_vVertexLD.Set(charInfo.minX,charInfo.maxY);//左下角

            
            m_vUVRU=charInfo.uvBottomRight;
            m_vUVRD=charInfo.uvTopRight;
            m_vUVLU=charInfo.uvBottomLeft;
            m_vUVLD=charInfo.uvTopLeft;
        }

        public void RebuildCharUV(CharacterInfo charInfo){
            m_vUVRU=charInfo.uvBottomRight;
            m_vUVRD=charInfo.uvTopRight;
            m_vUVLU=charInfo.uvBottomLeft;
            m_vUVLD=charInfo.uvTopLeft;
        }

        //把宽高转换成uv
        public static Rect ConvertToTexCoords(Rect rect,int width,int height){
            Rect final=rect;
            if(width!=0f&&height!=0f){
                final.xMin=rect.xMin/width;
                final.xMax=rect.xMax/height;
                //y是倒过来的
                final.yMin=1f-rect.yMax/height;
                final.yMax=1f-rect.yMin/height;
            }
            return final;
        }
        /**
         float uvR=outUV.xMax;
            float uvL=outUV.xMin;
            float uvB=outUV.yMin;
            float uvT=outUV.yMax;

            m_vUVRU.Set(uvR,uvB);
            m_vUVRD.Set(uvR,uvT);
            m_vUVLU.Set(uvL,uvB);
            m_vUVLU.Set(uvL,uvT);
         */
        public void InitSprite(int width=-1,int height=-1){
            m_cChar='\0';
            m_iAtlasID=-1;
            UISpriteSimple spriteInfo=HudAtlasManager.GetInstance().GetSpriteSimpleByIndex(this.m_iSpriteIndex);
            if(spriteInfo==null){
                return;
            }
            width = (int)(width < 0 ? spriteInfo.m_kRect.width : width);
            height = (int)(height < 0 ? spriteInfo.m_kRect.height : height);
            //资源
            m_iAtlasID =spriteInfo.m_iAtlasID;
            //宽高
            this.m_iWidth= (short)width;
            this.m_iHeight=(short)height;
            this.m_fScale=1.0f;//缩放

            Vector4 outUV=spriteInfo.m_kUV;


            m_vVertexRU.Set(width,0f);//右上角 widht,0
            m_vVertexRD.Set(width,height);//右下角 width,height
            m_vVertexLU.Set(0f,0f);//左上角 0,0
            m_vVertexLD.Set(0f,height);//左下角 0,height


            m_vUVRU.Set(outUV.z, outUV.y);
            m_vUVRD.Set(outUV.z, outUV.w);
            m_vUVLU.Set(outUV.x, outUV.y);
            m_vUVLD.Set(outUV.x, outUV.w);

            //颜色
            m_vColorLD=m_vColorLU=m_vColorRD=m_vColorRU=Color.white;
        }



        public void SliceFill(int width,int height,float offsetX,float offsetY,float uvL, float uvT, float uvR, float uvB){
            m_cChar='\0';
            m_fScale=1f;

            float fl=offsetX;
            float fb=offsetY;
            float fr=offsetX+width;
            float ft=offsetY+height;

            m_vVertexRU.Set(fr,fb);//右上角 widht,0
            m_vVertexRD.Set(fr,ft);//右下角 width,height
            m_vVertexLU.Set(fl,fb);//左上角 0,0
            m_vVertexLD.Set(fl,ft);//左下角 0,height
        
            m_vUVRU.Set(uvR, uvT);
            m_vUVRD.Set(uvR, uvB);
            m_vUVLU.Set(uvL, uvT);
            m_vUVLD.Set(uvL, uvB);
            
            //颜色
            m_vColorLD=m_vColorLU=m_vColorRD=m_vColorRU=Color.white;

            if(m_kHudMesh!=null){
                 m_kHudMesh.VertexDirty();
            }

        }
    }
}

