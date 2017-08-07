

using UnityEngine;

public class BloomPostProcess : PostProcess 
{
    [Range(1.0f, 8.0f)]
    public int bloomDownSample = 1;
    public Color colorThreshold = Color.gray; 
    public Color bloomColor = Color.white;
    [Range(0.0f, 1.0f)]
    public float bloomFactor = 0.5f;
    [Range(1.0f, 8.0f)]
    public int blurDownsample = 1;
    [Range(1.0f, 10.0f)]
    public float blurSize = 2;
    [Range(1, 4)]
    public int blurIterations = 1;

    private Material bloomComposeMaterial;

    public override bool IsEnableByLOD()
    {
        if (m_LodLevel >= LODMgr.LOD_LEVEL.LOD_LEVEL_HIGH)
            return true;

        return false;
    }


    public override void Init(PostProcessParam param)
    {
        base.Init(param);

        bloomComposeMaterial = new Material(Shader.Find("post/bloom"));

    }

    void OnDestroy()
    {
        bloomComposeMaterial = null;
    }


    public override void OnPostProcess(RenderTexture src, RenderTexture dest)
    {
        if (!IsEnable())
        {
            DoPostDefault(src, dest);
            return;
        }

        RenderTexture bloomRT = RenderTexture.GetTemporary(src.width >> bloomDownSample, src.height >> bloomDownSample, 0, RenderTextureFormat.ARGB32);
        bloomComposeMaterial.SetVector("_colorThreshold", colorThreshold);
        Graphics.Blit(src, bloomRT, bloomComposeMaterial, 0);

        RenderTexture blurRT = RenderTexture.GetTemporary(src.width >> blurDownsample, src.height >> blurDownsample, 0, RenderTextureFormat.ARGB32);
        PostProcessUtils.GaussianBlur(bloomRT, blurRT, new Vector2(src.width,src.height), blurDownsample, blurIterations, blurSize);

        bloomComposeMaterial.SetTexture("_BlurTex", blurRT);
        bloomComposeMaterial.SetVector("_bloomColor", bloomColor);
        bloomComposeMaterial.SetFloat("_bloomFactor", bloomFactor);

        if (nextPostProcess != null)
        {
            RenderTexture bloomDest = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, bloomDest, bloomComposeMaterial,1);
            nextPostProcess.OnPostProcess(bloomDest, dest);
            RenderTexture.ReleaseTemporary(bloomDest);
        }
        else
        {
            Graphics.Blit(src, dest, bloomComposeMaterial,1);
        }
 
        RenderTexture.ReleaseTemporary(bloomRT);
        RenderTexture.ReleaseTemporary(blurRT);

    }

  

}