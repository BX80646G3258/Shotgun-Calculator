using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class CalculatorFeature : ScriptableRendererFeature
{
    public Vector2[] spreadPattern;
    public Material partColorPassMat;
    public Material damagePassMat;
    public Material damageSpreadMat;
    public Material damageColorPassMat;
    public Material damagePatternMat;
    NormalRenderPass normalRenderPass;
    ModelPartRenderPass modelPartPass;
    ModelDepthRenderPass modelDepthPass;
    ModelColorRenderPass partColorPass;
    ModelColorRenderPass damageColorPass;
    FilterRenderPass modelDamagePass;
    FilterRenderPass damageSpreadPass;
    FilterRenderPass damagePatternPass;
    int spreadPatternID = Shader.PropertyToID("_SpreadPattern");

    /// <inheritdoc/>
    public override void Create()
    {
        UpdateSpreadPattern();

        normalRenderPass = new NormalRenderPass();
        modelPartPass = new ModelPartRenderPass();
        modelDepthPass = new ModelDepthRenderPass();
        partColorPass = new ModelColorRenderPass(partColorPassMat);
        modelDamagePass = new FilterRenderPass(damagePassMat);
        damageSpreadPass = new FilterRenderPass(damageSpreadMat);
        damagePatternPass = new FilterRenderPass(damagePatternMat);
        damageColorPass = new ModelColorRenderPass(damageColorPassMat);

        // Configures where the render pass should be injected.
        normalRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        modelPartPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        modelDepthPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        partColorPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        modelDamagePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        damageSpreadPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        damagePatternPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        damageColorPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(normalRenderPass);
        renderer.EnqueuePass(modelPartPass);
        renderer.EnqueuePass(modelDepthPass);
        // renderer.EnqueuePass(partColorPass);

        renderer.EnqueuePass(modelDamagePass);
        renderer.EnqueuePass(damageSpreadPass);
        renderer.EnqueuePass(damagePatternPass);
        renderer.EnqueuePass(damageColorPass);
    }

    public void UpdateSpreadPattern()
    {
        List<Vector4> vec4Array = new List<Vector4>();
        foreach (Vector2 v2 in spreadPattern)
            vec4Array.Add(new Vector4(v2.x, v2.y, 0, 0));
        damagePatternMat.SetVectorArray(spreadPatternID, vec4Array);
        Debug.Log("updated spread pattern");
    }
}


