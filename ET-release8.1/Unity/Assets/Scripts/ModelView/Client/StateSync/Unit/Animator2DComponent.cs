using System.Collections.Generic;

namespace ET.Client
{
	[ComponentOf(typeof(Unit))]
	public class Animator2DComponent : Entity, IAwake, IUpdate, IDestroy
	{
		public Dictionary<MotionType, int> InnerCDs = new Dictionary<MotionType, int>();
		public Dictionary<MotionType, long> InnerCDLastPlayTimes = new Dictionary<MotionType, long>();
		public HashSet<FrameSheetAnimType> AvailableAnims = new();

		public MotionType MotionType;
		public float MontionSpeed;
		public bool isStop;
		public float stopSpeed;
		public FrameSheetFacing Facing;
		public FrameSheetAnimPlayer AnimPlayer;
	}
}
