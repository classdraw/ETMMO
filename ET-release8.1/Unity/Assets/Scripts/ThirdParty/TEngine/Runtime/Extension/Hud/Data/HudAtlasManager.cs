using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using XEngine.Utilities;

namespace XEngine.Hud{
    public class HudAtlasManager : Singleton<HudAtlasManager>
    {
        protected Dictionary<string,UISpriteInfo> m_kAllSprites=new Dictionary<string, UISpriteInfo>();
        protected Dictionary<string,UITextureInfo> m_kAllTextures=new Dictionary<string, UITextureInfo>();
        protected Dictionary<int,UITextureInfo> m_kQueryAtlas=new Dictionary<int, UITextureInfo>();
        protected CMyArray<UITextureInfo> m_kAtlasPtr=new CMyArray<UITextureInfo>();
        protected CMyArray<UISpriteInfo> m_kSpritePtr=new CMyArray<UISpriteInfo>();
        protected CMyArray<int> m_kNeedReleaseAtlas=new CMyArray<int>();
        protected UISpriteInfo m_kDefSprite=new UISpriteInfo();
        protected float m_fNextUpdateTime=0f;

        /**
            public bool IsForceOneTexture()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // PC平台强制使用一张纹图
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return IsEditorMode();
        else if(Application.platform == RuntimePlatform.WindowsPlayer)
            return true;// IsEditorMode();
#endif
        return false;
    }*/
        public bool IsForceOneTexture()
        {
            return true;
        }
        static int s_iLowQuality=0;//是否低画质
        public static bool IsLowQuality(){
            if(s_iLowQuality==0){
                s_iLowQuality=SystemInfo.systemMemorySize<=1024?1:2;
            }
            return s_iLowQuality==1;
        }

        public static void SetQuality(int quality){
            s_iLowQuality=quality+1;
        }

        //public int GetAtlasRefBySpriteId(){
            // UITextureInfo atlas=Fastgeta
        //}

        public UITextureInfo FastGetAtlasBySpriteId(int spriteId){
            if(m_kSpritePtr.IsValid(spriteId-1)){
                UISpriteInfo sprite=m_kSpritePtr[spriteId-1];
                if(sprite!=null){
                    if(m_kAtlasPtr.IsValid(sprite.m_iAtlasID-1)){
                        return m_kAtlasPtr[sprite.m_iAtlasID-1];
                    }
                }
            }
            return null;
        }
        public UITextureInfo GetTextureInfoById(int atlasId){
            if(m_kAtlasPtr.IsValid(atlasId-1)){
                return m_kAtlasPtr[atlasId-1];
            }
            return null;
        }

        // 功能：申请图集的资源
        public void QueryTextureInfoByID(int atlasId)
        {
            UITextureInfo atlas = GetTextureInfoById(atlasId);
            QueryByAtlas(atlas);
        }

        public UITextureInfo GetTextureInfoByName(string atlasName)
        {
            if (string.IsNullOrEmpty(atlasName))
                return null;
            UITextureInfo atlas = null;
            if (m_kAllTextures.TryGetValue(atlasName, out atlas))
            {
            }
            return atlas;
        }

        public string GetAtlasNameBySpriteName(string spriteName){
            if (string.IsNullOrEmpty(spriteName))
                return null;

            UISpriteInfo sprite = null;
            if (m_kAllSprites.TryGetValue(spriteName, out sprite))
            {
                return sprite.m_sAtlasName;
            }
            return null;
        }

        public int GetAtlasIdBySpriteId(int spriteId){
            UISpriteInfo sprite=GetSpriteById(spriteId);
            if(sprite!=null){
                return sprite.m_iAtlasID;
            }
            return 0;
        }
        //
        public int SpriteNameToId(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
                return 0;
            UISpriteInfo sprite = null;
            if (m_kAllSprites.TryGetValue(spriteName, out sprite))
            {
                return sprite.m_iNameID;
            }
            return 0;
        }
        public UISpriteInfo GetSpriteById(int nSpriteID)
        {
            if (m_kSpritePtr.IsValid(nSpriteID - 1))
                return m_kSpritePtr[nSpriteID - 1];
            return null;
        }

        private void QueryByAtlas(UITextureInfo textureInfo)
        {
            if (textureInfo != null)
            {
                if (textureInfo.m_kMaterial == null || textureInfo.m_kMaterial.mainTexture == null)
                    QueryAtlasTex(textureInfo);
                textureInfo.m_iRef++;
                m_kQueryAtlas[textureInfo.m_iAtlasId] = textureInfo;
            }
        }
        //解决一个textureinfo 材质或者texture没有的问题 进行装填
        protected virtual void QueryAtlasTex(UITextureInfo textureInfo){
            string shaderName=textureInfo.m_sShaderName;
            if(textureInfo.m_kMaterial==null){
                if(IsForceOneTexture()){
                    // PC平台强制使用一张纹图
                    shaderName=shaderName.Replace(" MainAlpha","");
                }
                Shader shader=Shader.Find(shaderName);
                textureInfo.m_kMaterial=new Material(shader);
                textureInfo.m_kMaterial.name=textureInfo.m_sAtlasName;
            }

            if(textureInfo.m_kMaterial.mainTexture==null){
                textureInfo.m_kMaterial.mainTexture=CreateDefaultTexture();
                if(shaderName=="Unlit/Transparent Colored MainAlpha"){
                    textureInfo.m_kMaterial.SetTexture("_MainAlpha", textureInfo.m_kMaterial.mainTexture);
                }
            }//if

        }

        public UISpriteInfo GetSafeSpriteById(int spriteId){
            UISpriteInfo sprite=GetSpriteById(spriteId);
            if(sprite!=null){
                return sprite;
            }
            return m_kDefSprite;
        }

        public void ReleaseTextureInfoById(int atlasId){
            UITextureInfo textureInfo=GetTextureInfoById(atlasId);

        }

        private void ReleaseByTextureInfo(UITextureInfo atlas){
            if(atlas!=null){
                atlas.m_iRef--;
                if(atlas.m_iRef<=0){
                    //从前面添加到pool
                    PushReleaseTextureInfoId(atlas.m_iAtlasId);
                    //做标记
                    atlas.m_fReleaseTime=UnityEngine.Time.time;
                    atlas.m_iVersion++;
                    m_kQueryAtlas.Remove(atlas.m_iAtlasId);

                    if(IsLowQuality()&&m_kNeedReleaseAtlas.size()>5){
                        m_fNextUpdateTime = 0.0f;
                    }

                }
            }
        }

        private void PushReleaseTextureInfoId(int atlasId){
            for(int i=0,iLen=m_kNeedReleaseAtlas.size();i<iLen;++i){
                if(m_kNeedReleaseAtlas[i]==atlasId){
                    return;
                }

            }
            m_kNeedReleaseAtlas.push_front(atlasId);
        }
        // 功能：创建一个1*1大小的纹理
        Texture2D CreateDefaultTexture()
        {
            Texture2D tex = new Texture2D(1, 1);
            Color32[] newPixels = new Color32[1];
            newPixels[0] = new Color32(0, 0, 0, 0);
            tex.SetPixels32(newPixels);
            tex.Apply();
            return tex;
        }



        //新的图集数据
        protected Dictionary<int, UISpriteSimple> m_kNew_AllSprites = new Dictionary<int, UISpriteSimple>();

        public void Build() {
            
            m_kNew_AllSprites.Clear();
            for (var i=0; i< HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.m_kSprites.Count;i++) {
                var data = HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.m_kSprites[i];
                var spriteSimple = new UISpriteSimple();
                spriteSimple.m_iIndex=data.m_iIndex;
                spriteSimple.m_sName = data.m_sSpriteName;
                spriteSimple.m_kUV = data.m_kUV;
                spriteSimple.m_kRect = data.m_kRect;
                spriteSimple.m_kBorder = data.m_kBorder;
                m_kNew_AllSprites.Add(spriteSimple.m_iIndex,spriteSimple);

            }

            Debug.Log("spriteCount:" + m_kNew_AllSprites.Count);
        }

        public UISpriteSimple GetSpriteSimpleByIndex(int index) {
            if (!m_kNew_AllSprites.ContainsKey(index)) {
                return null;
            }
            return m_kNew_AllSprites[index];
        }

        public UISpriteSimple GetSpriteSimpleByName(string name) {
            foreach (var kvp in m_kNew_AllSprites) {
                if (kvp.Value.m_sName.Equals(name)) {
                    return kvp.Value;
                }
            }
            return null;
        }

        public Texture2D GetTextureByAtlasId(int id)
        {
            if (HudBoardSetting.GetInstance().m_kSpriteAtlasConfig != null)
            {
                return HudBoardSetting.GetInstance().m_kSpriteAtlasConfig.m_kTexture;
            }
            return null;
        }

        public void ReleaseTextureByAtlasId(int id) {
            int a = 0;
        }
    }
}
