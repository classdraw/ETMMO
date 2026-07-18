using System;
using UnityEngine;

namespace ET.Client
{
	[EntitySystemOf(typeof(AnimatorComponent))]
	[FriendOf(typeof(AnimatorComponent))]
	public static partial class AnimatorComponentSystem
	{
		[EntitySystem]
		private static void Destroy(this AnimatorComponent self)
		{
			self.animationClips = null;
			self.Parameter = null;
			self.Animator = null;
			self.InnerCDLastPlayTimes.Clear();
		}
			
		[EntitySystem]
		private static void Awake(this AnimatorComponent self)
		{
			var gameObject = self.GetParent<Unit>().GetComponent<GameObjectComponent>().GameObject;
			var refCtrl = gameObject.GetComponent<ReferenceCollector>();

			Animator animator = (refCtrl.GetObject("Root") as GameObject).GetComponent<Animator>();
			if (animator == null)
			{
				return;
			}

			if (animator.runtimeAnimatorController == null)
			{
				return;
			}

			if (animator.runtimeAnimatorController.animationClips == null)
			{
				return;
			}
			self.Animator = animator;
			foreach (AnimationClip animationClip in animator.runtimeAnimatorController.animationClips)
			{
				self.animationClips[animationClip.name] = animationClip;
			}
			foreach (AnimatorControllerParameter animatorControllerParameter in animator.parameters)
			{
				self.Parameter.Add(animatorControllerParameter.name);
			}
			
			self.InnerCDs.Add(MotionType.Hit,1000);
		}
		
		[EntitySystem]
		private static void Update(this AnimatorComponent self)
		{
			if (self.isStop)
			{
				return;
			}

			if (self.MotionType == MotionType.None)
			{
				return;
			}

			try
			{
				//self.Animator.SetFloat("MotionSpeed", self.MontionSpeed);
				//Log.Console("____"+self.MotionType);
				//self.Animator.SetTrigger(self.MotionType.ToString());
				self.Animator.CrossFade(self.MotionType.ToString().ToLower(),0.1f);
				self.Animator.speed = self.MontionSpeed;
				if (self.MotionType==MotionType.Run)
				{
					self.Animator.SetBool("isMoveing",true);
				}
				else if(self.MotionType==MotionType.Idle)
				{
					self.Animator.SetBool("isMoveing",false);
				}
				
				self.MontionSpeed = 1;
				self.MotionType = MotionType.None;
			}
			catch (Exception ex)
			{
				throw new Exception($"动作播放失败: {self.MotionType}", ex);
			}
		}

		public static bool HasParameter(this AnimatorComponent self, string parameter)
		{
			return self.Parameter.Contains(parameter);
		}

		public static void PlayInTime(this AnimatorComponent self, MotionType motionType, float time)
		{
			if (!self.CanPlayByInnerCD(motionType))
			{
				return;
			}

			AnimationClip animationClip;
			if (!self.animationClips.TryGetValue(motionType.ToString().ToLower(), out animationClip))
			{
				throw new Exception($"找不到该动作: {motionType}");
			}

			float motionSpeed = animationClip.length / time;
			if (motionSpeed < 0.01f || motionSpeed > 1000f)
			{
				Log.Error($"motionSpeed数值异常, {motionSpeed}, 此动作跳过");
				return;
			}
			self.MotionType = motionType;
			self.MontionSpeed = motionSpeed;
			self.RecordInnerCD(motionType);
		}

		public static void Play(this AnimatorComponent self, MotionType motionType, float motionSpeed)
		{
			if (!self.CanPlayByInnerCD(motionType))
			{
				return;
			}

			self.MotionType = motionType;
			self.MontionSpeed = motionSpeed;
			self.RecordInnerCD(motionType);
		}

		private static bool CanPlayByInnerCD(this AnimatorComponent self, MotionType motionType)
		{
			if (!self.InnerCDs.TryGetValue(motionType, out int cdMs) || cdMs <= 0)
			{
				return true;
			}

			long now = TimeInfo.Instance.ClientFrameTime();
			if (!self.InnerCDLastPlayTimes.TryGetValue(motionType, out long lastPlayTime))
			{
				return true;
			}

			long elapsed = now - lastPlayTime;
			if (elapsed >= cdMs)
			{
				return true;
			}

			Unit unit = self.GetParent<Unit>();
			//Log.Info($"AnimatorInnerCD blocked, unitId={unit.Id}, motionType={motionType}, cdMs={cdMs}, lastPlayTime={lastPlayTime}, now={now}, elapsed={elapsed}ms, remain={cdMs - elapsed}ms");
			return false;
		}

		private static void RecordInnerCD(this AnimatorComponent self, MotionType motionType)
		{
			if (!self.InnerCDs.TryGetValue(motionType, out int cdMs))
			{
				return;
			}

			long now = TimeInfo.Instance.ClientFrameTime();
			self.InnerCDLastPlayTimes[motionType] = now;
			Unit unit = self.GetParent<Unit>();
			//Log.Info($"AnimatorInnerCD play, unitId={unit.Id}, motionType={motionType}, cdMs={cdMs}, now={now}");
		}

		public static float AnimationTime(this AnimatorComponent self, MotionType motionType)
		{
			AnimationClip animationClip;
			if (!self.animationClips.TryGetValue(motionType.ToString(), out animationClip))
			{
				throw new Exception($"找不到该动作: {motionType}");
			}
			return animationClip.length;
		}

		public static void PauseAnimator(this AnimatorComponent self)
		{
			if (self.isStop)
			{
				return;
			}
			self.isStop = true;

			if (self.Animator == null)
			{
				return;
			}
			self.stopSpeed = self.Animator.speed;
			self.Animator.speed = 0;
		}

		public static void RunAnimator(this AnimatorComponent self)
		{
			if (!self.isStop)
			{
				return;
			}

			self.isStop = false;

			if (self.Animator == null)
			{
				return;
			}
			self.Animator.speed = self.stopSpeed;
		}

		public static void SetBoolValue(this AnimatorComponent self, string name, bool state)
		{
			if (!self.HasParameter(name))
			{
				return;
			}

			self.Animator.SetBool(name, state);
		}

		public static void SetFloatValue(this AnimatorComponent self, string name, float state)
		{
			if (!self.HasParameter(name))
			{
				return;
			}

			self.Animator.SetFloat(name, state);
		}

		public static void SetIntValue(this AnimatorComponent self, string name, int value)
		{
			if (!self.HasParameter(name))
			{
				return;
			}

			self.Animator.SetInteger(name, value);
		}

		public static void SetTrigger(this AnimatorComponent self, string name)
		{
			if (!self.HasParameter(name))
			{
				return;
			}

			self.Animator.SetTrigger(name);
		}

		public static void SetAnimatorSpeed(this AnimatorComponent self, float speed)
		{
			self.stopSpeed = self.Animator.speed;
			self.Animator.speed = speed;
		}

		public static void ResetAnimatorSpeed(this AnimatorComponent self)
		{
			self.Animator.speed = self.stopSpeed;
		}
	}
}