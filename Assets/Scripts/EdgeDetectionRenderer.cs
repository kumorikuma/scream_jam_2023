using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class EdgeDetectionRenderer : PostProcessEffectRenderer<EdgeDetection>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Custom/EdgeDetection"));
        sheet.properties.SetColor("_EdgeColor", settings.edgeColor);
        sheet.properties.SetFloat("_EdgeWidth", settings.edgeWidth);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
