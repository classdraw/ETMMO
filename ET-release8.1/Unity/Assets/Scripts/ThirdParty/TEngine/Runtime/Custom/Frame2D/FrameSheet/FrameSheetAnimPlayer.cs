using UnityEngine;

namespace ET
{
    [DisallowMultipleComponent]
    public class FrameSheetAnimPlayer : MonoBehaviour
    {
        [SerializeField] private FrameSheetAnimConfig animConfig;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private FrameSheetAnimType defaultAnim = FrameSheetAnimType.Idle;
        [SerializeField] private FrameSheetFacing defaultFacing = FrameSheetFacing.Down;

        private MaterialPropertyBlock propertyBlock;
        private FrameSheetAnimType currentAnim = FrameSheetAnimType.None;
        private FrameSheetFacing currentFacing = FrameSheetFacing.Down;
        private float currentSpeedMultiplier = 1f;
        private bool isPaused;

        public FrameSheetAnimType CurrentAnim => currentAnim;
        public FrameSheetFacing CurrentFacing => currentFacing;
        public float CurrentSpeedMultiplier => currentSpeedMultiplier;
        public bool IsPaused => isPaused;

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
            if (defaultAnim != FrameSheetAnimType.None)
            {
                Play(defaultAnim, defaultFacing);
            }
        }

        public void SetConfig(FrameSheetAnimConfig config)
        {
            animConfig = config;
        }

        public bool Play(FrameSheetAnimType animType)
        {
            return Play(animType, currentFacing);
        }

        public bool Play(FrameSheetAnimType animType, FrameSheetFacing facing)
        {
            return Play(animType, facing, 1f);
        }

        public bool TryGetClip(FrameSheetAnimType animType, out FrameSheetAnimClip clip)
        {
            clip = null;
            if (animConfig == null)
            {
                return false;
            }

            return animConfig.TryGetClip(animType, out clip);
        }

        public float GetClipDuration(FrameSheetAnimType animType)
        {
            if (!TryGetClip(animType, out FrameSheetAnimClip clip))
            {
                return 0f;
            }

            int frameCount = Mathf.Max(clip.endColumn - clip.startColumn + 1, 1);
            return frameCount * clip.interval;
        }

        public bool Play(FrameSheetAnimType animType, FrameSheetFacing facing, float speedMultiplier)
        {
            EnsureInitialized();

            if (animConfig == null || targetRenderer == null || animType == FrameSheetAnimType.None)
            {
                return false;
            }

            if (!animConfig.TryGetClip(animType, out FrameSheetAnimClip clip))
            {
                return false;
            }

            speedMultiplier = Mathf.Max(speedMultiplier, 0.0001f);

            targetRenderer.GetPropertyBlock(propertyBlock);
            animConfig.ApplyGrid(propertyBlock);
            FrameSheetAnimConfig.ApplyClip(propertyBlock, clip, facing);
            propertyBlock.SetFloat(FrameSheetAnimShaderIds.Interval, clip.interval / speedMultiplier);
            targetRenderer.SetPropertyBlock(propertyBlock);

            currentAnim = animType;
            currentFacing = facing;
            currentSpeedMultiplier = speedMultiplier;
            isPaused = false;
            return true;
        }

        public void PausePlayback()
        {
            if (isPaused || targetRenderer == null)
            {
                return;
            }

            EnsureInitialized();
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FrameSheetAnimShaderIds.Interval, 99999f);
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
            if (currentAnim != FrameSheetAnimType.None)
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

            if (currentAnim == FrameSheetAnimType.None)
            {
                currentFacing = facing;
                return true;
            }

            return Play(currentAnim, facing);
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
    }
}
