using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class NormalRenderPass : ScriptableRenderPass
{
    private static readonly int modelPartTextureID = Shader.PropertyToID("_ModelNormalsTexture");
    private static readonly RenderTargetIdentifier identifier = new RenderTargetIdentifier(modelPartTextureID);
    // RenderTargetIdentifier targetIdentifier = new RenderTargetIdentifier(modelPartTextureID);
    ShaderTagId shaderTagId = new ShaderTagId("Normals");

    // This method is called before executing the render pass.
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in a performant manner.
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {

        RenderTextureDescriptor cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        // RenderTextureDescriptor descriptor = new RenderTextureDescriptor(cameraDescriptor.width, cameraDescriptor.height, RenderTextureFormat.R16, 16);
        cmd.GetTemporaryRT(modelPartTextureID, cameraDescriptor.width, cameraDescriptor.height, cameraDescriptor.depthBufferBits, FilterMode.Point, RenderTextureFormat.ARGB32);

        ConfigureClear(ClearFlag.All, Color.black);
        ConfigureTarget(identifier);
    }

    // Here you can implement the rendering logic.
    // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
    // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
    // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
        DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagId, ref renderingData, sortingCriteria);
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
    }

    // Cleanup any allocated resources that were created during the execution of this render pass.
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(modelPartTextureID);
    }
}