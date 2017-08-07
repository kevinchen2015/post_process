

using UnityEngine;

public class PostProcessUtils 
{
    static Material blurMaterial;

    static void Check()
    {
        if(blurMaterial == null)
        {
            blurMaterial = new Material(Shader.Find("post/fast_blur"));
        }
    }

    public static void Flip(RenderTexture src, RenderTexture dest,Vector4 param)
    {
        Check();
        blurMaterial.SetVector("_flip_param", param);
        Graphics.Blit(src, dest, blurMaterial, 6);
    }

    public static void GaussianBlur(RenderTexture src, RenderTexture dest,
                                  Vector2 size, int blurDownsample, int blurIterations, float blurSize)
    {
        Check();
        src.filterMode = FilterMode.Bilinear;
        int rtW = (int)size.x >> blurDownsample;
        int rtH = (int)size.y >> blurDownsample;
        RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(src, rt, blurMaterial, 0);
        for (int i = 0; i < blurIterations; i++)
        {
            // vertical blur
            blurMaterial.SetVector("_offsets", new Vector4(0, blurSize, 0, 0));
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, blurMaterial, 5);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;

            // horizontal blur
            blurMaterial.SetVector("_offsets", new Vector4( blurSize,0, 0, 0));
            rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, blurMaterial, 5);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
        }
        Graphics.Blit(rt, dest);
        RenderTexture.ReleaseTemporary(rt);
    }


    public static void Blur(RenderTexture src, RenderTexture dest,
                    Vector2 size, int blurDownsample,int blurIterations,float blurSize)
    {
        Check();
        float widthMod = 1.0f / (1.0f * (1 << blurDownsample));
        blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
        src.filterMode = FilterMode.Bilinear;
        int rtW = (int)size.x >> blurDownsample;
        int rtH = (int)size.y >> blurDownsample;
        RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(src, rt, blurMaterial, 0);
        for (int i = 0; i < blurIterations; i++)
        {
            float iterationOffs = (i * 1.0f);
            blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

            // vertical blur
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, blurMaterial, 1);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;

            // horizontal blur
            rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, blurMaterial, 2);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
        }
        Graphics.Blit(rt, dest);
        RenderTexture.ReleaseTemporary(rt);
    }
  
}