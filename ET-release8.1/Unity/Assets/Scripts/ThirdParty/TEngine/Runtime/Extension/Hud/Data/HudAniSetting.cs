using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XEngine.Hud{
    [CreateAssetMenu(fileName ="HudAniSetting",menuName = "Scriptable Objects/hud数据",order =10)]
    public class HudAniSetting : ScriptableObject
    {
        public float m_fDurationTime=2.0f;//动画持续时间
        public float m_fCallbackTime=1.0f;

        [Header("血条参数")]
        public int m_iBloodBkWidth;
        public int m_iBloodBkHeight;
        public int m_iBloodWidth;
        public int m_iBloodHeight;
        public Shader m_kNumberShader;
        public Shader m_kSpriteShader;

        [Header("Title样式")]
        //一些限制参数
        public HudTitleAttribute PlayerCorp;//玩家名称
        public HudTitleAttribute Blood;//血条
        public HudTitleAttribute HeadIcon;//头像
        public HudTitleAttribute[]PlayerTitle;//玩家
        public HudTitleAttribute[]MonsterTitle;//玩家

        //跳字相关
        [Header("跳字动画")]
        public HudAnimeAttribute HurtAnimAttibute;//受伤动画

        public HudAnimeAttribute RecoverAnimAttibute;//恢复动画
    }

}
