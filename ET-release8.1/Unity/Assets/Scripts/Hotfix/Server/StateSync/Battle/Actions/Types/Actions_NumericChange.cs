namespace ET.Server
{
    [Actions(ActionsType.NumericChange)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Buff))]
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
            bool subtract = actionsRunType == ActionsRunType.BuffRemove;

            using (ListComponent<Unit> targets = ListComponent<Unit>.Create())
            {
                actions.CollectActionTargets(actionsRunType, targets);
                foreach (Unit owner in targets)
                {
                    NumericComponent numeric = owner.GetComponent<NumericComponent>();
                    if (numeric == null)
                    {
                        continue;
                    }

                    if (actionsRunType == ActionsRunType.BuffAdd || actionsRunType == ActionsRunType.BuffRemove)
                    {
                        Buff buff = actions.BuffSelf;
                        long before = numeric[numericType];
                        UnitControlHelper.ApplyNumericRef(numeric, numericType, numericValue, subtract);
                        long after = numeric[numericType];
                        if (buff != null && !buff.IsDisposed)
                        {
                            BuffLogHelper.LogNumericChange(buff, owner, config.Id, numericType, before, after, actionsRunType);
                        }
                    }
                    else
                    {
                        numeric[numericType] += numericValue;
                    }
                }
            }
        }
    }
}