

using UnityEngine;


public class GlowPostProcess : PostProcess
{
    public LayerMask glowLayers = 0; //(LayerMask)int.MaxValue;
    public bool glowMaskDepthEnable = false;
    public Color globalGlowTint = Color.white;
    [Range(0.0f, 100.0f)]
    public float glowPower = 1;
    [Range(1.0f, 8.0f)]
    public int maskDownsample = 1;
    [Range(1.0f, 8.0f)]
    public int blurDownsample = 1;
    [Range(0.0f, 100.0f)]
    public float blurPower = 1;
    [Range(1.0f, 10.0f)]
    public float blurSize = 3;
    [Range(1, 4)]
    public int blurIterations = 1;
    
    private Camera glowCamera;
    private Material glowComposeMaterial;
    private Shader glowMaskShader;
    private Shader glowMaskDepthShader;

    public override bool IsEnableByLOD()
    {
        if(m_LodLevel >= LODMgr.LOD_LEVEL.LOD_LEVEL_HIGH)
            return true;

        return false;
    }

    public override bool IsNeedDepth()
    {
        return (IsEnable() && glowMaskDepthEnable);
    }

    public override void Init(PostProcessParam param)
    {
        base.Init(param);

        GameObject glowCameraObj = new GameObject("GlowCameraObj");
        glowCameraObj.transform.parent = transform;
        glowCameraObj.transform.position = transform.position;
        glowCameraObj.transform.rotation = transform.rotation;
        glowCamera = glowCameraObj.AddComponent<Camera>();
        glowCamera.CopyFrom(m_mainCamera);
        glowCamera.enabled = false;

        glowComposeMaterial = new Material(Shader.Find("post/glow_compose"));

        glowMaskShader = Shader.Find("post/glowmask");
        glowMaskDepthShader = Shader.Find("post/glowmask_depth");
    }

    void OnDestroy()
    {
        glowMaskShader = null;
        glowMaskDepthShader = null;
        glowComposeMaterial = null;
    }

    public override void Enable()
    {
        base.Enable();

        if(IsEnable())
        {
            //todo...
        }
    }

    public override void Disable()
    {
        if (IsEnable())
        {
            //todo...
        }
        base.Disable();
    }

    public override bool IsRenderEnable()
    {
        if (m_mgr.GetLayerCounter(29) == 0)
            return false;

        return true;
    }

    public override void OnPostProcess(RenderTexture src, RenderTexture dest)
    {
        if (!IsEnable() || !IsRenderEnable())
        {
            DoPostDefault(src, dest);
            return;
        }

        glowCamera.CopyFrom(m_mainCamera);
        glowCamera.rect = new Rect(0, 0, 1, 1);
        glowCamera.cullingMask = glowLayers;
        glowCamera.backgroundColor = new Color(0, 0, 0, 0);
        glowCamera.clearFlags = CameraClearFlags.SolidColor;
        glowCamera.depthTextureMode = DepthTextureMode.None;
        glowCamera.renderingPath = RenderingPath.VertexLit;

        RenderTexture glowRT = RenderTexture.GetTemporary(src.width >> maskDownsample, src.height >> maskDownsample, 24, RenderTextureFormat.ARGB32);
        glowCamera.targetTexture = glowRT;

        if (maskDownsample == 0)
            Shader.SetGlobalFloat("_zShift", 0.999f);
        else
            Shader.SetGlobalFloat("_zShift", 0.99f);

        //Shader.SetGlobalColor("_GlowColor", Color.white);
        //render downsample glow mask to glowRT
        if (glowMaskDepthEnable)
        {
            glowCamera.RenderWithShader(glowMaskDepthShader, "RenderType");
        }
        else
        {
            glowCamera.RenderWithShader(glowMaskShader, "RenderType");
        }
        
        //blur
        RenderTexture blurRT = RenderTexture.GetTemporary(src.width >> blurDownsample, src.height >> blurDownsample, 0, RenderTextureFormat.ARGB32);
        PostProcessUtils.Blur(glowRT, blurRT,new Vector2(src.width, src.height),blurDownsample,blurIterations,blurSize);

        //glow compose
        glowComposeMaterial.SetTexture("_GlowMask", glowRT);
        glowComposeMaterial.SetTexture("_BlurMask", blurRT);
        glowComposeMaterial.SetFloat("_GlowPower", glowPower);
        glowComposeMaterial.SetFloat("_BlurPower", blurPower);
        glowComposeMaterial.SetColor("_GlobalTint", globalGlowTint);
        
        //blit
        if(nextPostProcess != null)
        {
            RenderTexture glowDest = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, glowDest, glowComposeMaterial);
            nextPostProcess.OnPostProcess(glowDest, dest);
            RenderTexture.ReleaseTemporary(glowDest);
        }
        else
        {
            Graphics.Blit(src, dest, glowComposeMaterial);
        }
        RenderTexture.ReleaseTemporary(glowRT);
        RenderTexture.ReleaseTemporary(blurRT);
    }
}