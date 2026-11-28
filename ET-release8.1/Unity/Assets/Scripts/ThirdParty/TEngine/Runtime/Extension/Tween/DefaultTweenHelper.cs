using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TEngine
{
    /// <summary>
    /// 内置 Tween 实现（无 DOTween 依赖）。安装 DOTween 后可改用 <c>DotweenTweenHelper</c> 并在此类名填入 TEngineGlobal。
    /// </summary>
    public sealed class DefaultTweenHelper : Utility.Tween.ITweenHelper
    {
        private static readonly Dictionary<long, TweenJob> Jobs = new();
        private static readonly Dictionary<object, List<long>> JobsByTarget = new();
        private static long nextId = 1;

        internal static void Tick(float deltaTime, float unscaledDeltaTime)
        {
            if (Jobs.Count == 0)
            {
                return;
            }

            long[] ids = new long[Jobs.Count];
            Jobs.Keys.CopyTo(ids, 0);
            for (int i = 0; i < ids.Length; i++)
            {
                long id = ids[i];
                if (!Jobs.TryGetValue(id, out TweenJob job))
                {
                    continue;
                }

                float dt = job.UseUnscaledTime ? unscaledDeltaTime : deltaTime;
                job.Elapsed += dt;
                if (job.Elapsed < job.StartDelay)
                {
                    continue;
                }

                float localTime = job.Elapsed - job.StartDelay;
                if (job.Duration <= 0f)
                {
                    job.Apply(1f);
                    FinishJob(id, job);
                    continue;
                }

                float t = Mathf.Clamp01(localTime / job.Duration);
                t = ApplyEase(job.Ease, t);
                job.Apply(t);

                if (localTime >= job.Duration + job.EndDelay)
                {
                    FinishJob(id, job);
                }
            }
        }

        private static void FinishJob(long id, TweenJob job)
        {
            job.CompleteAction?.Invoke();
            RemoveJob(id);
        }

        private static void RemoveJob(long id)
        {
            if (!Jobs.TryGetValue(id, out TweenJob job))
            {
                return;
            }

            Jobs.Remove(id);
            if (job.Target != null && JobsByTarget.TryGetValue(job.Target, out List<long> list))
            {
                list.Remove(id);
                if (list.Count == 0)
                {
                    JobsByTarget.Remove(job.Target);
                }
            }
        }

        private static long Create(object target, float duration, Ease ease, int cycles, CycleMode cycleMode,
            float startDelay, float endDelay, bool useUnscaledTime, Action<float> apply, Action onComplete)
        {
            long id = nextId++;
            TweenJob job = new TweenJob
            {
                Id = id,
                Target = target,
                Duration = Mathf.Max(0f, duration),
                Ease = ease,
                Cycles = Mathf.Max(1, cycles),
                CycleMode = cycleMode,
                StartDelay = Mathf.Max(0f, startDelay),
                EndDelay = Mathf.Max(0f, endDelay),
                UseUnscaledTime = useUnscaledTime,
                Apply = apply,
                CompleteAction = onComplete,
            };
            Jobs[id] = job;
            if (target != null)
            {
                if (!JobsByTarget.TryGetValue(target, out List<long> list))
                {
                    list = new List<long>();
                    JobsByTarget[target] = list;
                }

                list.Add(id);
            }

            return id;
        }

        private static float ApplyEase(Ease ease, float t)
        {
            return ease switch
            {
                Ease.Linear => t,
                Ease.InQuad => t * t,
                Ease.OutQuad => t * (2f - t),
                Ease.InOutQuad => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f,
                Ease.OutSine => Mathf.Sin(t * Mathf.PI * 0.5f),
                Ease.InSine => 1f - Mathf.Cos(t * Mathf.PI * 0.5f),
                _ => Mathf.SmoothStep(0f, 1f, t),
            };
        }

        public bool IsTweening(object onTarget)
        {
            return onTarget != null && JobsByTarget.TryGetValue(onTarget, out List<long> list) && list.Count > 0;
        }

        public int GetTweenCount(object onTarget)
        {
            return onTarget != null && JobsByTarget.TryGetValue(onTarget, out List<long> list) ? list.Count : 0;
        }

        public bool IsAlive(long tweenId) => Jobs.ContainsKey(tweenId);

        public void Stop(long tweenId) => RemoveJob(tweenId);

        public void Complete(long tweenId)
        {
            if (Jobs.TryGetValue(tweenId, out TweenJob job))
            {
                job.Apply(1f);
                FinishJob(tweenId, job);
            }
        }

        public int StopAll(object onTarget = null)
        {
            if (onTarget == null)
            {
                int count = Jobs.Count;
                Jobs.Clear();
                JobsByTarget.Clear();
                return count;
            }

            if (!JobsByTarget.TryGetValue(onTarget, out List<long> list))
            {
                return 0;
            }

            long[] ids = list.ToArray();
            for (int i = 0; i < ids.Length; i++)
            {
                RemoveJob(ids[i]);
            }

            return ids.Length;
        }

        public int CompleteAll(object onTarget = null)
        {
            if (onTarget == null)
            {
                long[] ids = new long[Jobs.Count];
                Jobs.Keys.CopyTo(ids, 0);
                for (int i = 0; i < ids.Length; i++)
                {
                    Complete(ids[i]);
                }

                return ids.Length;
            }

            if (!JobsByTarget.TryGetValue(onTarget, out List<long> list))
            {
                return 0;
            }

            long[] targetIds = list.ToArray();
            for (int i = 0; i < targetIds.Length; i++)
            {
                Complete(targetIds[i]);
            }

            return targetIds.Length;
        }

        public void OnComplete(long tweenId, Action onComplete)
        {
            if (Jobs.TryGetValue(tweenId, out TweenJob job))
            {
                job.CompleteAction += onComplete;
            }
        }

        public long Delay(float duration, Action onComplete = null, bool useUnscaledTime = false, bool warnIfTargetDestroyed = true)
        {
            return Create(null, duration, Ease.Linear, 1, CycleMode.Restart, 0f, 0f, useUnscaledTime, _ => { }, onComplete);
        }

        public long Delay(object target, float duration, Action onComplete = null, bool useUnscaledTime = false, bool warnIfTargetDestroyed = true)
        {
            return Create(target, duration, Ease.Linear, 1, CycleMode.Restart, 0f, 0f, useUnscaledTime, _ => { }, onComplete);
        }

        public long UIAnchoredPositionX(RectTransform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            float start = target.anchoredPosition.x;
            return UIAnchoredPositionX(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long UIAnchoredPositionX(RectTransform target, float startValue, float endValue, float duration, Ease ease = Ease.Default,
            int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector2 pos = target.anchoredPosition;
                pos.x = Mathf.LerpUnclamped(startValue, endValue, t);
                target.anchoredPosition = pos;
            }, null);
        }

        public long UIAnchoredPositionY(RectTransform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            float start = target.anchoredPosition.y;
            return UIAnchoredPositionY(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long UIAnchoredPositionY(RectTransform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector2 pos = target.anchoredPosition;
                pos.y = Mathf.LerpUnclamped(startValue, endValue, t);
                target.anchoredPosition = pos;
            }, null);
        }

        public long UIAnchoredPosition(RectTransform target, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Vector2 start = target.anchoredPosition;
            return UIAnchoredPosition(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long UIAnchoredPosition(RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.anchoredPosition = Vector2.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long UISizeDelta(RectTransform target, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Vector2 start = target.sizeDelta;
            return UISizeDelta(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long UISizeDelta(RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.sizeDelta = Vector2.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long Position(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Vector3 start = target.position;
            return Position(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long Position(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.position = Vector3.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long LocalPosition(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Vector3 start = target.localPosition;
            return LocalPosition(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long LocalPosition(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.localPosition = Vector3.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long Scale(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            float start = target.localScale.x;
            return Scale(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long Scale(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                float s = Mathf.LerpUnclamped(startValue, endValue, t);
                target.localScale = new Vector3(s, s, s);
            }, null);
        }

        public long Alpha(CanvasGroup target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            float start = target.alpha;
            return Alpha(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long Alpha(CanvasGroup target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.alpha = Mathf.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long Color(Graphic target, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Color start = target.color;
            return Color(target, start, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long Color(Graphic target, Color startValue, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.color = UnityEngine.Color.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        #region NotImplemented stubs — 可改用 DotweenTweenHelper 获得完整能力

        public long LocalRotation(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => LocalRotation(target, target.localEulerAngles, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long LocalRotation(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.localEulerAngles = Vector3.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long Rotation(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => Rotation(target, target.eulerAngles, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long Rotation(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.eulerAngles = Vector3.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long PositionX(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => PositionX(target, target.position.x, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long PositionX(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 p = target.position;
                p.x = Mathf.LerpUnclamped(startValue, endValue, t);
                target.position = p;
            }, null);
        }

        public long PositionY(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => PositionY(target, target.position.y, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long PositionY(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 p = target.position;
                p.y = Mathf.LerpUnclamped(startValue, endValue, t);
                target.position = p;
            }, null);
        }

        public long PositionZ(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => PositionZ(target, target.position.z, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long PositionZ(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 p = target.position;
                p.z = Mathf.LerpUnclamped(startValue, endValue, t);
                target.position = p;
            }, null);
        }

        public long LocalPositionX(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => LocalPositionX(target, target.localPosition.x, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long LocalPositionX(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 p = target.localPosition;
                p.x = Mathf.LerpUnclamped(startValue, endValue, t);
                target.localPosition = p;
            }, null);
        }

        public long LocalPositionY(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => LocalPositionY(target, target.localPosition.y, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long LocalPositionY(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 p = target.localPosition;
                p.y = Mathf.LerpUnclamped(startValue, endValue, t);
                target.localPosition = p;
            }, null);
        }

        public long LocalPositionZ(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => LocalPositionZ(target, target.localPosition.z, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long LocalPositionZ(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 p = target.localPosition;
                p.z = Mathf.LerpUnclamped(startValue, endValue, t);
                target.localPosition = p;
            }, null);
        }

        public long Rotation(Transform target, Quaternion endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => Rotation(target, target.rotation, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long Rotation(Transform target, Quaternion startValue, Quaternion endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.rotation = Quaternion.SlerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long LocalRotation(Transform target, Quaternion endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => LocalRotation(target, target.localRotation, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long LocalRotation(Transform target, Quaternion startValue, Quaternion endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.localRotation = Quaternion.SlerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long Scale(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => Scale(target, target.localScale, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long Scale(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.localScale = Vector3.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long ScaleX(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => ScaleX(target, target.localScale.x, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long ScaleX(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 s = target.localScale;
                s.x = Mathf.LerpUnclamped(startValue, endValue, t);
                target.localScale = s;
            }, null);
        }

        public long ScaleY(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => ScaleY(target, target.localScale.y, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long ScaleY(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 s = target.localScale;
                s.y = Mathf.LerpUnclamped(startValue, endValue, t);
                target.localScale = s;
            }, null);
        }

        public long ScaleZ(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => ScaleZ(target, target.localScale.z, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long ScaleZ(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Vector3 s = target.localScale;
                s.z = Mathf.LerpUnclamped(startValue, endValue, t);
                target.localScale = s;
            }, null);
        }

        public long Color(SpriteRenderer target, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => Color(target, target.color, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long Color(SpriteRenderer target, Color startValue, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.color = UnityEngine.Color.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long MaterialColor(Material target, Color startValue, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.color = UnityEngine.Color.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long Alpha(SpriteRenderer target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => Alpha(target, target.color.a, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long Alpha(SpriteRenderer target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Color c = target.color;
                c.a = Mathf.LerpUnclamped(startValue, endValue, t);
                target.color = c;
            }, null);
        }

        public long UISliderValue(Slider target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => UISliderValue(target, target.value, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long UISliderValue(Slider target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.value = Mathf.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long UINormalizedPosition(ScrollRect target, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => UINormalizedPosition(target, target.normalizedPosition, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long UINormalizedPosition(ScrollRect target, Vector2 startValue, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.normalizedPosition = Vector2.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long UIHorizontalNormalizedPosition(ScrollRect target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => UIHorizontalNormalizedPosition(target, target.horizontalNormalizedPosition, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long UIHorizontalNormalizedPosition(ScrollRect target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.horizontalNormalizedPosition = Mathf.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long UIVerticalNormalizedPosition(ScrollRect target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => UIVerticalNormalizedPosition(target, target.verticalNormalizedPosition, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long UIVerticalNormalizedPosition(ScrollRect target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.verticalNormalizedPosition = Mathf.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long UIAnchoredPosition3D(RectTransform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => UIAnchoredPosition3D(target, target.anchoredPosition3D, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long UIAnchoredPosition3D(RectTransform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.anchoredPosition3D = Vector3.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long Alpha(Graphic target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => Alpha(target, target.color.a, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long Alpha(Graphic target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                Color c = target.color;
                c.a = Mathf.LerpUnclamped(startValue, endValue, t);
                target.color = c;
            }, null);
        }

        public long UIFillAmount(Image target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
            => UIFillAmount(target, target.fillAmount, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);

        public long UIFillAmount(Image target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                if (target == null)
                {
                    return;
                }

                target.fillAmount = Mathf.LerpUnclamped(startValue, endValue, t);
            }, null);
        }

        public long MoveBezierPath(Transform target, Vector3[] path, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            if (path == null || path.Length == 0)
            {
                return 0;
            }

            Vector3 start = target != null ? target.position : Vector3.zero;
            Vector3 end = path[path.Length - 1];
            return Position(target, start, end, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime);
        }

        public long Custom<T>(T target, Vector3 startValue, Vector3 endValue, float duration, Action<T, Vector3> onValueChange, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                onValueChange?.Invoke(target, Vector3.LerpUnclamped(startValue, endValue, t));
            }, null);
        }

        public long Custom<T>(T target, long startValue, long endValue, float duration, Action<T, long> onValueChange, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                long v = (long)Mathf.Lerp(startValue, endValue, t);
                onValueChange?.Invoke(target, v);
            }, null);
        }

        public long Custom<T>(T target, float startValue, float endValue, float duration, Action<T, float> onValueChange, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
        {
            return Create(target, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime, t =>
            {
                onValueChange?.Invoke(target, Mathf.LerpUnclamped(startValue, endValue, t));
            }, null);
        }

        #endregion

        private sealed class TweenJob
        {
            public long Id;
            public object Target;
            public float Duration;
            public float Elapsed;
            public float StartDelay;
            public float EndDelay;
            public Ease Ease;
            public int Cycles;
            public CycleMode CycleMode;
            public bool UseUnscaledTime;
            public Action<float> Apply;
            public Action CompleteAction;
        }
    }
}