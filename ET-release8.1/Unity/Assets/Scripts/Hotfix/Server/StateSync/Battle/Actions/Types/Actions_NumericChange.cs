namespace ET.Server
{
    [Actions(ActionsType.NumericChange)]
    [FriendOf(typeof(Actions))]
    public class Actions_NumericChange:IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Unit owner = actions.Owner;
            if (owner == null || owner.IsDisposed || !owner.IsBattleUnit())
            {
                return;
            }

            ActionsConfig config = actions.Config;
            if (config.ActionsParam == null || config.ActionsParam.Length < 2)
            {
                Log.Error($"Actions_NumericChange ActionsParam invalid: configId={config.Id}");
                return;
            }

            int numericType = config.ActionsParam[0];
            int numericValue = config.ActionsParam[1];
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
                default:
                    break;
            }
        }
    }
}
