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

        private Material runtimeMaterial;

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

            this.EnsureRuntimeMaterial(renderer);

            if (body != null)
            {
                this.runtimeMaterial.SetTexture(BodyMapId, body);
            }

            if (head != null)
            {
                this.runtimeMaterial.SetTexture(HeadMapId, head);
            }

            this.runtimeMaterial.SetTexture(TailMapId, tail);
            this.runtimeMaterial.SetTexture(EquipMap1Id, shirt);
            this.runtimeMaterial.SetTexture(EquipMap2Id, pants);

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

        private void EnsureRuntimeMaterial(Renderer renderer)
        {
            if (this.runtimeMaterial == null)
            {
                this.runtimeMaterial = renderer.material;
            }
        }
    }
}
