

using UnityEngine;

//多种后模糊 混合在一起,需要在glow ，bloom ，distor 之后
[ExecuteInEditMode]
public class MultiBlurPostProcess : PostProcess 
{
    private Material m_ComposeMaterial;

    public bool useDepth = false;

    public RadialBlurSection m_radialBlur;
    public DOFSection m_dof;

    void Awake()
    {
        Check();
    }

    void Check()
    {
        if (m_radialBlur == null)
        {
            m_radialBlur = new RadialBlurSection();
        }
        if(m_dof == null)
        {
            m_dof = new DOFSection();
        }
    }

    public override Material GetMaterial(string name)
    {
        return m_ComposeMaterial;
    }

    public override bool IsEnableByLOD()
    {
        if (m_LodLevel >= LODMgr.LOD_LEVEL.LOD_LEVEL_HIGH)
            return true;
        return false;
    }

    public override bool IsRenderEnable()
    {
        return false;
    }

    public override bool IsNeedDepth()
    {
        return useDepth;
    }

    public override void Init(PostProcessParam param)
    {
        base.Init(param);
        m_ComposeMaterial = new Material(Shader.Find("post/multiblur_compose"));

        Check();
        m_Sections.Add(m_radialBlur);
        m_Sections.Add(m_dof);

        for (int i = 0; i < m_Sections.Count; ++i)
        {
            m_Sections[i].Init(this);
        }
    }

    void OnDestroy()
    {
        m_ComposeMaterial = null;
    }

    public override void PreRender()
    {
        base.PreRender();
    }

    public override void OnPostProcess(RenderTexture src, RenderTexture dest)
    {
        if(useDepth)
        {
            m_ComposeMaterial.EnableKeyword("_USE_CAMERA_DEPTH");
        }
        else
        {
            m_ComposeMaterial.DisableKeyword("_USE_CAMERA_DEPTH");
        }

        if (!IsEnable())
        {
            DoPostDefault(src, dest);
            return;
        }

        bool isRenderEnable = false;
        for (int i = 0; i < m_Sections.Count; ++i)
        {
            isRenderEnable |= m_Sections[i].IsRenderEnable();
        }
        if(!isRenderEnable)
        {
            DoPostDefault(src, dest);
            return;
        }

        for (int i = 0; i < m_Sections.Count; ++i)
        {
            m_Sections[i].OnPostProcess(src);
        }

        //blit
        if (nextPostProcess != null)
        {
            RenderTexture myDest = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, myDest, m_ComposeMaterial,0);
            nextPostProcess.OnPostProcess(myDest, dest);
            RenderTexture.ReleaseTemporary(myDest);
        }
        else
        {
            Graphics.Blit(src, dest, m_ComposeMaterial,0);
        }
    }

    public override void PostRender()
    {
        base.PostRender();
    }

  

}