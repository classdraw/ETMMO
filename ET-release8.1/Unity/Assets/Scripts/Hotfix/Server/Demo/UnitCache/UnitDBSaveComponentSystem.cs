using System;

namespace ET.Server
{
    [Invoke(TimerInvokeType.SaveChangeDBDate)]
    public class UnitDBSaveComponentTimer : ATimer<UnitDBSaveComponent>
    {
        protected override void Run(UnitDBSaveComponent self)
        {
            try
            {

                self?.SaveChange().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
    
    [EntitySystemOf(typeof(UnitDBSaveComponent))]
    [FriendOfAttribute(typeof(ET.Server.UnitDBSaveComponent))]
    public static partial class UnitDBSaveComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.UnitDBSaveComponent self)
        {

            self.Timer = self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(10 * 1000, TimerInvokeType.SaveChangeDBDate, self);
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.UnitDBSaveComponent self)
        {

            self.Root().GetComponent<TimerComponent>().Remove(ref self.Timer);
        }
        
        public static void AddToBytes(this UnitDBSaveComponent self, Type type, byte[] bytes)
        {
            self.Bytes[type] = bytes;
        }
        
        public static void AddChange(this UnitDBSaveComponent self, Type type)
        {
            self.ComponentTypes.Add(type);
            if (typeof(IUnitCache).IsAssignableFrom(type))
            {
                self.EntityChangeTypeSet.Add(type);
            }
            else if (typeof(ITransfer).IsAssignableFrom(type))
            {
                self.TransferChanges.Add(type);
            }
          
        }
        
        public static async ETTask SaveChange(this UnitDBSaveComponent self)
        {
            CoroutineLockComponent coroutineLockComponent = self.Root().GetComponent<CoroutineLockComponent>();
            using ( await coroutineLockComponent.Wait(CoroutineLockType.Mailbox, self.GetParent<Unit>().InstanceId))
            {
                self.SaveChangeNoWait();
            }
        }

        public static void SaveChangeNoWait(this UnitDBSaveComponent self)
        {
            if (self.IsDisposed || self.Parent == null)
            {
                return;
            }

            if (self.Root() == null)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();

            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            if (self.EntityChangeTypeSet.Count <= 0)
            {
                return;
            }
            
            Other2UnitCache_AddOrUpdateUnit message = Other2UnitCache_AddOrUpdateUnit.Create();
            message.UnitId = unit.Id;
            message.EntityTypes.Add(unit.GetType().FullName);
            message.EntityBytes.Add(unit.ToBson());
            foreach (Type type in self.EntityChangeTypeSet)
            {
                Entity entity = unit.GetComponent(type);
                if (entity == null || entity.IsDisposed)
                {
                    continue;
                }
                
                Log.Debug($"开始保存变化部分entity数据：{type.FullName}");

                byte[] bytes = entity.ToBson();
                message.EntityTypes.Add(type.FullName);
                message.EntityBytes.Add(bytes);
                self.AddToBytes(type,bytes);
            }
            
            self.EntityChangeTypeSet.Clear();

            StartSceneConfig unitCacheCfg = StartSceneConfigCategory.Instance.GetBySceneName(unit.Zone(), "UnitCache");
            self.Root().GetComponent<MessageSender>().Call(unitCacheCfg.ActorId,message).Coroutine();
        }
        
        public static void SaveTransfer(this UnitDBSaveComponent unitSaver)
        {
            Unit unit = unitSaver.GetParent<Unit>();
            // Transfer组件需要序列化
            foreach (Type type in unitSaver.TransferChanges)
            {
                Entity component = unit.GetComponent(type);
                if (component == null)
                {
                    continue;
                }

                try
                {
                    unitSaver.AddToBytes(type, component.ToBson());
                }
                catch (Exception e)
                {
                    Log.Error($"component to bson fail: {unit.Id} {type.Name} {e}");
                }
            }

            unitSaver.TransferChanges.Clear();
        }
    }
}

