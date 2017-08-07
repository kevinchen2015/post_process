
using UnityEngine;

public class GlowSection : PostProcessSection 
{

    public bool enable = true;
    public Color globalGlowTint = Color.white;
    [Range(0.0f, 100.0f)]
    public float glowPower = 1;
    [Range(0.0f, 8.0f)]
    public int maskDownsample = 1;
    [Range(1.0f, 8.0f)]
    public int blurDownsample = 1;
    [Range(0.0f, 100.0f)]
    public float blurPower = 1;
    [Range(1.0f, 10.0f)]
    public float blurSize = 3;
    [Range(1, 4)]
    public int blurIterations = 1;
    [Range(0, 1)]
    public float ZShift = 0.8f;

    private int glowLayer = 29;
    private bool isRenderEnable = false;

    private Shader glowMaskShader;
    private Shader glowMaskDepthShader;
    private RenderTexture rt = null;
    private RenderTexture blurRT = null;


    public override void Init(PostProcessInterface postProcess)
    {
        base.Init(postProcess);
        glowMaskShader = Shader.Find("post/glowmask");
        glowMaskDepthShader = Shader.Find("post/glowmask_depth");
    }

    public override void OnPreRenderByProcess()
    {
        Material mat = m_postProcess.GetMaterial("compose");
        if (!enable)
        {
            mat.DisableKeyword("_GLOW");
            return;
        }

        isRenderEnable = m_postProcess.GetProcessMgr().GetLayerCounter(glowLayer) == 0 ? false : true;
        if (IsRenderEnable())
        {
            mat.EnableKeyword("_GLOW");
        }
        else
        {
            mat.DisableKeyword("_GLOW");
        }
    }

    public override bool IsRenderEnable()
    {
        return isRenderEnable && enable;
    }

    public override void OnPostProcess(RenderTexture src)
    {
        if (!IsRenderEnable()) return;

        Camera camera = m_postProcess.GetCamera("RTCamera");
        if (camera != null)
        {
            camera.cullingMask = 1 << glowLayer;
      
            rt =  RenderTexture.GetTemporary(src.width >> maskDownsample, src.height >> maskDownsample, 24, RenderTextureFormat.ARGB32);
            
            camera.targetTexture = rt;

            if (m_postProcess.IsNeedDepth())
            {
                Shader.SetGlobalFloat("_zShift", ZShift);
                camera.RenderWithShader(glowMaskDepthShader, "");
            }
            else
                camera.RenderWithShader(glowMaskShader, "");
            camera.targetTexture = null;
        }

        Material mat = m_postProcess.GetMaterial("compose");
      
        //blur
        //if(blurDownsample > 0)
        {
            blurRT = RenderTexture.GetTemporary(src.width >> blurDownsample, src.height >> blurDownsample, 0, RenderTextureFormat.ARGB32);
            PostProcessUtils.Blur(rt, blurRT, new Vector2(src.width, src.height), blurDownsample, blurIterations, blurSize);
        }

        //set
        mat.SetTexture("_GlowMask",rt);
        mat.SetTexture("_BlurMask",blurRT);
        mat.SetFloat("_GlowPower",glowPower);
        mat.SetFloat("_BlurPower",blurPower);
        mat.SetColor("_GlobalTint",globalGlowTint);
    }

    public override void OnPostPostRender()
    {
        if(rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);
            rt = null;
        }
        if(blurRT != null)
        {
            RenderTexture.ReleaseTemporary(blurRT);
            blurRT = null;
        }
        base.OnPostPostRender();
    }


}