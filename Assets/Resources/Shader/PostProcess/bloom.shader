
Shader "post/bloom" {  
  
    Properties{  
        _MainTex("Base (RGB)", 2D) = "white" {}  
        _BlurTex("Blur", 2D) = "white"{}  
    }  
  
    CGINCLUDE  
    #include "UnityCG.cginc"  
      
    struct v2f_threshold  
    {  
        float4 pos : SV_POSITION;  
        float2 uv : TEXCOORD0;  
    };  
  
    struct v2f_bloom  
    {  
        float4 pos : SV_POSITION;  
        float2 uv  : TEXCOORD0;  
        float2 uv1 : TEXCOORD1;  
    };  
  
    sampler2D _MainTex;  
    float4 _MainTex_TexelSize;  
    sampler2D _BlurTex;  
    float4 _BlurTex_TexelSize;  

    float4 _colorThreshold;  
    float4 _bloomColor;  
    float _bloomFactor;  
  
    v2f_threshold vert_threshold(appdata_img v)  
    {  
        v2f_threshold o;  
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
        o.uv = v.texcoord.xy;
//#if UNITY_UV_STARTS_AT_TOP  
//        if (_MainTex_TexelSize.y < 0)  
//            o.uv.y = 1 - o.uv.y;  
//#endif    
        return o;  
    }  
  
    fixed4 frag_threshold(v2f_threshold i) : SV_Target  
    {  
        fixed4 color = tex2D(_MainTex, i.uv);   
        return saturate(color - _colorThreshold);  
    }  
  

    v2f_bloom vert_bloom(appdata_img v)  
    {  
        v2f_bloom o;  
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
  
    fixed4 frag_bloom(v2f_bloom i) : SV_Target  
    {  
        fixed4 ori = tex2D(_MainTex, i.uv);  
        fixed4 blur = tex2D(_BlurTex, i.uv1);  
        fixed4 final = ori + _bloomFactor * blur * _bloomColor;  
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
  
            CGPROGRAM  
            #pragma vertex vert_threshold  
            #pragma fragment frag_threshold  
            ENDCG  
        }  
 
        Pass  
        {  
  
            ZTest Off  
            Cull Off  
            ZWrite Off  
            Fog{ Mode Off }  
  
            CGPROGRAM  
            #pragma vertex vert_bloom  
            #pragma fragment frag_bloom  
            ENDCG  
        }  
    }  
}  