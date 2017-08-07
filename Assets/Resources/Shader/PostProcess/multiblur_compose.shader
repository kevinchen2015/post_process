
Shader "post/multiblur_compose" {  
  
    Properties{  
        _MainTex("Base (RGB)", 2D) = "white" {}  
    }  
  
    CGINCLUDE  
    #include "UnityCG.cginc"  

    #pragma multi_compile __ _USE_CAMERA_DEPTH

    #pragma multi_compile __ _RADIALBLUR
	#pragma multi_compile __ _DOF
      
    struct v2f_compose  
    {  
        float4 pos : SV_POSITION;  
        float2 uv : TEXCOORD0; 

        #ifdef _RADIALBLUR
        float2 uv4 : TEXCOORD3;
        #endif

		#ifdef _DOF
		float2 uv6 : TEXCOORD5;
		#endif

        #ifdef _USE_CAMERA_DEPTH
			float4 projPos : TEXCOORD6;
		#endif
    };  
  
    sampler2D _MainTex;  
    //float4 _MainTex_TexelSize;  
    
	sampler2D _BlurTex;
	//float4 _BlurTex_TexelSize;

    //#ifdef _USE_CAMERA_DEPTH
    sampler2D _CameraDepthTexture;
    //#endif

	#ifdef _DOF
	float _focalDistance;
	float _nearBlurScale;
	float _farBlurScale;
	#endif

    #ifdef _RADIALBLUR
    uniform sampler2D _RadialBlur;
    uniform float _LerpFactor;  
    uniform float4 _BlurCenter;
    uniform float _BlurFactor;  
    uniform half _SampleStrength;  //6   
    #endif

    fixed4 frag_blur(v2f_img i) : SV_Target  
    {  
        float4 outColor = 0; 

        #ifdef _RADIALBLUR
        float2 dir = i.uv - _BlurCenter.xy;  
        float t = clamp(_SampleStrength,1,9);
        for (int j = 0; j < t; ++j)  
        {  
            float2 uv = i.uv + _BlurFactor * dir * j;  
            outColor += tex2D(_MainTex, uv);  
        }  
        outColor /= t;  
        #endif

        return outColor;  
    }  
  
    v2f_compose vert_compose(appdata_img v)  
    {  
		v2f_compose o;
        UNITY_INITIALIZE_OUTPUT(v2f_compose, o)
		
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord.xy;
    
        #ifdef _RADIALBLUR
             o.uv4 = o.uv;
             #if UNITY_UV_STARTS_AT_TOP
               // o.uv4.y = 1 - o.uv4.y;
            #endif
        #endif

		#ifdef _DOF
				o.uv6 = o.uv;
				#if UNITY_UV_STARTS_AT_TOP
				//o.uv6.y = 1 - o.uv6.y;
				#endif
		#endif

        return o;  
    }  
  
    fixed4 frag_compose(v2f_compose i) : SV_Target  
    {  
        fixed alpha = 1.0f;

        //distor
        float2 offsetUV = float2(0,0);

        fixed4 color = tex2D(_MainTex , i.uv + offsetUV);
	
		//radial blur
		#ifdef _RADIALBLUR
		float2 dir = i.uv4 - _BlurCenter.xy;
		float dis = length(dir);
		fixed4 radialBlur = tex2D(_RadialBlur, i.uv4);
		color = lerp(color, radialBlur, _LerpFactor * dis);
		#endif
		
		//dof
		#ifdef _DOF
		fixed4 blur = tex2D(_BlurTex, i.uv6);
		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
		depth = Linear01Depth(depth);
		//return fixed4(depth, depth, depth, 1);
		float focalTest = clamp(sign(depth - _focalDistance), 0, 1);
		//return fixed4(focalTest, focalTest, focalTest, 1);
		color = (1 - focalTest) * color + focalTest * lerp(color, blur, clamp((depth - _focalDistance) * _farBlurScale, 0, 1));
		//return color;
		color = (focalTest)* color + (1 - focalTest) * lerp(color, blur, clamp((_focalDistance - depth) * _nearBlurScale, 0, 1));
		#endif

        return color;
    }  
    ENDCG  
  
    SubShader  
    {  
        //0
        Pass  
        {  
            ZTest Off  
            Cull Off  
            ZWrite Off  
            Fog{ Mode Off }  

            CGPROGRAM  
            #pragma vertex vert_compose  
            #pragma fragment frag_compose  
            ENDCG  
        }  

        //1
        Pass  
        {  
            Cull Off  
            ZWrite Off  
            Fog{ Mode off }  
  
            CGPROGRAM  
            #pragma fragmentoption ARB_precision_hint_fastest   
            #pragma vertex vert_img  
            #pragma fragment frag_blur   
            ENDCG  
        } 
    }  
}  