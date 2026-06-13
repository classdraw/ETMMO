using System;
using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(ActionsDispatcherComponent))]
    [FriendOf(typeof(ActionsDispatcherComponent))]
    public static partial class ActionsDispatcherComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.ActionsDispatcherComponent self)
        {
            self.ActionsDict.Clear();
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof(ActionsAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(ActionsAttribute), false);
                if (attrs.Length==0)
                {
                    continue;
                }
                ActionsAttribute actionsAttribute = (ActionsAttribute)attrs[0];
                IActions actions = Activator.CreateInstance(type) as IActions;
                if (actions == null)
                {
                    Log.Error($"Actions handler is not IActions: {type.Name}");
                    continue;
                }
                self.ActionsDict.Add(actionsAttribute.ActionsType, actions);
            }
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.ActionsDispatcherComponent self)
        {
            self.ActionsDict.Clear();
        }

        public static IActions Get(this ET.Server.ActionsDispatcherComponent self,int actionsType)
        {
            if (self.ActionsDict.ContainsKey(actionsType))
            {
                return self.ActionsDict[actionsType];
            }

            return null;
        }
    }
}

