using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

[CustomEditor( typeof(DistorMaskForRenderer))]
public class DistorMaskForRendererEditor : Editor {

	private Renderer rend;
	public void OnEnable ()
	{
        DistorMaskForRenderer gmask = (DistorMaskForRenderer)target;
		rend = gmask.gameObject.GetComponent<Renderer>();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

        DistorMaskForRenderer gmask = (DistorMaskForRenderer)target;
		gmask.render = rend;
        gmask.maskInfo.component = gmask;

		if (rend == null)
		{
			EditorGUILayout.HelpBox("Distor mask component should be placed on object with Renderer component present.",MessageType.Error);
			return;
		}

		{
			EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Material: " + rend.sharedMaterial.name, MessageType.None);
            gmask.maskInfo.bumpAmt = EditorGUILayout.Slider("distor bump amt", gmask.maskInfo.bumpAmt,0.0f,10.0f);
            gmask.maskInfo.bump = (Texture)EditorGUILayout.ObjectField("Distor bump mask texture", gmask.maskInfo.bump, typeof(Texture), false);

            {
                gmask.maskInfo.tiling = EditorGUILayout.Vector2Field("Tiling", gmask.maskInfo.tiling);
                gmask.maskInfo.offset = EditorGUILayout.Vector2Field("Offset", gmask.maskInfo.offset);
            }
            if (Application.isPlaying)
            {
                EditorGUILayout.ToggleLeft("Affect all instances of material(Read-only in Play mode)", gmask.maskInfo.affectAllInstancesOfMaterial);
            }
            else
            {
                gmask.maskInfo.affectAllInstancesOfMaterial = EditorGUILayout.ToggleLeft("Affect all instances of material", gmask.maskInfo.affectAllInstancesOfMaterial);
            }
            EditorGUILayout.EndVertical();
		}
		
		if (GUI.changed)
			EditorUtility.SetDirty (target);
	}
}
