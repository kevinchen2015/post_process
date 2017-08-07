using UnityEngine;

public class DistorMaskForRenderer : MonoBehaviour
{
    [HideInInspector]
    public DistorMaskInfo maskInfo;

    [HideInInspector]
    public Renderer render;

    [System.Serializable]
    public class DistorMaskInfo
    {
        public float m_bumpAmt = 1.0f;
        public Texture m_bump;
        public DistorMaskForRenderer component;

        public Vector2 m_tiling = Vector2.one;
        public Vector2 m_offset = Vector2.zero;
        public bool m_affectAllInstancesOfMaterial = true;

        public Texture bump
        {
            get
            {
                return m_bump;
            }
            set
            {
                m_bump = value;
                if (Application.isPlaying)
                    component.UpdateMaskInfo();
            }
        }
        public float bumpAmt
        {
            get
            {
                return m_bumpAmt;
            }
            set
            {
                m_bumpAmt = value;
                if (Application.isPlaying)
                    component.UpdateMaskInfo();
            }
        }
        public Vector2 tiling
        {
            get
            {
                return m_tiling;
            }
            set
            {
                m_tiling = value;
                if (Application.isPlaying)
                    component.UpdateMaskInfo();
            }
        }
        public Vector2 offset
        {
            get
            {
                return m_offset;
            }
            set
            {
                m_offset = value;
                if (Application.isPlaying)
                    component.UpdateMaskInfo();
            }
        }

        public bool affectAllInstancesOfMaterial
        {
            get
            {
                return m_affectAllInstancesOfMaterial;
            }
            set
            {
                if (!Application.isPlaying)
                    m_affectAllInstancesOfMaterial = value;
            }
        }

    }

    public void UpdateMaskInfo()
	{
        if (render == null) return;

        if(maskInfo.m_affectAllInstancesOfMaterial)
        {
            render.sharedMaterial.SetTexture("_BumpMap", maskInfo.bump);
            render.sharedMaterial.SetFloat("_BumpAmt", maskInfo.bumpAmt);
            render.sharedMaterial.SetTextureScale("_BumpMap", maskInfo.tiling);
            render.sharedMaterial.SetTextureOffset("_BumpMap", maskInfo.offset);
        }
        else
        {
            render.material.SetTexture("_BumpMap", maskInfo.bump);
            render.material.SetFloat("_BumpAmt", maskInfo.bumpAmt);
            render.material.SetTextureScale("_BumpMap", maskInfo.tiling);
            render.material.SetTextureOffset("_BumpMap", maskInfo.offset);
        }
    }
    void OnBecameVisible()
    {
        PostProcessMgr.OnVisiable(gameObject, true);
    }
    void OnBecameInvisible()
    {
        PostProcessMgr.OnVisiable(gameObject, false);
    }
    protected void Awake () 
	{
        render = GetComponent<Renderer>();

		if (render == null)
		{
			Debug.LogError(gameObject.name + " : distor mask component should be placed on object with Renderer component present. Disabling component.");
			this.enabled = false;
			return;
		}
        maskInfo.component = this;

        UpdateMaskInfo();
	}
}
