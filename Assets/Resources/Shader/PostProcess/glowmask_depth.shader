

Shader "post/glowmask_depth"
{
	Properties 
	{
		_GlowTint ("Color", Color) = (1,1,1,1)
		_GlowMask ("Glow mask(RGB)", 2D) = "white" {}

		//---for debug
		_GlowColor("Color", Color) = (1,1,1,1)
	}
	
	SubShader
	{
	    //Tags { "RenderType"="Opaque" }
		ZWrite On
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
			float4 scrPos : TEXCOORD0;			
			half2 texcoord : TEXCOORD1;
		};

		sampler2D_float _CameraDepthTexture;
		
		sampler2D _GlowMask;
		float4 _GlowMask_TexelSize;
		float4 _GlowMask_ST;
		
		half4 _MainTex_TexelSize;
		fixed4 _GlowColor;
		float _zShift;
		
		v2f vert (appdata_t v)
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.scrPos = ComputeScreenPos(o.pos);			
			o.texcoord = TRANSFORM_TEX(v.texcoord, _GlowMask);
//#if UNITY_UV_STARTS_AT_TOP  
//			if (_GlowMask_TexelSize.y < 0)
//				o.texcoord.y = 1 - o.texcoord.y;
//#endif  
			COMPUTE_EYEDEPTH(o.scrPos.z);
			return o;
		}		

		fixed4 frag(v2f i) : COLOR 
		{ 			
			float origDepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
			float depth = i.scrPos.z;
			fixed4 c = (0.0,0.0,0.0,0.0);
			
			float f = step(depth * _zShift, origDepth);
			c = tex2D(_GlowMask, i.texcoord) * _GlowColor * f;
								
			return c;			
		}
		
		ENDCG
	    }
	}
}