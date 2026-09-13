using System;
using Unity.Mathematics;
using UnityEngine;

namespace ET.Client
{
	[EntitySystemOf(typeof(Animator2DComponent))]
	[FriendOf(typeof(Animator2DComponent))]
	public static partial class Animator2DComponentSystem
	{
		[EntitySystem]
		private static void Destroy(this Animator2DComponent self)
		{
			self.AvailableAnims = null;
			self.AnimPlayer = null;
			self.InnerCDLastPlayTimes.Clear();
		}

		[EntitySystem]
		private static void Awake(this Animator2DComponent self)
		{
			GameObject gameObject = self.GetParent<Unit>().GetComponent<GameObjectComponent>().GameObject;
			FrameSheetAnimPlayer animPlayer = gameObject.GetComponentInChildren<FrameSheetAnimPlayer>();
			if (animPlayer == null)
			{
				return;
			}

			self.AnimPlayer = animPlayer;
			self.Facing = animPlayer.CurrentFacing;
			RefreshAvailableAnims(self);
		}

		[EntitySystem]
		private static void Update(this Animator2DComponent self)
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
				self.SyncFacingFromUnit();
				MotionType clipType = ResolveClipMotion(self.MotionType);
				if (clipType == MotionType.None || self.AnimPlayer == null)
				{
					self.MotionType = MotionType.None;
					return;
				}

				self.AnimPlayer.Play(clipType, self.Facing, self.MontionSpeed);
				self.MontionSpeed = 1f;
				self.MotionType = MotionType.None;
			}
			catch (Exception ex)
			{
				throw new Exception($"2D动作播放失败: {self.MotionType}", ex);
			}
		}

		public static bool HasAnim(this Animator2DComponent self, MotionType motionType)
		{
			MotionType clipType = ResolveClipMotion(motionType);
			return clipType != MotionType.None && self.AvailableAnims.Contains(motionType);
		}

		public static void PlayInTime(this Animator2DComponent self, MotionType motionType, float time)
		{
			if (!self.CanPlayByInnerCD(motionType))
			{
				return;
			}

			float clipDuration = self.AnimationTime(motionType);
			if (clipDuration <= 0f)
			{
				throw new Exception($"找不到该2D动作: {motionType}");
			}

			float motionSpeed = clipDuration / time;
			if (motionSpeed < 0.01f || motionSpeed > 1000f)
			{
				Log.Error($"motionSpeed数值异常, {motionSpeed}, 此动作跳过");
				return;
			}

			self.MotionType = motionType;
			self.MontionSpeed = motionSpeed;
			self.RecordInnerCD(motionType);
		}

		public static void Play(this Animator2DComponent self, MotionType motionType, float motionSpeed = 1f)
		{
			if (!self.CanPlayByInnerCD(motionType))
			{
				return;
			}

			if (!self.HasAnim(motionType))
			{
				return;
			}

			self.MotionType = motionType;
			self.MontionSpeed = motionSpeed;
			self.RecordInnerCD(motionType);
		}

		public static float AnimationTime(this Animator2DComponent self, MotionType motionType)
		{
			if (self.AnimPlayer == null)
			{
				return 0f;
			}

			MotionType clipType = ResolveClipMotion(motionType);
			return self.AnimPlayer.GetClipDuration(clipType);
		}

		public static void SetFacing(this Animator2DComponent self, FrameSheetFacing facing)
		{
			self.Facing = facing;
			if (self.AnimPlayer == null)
			{
				return;
			}

			self.AnimPlayer.SetFacing(facing);
		}

		public static void SyncFacingFromUnit(this Animator2DComponent self)
		{
			Unit unit = self.GetParent<Unit>();
			self.Facing = ForwardToFacing(unit.Forward);
			self.AnimPlayer?.SetFacing(self.Facing);
		}

		public static void PauseAnimator(this Animator2DComponent self)
		{
			if (self.isStop)
			{
				return;
			}

			self.isStop = true;
			if (self.AnimPlayer == null)
			{
				return;
			}

			self.stopSpeed = self.AnimPlayer.CurrentSpeedMultiplier;
			self.AnimPlayer.PausePlayback();
		}

		public static void RunAnimator(this Animator2DComponent self)
		{
			if (!self.isStop)
			{
				return;
			}

			self.isStop = false;
			if (self.AnimPlayer == null)
			{
				return;
			}

			self.AnimPlayer.ResumePlayback();
		}

		public static void SetAnimatorSpeed(this Animator2DComponent self, float speed)
		{
			if (self.AnimPlayer == null || self.AnimPlayer.CurrentAnim == MotionType.None)
			{
				return;
			}

			self.stopSpeed = self.AnimPlayer.CurrentSpeedMultiplier;
			self.AnimPlayer.Play(self.AnimPlayer.CurrentAnim, self.AnimPlayer.CurrentFacing, speed);
		}

		public static void ResetAnimatorSpeed(this Animator2DComponent self)
		{
			if (self.AnimPlayer == null || self.AnimPlayer.CurrentAnim == MotionType.None)
			{
				return;
			}

			self.AnimPlayer.Play(self.AnimPlayer.CurrentAnim, self.AnimPlayer.CurrentFacing, self.stopSpeed);
		}

		private static bool CanPlayByInnerCD(this Animator2DComponent self, MotionType motionType)
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

			return now - lastPlayTime >= cdMs;
		}

		private static void RecordInnerCD(this Animator2DComponent self, MotionType motionType)
		{
			if (!self.InnerCDs.TryGetValue(motionType, out int cdMs))
			{
				return;
			}

			self.InnerCDLastPlayTimes[motionType] = TimeInfo.Instance.ClientFrameTime();
		}

		private static void RefreshAvailableAnims(this Animator2DComponent self)
		{
			self.AvailableAnims.Clear();
			if (self.AnimPlayer == null)
			{
				return;
			}

			foreach (MotionType motionType in Enum.GetValues(typeof(MotionType)))
			{
				if (motionType == MotionType.None || motionType == MotionType.Hit)
				{
					continue;
				}

				MotionType clipType = ResolveClipMotion(motionType);
				if (clipType != MotionType.None && self.AnimPlayer.TryGetClip(clipType, out _))
				{
					self.AvailableAnims.Add(motionType);
				}
			}
		}

		private static MotionType ResolveClipMotion(MotionType motionType)
		{
			return motionType switch
			{
				MotionType.Death => MotionType.Stand,
				_ => motionType,
			};
		}

		private static FrameSheetFacing ForwardToFacing(float3 forward)
		{
			forward.y = 0;
			if (math.lengthsq(forward) <= math.EPSILON)
			{
				return FrameSheetFacing.Down;
			}

			forward = math.normalize(forward);
			if (math.abs(forward.x) > math.abs(forward.z))
			{
				return forward.x >= 0f ? FrameSheetFacing.Right : FrameSheetFacing.Left;
			}

			return forward.z >= 0f ? FrameSheetFacing.Up : FrameSheetFacing.Down;
		}
	}
}
