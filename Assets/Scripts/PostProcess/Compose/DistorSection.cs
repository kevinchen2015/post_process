
using UnityEngine;


public class DistorSection : PostProcessSection 
{
    public bool enable = true;

    private Shader maskShader;
    private RenderTexture rt = null;
    private bool isRenderEnable = false;
    private int distorLayer = 31;
    private bool useSimpleMode = true;


    public override void Init(PostProcessInterface postProcess)
    {
        base.Init(postProcess);
        maskShader = Shader.Find("post/distor_mask");
    }

    public override void OnPreRenderByProcess()
    {
        Material mat = m_postProcess.GetMaterial("compose");
        if (!enable)
        {
            mat.DisableKeyword("_DISTOR");
            return;
        }

        isRenderEnable = m_postProcess.GetProcessMgr().GetLayerCounter(distorLayer) == 0 ? false : true;
        if (IsRenderEnable())
        {
            mat.EnableKeyword("_DISTOR");
        }
        else
        {
            mat.DisableKeyword("_DISTOR");
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
            camera.cullingMask = 1 << distorLayer;
            rt = RenderTexture.GetTemporary(src.width, src.height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = rt;
            if (useSimpleMode)
                camera.Render();
            else
                camera.RenderWithShader(maskShader, "");
            camera.targetTexture = null;
        }

        Material mat = m_postProcess.GetMaterial("compose");
        
        mat.SetTexture("_DistorMask", rt);
    }

    public override void OnPostPostRender()
    {
        if (rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);
            rt = null;
        }
        base.OnPostPostRender();
    }

    public override bool OnVisable(GameObject go, bool isVisiable)
    {
        if (distorLayer == go.layer)
            return true;

        return false;
    }


}