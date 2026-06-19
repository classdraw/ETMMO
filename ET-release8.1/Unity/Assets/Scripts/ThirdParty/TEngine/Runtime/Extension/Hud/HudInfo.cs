using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEngine.Utilities;
namespace XEngine.Hud
{


    public class HudInfo : MonoBehaviour
    {
        private int m_iTitleUUID = 0;

        private GameObject m_kTarget;

        public void Init(GameObject target)
        {
            m_kTarget = target;
            TryRegisterTitle();
        }

        public void Release() {
            TryUnRegisterTitle();
        }

        public void DisplayPlayer(string sname) {
            TryRegisterTitle();
            HudTitleInfo titleInfo = HudTitleRender.GetInstance().GetTitle(m_iTitleUUID);
            titleInfo.Clear();

            titleInfo.SetOffsetY(1.5f);
            titleInfo.ShowTitle();

            titleInfo.BeginTitle();
            titleInfo.PushBlood(Enum_HudBloodType.Blood_Green, 1f);
            titleInfo.EndTitle();

            titleInfo.BeginTitle();
            titleInfo.PushTitle(sname, Enum_HudTitleType.PlayerTitle, 0);
            titleInfo.EndTitle();
        }
        public void DisplayMonster(string sname)
        {
            TryRegisterTitle();
            HudTitleInfo titleInfo = HudTitleRender.GetInstance().GetTitle(m_iTitleUUID);
            titleInfo.Clear();

            titleInfo.SetOffsetY(1.5f);
            titleInfo.ShowTitle();

            titleInfo.BeginTitle();
            titleInfo.PushBlood(Enum_HudBloodType.Blood_Green, 1f);
            titleInfo.EndTitle();

            titleInfo.BeginTitle();
            titleInfo.PushTitle(sname, Enum_HudTitleType.PlayerTitle, 0);
            titleInfo.EndTitle();
        }

        public void RefreshHpLv(float lv) {
            TryRegisterTitle();
            HudTitleInfo titleInfo = HudTitleRender.GetInstance().GetTitle(m_iTitleUUID);
            if (titleInfo!=null) {
                titleInfo.SetBloodPos(lv);
            }
        }
        void LateUpdate()
        {
            if (m_kTarget!=null) { 
                this.transform.position=m_kTarget.transform.position;
            }
        }


        private void TryRegisterTitle()
        {
            if (m_iTitleUUID == 0)
            {
                m_iTitleUUID = HudTitleRender.GetInstance().RegisterTitle(transform, 0f, false);
            }
        }
        private void TryUnRegisterTitle()
        {
            if (m_iTitleUUID == 0)
            {
                return;
            }

            var titleUuid = m_iTitleUUID;
            m_iTitleUUID = 0;
            if (Singleton<HudTitleRender>.HasInstance())
            {
                HudTitleRender.GetInstance().ReleaseTitle(titleUuid);
            }
        }
    }

}
