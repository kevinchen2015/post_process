
Shader "post/distor_cutout_mask"
{
	Properties 
	{
		_BumpMap ("_BumpMap", 2D) = "bump" {}
		_BumpAmt ("_BumpAmt", Float) = 1
		_CutoutMask("_CutoutMask(R)",2D) = "white"{}
	}
	
	SubShader
	{
	    Tags { "RenderType"="Opaque" }

	    Pass 
	    {
	    Fog { Mode Off }
		CGPROGRAM		
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
								
		struct appdata_t 
		{
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
		};	
								
		struct v2f {
			float4 pos : SV_POSITION;					
			half2 uv : TEXCOORD1;
			half2 uv2 : TEXCOORD2;
		};

		float _BumpAmt;

		sampler2D _BumpMap;
		float4 _BumpMap_TexelSize;
		float4 _BumpMap_ST;
		
		sampler2D _CutoutMask;
		float4 _CutoutMask_ST;
		
		v2f vert (appdata_t v)
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _BumpMap);
			o.uv2 = TRANSFORM_TEX(v.texcoord, _CutoutMask);
			return o;
		}		

		fixed4 frag(v2f i) : COLOR 
		{ 			
			fixed4 bump = (tex2D(_BumpMap, i.uv));
			fixed4 mask = (tex2D(_CutoutMask, i.uv2));
			bump.rgb *= mask.r;
			bump.a = _BumpAmt * 0.5;
			return bump;		
		}
		
		ENDCG
	    }
	}
}