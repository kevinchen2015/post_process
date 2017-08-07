

using UnityEngine;

public class DOFPostProcess : PostProcess 
{
    [Range(0.0f, 100.0f)]
    public float focalDistance = 10.0f;
    [Range(0.0f, 100.0f)]
    public float nearBlurScale = 0.0f;
    [Range(0.0f, 1000.0f)]
    public float farBlurScale = 50.0f;
    [Range(1.0f, 8.0f)]
    public int blurDownsample = 1;
    [Range(1.0f, 10.0f)]
    public float blurSize = 2;
    [Range(1, 4)]
    public int blurIterations = 1;

    private Material dofComposeMaterial;

    public override bool IsEnableByLOD()
    {
        if (m_LodLevel >= LODMgr.LOD_LEVEL.LOD_LEVEL_HIGH)
            return true;

        return false;
    }

    public override bool IsNeedDepth()
    {
        return (IsEnable() && true);
    }

    public override void Init(PostProcessParam param)
    {
        base.Init(param);

        dofComposeMaterial = new Material(Shader.Find("post/dof"));
    }

    public override bool IsRenderEnable()
    {
        return false;
    }

    void OnDestroy()
    {
        dofComposeMaterial = null;
    }


    public override void OnPostProcess(RenderTexture src, RenderTexture dest)
    {
        if (!IsEnable())
        {
            DoPostDefault(src, dest);
            return;
        }

        //blur
        RenderTexture blurRT = RenderTexture.GetTemporary(src.width >> blurDownsample, src.height >> blurDownsample, 0, RenderTextureFormat.ARGB32);
        PostProcessUtils.GaussianBlur(src, blurRT, new Vector2(src.width, src.height), blurDownsample, blurIterations, blurSize);

        dofComposeMaterial.SetTexture("_BlurTex", blurRT); 
        dofComposeMaterial.SetFloat("_focalDistance", FocalDistance01(focalDistance));
        dofComposeMaterial.SetFloat("_nearBlurScale", nearBlurScale);
        dofComposeMaterial.SetFloat("_farBlurScale", farBlurScale);

        if (nextPostProcess != null)
        {
            RenderTexture dofDest = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, dofDest, dofComposeMaterial);
            nextPostProcess.OnPostProcess(dofDest, dest);
            RenderTexture.ReleaseTemporary(dofDest);
        }
        else
        {
            Graphics.Blit(src, dest, dofComposeMaterial);
        }
        RenderTexture.ReleaseTemporary(blurRT);
    }

    private float FocalDistance01(float distance)
    {
        return m_mainCamera.WorldToViewportPoint((distance - m_mainCamera.nearClipPlane) * 
            m_mainCamera.transform.forward + m_mainCamera.transform.position).z / (m_mainCamera.farClipPlane - m_mainCamera.nearClipPlane);
    }

}