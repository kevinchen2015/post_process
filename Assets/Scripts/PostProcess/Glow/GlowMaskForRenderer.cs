using UnityEngine;
using System;


public class GlowMaskForRenderer : MonoBehaviour {

    [HideInInspector]
    public TextureInfo[] glowMasks;
    [HideInInspector]
    public Renderer rend;

	[System.Serializable]
	public class TextureInfo
	{
		[HideInInspector]
		[SerializeField]
		private GlowMaskForRenderer pGlowMaskComponent;

		[HideInInspector]
		[SerializeField]
		private Color pGlowTint = Color.white;

		[HideInInspector]
		[SerializeField]
		private Texture pTexture;

		[HideInInspector]
		[SerializeField]
		private bool pUseMainTextureTilingOffset = true;

		[HideInInspector]
		[SerializeField]
		private Vector2 pTiling = Vector2.one;

		[HideInInspector]
		[SerializeField]
		private Vector2 pOffset = Vector2.one;

		[HideInInspector]
		[SerializeField]
		private bool pAffectAllInstancesOfMaterial = true;

		public GlowMaskForRenderer glowMaskComponent
		{
			get
			{
				return pGlowMaskComponent;
			}
			set
			{
				pGlowMaskComponent = value;
			}
		}

		public Color glowTint
		{
			get
			{
				return pGlowTint;
			}
			set
			{
				pGlowTint = value;
				if (Application.isPlaying)
					glowMaskComponent.UpdateMaskInfo();
			}
		}

		public Texture texture
		{
			get
			{
				return pTexture;
			}
			set
			{
				pTexture = value;
				if (Application.isPlaying)
					glowMaskComponent.UpdateMaskInfo();
			}
		}

		public bool useMainTextureTilingOffset
		{
			get
			{
				return pUseMainTextureTilingOffset;
			}
			set
			{
				pUseMainTextureTilingOffset = value;
				if (Application.isPlaying)
					glowMaskComponent.UpdateMaskInfo();
			}
		}

		public Vector2 tiling
		{
			get
			{
				return pTiling;
			}
			set
			{
				pTiling = value;
				if (Application.isPlaying)
					glowMaskComponent.UpdateMaskInfo();
			}
		}


		public Vector2 offset
		{
			get
			{
				return pOffset;
			}
			set
			{
				pOffset = value;
				if (Application.isPlaying)
					glowMaskComponent.UpdateMaskInfo();
			}
		}

		public bool affectAllInstancesOfMaterial
		{
			get
			{
				return pAffectAllInstancesOfMaterial;
			}
			set
			{
				if (!Application.isPlaying)
					pAffectAllInstancesOfMaterial = value;
			}
		}
	}


	public void UpdateMaskInfo()
	{
		if (rend.sharedMaterials.Length != glowMasks.Length)
		{
			Debug.LogWarning(gameObject.name + " : Glow masks count doesn't fit materials count. If you change material count on this renderer in realtime, be sure to update glow masks accordingly.");
			Array.Resize(ref glowMasks, rend.sharedMaterials.Length);
		}
		for (int i = 0; i < rend.sharedMaterials.Length; i++)
		{
			if (glowMasks[i].affectAllInstancesOfMaterial)
			{
				rend.sharedMaterials[i].SetColor("_GlowColor",glowMasks[i].glowTint);
				rend.sharedMaterials[i].SetTexture("_GlowMask", glowMasks[i].texture);
				if (glowMasks[i].useMainTextureTilingOffset)
				{
					rend.sharedMaterials[i].SetTextureScale("_GlowMask", rend.sharedMaterials[i].mainTextureScale);
					rend.sharedMaterials[i].SetTextureOffset("_GlowMask", rend.sharedMaterials[i].mainTextureOffset);
				}
				else
				{
					rend.sharedMaterials[i].SetTextureScale("_GlowMask", glowMasks[i].tiling);
					rend.sharedMaterials[i].SetTextureOffset("_GlowMask", glowMasks[i].offset);
				}
			}
			else
			{
				rend.materials[i].SetColor("_GlowColor",glowMasks[i].glowTint);
				rend.materials[i].SetTexture("_GlowMask", glowMasks[i].texture);
				if (glowMasks[i].useMainTextureTilingOffset)
				{
					rend.materials[i].SetTextureScale("_GlowMask", rend.materials[i].mainTextureScale);
					rend.materials[i].SetTextureOffset("_GlowMask", rend.materials[i].mainTextureOffset);
				}
				else
				{
					rend.materials[i].SetTextureScale("_GlowMask", glowMasks[i].tiling);
					rend.materials[i].SetTextureOffset("_GlowMask", glowMasks[i].offset);
				}
			}
		}
	}
    void OnWillRenderObject()
    {
        PostProcessMgr.OnVisiable(gameObject, true);
    }
    protected void Awake () 
	{
		rend = GetComponent<Renderer>();

		if (rend == null)
		{
			Debug.LogError(gameObject.name + " : Glow mask component should be placed on object with Renderer component present. Disabling component.");
			this.enabled = false;
			return;
		}

		if (glowMasks == null)
		{
			glowMasks = new TextureInfo[rend.sharedMaterials.Length];
		}

		if (glowMasks.Length != rend.sharedMaterials.Length)
		{
			Array.Resize(ref glowMasks, rend.sharedMaterials.Length);
		}

		for (int i = 0; i < glowMasks.Length; i++)
		{
			if (glowMasks[i] == null)
				glowMasks[i] = new TextureInfo();
			glowMasks[i].glowMaskComponent = this;
		}
		UpdateMaskInfo();
	}


}
