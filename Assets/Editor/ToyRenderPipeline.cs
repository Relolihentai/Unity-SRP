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

    
    private CSM csm;
    private RenderTexture[] csmShadowTextures = new RenderTexture[4];
    public ToyRenderPipeline()
    {
        cmd = new CommandBuffer();
        cmd.name = "GBuffer";

        cameraRenderer = new CameraRenderer();
        lighting = new Lighting();

        gDepth = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        gBuffers[0] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        gBuffers[1] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);
        gBuffers[2] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
        gBuffers[3] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

        for (int i = 0; i < 4; i++)
        {
            gBufferID[i] = gBuffers[i];
        }

        for (int i = 0; i < 4; i++)
        {
            csmShadowTextures[i] = new RenderTexture(ToyRenderPipelineAsset.instance.renderPipelineData.CsmSettings.shadowMapResolution,
                                                    ToyRenderPipelineAsset.instance.renderPipelineData.CsmSettings.shadowMapResolution, 24,
                                                         RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        }
        csm = new CSM(ToyRenderPipelineAsset.instance.renderPipelineData.CsmSettings);
        
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            Debug.Log("Screen Width : " + Screen.width + "Screen Height : " + Screen.height);
            
            //这里放在最开始，是因为后面要走Deferred，要重新设置渲染目标GT0123
            context.SetupCameraProperties(camera);
            DrawShadowPass(context, camera);
            cmd.Clear();
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

    void DrawShadowPass(ScriptableRenderContext context, Camera camera)
    {
        Light light = RenderSettings.sun;
        Vector3 lightDir = light.transform.rotation * Vector3.forward;
        csm.Update(camera, lightDir);
        csm.SaveMainCameraSettings(ref camera);
        for (int level = 0; level < 4; level++)
        {
            cmd.SetGlobalTexture("_ShadowMap_" + level, csmShadowTextures[level]);
            cmd.SetGlobalFloat("_CSM_Split_" + level, csm.csm_settings.splits[level]);
            
            csm.ConfigCameraToShadowSpace(ref camera, lightDir, level);
            Matrix4x4 v = camera.worldToCameraMatrix;
            Matrix4x4 p = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            cmd.SetGlobalMatrix("Toy_ShadowMatrixVP_" + level, p * v);
            cmd.SetGlobalFloat("_CSM_Radius_" + level, camera.farClipPlane / 2.0f);
            cmd.SetGlobalVector("_CSM_SphereCenter_" + level, camera.transform.position);
            cmd.name = "shadowMap" + level;
            context.SetupCameraProperties(camera);
            cmd.SetRenderTarget(csmShadowTextures[level]);
            cmd.ClearRenderTarget(true, true, Color.clear);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            camera.TryGetCullingParameters(out var cullingParameters);
            var cullResults = context.Cull(ref cullingParameters);

            ShaderTagId shaderTagId = new ShaderTagId("DepthOnly");
            SortingSettings sortingSettings = new SortingSettings(camera);
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            context.DrawRenderers(cullResults, ref drawingSettings, ref filteringSettings);
            context.Submit();
        }
        csm.RevertMainCameraSettings(ref camera);
        //注意每次改变摄像机参数后，都要重新setup
        context.SetupCameraProperties(camera);
    }
}
