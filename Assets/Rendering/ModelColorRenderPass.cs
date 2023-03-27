using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ModelColorRenderPass : ScriptableRenderPass
{
    public Material mat;
    private static readonly int modelPartTextureID = Shader.PropertyToID("_ModelPartTexture");
    private static readonly RenderTargetIdentifier sourceID = new RenderTargetIdentifier(modelPartTextureID);
    // ShaderTagId shaderTagId = new ShaderTagId("VisualizeTag");

    public ModelColorRenderPass(Material mat)
    {
        this.mat = mat;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("ModelColorRenderPass");
        Blit(cmd, sourceID, renderingData.cameraData.renderer.cameraColorTarget, mat);
        context.ExecuteCommandBuffer(cmd);
        cmd.Release();
    }
}
