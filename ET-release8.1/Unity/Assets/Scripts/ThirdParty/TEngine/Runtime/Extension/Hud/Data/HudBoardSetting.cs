using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using XEngine.Utilities;

namespace XEngine.Hud{
    public class HudBoardSetting : Singleton<HudBoardSetting>
    {

        public float m_fAllWidth=720f;
        public float m_fAllHeight = 1280f;
        public float m_fDurationTime=2.0f;//动画持续时间
        public float m_fCallbackTime=1.0f;
        //最大最小缩放
        public float m_fTitleScaleMin = 0.9f;
        public float m_fTitleScaleMax = 0.9f;

        public float m_fNumberScaleMin = 0.4f;
        public float m_fNumberScaleMax = 0.4f;
        public float CameraNearDist = 6.5f;
        public float CameraFarDist = 60.0f;

        public float m_fTitleOffsetY = 0.5f;//所有title通用高度
    
        public int m_iBloodRed;
        public int m_iBloodGreen;
        public int m_iBloodBlue;
        public int m_iBloodBkWidth;
        public int m_iBloodBkHeight;
        public int m_iBloodWidth;
        public int m_iBloodHeight;

        public HudAnimeAttribute[] NumberAttributes;//hud show number
        public HudTitleLabelSet[] TitleSets;//title number

        public float m_fTestBloodPos = 1.0f;


        public bool m_bHideAllTitle = false;

        public HudAniSetting m_kHudAniSetting;
        public SpriteAtlasConfig m_kSpriteAtlasConfig;

        public UIFont m_kFont;
        public Shader m_kNumberShader;
        public Shader m_kSpriteShader;

        private HudObject m_kHudObject;
       // private ResHandle m_kHudObjectHandle;
        //bundle 方式
        public void BuildBundle() {
            var obj= ModuleSystem.GetModule<IResourceModuleET>().LoadGameObject("Assets/Bundles/Tools/HudTools.prefab");
            m_kHudObject=obj.gameObject.GetComponent<HudObject>();
            this.Build(m_kHudObject.m_kConfig,m_kHudObject.m_kHudAniSetting,m_kHudObject.m_kFont);
        }
        //塞入方式
        public void Build(SpriteAtlasConfig config,HudAniSetting hudAniSetting,UIFont uiFont) {
            NumberAttributes = new HudAnimeAttribute[(int)Enum_NumberRender_Type.HUD_SHOW_NUMBER];
            TitleSets = new HudTitleLabelSet[(int)Enum_HudTitleType.Count];
            m_kSpriteAtlasConfig = config;
            m_kFont = uiFont;

            var texture2D= ModuleSystem.GetModule<IResourceModuleET>().LoadAsset<Texture2D>("Assets/Bundles/Tools/Hud_Test.png");
            if (texture2D!=null)//防止texture引用丢失
            {
                m_kSpriteAtlasConfig.m_kTexture=texture2D;
                Log.Debug("Build "+texture2D.name+" Hud!!!");
            }
            ApplySetting(hudAniSetting);
        }


        
        private void ApplySetting(HudAniSetting hudAniSetting){
            if(hudAniSetting==null){
                return;
            }
            m_kHudAniSetting = hudAniSetting;

            //目前跳字动画先用一样的
            NumberAttributes[(int)Enum_NumberRender_Type.HUD_SHOW_HP_HURT] = hudAniSetting.HurtAnimAttibute;
            NumberAttributes[(int)Enum_NumberRender_Type.HUD_SHOW_HP_ADD] = hudAniSetting.RecoverAnimAttibute;
            NumberAttributes[(int)Enum_NumberRender_Type.HUD_SHOW_TIP_NUM] = hudAniSetting.HurtAnimAttibute;
            NumberAttributes[(int)Enum_NumberRender_Type.HUD_SHOW_HP_HURT_NEW] = hudAniSetting.HurtAnimAttibute;




            TitleSets[(int)Enum_HudTitleType.PlayerTitle]=new HudTitleLabelSet(hudAniSetting.PlayerTitle);
            TitleSets[(int)Enum_HudTitleType.PlayerCorp] =new HudTitleLabelSet(hudAniSetting.PlayerCorp);
            TitleSets[(int)Enum_HudTitleType.MonsterName] = new HudTitleLabelSet(hudAniSetting.MonsterTitle);
            TitleSets[(int)Enum_HudTitleType.Blood] = new HudTitleLabelSet(hudAniSetting.Blood);
            TitleSets[(int)Enum_HudTitleType.HeadIcon] = new HudTitleLabelSet(hudAniSetting.HeadIcon);

            m_fDurationTime=hudAniSetting.m_fDurationTime;
            m_fCallbackTime=hudAniSetting.m_fCallbackTime;

            m_iBloodBkWidth = hudAniSetting.m_iBloodBkWidth;
            m_iBloodBkHeight = hudAniSetting.m_iBloodBkHeight;
            m_iBloodWidth=hudAniSetting.m_iBloodWidth;
            m_iBloodHeight = hudAniSetting.m_iBloodHeight;
            m_kNumberShader = hudAniSetting.m_kNumberShader;
            m_kSpriteShader=hudAniSetting.m_kSpriteShader;
        }
    }

}
