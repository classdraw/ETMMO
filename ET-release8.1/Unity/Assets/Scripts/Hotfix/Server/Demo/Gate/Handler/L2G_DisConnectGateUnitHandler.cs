namespace ET.Server
{
    [MessageHandler(SceneType.Gate)]
    [FriendOf(typeof(Player))]
    public class L2G_DisConnectGateUnitHandler: MessageHandler<Scene,L2G_DisConnectGateUnit, G2L_DisConnectGateUnit>
    {
        protected override async ETTask Run(Scene root, L2G_DisConnectGateUnit request, G2L_DisConnectGateUnit response)
        {
            var coroutineLockComponent = root.GetComponent<CoroutineLockComponent>();
            
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate, request.AccountName.GetLongHashCode()))
            {
                PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
                Player player = playerComponent.GetByAccount(request.AccountName);
                if (player==null)
                {
                    return;//没有玩家在线
                }
                root.GetComponent<GateSessionKeyComponent>().Remove(request.AccountName);
                Session gateSession = player.GetComponent<PlayerSessionComponent>()?.Session;
                if (gateSession!=null&&!gateSession.IsDisposed)
                {
                    A2C_Disconnet a2CDisconnet = A2C_Disconnet.Create();
                    a2CDisconnet.Error = 2;//0重复登陆 1超时 2顶号
                    gateSession.Send(a2CDisconnet);
                    gateSession?.Disconnect().Coroutine();
                }

                if (player.GetComponent<PlayerSessionComponent>()?.Session!=null)
                {
                    player.GetComponent<PlayerSessionComponent>().Session = null;
                }

                if (player.GetComponent<PlayerOfflineOutTimeComponent>() == null)
                {
                    player.AddComponent<PlayerOfflineOutTimeComponent>();
                }
            }
        }
    }
}
