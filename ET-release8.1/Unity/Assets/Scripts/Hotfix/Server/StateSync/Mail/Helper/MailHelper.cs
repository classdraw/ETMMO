namespace ET.Server
{
    public static class MailHelper
    {
        public static void SendToClient(MailUnit unit, IMessage message)
        {
            unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(unit.Id, message).Coroutine();
        }
    }
}

