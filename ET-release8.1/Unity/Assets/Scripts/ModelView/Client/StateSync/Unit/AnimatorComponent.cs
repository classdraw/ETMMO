using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
	public enum MotionType
	{
		None,
		Idle,
		Run,
		Attack,
		Attack1,
		Hit,
		Death
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