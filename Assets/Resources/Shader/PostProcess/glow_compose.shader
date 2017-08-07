
Shader "post/glow_compose" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_GlowMask ("Glow mask", 2D) = "black" {}
		_BlurMask ("Blurred glow mask", 2D) = "black" {}

		//---just for debug
		_GlobalTint ("Global tint", color) = (1,1,1,1)
		_GlowPower ("Glow Power", float) = 1
		_BlurPower("Blur Power", float) = 1
	}

 SubShader {
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag	

			uniform sampler2D _MainTex;
			uniform sampler2D _GlowMask;
			uniform sampler2D _BlurMask;

			half4 _MainTex_TexelSize;
			half4 _GlowMask_TexelSize;
			half4 _BlurMask_TexelSize;

			fixed4 _GlobalTint;
			fixed _GlowPower;
			fixed _BlurPower;

			struct v2f {
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;			
					float2 uv1 : TEXCOORD1;
					float2 uv2 : TEXCOORD2;					
				};

			v2f vert( appdata_img v )
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord.xy;  //MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
				
				o.uv1 = o.uv;
				o.uv2 = o.uv;

				#if defined(UNITY_HALF_TEXEL_OFFSET)		
					o.uv.y = o.uv.y - _MainTex_TexelSize * 2;
				#else		
				#endif
				
				#if UNITY_UV_STARTS_AT_TOP		
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;	
				if(_GlowMask_TexelSize.y < 0)
					o.uv1.y = 1 - o.uv1.y;
				if(_BlurMask_TexelSize.y < 0)
					o.uv2.y = 1 - o.uv2.y;
				#endif

				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 mT = tex2D(_MainTex, i.uv); 
				//return (mT*_GlobalTint);

				fixed4 gM = tex2D(_GlowMask, i.uv1);
				fixed4 bM = tex2D(_BlurMask, i.uv2);	
				
				mT = saturate(mT + 
					gM * _GlobalTint * _GlowPower + 
					bM * _GlobalTint * _BlurPower);
						
				return mT;
			}
			ENDCG
		}
	} 
}