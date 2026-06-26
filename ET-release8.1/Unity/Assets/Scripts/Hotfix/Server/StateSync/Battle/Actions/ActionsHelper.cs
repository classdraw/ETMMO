namespace ET.Server
{
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(Buff))]
    public static class ActionsHelper
    {
        public static IActions GetIActions(Scene scene, int actionsType)
        {
            ActionsDispatcherComponent actionsDispatcherComponent = scene.GetComponent<ActionsDispatcherComponent>();
            if (actionsDispatcherComponent == null)
            {
                return null;
            }

            return actionsDispatcherComponent.Get(actionsType);
        }

        public static Actions CreateActions(this ActionsTempComponent self, int configId)
        {
            return self.AddChild<Actions, int>(configId);
        }

        public static Actions CreateActions(this BulletComponent bulletComponent,int configId,Unit owner,Unit caster, ActionsRunType actionsRunType, bool autoRun = true, bool autoDispose = true)
        {
            //owner是目标
            Actions actions = bulletComponent.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Caster = caster;
            actions.Owner = owner;
            RunActions(bulletComponent.Root(), actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }

            return actions;
        }

        public static Actions CreateActions(this Cast cast, int configId, Unit owner, ActionsRunType actionsRunType, bool autoRun = true, bool autoDispose = true)
        {
            Actions actions = cast.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Caster = cast.Caster;
            actions.Owner = owner;
            RunActions(cast.Root(), actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }

            return actions;
        }
        public static Actions CreateActions(this Buff buff, int configId, ActionsRunType actionsRunType, bool autoRun = true, bool autoDispose = true)
        {
            Actions actions = buff.GetComponent<ActionsTempComponent>().CreateActions(configId);
            actions.Owner = buff.Owner;
            RunActions(buff.Root(), actions, actionsRunType, autoRun, autoDispose);
            if (actions.IsDisposed)
            {
                return null;
            }

            return actions;
        }
        

        public static void RunActions(Scene scene, Actions actions, ActionsRunType actionsRunType, bool autoRun = true, bool autoDispose = true)
        {
            if (!autoRun)
            {
                return;
            }

            if (autoDispose)
            {
                using (actions)//using会自动释放
                {
                    RunActions(scene, actions, actionsRunType);
                }
            }
            else
            {
                RunActions(scene, actions, actionsRunType);
            }
        }

        public static void RunActions(Scene scene, Actions actions, ActionsRunType actionsRunType)
        {
            IActions iActions = GetIActions(scene, actions.Config.Type);
            if (iActions == null)
            {
                Log.Error($"Actions not found: {actions.ConfigId}");
                return;
            }

            iActions.Run(actions, actionsRunType);
        }
    }
}
