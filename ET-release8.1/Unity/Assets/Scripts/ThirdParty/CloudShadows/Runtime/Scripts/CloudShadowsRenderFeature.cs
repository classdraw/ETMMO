using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SSCS {

    public class CloudShadowsRenderFeature : ScriptableRendererFeature {

        class CloudShadowsRenderPass : ScriptableRenderPass {

            public CloudShadowsRenderPass() {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
                ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
                Camera cam = renderingData.cameraData.camera;
                if (cam == null || cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView) {
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get("SSCS Cloud Shadows");
                CloudShadows.RenderAllForCamera(cam, cmd);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        CloudShadowsRenderPass m_Pass;

        public override void Create() {
            m_Pass = new CloudShadowsRenderPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            if (!CloudShadows.HasActiveInstances) {
                return;
            }

            renderer.EnqueuePass(m_Pass);
        }
    }

}
