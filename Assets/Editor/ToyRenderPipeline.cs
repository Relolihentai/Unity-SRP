using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ToyRenderPipeline : RenderPipeline
{
    private RenderTexture gDepth;
    private RenderTexture[] gBuffers = new RenderTexture[4];
    private RenderTargetIdentifier[] gBufferID = new RenderTargetIdentifier[4];

    public ToyRenderPipeline()
    {
        gDepth = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        gBuffers[0] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        gBuffers[1] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);
        gBuffers[2] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
        gBuffers[3] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

        for (int i = 0; i < 4; i++)
        {
            gBufferID[i] = gBuffers[i];
        }

        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        Camera camera = cameras[0];
        context.SetupCameraProperties(camera);

        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "GBuffer";
        
        cmd.SetRenderTarget(gBufferID, gDepth);
        cmd.SetGlobalTexture("_GDepth", gDepth);
        for (int i = 0; i < 4; i++)
        {
            cmd.SetGlobalTexture("_GT" + i, gBuffers[i]);
        }
        
        //清屏
        cmd.ClearRenderTarget(true, true, Color.red);
        //ScriptableRenderContext接受图形命令
        context.ExecuteCommandBuffer(cmd);

        //剔除
        camera.TryGetCullingParameters(out var cullingParameters);
        var cullingResults = context.Cull(ref cullingParameters);

        //DrawRenderers的配置
        ShaderTagId shaderTagId = new ShaderTagId("GBuffer");
        SortingSettings sortingSettings = new SortingSettings(camera);
        DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;
        
        //绘制
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        
        //skybox gizmos
        context.DrawSkybox(camera);
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
        
        LightPass(context, camera);
        
        //submit的时候才会提交命令
        context.Submit();
    }

    void LightPass(ScriptableRenderContext context, Camera camera)
    {
        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "lightPass";
        Material material = new Material(Shader.Find("ToyRenderPipeline/lightPass"));
        cmd.Blit(gBufferID[0], BuiltinRenderTextureType.CameraTarget, material);
        context.ExecuteCommandBuffer(cmd);
    }
}
