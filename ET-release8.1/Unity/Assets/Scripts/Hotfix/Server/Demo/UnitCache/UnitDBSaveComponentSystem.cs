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
            //正式上线 每10-15分钟随机存储一次
            //long time = RandomGenerator.RandomNumber(10, 16) * 60 * 1000;
            long time = 10 * 1000;
            self.Timer = self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(time, TimerInvokeType.SaveChangeDBDate, self);
        }
        
        [EntitySystem]
        private static void Destroy(this ET.Server.UnitDBSaveComponent self)
        {

            self.Root().GetComponent<TimerComponent>().Remove(ref self.Timer);
            //self.ComponentTypes.Clear();
            self.EntityChangeTypeSet.Clear();
            self.TransferChanges.Clear();
            self.Bytes.Clear();
        }
        //这里的type只能是IUnitCache ITransfer
        public static void AddToBytes(this UnitDBSaveComponent self, Type type, byte[] bytes)
        {
            self.Bytes[type] = bytes;//数据库已经存储了 缓存在unitCache, 这里是给传送用  序列化
        }
        //发现改动 及时发送数据给缓存服  缓存服定时更新
        public static void AddChange(this UnitDBSaveComponent self, Type type)
        {
            //self.ComponentTypes.Add(type);//一个存档 记录所有改变的组件
            if (typeof(IUnitCache).IsAssignableFrom(type))
            {
                self.EntityChangeTypeSet.Add(type);
            }
            
            //else if (typeof(ITransfer).IsAssignableFrom(type))
            //{
            //   self.TransferChanges.Add(type);
            //}
          
        }
        
        public static async ETTask SaveChange(this UnitDBSaveComponent self)
        {
            CoroutineLockComponent coroutineLockComponent = self.Root().GetComponent<CoroutineLockComponent>();
            //数据完成更新之后再进行网络消息整理
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
                self.AddToBytes(type,bytes);//加到bytes目的是传送时候方便序列化，到另外一个场景不会丢失  比如背包数据
            }
            
            self.EntityChangeTypeSet.Clear();
            
            
            //通知缓存服更新数据   这里会更新缓存 也会更新数据库数据 最终在AddOrUpdate处理
            StartSceneConfig unitCacheCfg = StartSceneConfigCategory.Instance.GetBySceneType(unit.Zone(), SceneType.UnitCache);
            self.Root()?.GetComponent<MessageSender>().Call(unitCacheCfg.ActorId,message).Coroutine();
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

