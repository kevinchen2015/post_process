
Shader "post/distor_mask"
{
	Properties 
	{
		_BumpMap ("_BumpMap", 2D) = "bump" {}
		_BumpAmt ("_BumpAmt", Float) = 1
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
		};

		sampler2D _BumpMap;
		float4 _BumpMap_TexelSize;
		float4 _BumpMap_ST;
		float _BumpAmt;
		
		v2f vert (appdata_t v)
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _BumpMap);
			return o;
		}		

		fixed4 frag(v2f i) : COLOR 
		{ 			
			fixed4 bump = (tex2D(_BumpMap, i.uv));
			bump.a = _BumpAmt * 0.5;
			return bump;		
		}
		
		ENDCG
	    }
	}
}