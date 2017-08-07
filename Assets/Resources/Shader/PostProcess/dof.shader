

Shader "post/dof" {  
  
    Properties{  
        _MainTex("Base (RGB)", 2D) = "white" {}  
        _BlurTex("Blur", 2D) = "white"{}  

		//----for debug
		_focalDistance("focal distance",float) = 0.5
		_nearBlurScale("near blur scale",float) = 0.2
		_farBlurScale("far distance",float) = 0.8
    }  
  
    CGINCLUDE  
    #include "UnityCG.cginc"  
  
    struct v2f_dof  
    {  
        float4 pos : SV_POSITION;  
        float2 uv  : TEXCOORD0;  
        float2 uv1 : TEXCOORD1;  
    };  
  
    sampler2D _MainTex;  
    float4 _MainTex_TexelSize;  
    sampler2D _BlurTex;  
    float4 _BlurTex_TexelSize; 
    sampler2D_float _CameraDepthTexture;  
    float _focalDistance;  
    float _nearBlurScale;  
    float _farBlurScale;  
  
    v2f_dof vert_dof(appdata_img v)  
    {  
        v2f_dof o;  
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
        o.uv.xy = v.texcoord.xy;  
        o.uv1.xy = o.uv.xy;  
        
		#if UNITY_UV_STARTS_AT_TOP  
        if (_MainTex_TexelSize.y < 0)  
            o.uv.y = 1 - o.uv.y;  
        if(_BlurTex_TexelSize.y < 0)
			o.uv1.y = 1 - o.uv1.y;
        #endif    

        return o;  
    }  
  
    fixed4 frag_dof(v2f_dof i) : SV_Target  
    {  
        fixed4 ori = tex2D(_MainTex, i.uv);   
        fixed4 blur = tex2D(_BlurTex, i.uv1);  
        float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);  
        depth = Linear01Depth(depth);  
		float focalTest = clamp(sign(depth - _focalDistance),0,1);  
        fixed4 final = (1 - focalTest) * ori + focalTest * lerp(ori, blur, clamp((depth - _focalDistance) * _farBlurScale, 0, 1));  
        final = (focalTest)* final + (1 - focalTest) * lerp(ori, blur, clamp((_focalDistance - depth) * _nearBlurScale, 0, 1));  
        return final;  
    }  
  
    ENDCG  
  
    SubShader  
    {  
        Pass  
        {  
            ZTest Off  
            Cull Off  
            ZWrite Off  
            Fog{ Mode Off }  
            ColorMask RGBA  
  
            CGPROGRAM  
            #pragma vertex vert_dof  
            #pragma fragment frag_dof  
            ENDCG  
        }  
  
    }  
}  