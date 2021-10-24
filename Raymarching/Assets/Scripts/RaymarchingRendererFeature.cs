using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RaymarchingRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class RaymarchingSettings
    {
        public bool IsEnabled = true;

        public bool UseComputeShader = false;

        public ComputeShader RaymarchingCompute;
        public Material Mat;

        public string ProfileTag;
        public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;

        public Vector3 TempValue;
        public Vector3 TempValue2;
    }

    public RaymarchingSettings settings = new RaymarchingSettings();
    RenderTargetHandle RenderTarget;
    RaymarchingRenderPass RenderPass;

    public override void Create()
    {
        RenderPass = new RaymarchingRenderPass(settings);
        RenderPass.renderPassEvent = settings.WhenToInsert;

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.IsEnabled)
        {
            return;
        }


        var cameraColorTargetIdent = renderer.cameraColorTarget;
        RenderPass.Setup(cameraColorTargetIdent);
        renderer.EnqueuePass(RenderPass);

    }
}

class RaymarchingRenderPass : ScriptableRenderPass
{
    string profilerTag;
    Material materialToBlit;
    RenderTargetIdentifier cameraColorTargetIdent;
    RenderTargetHandle tempTexture;

    RaymarchingRendererFeature.RaymarchingSettings Settings;

    RenderTexture ResultTexture;

    int Width;
    int Height;

    public RaymarchingRenderPass(RaymarchingRendererFeature.RaymarchingSettings settings)
    {
        Settings = settings;
    }

    public void Setup(RenderTargetIdentifier cameraColorTargetIdent)
    {
        this.cameraColorTargetIdent = cameraColorTargetIdent;

    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);

        Width = cameraTextureDescriptor.width;
        Height = cameraTextureDescriptor.height;
        if (ResultTexture == null)
        {
            ResultTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            ResultTexture.enableRandomWrite = true;
            ResultTexture.Create();
        }
        
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();


        if (Settings.UseComputeShader == true)
        {
            int groupX = Mathf.CeilToInt(Width / 8.0f);
            int groupY = Mathf.CeilToInt(Height / 8.0f);

            // 1. ī�޶� ���� �ؽ��� �޾ƿ�
            // �ƴ�. �޾ƿ� �ʿ� ����
            // 2. ī�޶� ���� �ؽ��ĸ� ��ǻƮ ���̴��� �ְ� dispatch
            // 3. ��ǻƮ ���̴��� ������� �ް� ������ ����
            // https://answers.unity.com/questions/1700156/computebuffer-stays-empty-in-compute-shader-in-urp.html
            //cmd.SetComputeTextureParam(Settings.RaymarchingCompute, 0, "Result", cameraColorTargetIdent);
            Settings.RaymarchingCompute.SetTexture(0, "Result", ResultTexture);

            Settings.RaymarchingCompute.SetMatrix("_CameraToWorld", Camera.main.cameraToWorldMatrix);
            Settings.RaymarchingCompute.SetMatrix("_InverseCameraProjection", Camera.main.projectionMatrix.inverse);

            Settings.RaymarchingCompute.SetVector("TempValue", Settings.TempValue);
            Settings.RaymarchingCompute.SetVector("TempValue2", Settings.TempValue2);

            Settings.RaymarchingCompute.SetFloat("_Time", Time.realtimeSinceStartup);

            Settings.RaymarchingCompute.Dispatch(0, groupX, groupY, 1);
            cmd.Blit(ResultTexture, cameraColorTargetIdent);
        }
        else
        {
            Settings.Mat.SetVector("_TempValue", Settings.TempValue);
            cmd.Blit(ResultTexture, cameraColorTargetIdent, Settings.Mat);
        }

        //cmd.Blit(cameraColorTargetIdent, tempTexture.Identifier(), materialToBlit, 0);
        //cmd.Blit(tempTexture.Identifier(), cameraColorTargetIdent);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempTexture.id);
    }
}