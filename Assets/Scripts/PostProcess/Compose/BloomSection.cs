
using UnityEngine;

public class BloomSection : PostProcessSection 
{
    public bool enable = false;

    [Range(1.0f, 8.0f)]
    public int bloomDownSample = 1;
    public Color colorThreshold = Color.gray;
    public Color bloomColor = Color.white;
    [Range(0.0f, 2.0f)]
    public float bloomFactor = 0.5f;
    [Range(1.0f, 8.0f)]
    public int blurDownsample = 1;
    [Range(1.0f, 10.0f)]
    public float blurSize = 2;
    [Range(0, 4)]
    public int blurIterations = 1;


    private RenderTexture bloomRT;
    private RenderTexture blurRT;
    private Material bloomComposeMaterial;
    public override void Init(PostProcessInterface postProcess)
    {
        base.Init(postProcess);
        bloomComposeMaterial = new Material(Shader.Find("post/bloom"));
    }

    public override void OnPreRenderByProcess()
    {
        Material compose = m_postProcess.GetMaterial("compose");
        if (enable)
        {
            compose.EnableKeyword("_BLOOM");
        }
        else
        {
            compose.DisableKeyword("_BLOOM");
        }
    }

    public override bool IsRenderEnable()
    {
        return enable;
    }

    public override void OnPostProcess(RenderTexture src)
    {
        if (!IsRenderEnable()) return;

        bloomRT = RenderTexture.GetTemporary(src.width >> bloomDownSample, src.height >> bloomDownSample, 0, RenderTextureFormat.ARGB32);
        bloomComposeMaterial.SetVector("_colorThreshold", colorThreshold);
        Graphics.Blit(src, bloomRT, bloomComposeMaterial, 0);

        Material compose = m_postProcess.GetMaterial("compose");
        compose.EnableKeyword("_BLOOM");
        if (blurIterations > 0)
        {
            blurRT = RenderTexture.GetTemporary(src.width >> blurDownsample, src.height >> blurDownsample, 0, RenderTextureFormat.ARGB32);
            PostProcessUtils.GaussianBlur(bloomRT, blurRT, new Vector2(src.width, src.height), blurDownsample, blurIterations, blurSize);
            compose.SetTexture("_BloomMask", blurRT);
        }
        else
        {
            compose.SetTexture("_BloomMask", bloomRT);
        }
        compose.SetVector("_BloomColor", bloomColor);
        compose.SetFloat("_BloomFactor", bloomFactor);
    }

    public override void OnPostPostRender()
    {
        if(bloomRT != null)
        {
            RenderTexture.ReleaseTemporary(bloomRT);
            bloomRT = null;
        }
        if(blurRT != null)
        {
            RenderTexture.ReleaseTemporary(blurRT);
            blurRT = null;
        }
    }
}