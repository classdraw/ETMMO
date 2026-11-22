using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SSCS {

    /// <summary>
    /// Draws SSCS cloud overlays after ProPixelizer recomposition (Game / orthographic cameras).
    /// </summary>
    public class CloudShadowsRenderFeature : ScriptableRendererFeature {

        class CloudShadowsRenderPass : ScriptableRenderPass {

            readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SSCS Cloud Shadows");

            public CloudShadowsRenderPass() {
                // After ProPixelizer export / color grading; before final present.
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
                ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
                Camera cam = renderingData.cameraData.camera;
                if (!ShouldRenderForCamera(cam)) {
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get("SSCS Cloud Shadows");
                using (new ProfilingScope(cmd, m_ProfilingSampler)) {
                    CloudShadows.RenderAllForCamera(cam, cmd);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        static bool ShouldRenderForCamera(Camera cam) {
            if (cam == null || !cam.orthographic) {
                return false;
            }
            if (cam.cameraType != CameraType.Game) {
                return false;
            }
            if (cam.targetTexture != null) {
                return false;
            }
            return true;
        }

        CloudShadowsRenderPass m_Pass;

        public override void Create() {
            m_Pass = new CloudShadowsRenderPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            if (!CloudShadows.DrawOverlayViaRenderFeature || !CloudShadows.HasActiveInstances) {
                return;
            }
            if (!ShouldRenderForCamera(renderingData.cameraData.camera)) {
                return;
            }

            renderer.EnqueuePass(m_Pass);
        }
    }

}