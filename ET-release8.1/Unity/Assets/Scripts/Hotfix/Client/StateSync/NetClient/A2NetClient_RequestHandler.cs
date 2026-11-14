namespace ET.Client
{
    [MessageHandler(SceneType.NetClient)]
    public class A2NetClient_RequestHandler: MessageHandler<Scene, A2NetClient_Request, A2NetClient_Response>
    {
        protected override async ETTask Run(Scene root, A2NetClient_Request request, A2NetClient_Response response)
        {
            int rpcId = request.RpcId;
            IRequest message = request.MessageObject;
            Session session = root.GetComponent<SessionComponent>()?.Session;
            if (message == null || session == null)
            {
                if (message != null)
                {
                    response.MessageObject = MessageHelper.CreateResponse(message.GetType(), rpcId, ErrorCode.ERR_NetWorkError);
                }

                return;
            }

            IResponse res = await session.Call(message);
            res.RpcId = rpcId;
            response.MessageObject = res;
        }
    }
}