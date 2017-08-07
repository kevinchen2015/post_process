
using UnityEngine;

public class PostProcessSection : ScriptableObject
{
    protected PostProcessInterface m_postProcess;



    public virtual void Init(PostProcessInterface postProcess)
    {
        m_postProcess = postProcess;
    }

    public virtual bool IsRenderEnable()
    {
        return true;
    }

    public virtual void OnPreRenderByProcess()
    {

    }

    public virtual void OnPostProcess(RenderTexture src)
    {

    }

    public virtual void OnPostPostRender()
    {

    }

    public virtual bool OnVisable(GameObject go, bool isVisiable)
    {
        return false;
    }
}