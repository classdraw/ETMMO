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

        public FrameSheetAnimType CurrentAnim => currentAnim;
        public FrameSheetFacing CurrentFacing => currentFacing;

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
            EnsureInitialized();

            if (animConfig == null || targetRenderer == null || animType == FrameSheetAnimType.None)
            {
                return false;
            }

            if (!animConfig.TryGetClip(animType, out FrameSheetAnimClip clip))
            {
                return false;
            }

            targetRenderer.GetPropertyBlock(propertyBlock);
            animConfig.ApplyGrid(propertyBlock);
            FrameSheetAnimConfig.ApplyClip(propertyBlock, clip, facing);
            targetRenderer.SetPropertyBlock(propertyBlock);

            currentAnim = animType;
            currentFacing = facing;
            return true;
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
