using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

[CustomEditor( typeof(MultiBlurPostProcess))]
public class MultiBlurPostProcessEditor : Editor 
{

	public void OnEnable ()
	{
		
	}

	public override void OnInspectorGUI()
	{
        //serializedObject.Update();
        MultiBlurPostProcess compose = (MultiBlurPostProcess)target;

        EditorGUILayout.BeginVertical("box");
        compose.useDepth = EditorGUILayout.Toggle("打开深度", compose.useDepth);
        EditorGUILayout.EndVertical();

        RadialBlurSection radialBlur = compose.m_radialBlur;
        if (radialBlur != null)
        {
            radialBlur.enable = EditorGUILayout.Toggle("radial blur 开关", radialBlur.enable);
            if (EditorGUILayout.Foldout(radialBlur.enable, "radial blur 属性项"))
            {
                EditorGUILayout.BeginVertical("box");
                radialBlur.downSample = EditorGUILayout.IntSlider("downsample", radialBlur.downSample, 1, 8);
                radialBlur.blurFactor = EditorGUILayout.Slider("blur factor", radialBlur.blurFactor, 0.0f, 0.1f);
                radialBlur.lerpFactor = EditorGUILayout.Slider("lerp factor", radialBlur.lerpFactor, 0.0f, 2.0f);
                radialBlur.sampleStrength = EditorGUILayout.IntSlider("sample strength", radialBlur.sampleStrength,0,9);
                radialBlur.blurCenter = EditorGUILayout.Vector2Field("blur center(0~1)", radialBlur.blurCenter);
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        DOFSection dof = compose.m_dof;
        if(dof != null)
        {
            dof.enable = EditorGUILayout.Toggle("dof 开关", dof.enable);
            if (EditorGUILayout.Foldout(dof.enable, "dof 属性项"))
            {
                EditorGUILayout.BeginVertical("box");
                dof.blurDownsample = EditorGUILayout.IntSlider("downsample", dof.blurDownsample, 1, 8);
                dof.focalDistance = EditorGUILayout.Slider("focalDistance", dof.focalDistance, 0.0f, 100.0f);
                dof.nearBlurScale = EditorGUILayout.Slider("nearBlurScale", dof.nearBlurScale, 0.0f, 100.0f);
                dof.farBlurScale = EditorGUILayout.Slider("farBlurScale", dof.farBlurScale, 0.0f,100.0f);
                dof.blurSize = EditorGUILayout.Slider("blur size", dof.blurSize, 1, 10);
                dof.blurIterations = EditorGUILayout.IntSlider("blur iteration", dof.blurIterations, 0, 4);
                EditorGUILayout.EndVertical();
            }
        }

        if (GUI.changed)
			EditorUtility.SetDirty (target);
	}

}
