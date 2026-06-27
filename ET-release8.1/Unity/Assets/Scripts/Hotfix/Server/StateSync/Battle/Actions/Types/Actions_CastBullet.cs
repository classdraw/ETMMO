namespace ET.Server
{
    [Actions(ActionsType.CastBullet)]
    [FriendOf(typeof(Actions))]
    [FriendOf(typeof(Cast))]
    public class Actions_CastBullet:IActions
    {
        public void Run(Actions actions, ActionsRunType actionsRunType)
        {
            
        }
    }
}

