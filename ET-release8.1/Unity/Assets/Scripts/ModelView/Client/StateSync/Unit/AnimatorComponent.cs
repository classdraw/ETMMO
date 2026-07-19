using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
	public enum MotionType
	{
		None=0,
		Idle=1,
		Run=2,
		Attack=3,
		Attack1=4,
		Hit=5,
		Death=6
	}

	[ComponentOf(typeof(Unit))]
	public class AnimatorComponent : Entity, IAwake, IUpdate, IDestroy
	{
		public Dictionary<MotionType, int> InnerCDs = new Dictionary<MotionType, int>();
		public Dictionary<MotionType, long> InnerCDLastPlayTimes = new Dictionary<MotionType, long>();
		public Dictionary<string, AnimationClip> animationClips = new();
		public HashSet<string> Parameter = new();

		public MotionType MotionType;
		public float MontionSpeed;
		public bool isStop;
		public float stopSpeed;
		public Animator Animator;
	}
}