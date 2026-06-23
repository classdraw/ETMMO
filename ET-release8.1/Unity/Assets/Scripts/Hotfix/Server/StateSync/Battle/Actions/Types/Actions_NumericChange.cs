namespace ET.Server
{
    [Actions(ActionsType.NumericChange)]
    [FriendOf(typeof(Actions))]
    public class Actions_NumericChange:IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            Unit owner = actions.Owner;
            if (owner==null||owner.IsDisposed)
            {
                return;
            }
            //第一个参数就是改变数值类型
            int numericType = actions.Config.ActionsParam[0];
            int numericValue = actions.Config.ActionsParam[1];
            switch (actionsRunType)
            {
                case ActionsRunType.CastHit:
                case ActionsRunType.BuffAdd:
                {
                    //根据参数增加或者减少对应的属性数值
                    NumericComponent numericComponent = owner.GetComponent<NumericComponent>();
                    numericComponent[numericType] += numericValue;
                    break; 
                }
                case ActionsRunType.BuffRemove:
                {
                    NumericComponent numericComponent = owner.GetComponent<NumericComponent>();
                    numericComponent[numericType] -= numericValue;
                    break;
                }
                default:
                    break;
            }
        }
    }
}

