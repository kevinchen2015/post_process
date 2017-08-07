

using UnityEngine;

public class RadialBlurPostProcess : PostProcess 
{
    [Range(0, 0.1f)]
    public float blurFactor = 0.05f;
    [Range(0.0f, 2.0f)]
    public float lerpFactor = 0.5f;
    [Range(1, 8)]
    public int downSample = 2;
    [Range(1, 9)]
    public int sampleStrength = 6;
    public Vector2 blurCenter = new Vector2(0.5f, 0.5f);

    private Material composeMaterial;
    public override bool IsEnableByLOD()
    {
        if (m_LodLevel >= LODMgr.LOD_LEVEL.LOD_LEVEL_HIGH)
            return true;

        return false;
    }

    public override void Init(PostProcessParam param)
    {
        base.Init(param);
        composeMaterial = new Material(Shader.Find("post/radial_blur"));
    }

    void OnDestroy()
    {
        composeMaterial = null;
    }

    public override void OnPostProcess(RenderTexture src, RenderTexture dest)
    {
        if (!IsEnable())
        {
            DoPostDefault(src, dest);
            return;
        }

        RenderTexture blurRT = RenderTexture.GetTemporary(src.width >> downSample, src.height >> downSample, 0, RenderTextureFormat.ARGB32);
        composeMaterial.SetFloat("_BlurFactor", blurFactor);
        composeMaterial.SetVector("_BlurCenter", blurCenter);
        composeMaterial.SetFloat("_SampleStrength", sampleStrength);
        Graphics.Blit(src, blurRT, composeMaterial, 0);

        composeMaterial.SetTexture("_BlurTex", blurRT);
        composeMaterial.SetFloat("_LerpFactor", lerpFactor);

        if (nextPostProcess != null)
        {
            RenderTexture radialBlurDest = RenderTexture.GetTemporary(src.width , src.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, radialBlurDest, composeMaterial,1);
            nextPostProcess.OnPostProcess(radialBlurDest, dest);
            RenderTexture.ReleaseTemporary(radialBlurDest);
        }
        else
        {
            Graphics.Blit(src, dest, composeMaterial,1);
        }
        RenderTexture.ReleaseTemporary(blurRT);
    }
}