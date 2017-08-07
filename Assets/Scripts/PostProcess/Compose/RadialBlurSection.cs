
using UnityEngine;

public class RadialBlurSection : PostProcessSection 
{
    public bool enable = false;

    [Range(1, 8)]
    public int downSample = 1;
    [Range(0, 0.1f)]
    public float blurFactor = 0.02f;
    [Range(0.0f, 2.0f)]
    public float lerpFactor = 0.5f;
    [Range(0, 9)]
    public int sampleStrength = 6;
    public Vector2 blurCenter = new Vector2(0.5f, 0.5f);

    private bool isRenderEnable = false;

    private RenderTexture rt = null;
    private float timeCount = 0.0f;
    private float time;
    private float blurFrom;



    public void DynamicBlur(float from,float time)
    {
        blurFrom = from;
        timeCount = time;
        this.time = time;
        isRenderEnable = true;
    }

    public override void OnPreRenderByProcess()
    {
        /*
        if(isRenderEnable)
        {
            if (timeCount > 0.0f && time > 0.0f)
            {
                float f = timeCount / time;
                blurFactor = Mathf.Lerp(blurFrom, 0.0f, f);

                timeCount -= Time.deltaTime;
            }

            if(timeCount <= 0.0f)
            {
                isRenderEnable = false;
            }
        }
        */
    
        Material mat = m_postProcess.GetMaterial("compose");
        if (IsRenderEnable())
        {
            mat.EnableKeyword("_RADIALBLUR");
        }
        else
        {
            mat.DisableKeyword("_RADIALBLUR");
        }       
    }

    public override bool IsRenderEnable()
    {
        return enable;
        //return isRenderEnable && enable;
       
    }

    public override void OnPostProcess(RenderTexture src)
    {
        if (!IsRenderEnable()) return;

        Material mat = m_postProcess.GetMaterial("compose");
        rt = RenderTexture.GetTemporary(src.width >> downSample, src.height >> downSample, 0, RenderTextureFormat.ARGB32);
        mat.SetFloat("_BlurFactor", blurFactor);
        mat.SetVector("_BlurCenter", blurCenter);
        mat.SetFloat("_SampleStrength", sampleStrength);
        Graphics.Blit(src, rt, mat, 1);

        mat.SetTexture("_RadialBlur", rt);
        mat.SetFloat("_LerpFactor", lerpFactor);
    }

    public override void OnPostPostRender()
    {
        if (rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);
            rt = null;
        }
    }
}