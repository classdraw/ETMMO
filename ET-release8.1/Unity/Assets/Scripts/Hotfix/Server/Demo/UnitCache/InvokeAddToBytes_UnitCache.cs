namespace ET.Server
{
    /// <summary>
    /// Gate/Map 侧从 UnitCache 拉回组件 BSON 后，写入本地 Unit 的 <see cref="UnitDBSaveComponent.Bytes"/>，供传送等逻辑使用。
    /// </summary>
    [Invoke((long)SceneType.UnitCache)]
    public class InvokeAddToBytes_UnitCache : AInvokeHandler<AddToBytes>
    {
        public override void Handle(AddToBytes args)
        {
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            UnitDBSaveComponent saver = unit.GetComponent<UnitDBSaveComponent>();
            if (saver == null)
            {
                saver = unit.AddComponent<UnitDBSaveComponent>();
            }

            saver.AddToBytes(args.Type, args.Bytes);
        }
    }
}
