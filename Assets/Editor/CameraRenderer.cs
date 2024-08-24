using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;
    private CommandBuffer cmd;
    
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
        DrawVisibleGeometry();
        DrawGizmos();
        Submit();
    }

    void Setup()
    {
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
        var cullingResults = context.Cull(ref cullingParameters);
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        ShaderTagId shaderTagId = new ShaderTagId("GBuffer");
        var drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        context.DrawSkybox(camera);
    }
    void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }
    
    void Submit()
    {
        cmd.EndSample(cmd.name);
        ExecuteBuffer();
        context.Submit();
    }
}
