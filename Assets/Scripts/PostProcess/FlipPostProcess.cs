
using UnityEngine;

public class FlipPostProcess : PostProcess 
{
    public override bool IsEnableByLOD()
    {
        return true;
    }

    public override void OnPostProcess(RenderTexture src, RenderTexture dest)
    {
#if UNITY_EDITOR
        PostProcess[] process = m_mgr.GetPostProcess();
        bool renderEnable = false;
        for (int i=0;i < process.Length;++i)
        {
            renderEnable |= process[i].IsRenderEnable();
        }
        if(!renderEnable)
        {
            DoPostDefault(src, dest);
            return;
        }

        PostProcessUtils.Flip(src, dest,new Vector4(1, -1, 1, 1));
#else
        DoPostDefault(src, dest);
#endif

    }

}