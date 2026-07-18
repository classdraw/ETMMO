namespace ET.Client
{
    /// <summary>
    /// 客户端监视 Hp 数值变化，更新 HUD 血条。
    /// </summary>
    [NumericWatcher(SceneType.Current, NumericType.Hp)]
    public class NumericWatcher_Hp_ShowUI : INumericWatcher
    {
        public void Run(Unit unit, NumbericChange args)
        {
            if (args.New<args.Old)//血变少了
            {
                if (unit.GetHpLv()<=0f)
                {
                    //死亡
                    unit.GetComponent<AnimatorComponent>()?.Play(MotionType.Death,1.0f);
                }
                else
                {
                    unit.GetComponent<AnimatorComponent>()?.Play(MotionType.Hit,1.0f);
                }
            }
            else
            {
                //加血
            }


            unit.GetComponent<UnitTopUIComponent>()?.RefreshHpBar();
        }
    }
}
