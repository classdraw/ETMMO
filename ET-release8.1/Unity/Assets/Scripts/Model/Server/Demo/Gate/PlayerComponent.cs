using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
	//服务器所有登陆玩家
	[ComponentOf(typeof(Scene))]
	public class PlayerComponent : Entity, IAwake, IDestroy
	{
		public Dictionary<string, EntityRef<Player>> dictionary = new Dictionary<string, EntityRef<Player>>();
	}
}