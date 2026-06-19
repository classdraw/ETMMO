using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XEngine.Utilities;

namespace XEngine.Hud {
    public class HudRenderPassFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private SpriteAtlasConfig m_kConfig;
        
        private class HudRenderPass : ScriptableRenderPass
        {
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                
            }
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                
            }
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (HudTitleRender.GetInstance().m_bStart) {
                    CommandBuffer hudTitleCndBuffer = HudTitleRender.GetInstance().GetCmdBuffer();
                    if (hudTitleCndBuffer != null && hudTitleCndBuffer.sizeInBytes > 0)
                    {
                        context.ExecuteCommandBuffer(hudTitleCndBuffer);
                    }
                }
               
                if (HudNumberRender.GetInstance().m_bStart) {
                    CommandBuffer hudNumberCndBuffer = HudNumberRender.GetInstance().GetCmdBuffer();

                    if (hudNumberCndBuffer != null && hudNumberCndBuffer.sizeInBytes > 0)
                    {
                        context.ExecuteCommandBuffer(hudNumberCndBuffer);
                    }
                }
                
            }
        }


        private HudRenderPass m_kHudRenderPass;
        public override void Create()
        {
            m_kHudRenderPass = new HudRenderPass();
            m_kHudRenderPass.renderPassEvent = m_kConfig.m_eRenderPassEvent;
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_kHudRenderPass);
        }


    }


}


