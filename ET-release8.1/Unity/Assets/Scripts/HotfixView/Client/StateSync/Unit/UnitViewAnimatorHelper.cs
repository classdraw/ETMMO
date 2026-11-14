using UnityEngine;

namespace ET.Client
{
    public static class UnitViewAnimatorHelper
    {
        /// <summary>受击动作播完时长（毫秒），只影响播放速度。</summary>
        public const int HitPlayDurationMs = 600;

        /// <summary>Hit 连播间隔（毫秒），与播完时长分开计时。</summary>
        public const int HitReplayIntervalMs = 600;

        public static bool TryGetRoleAnimator(GameObject unitGo, out Animator animator)
        {
            return AnimatorComponentSystem.TryGetRoleAnimator(unitGo, out animator);
        }

        public static bool TryPlayMotion(Unit unit, MotionType motionType, float motionSpeed = 1f)
        {
            if (unit == null || unit.IsDisposed || motionType == MotionType.None)
            {
                return false;
            }

            AnimatorComponent animator3D = unit.GetComponent<AnimatorComponent>();
            if (animator3D == null || animator3D.IsDisposed || !animator3D.IsReady())
            {
                return false;
            }

            animator3D.Play(motionType, motionSpeed);
            return true;
        }

        /// <summary>按技能时长（毫秒）缩放动作，使片段播完时间对齐配置。</summary>
        public static bool TryPlayMotionInTime(Unit unit, MotionType motionType, int durationMs)
        {
            if (unit == null || unit.IsDisposed || motionType == MotionType.None)
            {
                return false;
            }

            AnimatorComponent animator3D = unit.GetComponent<AnimatorComponent>();
            if (animator3D == null || animator3D.IsDisposed || !animator3D.IsReady())
            {
                return false;
            }

            if (durationMs <= 0 || !animator3D.TryGetClipLength(motionType, out float clipLength))
            {
                return TryPlayMotion(unit, motionType, 1f);
            }

            float motionSpeed = clipLength / (durationMs / 1000f);
            if (motionSpeed < 0.01f || motionSpeed > 1000f)
            {
                Log.Error($"motionSpeed数值异常, {motionSpeed}, 动作 {motionType} 按原速播放");
                motionSpeed = 1f;
            }

            return TryPlayMotion(unit, motionType, motionSpeed);
        }

        public static bool Uses3DAnimator(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return false;
            }

            AnimatorComponent animator3D = unit.GetComponent<AnimatorComponent>();
            return animator3D != null && !animator3D.IsDisposed && animator3D.IsReady();
        }
    }
}
