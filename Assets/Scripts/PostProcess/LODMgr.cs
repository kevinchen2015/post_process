using System;
using UnityEngine;


public class LODMgr : MonoBehaviour
{
    static LODMgr s_instance;
    public static LODMgr GetInstance()
    {
        return s_instance;
    }
    public enum LOD_LEVEL
    {
        LOD_LEVEL_LOW = 0,
        LOD_LEVEL_MIDDLE,
        LOD_LEVEL_HIGH,
    }

    private LOD_LEVEL m_LODLevel = LOD_LEVEL.LOD_LEVEL_LOW;
    private event Action<LOD_LEVEL> m_onLodChanged;

    void Awake()
    {
        s_instance = this;
        SetLODLevel(LOD_LEVEL.LOD_LEVEL_HIGH);
    }

    void OnDestroy()
    {
        s_instance = null;
    }

    public void Init()
    {
        //todo....
    }

    public void UnInit()
    {

    }

    public void SetLODLevel(LOD_LEVEL lodLevel)
    {
        m_LODLevel = lodLevel;
        _OnLODChanged();
    }

    public LOD_LEVEL GetLODLevel()
    {
        return m_LODLevel;
    }

    private void _OnLODChanged()
    {
        switch(m_LODLevel)
        {
            case LOD_LEVEL.LOD_LEVEL_LOW:
                {
                    Shader.globalMaximumLOD = 200;
                }
                break;

            case LOD_LEVEL.LOD_LEVEL_MIDDLE:
                {
                    Shader.globalMaximumLOD = 250;
                }
                break;

            case LOD_LEVEL.LOD_LEVEL_HIGH:
                {
                    Shader.globalMaximumLOD = 300;
                }
                break;
        }

        if(m_onLodChanged != null)
        {
            m_onLodChanged(m_LODLevel);
        }
    }

    public void AddLODChangedListener(Action<LOD_LEVEL> onLodChangedCb)
    {
        m_onLodChanged += onLodChangedCb;
    }

    public void RemoveLODChangedListener(Action<LOD_LEVEL> onLodChangedCb)
    {
        m_onLodChanged -= onLodChangedCb;
    }

    public void SetGlobalShaderKey(string keyName,bool enable)
    {
        if (enable)
            Shader.EnableKeyword(keyName);
        else
            Shader.DisableKeyword(keyName);
    }

}
