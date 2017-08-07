
Shader "post/radial_blur"   
{  
    Properties   
    {  
        _MainTex ("Base (RGB)", 2D) = "white" {}  
        _BlurTex("Blur Tex", 2D) = "white"{}  
    }  
  
    CGINCLUDE  
    uniform sampler2D _MainTex;  
    uniform sampler2D _BlurTex;  
    uniform float _BlurFactor;    
    uniform float _LerpFactor;  
    uniform float4 _BlurCenter; 
    uniform float4 _MainTex_TexelSize;
    uniform float4 _BlurTex_TexelSize;
    uniform half _SampleStrength;  //6

    #include "UnityCG.cginc"  
  
    fixed4 frag_blur(v2f_img i) : SV_Target  
    {  
        float2 dir = i.uv - _BlurCenter.xy;  
        float4 outColor = 0;  
        float t = clamp(_SampleStrength,1,9);
        for (int j = 0; j < t; ++j)  
        {  
            float2 uv = i.uv + _BlurFactor * dir * j;  
            outColor += tex2D(_MainTex, uv);  
        }  
        outColor /= t;  
        return outColor;  
    }  
  
    struct v2f_lerp  
    {  
        float4 pos : SV_POSITION;  
        float2 uv1 : TEXCOORD0;   
        float2 uv2 : TEXCOORD1; 
    };  
      
    v2f_lerp vert_lerp(appdata_img v)  
    {  
        v2f_lerp o;  
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

        o.uv1 = v.texcoord.xy;  
        o.uv2 = v.texcoord.xy;  
        
        #if UNITY_UV_STARTS_AT_TOP  
        if (_MainTex_TexelSize.y < 0)  
            o.uv1.y = 1 - o.uv1.y;  
        #endif  

        #if UNITY_UV_STARTS_AT_TOP  
        if (_BlurTex_TexelSize.y < 0)  
            o.uv2.y = 1 - o.uv2.y;  
        #endif  

        return o;  
    }  
  
    fixed4 frag_lerp(v2f_lerp i) : SV_Target  
    {  
        float2 dir = i.uv1 - _BlurCenter.xy;  
        float dis = length(dir);  
        fixed4 oriTex = tex2D(_MainTex, i.uv1);  
        fixed4 blurTex = tex2D(_BlurTex, i.uv2);  
        return lerp(oriTex, blurTex, _LerpFactor * dis);  
    }  
    ENDCG  
  
    SubShader  
    {   
        Pass  
        {  
            ZTest Always  
            Cull Off  
            ZWrite Off  
            Fog{ Mode off }  
  
            CGPROGRAM  
            #pragma fragmentoption ARB_precision_hint_fastest   
            #pragma vertex vert_img  
            #pragma fragment frag_blur   
            ENDCG  
        }  
  
        Pass  
        {  
            ZTest Always  
            Cull Off  
            ZWrite Off  
            Fog{ Mode off }  
     
            CGPROGRAM  
            #pragma fragmentoption ARB_precision_hint_fastest     
            #pragma vertex vert_lerp  
            #pragma fragment frag_lerp   
            ENDCG  
        }  
    }  
    Fallback off  
}  