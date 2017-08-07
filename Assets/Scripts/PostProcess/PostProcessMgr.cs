
using System;
using System.Collections.Generic;
using UnityEngine;

//all post process manager

public interface PostProcessMgrInterface
{
    RenderTexture GetSrcRT();
    int GetLayerCounter(int layerIndex);
    PostProcess[] GetPostProcess();
    Camera GetMainCamera();
    string GetName();
}


public class PostProcessMgr : MonoBehaviour, PostProcessMgrInterface
{
    //----------static----------------------------------------------------------
    static List<PostProcessMgr> s_PostProcessMgrs = new List<PostProcessMgr>();
    static List<System.Action<PostProcessMgrInterface, bool>> s_listener = new List<System.Action<PostProcessMgrInterface, bool>>();

    public static void AddListener(System.Action<PostProcessMgrInterface, bool> listener)
    {
        s_listener.Remove(listener);
        s_listener.Add(listener);
    }

    public static void RemoveListener(System.Action<PostProcessMgrInterface, bool> listener)
    {
        s_listener.Remove(listener);
    }

    static void Regist(PostProcessMgr mgr)
    {
        s_PostProcessMgrs.Remove(mgr);
        s_PostProcessMgrs.Add(mgr);

        if(s_listener != null)
        {
            for(int i = 0; i < s_listener.Count;++i)
            {
                s_listener[i](mgr,true);
            }
        }
    }

    static void UnRegist(PostProcessMgr mgr)
    {
        s_PostProcessMgrs.Remove(mgr);

        if (s_listener != null)
        {
            for (int i = 0; i < s_listener.Count; ++i)
            {
                s_listener[i](mgr,false);
            }
        }
    }

    public static void OnVisiable(GameObject go ,bool isVisiable)
    {
        for (int i = 0; i < s_PostProcessMgrs.Count;++i)
        {
            s_PostProcessMgrs[i].OnVisable(go, isVisiable);
        }
    }
    //-------------------------------------------------------------------------

    public PostProcess[] m_PostProcessList;
    private Camera m_MainCamera;
    private bool m_MainCameraDepth = false;
    private bool m_CapterSrcRT = false;
    private RenderTexture m_SrcRT;
    private FlipPostProcess m_flipPostProcess;
    private int[] m_LayerObjectCounter = new int[32];

    public string GetName()
    {
        return gameObject.name;
    }

    public Camera GetMainCamera()
    {
        return m_MainCamera;
    }

    public RenderTexture GetSrcRT()
    {
        return m_SrcRT;
    }

    void OnVisable(GameObject go, bool isVisiable)
    {
        if (go == null) return;

        bool isVaild = false;
        if ((m_MainCamera.cullingMask & 1 << go.layer) != 0)
        {
            isVaild = true;
        }

        if(!isVaild)
        {
            for (int i = 0; i < m_PostProcessList.Length; ++i)
            {
                isVaild |= m_PostProcessList[i].OnVisable(go, isVisiable);
            }
        }
        
        if(isVaild)
        {
            m_LayerObjectCounter[go.layer] += (isVisiable ? 1 : -1);
        }
    }

    public int GetLayerCounter(int layerIndex)
    {
        if(layerIndex >= 0 && layerIndex < 32)
        {
            return m_LayerObjectCounter[layerIndex];
        }
        return 0;
    }

    public PostProcess[] GetPostProcess()
    {
        return m_PostProcessList;
    }

    void Awake()
    {
        m_MainCamera = gameObject.GetComponent<Camera>();
        if(m_MainCamera == null)
        {
            Debug.LogError("this script must be main camera component!");
        }

        LODMgr.LOD_LEVEL lodLevel = LODMgr.LOD_LEVEL.LOD_LEVEL_HIGH;
        m_flipPostProcess = gameObject.AddComponent<FlipPostProcess>();
        GenPostProcessChain();
        PostProcessParam param = new PostProcessParam();
        param.mgr = this;
        param.mainCamera = m_MainCamera;
        param.lodLevel = lodLevel;
        for (int i = 0; i < m_PostProcessList.Length; ++i)
        {
            m_PostProcessList[i].Init(param);
        }
        m_flipPostProcess.Init(param);
        Regist(this);
    }
    void OnDestroy()
    {
        ReleaseSrcRT();
        if (LODMgr.GetInstance() != null)
        {
            LODMgr.GetInstance().RemoveLODChangedListener(OnLodChanged);
        }
        UnRegist(this);
    }
    void Start()
    {
        LODMgr.LOD_LEVEL lodLevel = LODMgr.LOD_LEVEL.LOD_LEVEL_HIGH;
        if (LODMgr.GetInstance() != null)
        {
            lodLevel = LODMgr.GetInstance().GetLODLevel();
            OnLodChanged(lodLevel);
            LODMgr.GetInstance().AddLODChangedListener(OnLodChanged);
        }
    }
    void OnLodChanged(LODMgr.LOD_LEVEL lodLevel)
    {
        for (int i = 0; i < m_PostProcessList.Length; ++i)
        {
            m_PostProcessList[i].SetLodLevel(lodLevel);
        }
        m_flipPostProcess.SetLodLevel(lodLevel);
    }
    void GenPostProcessChain()
    {
        if (m_PostProcessList.Length == 0) return;
        for (int i = 0; i < m_PostProcessList.Length; ++i)
        {
            m_PostProcessList[i].nextPostProcess = null;
        }

        PostProcess lastVaildPost = m_PostProcessList[0];
        for (int i = 1; i < m_PostProcessList.Length ; ++i)
        {
            if(lastVaildPost != null)
            {
                lastVaildPost.nextPostProcess = m_PostProcessList[i];
            }
            lastVaildPost = m_PostProcessList[i];
        }
#if UNITY_EDITOR
        if(lastVaildPost != null)
        {
            lastVaildPost.nextPostProcess = m_flipPostProcess;
        }
#endif
    }

    void OnEnable()
    {
        for (int i=0;i < m_PostProcessList.Length;++i)
        {
            m_PostProcessList[i].Enable();
          
        }
        CheckParam();
    }

    void CheckParam()
    {
        for (int i = 0; i < m_PostProcessList.Length; ++i)
        {
            m_MainCameraDepth |= m_PostProcessList[i].IsNeedDepth();
            m_CapterSrcRT |= m_PostProcessList[i].IsCaptureSrcRT();
        }
        SetDepthEnable(m_MainCameraDepth);
        SetCapterSrcRT(m_CapterSrcRT);
    }

    void SetCapterSrcRT(bool enable)
    {
        if(enable)
        {
            CreateSrcRT(m_MainCamera.pixelWidth,m_MainCamera.pixelHeight);
        }
        else
        {
            ReleaseSrcRT();
        }
    }

    void SetDepthEnable(bool enable)
    {
        if (enable)
        {
            m_MainCamera.depthTextureMode |= DepthTextureMode.Depth;
        }
        else
        {
            m_MainCamera.depthTextureMode &= ~DepthTextureMode.Depth;
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < m_PostProcessList.Length; ++i)
        {
             m_PostProcessList[i].Disable();
        }
        m_MainCameraDepth = false;
        SetDepthEnable(m_MainCameraDepth);
    }
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        int enableCounter = 0;
        if(m_PostProcessList.Length == 0)
        {
            return;
        }

        for (int i = 0; i < m_PostProcessList.Length; ++i)
        {
            if (m_PostProcessList[i].IsEnable())
                ++enableCounter;
        }

        if(enableCounter == 0)
        {
            return;
        }

        if (m_CapterSrcRT)
        {
            RenderTexture rt = GetSrcRT();
            if(rt != null)
            {
                Graphics.Blit(src, rt);
            }
        }
        //call post chain
        m_PostProcessList[0].OnPostProcess(src, dest);

        //post
        for (int i = 0; i < m_PostProcessList.Length; ++i)
        {
            m_PostProcessList[i].PostRender();
        }
        m_flipPostProcess.PostRender();

        for (int i = 0; i < 32; ++i)
        {
            m_LayerObjectCounter[i] = 0;
        }
    }

    void OnPreRender()
    {
        for (int i = 0; i < m_PostProcessList.Length; ++i)
        {
            m_PostProcessList[i].PreRender();
        }
        m_flipPostProcess.PreRender();
    }

    void CreateSrcRT(int width, int height)
    {
        ReleaseSrcRT();
        m_SrcRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
    }

    void ReleaseSrcRT()
    {
        if (m_SrcRT != null)
        {
            RenderTexture.ReleaseTemporary(m_SrcRT);
            m_SrcRT = null;
        }
    }


}