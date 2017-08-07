
using UnityEngine;

public class DOFSection : PostProcessSection 
{
    public bool enable = false;

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

    private RenderTexture rt = null;


    public override void OnPreRenderByProcess()
    {    
        Material mat = m_postProcess.GetMaterial("compose");
        if (IsRenderEnable())
        {
            mat.EnableKeyword("_DOF");
        }
        else
        {
            mat.DisableKeyword("_DOF");
        }       
    }

    public override bool IsRenderEnable()
    {
        return enable;
    }

    public override void OnPostProcess(RenderTexture src)
    {
        if (!IsRenderEnable()) return;

        Material mat = m_postProcess.GetMaterial("compose");
        rt = RenderTexture.GetTemporary(src.width >> blurDownsample, src.height >> blurDownsample, 0, RenderTextureFormat.ARGB32);
        PostProcessUtils.GaussianBlur(src, rt, new Vector2(src.width, src.height), blurDownsample, blurIterations, blurSize);

        mat.SetTexture("_BlurTex", rt);
        mat.SetFloat("_focalDistance", FocalDistance01(focalDistance));
        mat.SetFloat("_nearBlurScale", nearBlurScale);
        mat.SetFloat("_farBlurScale", farBlurScale);
    }

    public override void OnPostPostRender()
    {
        if (rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);
            rt = null;
        }
    }

    private float FocalDistance01(float distance)
    {
        Camera camera = m_postProcess.GetProcessMgr().GetMainCamera();
        return camera.WorldToViewportPoint((distance - camera.nearClipPlane) *
            camera.transform.forward + camera.transform.position).z / (camera.farClipPlane - camera.nearClipPlane);
    }
}