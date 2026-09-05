using UnityEngine;

namespace ET
{
    /// <summary>
    /// 挂在与 <see cref="FrameSheetAnimPlayer"/> 同一节点，将部位贴图设置到 SR_Character 材质上。
    /// </summary>
    [DisallowMultipleComponent]
    public class RedressAvatar : MonoBehaviour, IFrameSheetClipTextureRouter
    {
        private static readonly int[] AllSlotPropertyIds = BuildAllSlotPropertyIds();

        [SerializeField]
        private FrameSheetAnimPlayer animPlayer;

        private MaterialPropertyBlock propertyBlock;
        private Texture2D bodyTexture;
        private Texture2D headTexture;
        private Texture2D tailTexture;
        private Texture2D shirtTexture;
        private Texture2D pantsTexture;

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
            this.bodyTexture = body;
            this.headTexture = head;
            this.tailTexture = tail;
            this.shirtTexture = shirt;
            this.pantsTexture = pants;

            this.RefreshTextureSlots();
            this.ReplayAnimation();
        }

        public void RefreshTextureSlots()
        {
            Renderer renderer = this.GetRenderer();
            if (renderer == null)
            {
                return;
            }

            FrameSheetAnimClip clip = null;
            FrameSheetFacing facing = FrameSheetFacing.Down;
            if (this.animPlayer != null)
            {
                facing = this.animPlayer.CurrentFacing;
                if (this.animPlayer.CurrentAnim != FrameSheetAnimType.None
                    && this.animPlayer.TryGetClip(this.animPlayer.CurrentAnim, out FrameSheetAnimClip currentClip))
                {
                    clip = currentClip;
                }
            }

            this.ApplyClipTextureRouting(renderer, clip, facing);
        }

        public void ApplyClipTextureRouting(FrameSheetAnimClip clip, FrameSheetFacing facing)
        {
            Renderer renderer = this.GetRenderer();
            if (renderer == null)
            {
                return;
            }

            this.ApplyClipTextureRouting(renderer, clip, facing);
        }

        private void ApplyClipTextureRouting(Renderer renderer, FrameSheetAnimClip clip, FrameSheetFacing facing)
        {
            this.EnsurePropertyBlock();
            renderer.GetPropertyBlock(this.propertyBlock);

            ClearAllSlots(this.propertyBlock);

            if (clip == null)
            {
                RoutePart(this.propertyBlock, CharacterTextureSlot.BodyTexture, this.bodyTexture, required: true);
                RoutePart(this.propertyBlock, CharacterTextureSlot.HeadTexture, this.headTexture, required: true);
                RoutePart(this.propertyBlock, CharacterTextureSlot.TailTexture2, this.tailTexture, required: false);
                RoutePart(this.propertyBlock, CharacterTextureSlot.EquipTexture1, this.shirtTexture, required: false);
                RoutePart(this.propertyBlock, CharacterTextureSlot.EquipTexture2, this.pantsTexture, required: false);
            }
            else
            {
                FrameSheetAnimResolvedPartSlots slots = clip.GetResolvedPartSlots(facing);
                RoutePart(this.propertyBlock, slots.bodyTextureSlot, this.bodyTexture, required: true);
                RoutePart(this.propertyBlock, slots.headTextureSlot, this.headTexture, required: true);
                RoutePart(this.propertyBlock, slots.tailTextureSlot, this.tailTexture, required: false);
                RoutePart(this.propertyBlock, slots.shirtTextureSlot, this.shirtTexture, required: false);
                RoutePart(this.propertyBlock, slots.pantsTextureSlot, this.pantsTexture, required: false);
            }

            renderer.SetPropertyBlock(this.propertyBlock);
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

        private static int[] BuildAllSlotPropertyIds()
        {
            int slotCount = System.Enum.GetValues(typeof(CharacterTextureSlot)).Length;
            int[] ids = new int[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                ids[i] = CharacterTextureSlotHelper.GetShaderPropertyId((CharacterTextureSlot)i);
            }

            return ids;
        }

        private static void ClearAllSlots(MaterialPropertyBlock block)
        {
            for (int i = 0; i < AllSlotPropertyIds.Length; i++)
            {
                block.SetTexture(AllSlotPropertyIds[i], EmptyPartTexture);
            }
        }

        private static void RoutePart(MaterialPropertyBlock block, CharacterTextureSlot slot, Texture2D texture, bool required)
        {
            int propertyId = CharacterTextureSlotHelper.GetShaderPropertyId(slot);
            SetPartTexture(block, propertyId, texture, optional: !required);
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
