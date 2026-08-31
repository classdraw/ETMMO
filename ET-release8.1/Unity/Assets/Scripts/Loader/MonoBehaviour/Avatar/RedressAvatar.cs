using UnityEngine;

namespace ET
{
    /// <summary>
    /// 挂在与 <see cref="FrameSheetAnimPlayer"/> 同一节点，将部位贴图设置到 SR_Character 材质上。
    /// </summary>
    [DisallowMultipleComponent]
    public class RedressAvatar : MonoBehaviour
    {
        private static readonly int BodyMapId = Shader.PropertyToID("_BodyMap");
        private static readonly int HeadMapId = Shader.PropertyToID("_HeadMap");
        private static readonly int TailMapId = Shader.PropertyToID("_TailMap");
        private static readonly int EquipMap1Id = Shader.PropertyToID("_EquipMap1");
        private static readonly int EquipMap2Id = Shader.PropertyToID("_EquipMap2");

        [SerializeField]
        private FrameSheetAnimPlayer animPlayer;

        private MaterialPropertyBlock propertyBlock;

        // 1x1 占位图：SR_Character 通过 texelSize 判断该层是否生效，1x1 时 AssignedMapMask=0。
        private static Texture2D s_EmptyPartTexture;

        private static Texture2D EmptyPartTexture
        {
            get
            {
                if (s_EmptyPartTexture == null)
                {
                    s_EmptyPartTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    s_EmptyPartTexture.SetPixel(0, 0, Color.clear);
                    s_EmptyPartTexture.Apply(false, true);
                }

                return s_EmptyPartTexture;
            }
        }

        private void Awake()
        {
            if (this.animPlayer == null)
            {
                this.animPlayer = this.GetComponent<FrameSheetAnimPlayer>();
            }
        }

        public void ApplyTextures(Texture2D body, Texture2D head, Texture2D tail, Texture2D shirt, Texture2D pants)
        {
            Renderer renderer = this.GetRenderer();
            if (renderer == null)
            {
                return;
            }

            this.EnsurePropertyBlock();
            renderer.GetPropertyBlock(this.propertyBlock);

            SetPartTexture(this.propertyBlock, BodyMapId, body, optional: false);
            SetPartTexture(this.propertyBlock, HeadMapId, head, optional: false);
            SetPartTexture(this.propertyBlock, TailMapId, tail, optional: true);
            SetPartTexture(this.propertyBlock, EquipMap1Id, shirt, optional: true);
            SetPartTexture(this.propertyBlock, EquipMap2Id, pants, optional: true);

            renderer.SetPropertyBlock(this.propertyBlock);

            this.ReplayAnimation();
        }

        private void ReplayAnimation()
        {
            if (this.animPlayer == null)
            {
                return;
            }

            if (this.animPlayer.CurrentAnim != FrameSheetAnimType.None)
            {
                this.animPlayer.Play(this.animPlayer.CurrentAnim, this.animPlayer.CurrentFacing);
                return;
            }

            this.animPlayer.Play(FrameSheetAnimType.Idle);
        }

        private Renderer GetRenderer()
        {
            if (this.animPlayer != null)
            {
                return this.animPlayer.GetComponentInChildren<Renderer>();
            }

            return this.GetComponentInChildren<Renderer>();
        }

        private void EnsurePropertyBlock()
        {
            if (this.propertyBlock == null)
            {
                this.propertyBlock = new MaterialPropertyBlock();
            }
        }

        private static void SetPartTexture(MaterialPropertyBlock block, int propertyId, Texture2D texture, bool optional)
        {
            if (texture != null)
            {
                block.SetTexture(propertyId, texture);
                return;
            }

            if (optional)
            {
                block.SetTexture(propertyId, EmptyPartTexture);
            }
        }
    }
}
