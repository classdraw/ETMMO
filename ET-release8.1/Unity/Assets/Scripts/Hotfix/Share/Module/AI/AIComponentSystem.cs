using System;

namespace ET
{
    [EntitySystemOf(typeof(AIComponent))]
    [FriendOf(typeof(AIComponent))]
    [FriendOf(typeof(AIDispatcherComponent))]
    [FriendOfAttribute(typeof(ET.XunLuoPathComponent))]
    public static partial class AIComponentSystem
    {
        [Invoke(TimerInvokeType.AITimer)]
        public class AITimer : ATimer<AIComponent>
        {
            protected override void Run(AIComponent self)
            {
                try
                {
                    self.Check();
                }
                catch (Exception e)
                {
                    Log.Error($"ai timer error: {self.Id}\n{e}");
                }
            }
        }

        [EntitySystem]
        private static void Awake(this AIComponent self, int aiConfigId)
        {
            self.AIConfigId = aiConfigId;
            self.Timer = self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(1000, TimerInvokeType.AITimer, self);
        }

        [EntitySystem]
        private static void Destroy(this AIComponent self)
        {
            self.Root().GetComponent<TimerComponent>()?.Remove(ref self.Timer);
            self.Current = 0;
        }

        [EntitySystem]
        private static void Update(this AIComponent self)
        {
            if (self.Current == 0)
            {
                return;
            }

            if (!AIConfigCategory.Instance.AIConfigs.TryGetValue(self.AIConfigId, out var nodeConfigs))
            {
                self.Current = 0;
                return;
            }

            if (!nodeConfigs.TryGetValue(self.Current, out AIConfig aiConfig))
            {
                self.Current = 0;
                return;
            }

            AAIHandler aaiHandler = AIDispatcherComponent.Instance.Get(aiConfig.Name);
            if (aaiHandler == null)
            {
                Log.Error($"not found aihandler: {aiConfig.Name}");
                self.Current = 0;
                return;
            }

            try
            {
                if (aaiHandler.Update(self, aiConfig) != 0)
                {
                    self.Current = 0;
                }
            }
            catch (Exception e)
            {
                Log.Error($"ai update error: {self.Id} node={self.Current}\n{e}");
                self.Current = 0;
            }
        }

        private static void Check(this AIComponent self)
        {
            Fiber fiber = self.Fiber();
            if (self.Parent == null)
            {
                fiber.Root.GetComponent<TimerComponent>().Remove(ref self.Timer);
                return;
            }

            var oneAI = AIConfigCategory.Instance.AIConfigs[self.AIConfigId];

            foreach (AIConfig aiConfig in oneAI.Values)
            {
                AAIHandler aaiHandler = AIDispatcherComponent.Instance.Get(aiConfig.Name);

                if (aaiHandler == null)
                {
                    Log.Error($"not found aihandler: {aiConfig.Name}");
                    continue;
                }

                int ret = aaiHandler.Check(self, aiConfig);
                if (ret != 0)
                {
                    continue;
                }

                if (self.Current == aiConfig.Id)
                {
                    continue;
                }

                if (self.Current != 0)
                {
                    self.OnSwitchNode();
                }

                self.Current = aiConfig.Id;
                return;
            }
        }

        private static void OnSwitchNode(this AIComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            unit.GetComponent<MoveComponent>()?.Stop(false);
            XunLuoPathComponent path = unit.GetComponent<XunLuoPathComponent>();
            if (path != null)
            {
                path.PatrolPhase = 0;
            }
        }
    }
}
