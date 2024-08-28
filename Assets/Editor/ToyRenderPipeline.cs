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

    private CameraRenderer cameraRenderer;
    private Lighting lighting;
    private CommandBuffer cmd;

    public Vector2 ScreenResolution;
    public ToyRenderPipeline()
    {
        cmd = new CommandBuffer();
        cmd.name = "GBuffer";

        cameraRenderer = new CameraRenderer();
        lighting = new Lighting();
        
        gDepth = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
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
        foreach (var camera in cameras)
        {
            Debug.Log("Screen Width : " + Screen.width + "Screen Height : " + Screen.height);
            context.SetupCameraProperties(camera);
            
            cmd.SetRenderTarget(gBufferID, gDepth);
            cmd.SetGlobalTexture("_GDepth", gDepth);
            for (int i = 0; i < 4; i++)
            {
                cmd.SetGlobalTexture("_GT" + i, gBuffers[i]);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            cameraRenderer.Render(context, camera);
            
            lighting.Setup(context, cameraRenderer.cullingResults);
            
            DrawLightPass(context, camera);
            
            context.DrawSkybox(camera);
            if (Handles.ShouldRenderGizmos())
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
            
            context.Submit();
        }
    }
    void DrawLightPass(ScriptableRenderContext context, Camera camera)
    {
        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "lightPass";
        Material material = new Material(Shader.Find("ToyRenderPipeline/lightPass"));
        cmd.Blit(null, BuiltinRenderTextureType.CameraTarget, material);
        context.ExecuteCommandBuffer(cmd);
    }
}
