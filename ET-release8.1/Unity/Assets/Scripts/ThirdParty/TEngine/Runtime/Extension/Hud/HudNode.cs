using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using XEngine.Utilities;

namespace XEngine.Hud {
    public class HudNode : MonoBehaviour
    {
        private int m_iTitleUUID = 0;

        public string m_sName="AAABBB";

        private bool m_bIsDirty;
        private static bool m_bFirst=true;
        public void TryRegisterTitle() {
            if (m_iTitleUUID == 0) {
                m_iTitleUUID=HudTitleRender.GetInstance().RegisterTitle(transform, GetAllOffsetY(), m_bFirst);
                if (m_bFirst) {
                    m_bFirst = false;
                }
                Log.Debug("注册成功:"+m_iTitleUUID);
            }
        }

        public void TryUnRegisterTitle() {
            if (m_iTitleUUID == 0)
            {
                return;
            }

            var titleUuid = m_iTitleUUID;
            m_iTitleUUID = 0;
            if (Singleton<HudTitleRender>.HasInstance())
            {
                HudTitleRender.GetInstance().ReleaseTitle(titleUuid);
                Log.Debug("注销成功:" + titleUuid);
            }
        }

        private void RefreshTest() { 
            TryRegisterTitle();
            HudTitleInfo titleInfo = HudTitleRender.GetInstance().GetTitle(m_iTitleUUID);
            titleInfo.Clear();

            titleInfo.SetOffsetY(GetHeadNameOffsetY());
            titleInfo.ShowTitle();

            hpLv = Random.Range(0f, 1f);
            var r = (Enum_HudBloodType)Random.Range(1,4);
            titleInfo.BeginTitle();
            titleInfo.PushBlood(r, hpLv);
            titleInfo.EndTitle();
            
            //文本显示
            titleInfo.BeginTitle();
            titleInfo.PushTitle(m_sName, Enum_HudTitleType.PlayerTitle, 0);
            titleInfo.EndTitle();

            
            //工会标显示
            titleInfo.BeginTitle();
            titleInfo.PushTitle("工会X", Enum_HudTitleType.PlayerCorp, 0);
            titleInfo.EndTitle();
            titleInfo.BeginTitle();
            titleInfo.PushIcon(Enum_HudTitleType.HeadIcon, Random.Range(0,2));
            titleInfo.EndTitle();




            //titleInfo.pus
            Log.Debug("RefreshTest");
        }

        private void Awake()
        {
            RefreshTest();
        }

        private float hpLv = 1.0f;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                RefreshTest();
                //HudNumberRender.GetInstance().ShowHurtNumber(transform, Enum_NumberRender_Type.HUD_SHOW_HP_HURT, Random.Range(1, 999), true, true, true);
            }
            else if (Input.GetKeyDown(KeyCode.H)) {
                TryUnRegisterTitle();
            } else if (Input.GetKeyDown(KeyCode.W)) {
                SetBloodPos(Random.Range(0f, 1f));
            } else if (Input.GetKeyDown(KeyCode.P)) {
                HudNumberRender.GetInstance().ShowHurtNumber(transform,Enum_NumberRender_Type.HUD_SHOW_HP_Crit,1000,true);
            }
            
            if (m_iTitleUUID!=0) {
                hpLv -= UnityEngine.Time.deltaTime * 5f;
                if (hpLv<0f) {
                    hpLv = 1f;
                }
                SetBloodPos(hpLv);
            }

            m_fTimeNumber -= UnityEngine.Time.deltaTime;
            if (m_fTimeNumber<=0f) {
                Enum_NumberRender_Type r = (Enum_NumberRender_Type)Random.Range(0, 4);
                
                m_fTimeNumber = 0.3f;
                HudNumberRender.GetInstance().ShowHurtNumber(transform, r, Random.Range(-9999, 9999), true);
            }


        }
        private float m_fTimeNumber = 0.3f;
        private float GetAllOffsetY() {
            return 1.8f;
        }
        private float GetHeadNameOffsetY() {
            return 0.5f;
        }

        public void SetBloodPos(float pos) {
            HudTitleInfo titleInfo = HudTitleRender.GetInstance().GetTitle(m_iTitleUUID);
            titleInfo.SetBloodPos(pos);
        }


    }

}
