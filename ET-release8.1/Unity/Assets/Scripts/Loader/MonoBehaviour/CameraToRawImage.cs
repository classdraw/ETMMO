using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>
    /// 挂在 RawImage 上，将指定相机的画面渲染到 RawImage。
    /// 相机仅渲染 UIObject 图层，背景透明，不显示天空盒。
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    [DisallowMultipleComponent]
    public class CameraToRawImage : MonoBehaviour
    {
        [SerializeField]
        private Camera previewCamera;

        [SerializeField]
        private string renderLayerName = "UIObject";

        [SerializeField]
        private int textureWidth = 512;

        [SerializeField]
        private int textureHeight = 512;

        [SerializeField]
        private bool matchRawImageSize = true;

        private RawImage rawImage;
        private RenderTexture renderTexture;

        private void Awake()
        {
            this.rawImage = this.GetComponent<RawImage>();
            this.Setup();
        }

        private void OnDestroy()
        {
            this.ReleaseRenderTexture();
        }

        /// <summary>
        /// 运行时更换相机时调用。
        /// </summary>
        public void SetCamera(Camera camera)
        {
            if (this.previewCamera != null && this.previewCamera.targetTexture == this.renderTexture)
            {
                this.previewCamera.targetTexture = null;
            }

            this.previewCamera = camera;
            this.Setup();
        }

        private void Setup()
        {
            if (this.previewCamera == null || this.rawImage == null)
            {
                return;
            }

            this.CreateRenderTexture();
            this.ConfigureCamera();
            this.rawImage.texture = this.renderTexture;
        }

        private void CreateRenderTexture()
        {
            int width = this.textureWidth;
            int height = this.textureHeight;

            if (this.matchRawImageSize)
            {
                RectTransform rectTransform = this.rawImage.rectTransform;
                width = Mathf.Max(1, Mathf.RoundToInt(rectTransform.rect.width));
                height = Mathf.Max(1, Mathf.RoundToInt(rectTransform.rect.height));

                Canvas canvas = this.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    float scale = canvas.scaleFactor;
                    width = Mathf.Max(1, Mathf.RoundToInt(width * scale));
                    height = Mathf.Max(1, Mathf.RoundToInt(height * scale));
                }
            }

            if (this.renderTexture != null
                && this.renderTexture.width == width
                && this.renderTexture.height == height)
            {
                return;
            }

            this.ReleaseRenderTexture();

            this.renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = $"{this.name}_PreviewRT",
                antiAliasing = 1,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            this.renderTexture.Create();
        }

        private void ConfigureCamera()
        {
            int layerMask = LayerMask.GetMask(this.renderLayerName);
            if (layerMask == 0)
            {
                Debug.LogWarning($"[CameraToRawImage] 未找到 Layer: {this.renderLayerName}", this);
            }

            this.previewCamera.clearFlags = CameraClearFlags.SolidColor;
            this.previewCamera.backgroundColor = Color.clear;
            this.previewCamera.cullingMask = layerMask;
            this.previewCamera.targetTexture = this.renderTexture;
            this.previewCamera.enabled = true;
        }

        private void ReleaseRenderTexture()
        {
            if (this.previewCamera != null && this.previewCamera.targetTexture == this.renderTexture)
            {
                this.previewCamera.targetTexture = null;
            }

            if (this.renderTexture == null)
            {
                return;
            }

            this.renderTexture.Release();
            Destroy(this.renderTexture);
            this.renderTexture = null;
        }
    }
}
