using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct ToyRenderPipelineData
{
    public Vector2Int ScreenResolution;
}

[CreateAssetMenu(menuName = "RenderPipeline/ToyRenderPipeline")]
public class ToyRenderPipelineAsset : RenderPipelineAsset
{
    public static ToyRenderPipelineAsset instance;
    public ToyRenderPipelineData renderPipelineData;
    protected override RenderPipeline CreatePipeline()
    {
        instance = this;
        return new ToyRenderPipeline();
    }
}
