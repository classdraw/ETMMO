namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    [FriendOf(typeof(ClientSkillStatusComponent))]
    public class M2C_CoolDownChangeHandler: MessageHandler<Scene,M2C_CoolDownChange>
    {
        protected override async ETTask Run(Scene root, M2C_CoolDownChange message)
        {
            if (message.CastConfigIds == null || message.CastConfigIds.Count == 0)
            {
                return;
            }

            Scene currentScene = root.CurrentScene();
            if (currentScene == null)
            {
                return;
            }

            Unit unit = UnitHelper.GetMyUnitFromCurrentScene(currentScene);
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            ClientSkillStatusComponent clientSkillStatusComponent = unit.GetComponent<ClientSkillStatusComponent>()
                ?? unit.AddComponent<ClientSkillStatusComponent>();
            clientSkillStatusComponent.ApplyCoolDownChange(message);

            int count = message.CastConfigIds.Count;
            for (int i = 0; i < count; i++)
            {
                int castConfigId = message.CastConfigIds[i];
                long coolDownEndTime = message.CoolDownTimes[i];
                long coolDownStartTime = message.CoolDownStartTimes[i];
                Log.Console($"[CoolDown] 玩家 {unit.Id} 技能 {castConfigId} CD变更 StartTime={coolDownStartTime} EndTime={coolDownEndTime}");

                CoolDownChange coolDownChange = new CoolDownChange
                {
                    Unit = unit,
                    CastConfigId = castConfigId,
                    CoolDownEndTime = coolDownEndTime,
                    CoolDownStartTime = coolDownStartTime,
                };
                EventSystem.Instance.Publish(currentScene, coolDownChange);
            }

            await ETTask.CompletedTask;
        }
    }
}
