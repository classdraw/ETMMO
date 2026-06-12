namespace ET.Server
{
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    public static class ActionsHelper
    {
        public static Actions CreateActions(this ActionsTempComponent self,int configId)
        {
            return self.AddChild<Actions,int>(configId);
        }
        
        public static Actions CreateActions(this Cast cast,int configId,Unit owner,ActionsRunType actionsRunType,bool autoRun=true,bool autoDispose=true)
        {
            Actions actions = cast.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Caster = cast.Caster;
            actions.Owner = owner;
            RunActions(actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }

            return actions;
        }

        public static void RunActions(Actions actions,ActionsRunType actionsRunType,bool autoRun=true,bool autoDispose=true)
        {
            if (autoRun)
            {
                if (autoDispose)
                {
                    using (actions)//using会自动释放
                    {
                        RunActions(actions, actionsRunType);
                    }
                }
                else
                {
                    RunActions(actions, actionsRunType);
                }
            }
        }

        public static void RunActions(Actions actions,ActionsRunType actionsRunType)
        {
            
        }

    }
}

