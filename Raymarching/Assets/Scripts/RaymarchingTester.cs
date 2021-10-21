
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RaymarchingTester : MonoBehaviour
{
    [SerializeField]
    private ComputeShader RaymarchingCompute = null;


    void Update()
    {
        if (RaymarchingCompute == null)
            return;
            
        CommandBuffer cmd = CommandBufferPool.Get("Test000");
        cmd.Clear();

        var rt = Camera.main.targetTexture;


    }
}
