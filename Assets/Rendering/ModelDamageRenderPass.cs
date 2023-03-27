using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FilterRenderPass : ScriptableRenderPass
{
    public Material mat;
    private static readonly int tempTextureID = Shader.PropertyToID("_TempFilterTexture");
    private static readonly RenderTargetIdentifier tempID = new RenderTargetIdentifier(tempTextureID);
    private static readonly int modelPartTextureID = Shader.PropertyToID("_ModelPartTexture");
    private static readonly RenderTargetIdentifier targetID = new RenderTargetIdentifier(modelPartTextureID);
    // ShaderTagId shaderTagId = new ShaderTagId("VisualizeTag");

    public FilterRenderPass(Material mat)
    {
        this.mat = mat;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {

        RenderTextureDescriptor cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        cmd.GetTemporaryRT(tempTextureID, cameraDescriptor.width, cameraDescriptor.height, cameraDescriptor.depthBufferBits, FilterMode.Point, RenderTextureFormat.R16);

        ConfigureClear(ClearFlag.All, Color.black);
        ConfigureTarget(tempID);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // CommandBuffer cmd = new CommandBuffer();
        CommandBuffer cmd = CommandBufferPool.Get("FilterRenderPass");
        Blit(cmd, modelPartTextureID, tempID, mat);
        Blit(cmd, tempID, modelPartTextureID);
        context.ExecuteCommandBuffer(cmd);
        cmd.Release();
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempTextureID);
    }
}
