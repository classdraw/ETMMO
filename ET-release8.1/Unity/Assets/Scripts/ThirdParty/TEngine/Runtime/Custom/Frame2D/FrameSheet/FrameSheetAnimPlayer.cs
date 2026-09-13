using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Serializable]
    public class FrameSheetAnimBoneBinding
    {
        public FrameAnimBindBoneType boneType = FrameAnimBindBoneType.Body;
        public Transform bone;
    }

    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class FrameSheetAnimPlayer : MonoBehaviour
    {
        [SerializeField] private FrameSheetAnimConfig animConfig;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private MotionType defaultAnim = MotionType.Idle;
        [SerializeField] private FrameSheetFacing defaultFacing = FrameSheetFacing.Down;

        [Header("Bone Bindings")]
        [Tooltip("拖入各骨骼 GameObject；当前动画/方向无 Bone Config 时位置重置为 (0,0,0)")]
        [SerializeField] private List<FrameSheetAnimBoneBinding> boneBindings = new List<FrameSheetAnimBoneBinding>();

        private MaterialPropertyBlock propertyBlock;
        private MotionType currentAnim = MotionType.None;
        private FrameSheetFacing currentFacing = FrameSheetFacing.Down;
        private float currentSpeedMultiplier = 1f;
        private bool isPaused;
        private FrameSheetAnimClip currentClip;
        private float playStartTime;
        private bool pendingReturnToIdle;

        public MotionType CurrentAnim => currentAnim;
        public FrameSheetFacing CurrentFacing => currentFacing;
        public float CurrentSpeedMultiplier => currentSpeedMultiplier;
        public bool IsPaused => isPaused;

        /// <summary>
        /// 返回 false 时阻止 returnToIdleAfterPlay 自动切回 Idle（如施法中）。
        /// </summary>
        public Func<bool> CanAutoReturnToIdle { get; set; }

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            propertyBlock = new MaterialPropertyBlock();
            currentFacing = defaultFacing;
        }

        private void Start()
        {
            if (defaultAnim != MotionType.None)
            {
                Play(defaultAnim, defaultFacing);
            }
        }

        private void Update()
        {
            UpdateBonePositions();
            TryReturnToIdleAfterClipFinished();
        }

        private void OnValidate()
        {
            EnsureDefaultBoneBindings();
        }

        public void SetConfig(FrameSheetAnimConfig config)
        {
            animConfig = config;
        }

        public bool Play(MotionType animType)
        {
            return Play(animType, currentFacing);
        }

        public bool Play(MotionType animType, FrameSheetFacing facing)
        {
            return Play(animType, facing, 1f);
        }

        public bool TryGetClip(MotionType animType, out FrameSheetAnimClip clip)
        {
            clip = null;
            if (animConfig == null)
            {
                return false;
            }

            return animConfig.TryGetClip(animType, out clip);
        }

        public float GetClipDuration(MotionType animType)
        {
            if (!TryGetClip(animType, out FrameSheetAnimClip clip))
            {
                return 0f;
            }

            return GetClipDurationAtSpeed(clip, 1f);
        }

        public bool IsCurrentClipFinished()
        {
            if (currentClip == null || currentClip.loop || currentAnim == MotionType.None || isPaused)
            {
                return false;
            }

            return Time.time - playStartTime >= GetClipDurationAtSpeed(currentClip, currentSpeedMultiplier);
        }

        public bool IsPlayingNonLoopAction()
        {
            return currentClip != null
                   && !currentClip.loop
                   && currentAnim != MotionType.None
                   && !isPaused;
        }

        private static float GetClipDurationAtSpeed(FrameSheetAnimClip clip, float speedMultiplier)
        {
            speedMultiplier = Mathf.Max(speedMultiplier, 0.0001f);
            int frameCount = Mathf.Max(clip.endColumn - clip.startColumn + 1, 1);
            return frameCount * clip.interval / speedMultiplier;
        }

        public bool Play(MotionType animType, FrameSheetFacing facing, float speedMultiplier)
        {
            EnsureInitialized();

            if (animConfig == null || targetRenderer == null || animType == MotionType.None)
            {
                currentClip = null;
                ResetAllBonePositions();
                ResetMeshTransform();
                return false;
            }

            if (!animConfig.TryGetClip(animType, out FrameSheetAnimClip clip))
            {
                currentClip = null;
                ResetAllBonePositions();
                ResetMeshTransform();
                return false;
            }

            speedMultiplier = Mathf.Max(speedMultiplier, 0.0001f);
            playStartTime = Time.time;

            targetRenderer.GetPropertyBlock(propertyBlock);
            animConfig.ApplyGrid(propertyBlock);
            FrameSheetAnimConfig.ApplyClip(propertyBlock, clip, facing);
            propertyBlock.SetFloat(FrameSheetAnimShaderIds.Interval, clip.interval / speedMultiplier);
            propertyBlock.SetFloat(FrameSheetAnimShaderIds.AnimStartTime, playStartTime);
            targetRenderer.SetPropertyBlock(propertyBlock);

            IFrameSheetClipTextureRouter textureRouter = GetComponent<IFrameSheetClipTextureRouter>();
            textureRouter?.ApplyClipTextureRouting(clip, facing);

            currentAnim = animType;
            currentFacing = facing;
            currentSpeedMultiplier = speedMultiplier;
            currentClip = clip;
            isPaused = false;
            pendingReturnToIdle = ShouldReturnToIdleAfterClip(animType, clip);
            UpdateBonePositions();
            return true;
        }

        public void PausePlayback()
        {
            if (isPaused || targetRenderer == null || currentClip == null)
            {
                return;
            }

            EnsureInitialized();
            const float pauseInterval = 99999f;
            int frozenFrame = CalculateCurrentFrameIndex(currentClip);
            playStartTime = Time.time - frozenFrame * pauseInterval;

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FrameSheetAnimShaderIds.Interval, pauseInterval);
            propertyBlock.SetFloat(FrameSheetAnimShaderIds.AnimStartTime, playStartTime);
            targetRenderer.SetPropertyBlock(propertyBlock);
            isPaused = true;
        }

        public void ResumePlayback()
        {
            if (!isPaused)
            {
                return;
            }

            isPaused = false;
            if (currentAnim != MotionType.None)
            {
                Play(currentAnim, currentFacing, currentSpeedMultiplier);
            }
        }

        public bool SetFacing(FrameSheetFacing facing)
        {
            if (currentFacing == facing)
            {
                return true;
            }

            if (currentAnim == MotionType.None)
            {
                currentFacing = facing;
                return true;
            }

            float elapsedTime = Time.time - playStartTime;
            bool success = Play(currentAnim, facing, currentSpeedMultiplier);
            if (success)
            {
                playStartTime = Time.time - elapsedTime;
                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(FrameSheetAnimShaderIds.AnimStartTime, playStartTime);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }

            return success;
        }

        public bool ShouldReturnToIdleAfterCurrentClip()
        {
            return currentClip != null
                   && !currentClip.loop
                   && currentClip.returnToIdleAfterPlay
                   && currentAnim != MotionType.Idle
                   && currentAnim != MotionType.Stand;
        }

        private void EnsureInitialized()
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }
        }

        private void EnsureDefaultBoneBindings()
        {
            if (boneBindings == null)
            {
                boneBindings = new List<FrameSheetAnimBoneBinding>();
            }

            foreach (FrameAnimBindBoneType boneType in Enum.GetValues(typeof(FrameAnimBindBoneType)))
            {
                if (HasBoneBinding(boneType))
                {
                    continue;
                }

                boneBindings.Add(new FrameSheetAnimBoneBinding
                {
                    boneType = boneType,
                });
            }
        }

        private bool HasBoneBinding(FrameAnimBindBoneType boneType)
        {
            for (int i = 0; i < boneBindings.Count; i++)
            {
                if (boneBindings[i].boneType == boneType)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateBonePositions()
        {
            if (boneBindings == null || boneBindings.Count == 0)
            {
                return;
            }

            if (currentAnim == MotionType.None || currentClip == null)
            {
                ResetAllBonePositions();
                ResetMeshTransform();
                return;
            }

            FrameAnimBoneConfig boneConfig = currentClip.GetFacingBoneConfig(currentFacing);
            if (boneConfig == null)
            {
                ResetAllBonePositions();
                ResetMeshTransform();
                return;
            }

            int frameIndex = CalculateCurrentFrameIndex(currentClip);
            ApplyBoneConfig(boneConfig, frameIndex);
            ApplyMeshTransform(boneConfig, frameIndex);
        }

        private static bool ShouldReturnToIdleAfterClip(MotionType animType, FrameSheetAnimClip clip)
        {
            return clip != null
                   && !clip.loop
                   && clip.returnToIdleAfterPlay
                   && animType != MotionType.Idle
                   && animType != MotionType.Stand;
        }

        private void TryReturnToIdleAfterClipFinished()
        {
            if (!pendingReturnToIdle || currentClip == null || isPaused)
            {
                return;
            }

            if (!IsCurrentClipFinished())
            {
                return;
            }

            if (CanAutoReturnToIdle != null && !CanAutoReturnToIdle())
            {
                return;
            }

            pendingReturnToIdle = false;
            MotionType idleAnim = ResolveReturnToIdleAnim();
            if (idleAnim == MotionType.None)
            {
                return;
            }

            Play(idleAnim, currentFacing);
        }

        private MotionType ResolveReturnToIdleAnim()
        {
            if (TryGetClip(MotionType.Idle, out _))
            {
                return MotionType.Idle;
            }

            if (defaultAnim != MotionType.None && TryGetClip(defaultAnim, out _))
            {
                return defaultAnim;
            }

            return MotionType.None;
        }

        private int CalculateCurrentFrameIndex(FrameSheetAnimClip clip)
        {
            float interval = isPaused ? 99999f : clip.interval / currentSpeedMultiplier;
            interval = Mathf.Max(interval, 0.0001f);

            int frameCount = Mathf.Max(clip.endColumn - clip.startColumn + 1, 1);
            int elapsed = Mathf.Max(Mathf.FloorToInt((Time.time - playStartTime) / interval), 0);
            return clip.loop ? elapsed % frameCount : Mathf.Min(elapsed, frameCount - 1);
        }

        private void ApplyBoneConfig(FrameAnimBoneConfig boneConfig, int frameIndex)
        {
            for (int i = 0; i < boneBindings.Count; i++)
            {
                Transform bone = boneBindings[i].bone;
                if (bone == null)
                {
                    continue;
                }

                Vector3 localPosition = Vector3.zero;
                if (boneConfig.TryGetFramePosition(boneBindings[i].boneType, frameIndex, out Vector3 configPosition))
                {
                    localPosition.x = configPosition.x;
                    localPosition.z = configPosition.z;
                }

                bone.localPosition = localPosition;
            }
        }

        private void ResetAllBonePositions()
        {
            if (boneBindings == null)
            {
                return;
            }

            for (int i = 0; i < boneBindings.Count; i++)
            {
                Transform bone = boneBindings[i].bone;
                if (bone != null)
                {
                    bone.localPosition = Vector3.zero;
                }
            }
        }

        private void ApplyMeshTransform(FrameAnimBoneConfig boneConfig, int frameIndex)
        {
            if (targetRenderer == null)
            {
                return;
            }

            Transform meshTransform = targetRenderer.transform;
            if (boneConfig.TryGetFrameMeshTransform(frameIndex, out FrameAnimMeshFrameData meshFrame))
            {
                meshTransform.localPosition = meshFrame.ToLocalPosition();
                meshTransform.localScale = meshFrame.ToLocalScale();
            }
            else
            {
                ResetMeshTransform();
            }
        }

        private void ResetMeshTransform()
        {
            if (targetRenderer == null)
            {
                return;
            }

            Transform meshTransform = targetRenderer.transform;
            meshTransform.localPosition = Vector3.zero;
            meshTransform.localScale = Vector3.one;
        }
    }
}
