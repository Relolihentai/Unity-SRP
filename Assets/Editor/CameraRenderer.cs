using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;
    private CommandBuffer cmd;
    private CameraClearFlags clearFlags;
    
    public CullingResults cullingResults;
    public CameraRenderer()
    {
        cmd = new CommandBuffer();
        cmd.name = "Camera Render";
    }

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;
        
        Setup();
        DrawSceneWindowUI();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        Submit();
    }

    void Setup()
    {
        SetCmdNameByCameraName();
        clearFlags = camera.clearFlags;
        // cmd.ClearRenderTarget(clearFlags <= CameraClearFlags.Depth,clearFlags == CameraClearFlags.Color,
        //     clearFlags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        cmd.ClearRenderTarget(true, true, Color.clear);
        cmd.BeginSample(cmd.name);
        ExecuteBuffer();
    }
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    void DrawVisibleGeometry()
    {
        camera.TryGetCullingParameters(out var cullingParameters);
        cullingResults = context.Cull(ref cullingParameters);
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        ShaderTagId shaderTagId = new ShaderTagId("GBuffer");
        var drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        //绘制顺序从后往前
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
    
    void Submit()
    {
        cmd.EndSample(cmd.name);
        ExecuteBuffer();
        context.Submit();
    }
    
    partial void DrawUnsupportedShaders();
    partial void DrawSceneWindowUI();
    
    partial void SetCmdNameByCameraName();
#if UNITY_EDITOR
    private static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    private static Material errorMaterial;
    partial void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/Core/FallbackError"));
        }

        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
        {
            overrideMaterial = errorMaterial
        };
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }

        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    partial void DrawSceneWindowUI()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }

    partial void SetCmdNameByCameraName()
    {
        Profiler.BeginSample("Editor Only");
        cmd.name = camera.name;
        Profiler.EndSample();
    }
#endif
}
