
Shader "post/compose" {  
  
    Properties{  
        _MainTex("Base (RGB)", 2D) = "white" {}  
    }  
  
    CGINCLUDE  
    #include "UnityCG.cginc"  

    #pragma multi_compile __ _USE_CAMERA_DEPTH
    #pragma multi_compile __ _GLOW
    #pragma multi_compile __ _DISTOR
    #pragma multi_compile __ _BLOOM

      
    struct v2f_compose  
    {  
        float4 pos : SV_POSITION;  
        float2 uv : TEXCOORD0; 

        #ifdef _DISTOR
        float2 uv1 : TEXCOORD1;
        #endif

        #ifdef _GLOW
        float4 uv2_3 : TEXCOORD2;
        #endif

		#ifdef _BLOOM
		float2 uv5 : TEXCOORD4;
		#endif

        #ifdef _USE_CAMERA_DEPTH
			float4 projPos : TEXCOORD6;
		#endif
    };  
  
    sampler2D _MainTex;  
    //float4 _MainTex_TexelSize;  
    

    #ifdef _GLOW
    uniform sampler2D _GlowMask;
	uniform sampler2D _BlurMask;
    uniform	fixed4 _GlobalTint;
	uniform	fixed _GlowPower;
	uniform	fixed _BlurPower;
    #endif

    #ifdef _DISTOR
    sampler2D _DistorMask;
    float4 _DistorMask_TexelSize;
    #endif

    //#ifdef _USE_CAMERA_DEPTH
    sampler2D _CameraDepthTexture;
    //#endif

	#ifdef _BLOOM
	uniform sampler2D _BloomMask;
	uniform	fixed4 _BloomColor;
	uniform	fixed _BloomFactor;
	#endif

	float _InvFade;

    v2f_compose vert_compose(appdata_img v)  
    {  
		v2f_compose o;
		UNITY_INITIALIZE_OUTPUT(v2f_compose, o)
		
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
        o.uv = v.texcoord.xy;
    
        #ifdef _DISTOR
			#ifdef _USE_CAMERA_DEPTH
				o.projPos = ComputeScreenPos(o.pos);
				COMPUTE_EYEDEPTH(o.projPos.z);
			#endif

            o.uv1 = o.uv;
            #if UNITY_UV_STARTS_AT_TOP
            //if(_DistorMask_TexelSize.y < 0)
                o.uv1.y = 1 - o.uv1.y;
            #endif
        #endif

        #ifdef _GLOW
            o.uv2_3.xy = o.uv;
            o.uv2_3.zw = o.uv;

            #if UNITY_UV_STARTS_AT_TOP
                o.uv2_3.y = 1 - o.uv2_3.y;
                o.uv2_3.w = 1 - o.uv2_3.w;
            #endif
        #endif

		#ifdef _BLOOM
				o.uv5 = o.uv;
				#if UNITY_UV_STARTS_AT_TOP
				o.uv5.y = 1 - o.uv5.y;
				#endif
		#endif

        return o;  
    }  
  
    fixed4 frag_compose(v2f_compose i) : SV_Target  
    {  
        fixed alpha = 1.0f;

        //distor
        float2 offsetUV = float2(0,0);

        #ifdef _DISTOR
		
            #ifdef _USE_CAMERA_DEPTH
            if (_InvFade > 0.0001)
            {
                float sceneZ = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
                float partZ = i.projPos.z;
                float fade = saturate(_InvFade * (sceneZ - partZ));
                alpha *= fade;
            }
            #endif

        fixed4 bump = tex2D(_DistorMask, i.uv1);
        half  bumpAmt = bump.a * 2.0;
        half3 offset = bump.rgb * 2 - 1;
        offsetUV = offset.xy * bumpAmt;
		offsetUV.x = offsetUV.x * step(0.005, bump.x);
		offsetUV.y = offsetUV.y * step(0.005, bump.y);

            #ifdef _USE_CAMERA_DEPTH
		        offsetUV *= alpha;
            #endif

        #endif
        
        fixed4 color = tex2D(_MainTex , i.uv + offsetUV);
		
		//bloom
		#ifdef _BLOOM
		fixed4 bloom = tex2D(_BloomMask, i.uv5);
		color = color + _BloomFactor * bloom * _BloomColor;
		#endif

        //glow
        #ifdef _GLOW
        fixed4 gM = tex2D(_GlowMask, i.uv2_3.xy);
		fixed4 bM = tex2D(_BlurMask, i.uv2_3.zw);
        color = saturate(color + gM * _GlobalTint * _GlowPower + bM * _GlobalTint * _BlurPower); 
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
    }  
}  