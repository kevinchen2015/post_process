

using UnityEngine;
using System.Collections.Generic;

public interface PostProcessInterface
{
    Material GetMaterial(string name);
    RenderTexture GetRenderTexture(string name);
    Camera GetCamera(string name);
    PostProcessMgrInterface GetProcessMgr();
    bool IsNeedDepth();
    List<PostProcessSection> GetProcessSection();
}

public struct PostProcessParam
{
    public PostProcessMgrInterface mgr;
    public Camera mainCamera;
    public LODMgr.LOD_LEVEL lodLevel;
}

public class PostProcess : MonoBehaviour , PostProcessInterface
{
#if UNITY_EDITOR
    public bool debugEditorEnable = true;
#endif

    [HideInInspector]
    public PostProcess nextPostProcess;

    protected Camera m_mainCamera;
    protected LODMgr.LOD_LEVEL m_LodLevel = LODMgr.LOD_LEVEL.LOD_LEVEL_HIGH;
    protected bool m_enable = false;
    protected PostProcessMgrInterface m_mgr = null;
    protected List<PostProcessSection> m_Sections = new List<PostProcessSection>();
    protected Dictionary<string, RenderTexture> m_commonTex = new Dictionary<string, RenderTexture>();

    public virtual Material GetMaterial(string name)
    {
        return null;
    }
    public PostProcessMgrInterface GetProcessMgr()
    {
        return m_mgr;
    }
    public virtual RenderTexture GetRenderTexture(string name)
    {
        RenderTexture tex = null;
        if(m_commonTex.TryGetValue(name,out tex))
        {
            return tex;
        }
        return null;
    }
    public virtual Camera GetCamera(string name)
    {
        return null;
    }
    public virtual bool IsEnableByLOD()
    {
        return false;
    }
    public virtual List<PostProcessSection> GetProcessSection()
    {
        return m_Sections;
    }
    public bool IsEnable()
    {
#if UNITY_EDITOR
        return (m_enable && debugEditorEnable && IsEnableByLOD());
#endif
        return m_enable && IsEnableByLOD();
    }

    public void SetLodLevel(LODMgr.LOD_LEVEL lod)
    {
        m_LodLevel = lod;
    }
    public virtual bool IsNeedDepth()
    {
        return false;
    }

    public virtual bool IsCaptureSrcRT()
    {
        return false;
    }

    public virtual bool IsRenderEnable()
    {
        return true;
    }

    public T GetPostProcessSection<T>() where T : PostProcessSection
    {
        for (int i = 0; i < m_Sections.Count; ++i)
        {
            if(m_Sections[i].GetType() == typeof(T))
            {
                return m_Sections[i] as T;
            }
        }
        return default(T);
    }

    public virtual void Init(PostProcessParam param)
    {
        m_mgr = param.mgr;
        m_mainCamera = param.mainCamera;
        m_LodLevel = param.lodLevel;
        m_enable = false;
    }

    public virtual void Enable()
    {
        m_enable = true;
    }

    public virtual void Disable()
    {
        m_enable = false;
    }

    protected void DoPostDefault(RenderTexture src, RenderTexture dest)
    {
        if (nextPostProcess != null)
        {
            nextPostProcess.OnPostProcess(src, dest);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    public virtual void OnPostProcess(RenderTexture src, RenderTexture dest)
    {
        if (!IsEnable())
        {
            DoPostDefault(src,dest);
            return;
        }
        
        {
            for (int i = 0; i < m_Sections.Count; ++i)
            {
                m_Sections[i].OnPostProcess(src);
            }
            //override ,todo
            Debug.LogError("post process do not implement!");
        }
    }

    public virtual void PreRender()
    {
        for (int i = 0; i < m_Sections.Count; ++i)
        {
            m_Sections[i].OnPreRenderByProcess();
        }
    }

    public virtual void PostRender()
    {
        for (int i = 0; i < m_Sections.Count; ++i)
        {
            m_Sections[i].OnPostPostRender();
        }
        foreach (KeyValuePair<string,RenderTexture> kv in m_commonTex)
        {
            RenderTexture.ReleaseTemporary(kv.Value);
        }
        m_commonTex.Clear();
    }

    public virtual bool OnVisable(GameObject go,bool isVisiable)
    {
        bool isVaild = false;
        for (int i = 0; i < m_Sections.Count; ++i)
        {
            isVaild |= m_Sections[i].OnVisable(go, isVisiable);
        }
        return isVaild;
    }


}