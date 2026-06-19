using XEngine.Utilities;

namespace XEngine.Hud {
    public class HudFacade : MonoSingleton<HudFacade>
    {
        private bool m_Init = false;
        public void Build() {
            if (!this.m_Init)
            {
                this.m_Init = true;
                HudBoardSetting.GetInstance().BuildBundle();
                HudAtlasManager.GetInstance().Build();
                HudNumberRender.GetInstance().Build();
                HudTitleRender.GetInstance().Build();
            }


        }

        public void EnterGame() {
            HudTitleRender.GetInstance().EnterGame();
            HudNumberRender.GetInstance().EnterGame();
        }

        public void LeaveGame() {
            HudTitleRender.GetInstance().LeaveGame();
            HudNumberRender.GetInstance().LeaveGame();
        }
        private void Update()
        {
            HudTitleRender.GetInstance().Tick();
            HudNumberRender.GetInstance().Tick();
        }
    }

}
