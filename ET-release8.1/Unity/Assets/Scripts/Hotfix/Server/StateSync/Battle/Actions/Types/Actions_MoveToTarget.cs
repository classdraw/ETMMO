namespace ET.Server
{
    [Actions(ActionsType.MoveToTarget)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    public class Actions_MoveToTarget:IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            
        }
    }
}
