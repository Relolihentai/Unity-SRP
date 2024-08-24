using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpaqueMaterialPropertyBlock : MonoBehaviour
{
    private static int _BaseColorID = Shader.PropertyToID("_BaseColor");
    private static MaterialPropertyBlock materialPropertyBlock;

    [SerializeField] private Color baseColor = Color.white;

    private void OnValidate()
    {
        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }
        materialPropertyBlock.SetColor(_BaseColorID, baseColor);
        GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
    }

    private void Awake()
    {
        OnValidate();
    }
}
