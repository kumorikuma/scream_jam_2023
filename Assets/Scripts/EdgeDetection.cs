using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
[PostProcess(typeof(EdgeDetectionRenderer), PostProcessEvent.AfterStack, "Custom/EdgeDetection")]
public sealed class EdgeDetection : PostProcessEffectSettings
{
    [Tooltip("Color of the edges")]
    public ColorParameter edgeColor = new ColorParameter { value = Color.black };

    [Tooltip("Width of the edges")]
    public FloatParameter edgeWidth = new FloatParameter { value = 0.01f };
}

