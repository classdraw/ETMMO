namespace ET.Server
{
    public static class DisconnectHelper
    {
        //延迟1秒，如果instanceId不同表示被复用了 如果相同断开连接
        public static async ETTask Disconnect(this Session self)
        {
            if (self==null||self.IsDisposed)
            {
                return;
            }

            long instanceId = self.InstanceId;
            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            await timerComponent.WaitAsync(1000);
            if (self.InstanceId != instanceId)//被释放复用
            {
                return;
            }
            self.Dispose();
        }
    }
}
