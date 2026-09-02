namespace ET.Client
{
	[ComponentOf(typeof(Unit))]
	public class Avatar2DComponent : Entity, IAwake, IDestroy
	{
		public FrameSheetAnimPlayer AnimPlayer;
		public RedressAvatar RedressAvatar;
	}
}
