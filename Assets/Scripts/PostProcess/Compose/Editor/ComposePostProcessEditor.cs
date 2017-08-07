using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

[CustomEditor( typeof(ComposePostProcess))]
public class ComposePostProcessEditor : Editor 
{

	public void OnEnable ()
	{
		
	}

	public override void OnInspectorGUI()
	{
        //serializedObject.Update();
        ComposePostProcess compose = (ComposePostProcess)target;

        EditorGUILayout.BeginVertical("box");
        compose.useDepth = EditorGUILayout.Toggle("打开深度", compose.useDepth);
        EditorGUILayout.EndVertical();

        GlowSection glow = compose.m_glow;
        if (glow != null)
        {
            glow.enable = EditorGUILayout.Toggle("glow 开关",glow.enable);
            if (EditorGUILayout.Foldout(glow.enable, "glow属性项"))
            {
                EditorGUILayout.BeginVertical("box");
                glow.globalGlowTint = EditorGUILayout.ColorField("global color",glow.globalGlowTint);
                glow.glowPower = EditorGUILayout.Slider("glow power", glow.glowPower,0,100);
                glow.maskDownsample = EditorGUILayout.IntSlider("glow downsample", glow.maskDownsample,0,8);
                glow.blurSize = EditorGUILayout.Slider("blur size", glow.blurSize,1,10);
                glow.blurDownsample = EditorGUILayout.IntSlider("blur downsample", glow.blurDownsample,1,8);
                glow.blurPower = EditorGUILayout.FloatField("blur power", glow.blurPower);
                glow.blurIterations = EditorGUILayout.IntSlider("blur iteration", glow.blurIterations,1,4);
                glow.ZShift = EditorGUILayout.Slider("z shift", glow.ZShift, 0, 1);

                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        BloomSection bloom = compose.m_bloom;
        if (bloom != null)
        {
            bloom.enable = EditorGUILayout.Toggle("bloom 开关", bloom.enable);
            if (EditorGUILayout.Foldout(bloom.enable, "bloom 属性项"))
            {
                EditorGUILayout.BeginVertical("box");
                bloom.colorThreshold = EditorGUILayout.ColorField("threshold color", bloom.colorThreshold);
                bloom.bloomColor = EditorGUILayout.ColorField("bloom color", bloom.bloomColor);
                bloom.bloomDownSample = EditorGUILayout.IntSlider("bloom downsample", bloom.bloomDownSample, 0, 4);
                bloom.bloomFactor = EditorGUILayout.Slider("bloom factor", bloom.bloomFactor, 0, 2.0f);
                bloom.blurSize = EditorGUILayout.Slider("blur size", bloom.blurSize, 1, 10);
                bloom.blurDownsample = EditorGUILayout.IntSlider("blur downsample", bloom.blurDownsample, 1, 8);
                bloom.blurIterations = EditorGUILayout.IntSlider("blur iteration", bloom.blurIterations, 0, 4);
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        DistorSection distor = compose.m_distor;
        if (distor != null)
        {
            distor.enable = EditorGUILayout.Toggle("distor 开关", distor.enable);
            if (EditorGUILayout.Foldout(distor.enable, "distor 属性项"))
            {
                EditorGUILayout.BeginVertical("box");
                //nothing
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUI.changed)
			EditorUtility.SetDirty (target);
	}

}
