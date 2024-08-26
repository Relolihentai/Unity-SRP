using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string cmdName = "Lighting";
    private CommandBuffer cmd = new CommandBuffer()
    {
        name = cmdName
    };

    private const int maxDirLightCount = 4;
    private static int dirLightCountID = Shader.PropertyToID("_DirectionalLightCount");
    private static int dirLightColorsID = Shader.PropertyToID("_DirectionalLightColors");
    private static int dirLightDirectionsID = Shader.PropertyToID("_DirectionalLightDirections");

    private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

    private CullingResults cullingResults;
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
    {
        this.cullingResults = cullingResults;
        cmd.BeginSample(cmdName);
        SetupLights();
        cmd.EndSample(cmdName);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    private void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                //ref 时间换空间更快
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                {
                    break;
                }
            }
        }
        
        cmd.SetGlobalInt(dirLightCountID, dirLightCount);
        cmd.SetGlobalVectorArray(dirLightColorsID, dirLightColors);
        cmd.SetGlobalVectorArray(dirLightDirectionsID, dirLightDirections);
    }
    private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;
        //第三列，注意取反
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }
}
