

using UnityEngine;

//glow bloom distor 常用后处理合并在一起
[ExecuteInEditMode]
public class ComposePostProcess : PostProcess 
{
    private Material m_ComposeMaterial;
    private Camera m_RTCamera;

    public bool useDepth = false;

    public DistorSection m_distor;
    public GlowSection m_glow;
    public BloomSection m_bloom;

    void Awake()
    {
        Check();
    }

    void Check()
    {
        if (m_distor == null)
        {
            m_distor = new DistorSection();
        }
        if (m_glow == null)
        {
            m_glow = new GlowSection();
        }
        if(m_bloom == null)
        {
            m_bloom = new BloomSection();
        }
    }

    public override Material GetMaterial(string name)
    {
        return m_ComposeMaterial;
    }

    public override Camera GetCamera(string name)
    {
        return m_RTCamera;
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
        m_ComposeMaterial = new Material(Shader.Find("post/compose"));

        GameObject glowCameraObj = new GameObject("RTCameraObj");
        glowCameraObj.transform.parent = transform;
        glowCameraObj.transform.position = transform.position;
        glowCameraObj.transform.rotation = transform.rotation;
        m_RTCamera = glowCameraObj.AddComponent<Camera>();
        m_RTCamera.CopyFrom(m_mainCamera);
        m_RTCamera.enabled = false;

        Check();
        m_Sections.Add(m_distor);
        m_Sections.Add(m_glow);
        m_Sections.Add(m_bloom);

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
        //camera
        m_RTCamera.CopyFrom(m_mainCamera);
        m_RTCamera.rect = new Rect(0, 0, 1, 1);
        m_RTCamera.cullingMask = 0;
        m_RTCamera.backgroundColor = new Color(0, 0, 0, 0);
        m_RTCamera.clearFlags = CameraClearFlags.SolidColor;
        m_RTCamera.depthTextureMode = DepthTextureMode.None;
        m_RTCamera.renderingPath = RenderingPath.VertexLit;

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

        m_ComposeMaterial.SetFloat("_InvFade", 0.8f);

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