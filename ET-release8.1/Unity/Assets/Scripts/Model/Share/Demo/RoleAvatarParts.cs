using System.Collections.Generic;

namespace ET
{
	/// <summary>
	/// 记录角色当前各部位使用的 AvatarConfigId。
	/// key: <see cref="AvatarPartType"/>，value: AvatarConfig Id
	/// </summary>
	[ComponentOf(typeof(Unit))]
	public class RoleAvatarParts : Entity, IAwake, IDestroy
	{
		public readonly Dictionary<AvatarPartType, int> PartToAvatarConfigId = new Dictionary<AvatarPartType, int>();
	}
}
