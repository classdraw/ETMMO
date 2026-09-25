namespace ET.Server
{
    [Actions(ActionsType.NumericChange)]
    [FriendOf(typeof(Actions))]
    public class Actions_NumericChange : IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 2)
            {
                Log.Error($"Actions_NumericChange ActionsParam invalid: configId={config.Id}");
                return;
            }

            int numericType = config.ActionsParam[0];
            int numericValue = config.ActionsParam[1];

            using (ListComponent<Unit> targets = ListComponent<Unit>.Create())
            {
                actions.CollectActionTargets(actionsRunType, targets);
                foreach (Unit owner in targets)
                {
                    ApplyNumericChange(owner, actionsRunType, numericType, numericValue);
                }
            }
        }

        private static void ApplyNumericChange(Unit owner, ActionsRunType actionsRunType, int numericType, int numericValue)
        {
            NumericComponent numericComponent = owner.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return;
            }

            switch (actionsRunType)
            {
                case ActionsRunType.CastHit:
                case ActionsRunType.BuffAdd:
                {
                    numericComponent[numericType] += numericValue;
                    break;
                }
                case ActionsRunType.BuffRemove:
                {
                    numericComponent[numericType] -= numericValue;
                    break;
                }
            }
        }
    }
}
