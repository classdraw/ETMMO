using UnityEngine;

namespace ET.Client
{
    public static class UnitViewAnimatorHelper
    {
        public static bool TryGetRoleAnimator(GameObject unitGo, out Animator animator)
        {
            animator = null;
            if (unitGo == null)
            {
                return false;
            }

            ReferenceCollector referenceCollector = unitGo.GetComponent<ReferenceCollector>();
            if (referenceCollector != null)
            {
                GameObject rootGo = referenceCollector.Get<GameObject>("Root");
                if (rootGo != null)
                {
                    animator = rootGo.GetComponent<Animator>();
                    if (animator != null && animator.runtimeAnimatorController != null)
                    {
                        return true;
                    }
                }
            }

            animator = unitGo.GetComponentInChildren<Animator>();
            return animator != null && animator.runtimeAnimatorController != null;
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
