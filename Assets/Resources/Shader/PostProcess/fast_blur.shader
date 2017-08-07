

Shader "post/fast_blur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}		
	}
	
	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;		
		uniform half4 _MainTex_TexelSize;
		uniform half4 _Parameter;

		struct v2f_tap
		{
			float4 pos : SV_POSITION;
			half2 uv20 : TEXCOORD0;
			half2 uv21 : TEXCOORD1;
			half2 uv22 : TEXCOORD2;
			half2 uv23 : TEXCOORD3;
		};			

		v2f_tap vert4Tap(appdata_img v)
		{
			v2f_tap o;

			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

			half2 uv = v.texcoord;

			o.uv20 = uv + _MainTex_TexelSize.xy;
			o.uv21 = uv + _MainTex_TexelSize.xy * half2(-0.5h, -0.5h);
			o.uv22 = uv + _MainTex_TexelSize.xy * half2(0.5h, -0.5h);
			o.uv23 = uv + _MainTex_TexelSize.xy * half2(-0.5h, 0.5h);

			return o; 
		}					
		
		fixed4 fragDownsample ( v2f_tap i ) : SV_Target
		{				
			fixed4 color = tex2D (_MainTex, i.uv20);
			color += tex2D (_MainTex, i.uv21);
			color += tex2D (_MainTex, i.uv22);
			color += tex2D (_MainTex, i.uv23);
			return color / 4;
		}
	
		// weight curves

		static const half curve[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };  // gauss'ish blur weights

		static const half4 curve4[7] = { half4(0.0205,0.0205,0.0205,0.0205), half4(0.0855,0.0855,0.0855,0.0855), half4(0.232,0.232,0.232,0.232),
			half4(0.324,0.324,0.324,0.324), half4(0.232,0.232,0.232,0.232), half4(0.0855,0.0855,0.0855,0.0855), half4(0.0205,0.0205,0.0205,0.0205) };

		struct v2f_withBlurCoords8 
		{
			float4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
			half2 offs : TEXCOORD1;
		};	
		
		struct v2f_withBlurCoordsSGX 
		{
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half4 offs[3] : TEXCOORD1;
		};

		v2f_withBlurCoords8 vertBlurHorizontal (appdata_img v)
		{
			v2f_withBlurCoords8 o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
//#if UNITY_UV_STARTS_AT_TOP  
//			if (_MainTex_TexelSize.y < 0)
//				o.uv.y = 1 - o.uv.y;
//#endif   
			o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _Parameter.x;

			return o; 
		}
		
		v2f_withBlurCoords8 vertBlurVertical (appdata_img v)
		{
			v2f_withBlurCoords8 o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
//#if UNITY_UV_STARTS_AT_TOP  
//			if (_MainTex_TexelSize.y < 0)
//				o.uv.y = 1 - o.uv.y;
//#endif   
			o.offs = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _Parameter.x;
			 
			return o; 
		}	

		half4 fragBlur8 ( v2f_withBlurCoords8 i ) : SV_Target
		{
			half2 uv = i.uv.xy; 
			half2 netFilterWidth = i.offs;  
			half2 coords = uv - netFilterWidth * 3.0;  
			
			half4 color = 0;
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, coords);
				color += tap * curve4[l];
				coords += netFilterWidth;
  			}
			return color;
		}


		v2f_withBlurCoordsSGX vertBlurHorizontalSGX (appdata_img v)
		{
			v2f_withBlurCoordsSGX o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = v.texcoord.xy;

//#if UNITY_UV_STARTS_AT_TOP  
//			if (_MainTex_TexelSize.y < 0)
//				o.uv.y = 1 - o.uv.y;
//#endif   

			half2 netFilterWidth = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _Parameter.x; 
			half4 coords = -netFilterWidth.xyxy * 3.0;
			
			o.offs[0] = v.texcoord.xyxy + coords * half4(1.0h,1.0h,-1.0h,-1.0h);
			coords += netFilterWidth.xyxy;
			o.offs[1] = v.texcoord.xyxy + coords * half4(1.0h,1.0h,-1.0h,-1.0h);
			coords += netFilterWidth.xyxy;
			o.offs[2] = v.texcoord.xyxy + coords * half4(1.0h,1.0h,-1.0h,-1.0h);

			return o; 
		}		
		
		v2f_withBlurCoordsSGX vertBlurVerticalSGX (appdata_img v)
		{
			v2f_withBlurCoordsSGX o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);

//#if UNITY_UV_STARTS_AT_TOP  
//			if (_MainTex_TexelSize.y < 0)
//				o.uv.y = 1 - o.uv.y;
//#endif   
			half2 netFilterWidth = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _Parameter.x;
			half4 coords = -netFilterWidth.xyxy * 3.0;
			
			o.offs[0] = v.texcoord.xyxy + coords * half4(1.0h,1.0h,-1.0h,-1.0h);
			coords += netFilterWidth.xyxy;
			o.offs[1] = v.texcoord.xyxy + coords * half4(1.0h,1.0h,-1.0h,-1.0h);
			coords += netFilterWidth.xyxy;
			o.offs[2] = v.texcoord.xyxy + coords * half4(1.0h,1.0h,-1.0h,-1.0h);

			return o; 
		}	

		half4 fragBlurSGX ( v2f_withBlurCoordsSGX i ) : SV_Target
		{
			half2 uv = i.uv.xy;
			
			half4 color = tex2D(_MainTex, i.uv) * curve4[3];
			
  			for( int l = 0; l < 3; l++ )  
  			{   
				half4 tapA = tex2D(_MainTex, i.offs[l].xy);
				half4 tapB = tex2D(_MainTex, i.offs[l].zw); 
				color += (tapA + tapB) * curve4[l];
  			}

			return color;

		}	

		//Gaussian Blur
		struct v2f_blur  
		{  
			float4 pos : SV_POSITION;  
			float2 uv  : TEXCOORD0;  
			float4 uv01 : TEXCOORD1;  
			float4 uv23 : TEXCOORD2;  
			float4 uv45 : TEXCOORD3;  
		};  
		uniform float4 _offsets;

		v2f_blur vert_blur(appdata_img v)  
		{  
			v2f_blur o;  
			_offsets *= _MainTex_TexelSize.xyxy;  
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord.xy;  

//#if UNITY_UV_STARTS_AT_TOP  
//			if (_MainTex_TexelSize.y < 0)
//			{
//				o.uv.y = 1 - o.uv.y;
//			}
//#endif   
			o.uv01 = o.uv.xyxy + _offsets.xyxy * float4(1, 1, -1, -1);
			o.uv23 = o.uv.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
			o.uv45 = o.uv.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 3.0;
			return o;  
		} 

		fixed4 frag_blur(v2f_blur i) : SV_Target  
		{  
			fixed4 color = fixed4(0,0,0,0);  
			color += 0.40 * tex2D(_MainTex, i.uv);  
			color += 0.15 * tex2D(_MainTex, i.uv01.xy);  
			color += 0.15 * tex2D(_MainTex, i.uv01.zw);  
			color += 0.10 * tex2D(_MainTex, i.uv23.xy);  
			color += 0.10 * tex2D(_MainTex, i.uv23.zw);  
			color += 0.05 * tex2D(_MainTex, i.uv45.xy);  
			color += 0.05 * tex2D(_MainTex, i.uv45.zw);  
			return color;  
		} 

		struct v2f_flip
		{
			float4 pos : SV_POSITION;
			float2 uv  : TEXCOORD0;
		};
		uniform float4 _flip_param;

		v2f_flip vert_flip(appdata_img v)
		{
			v2f_flip o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord.xy;
			if(_flip_param.x < 0)
				o.uv.x = 1 - o.uv.x;
			if (_flip_param.y < 0)
				o.uv.y = 1 - o.uv.y;
			return o;
		}

		fixed4 frag_flip(v2f_flip i) : SV_Target
		{
			fixed4 color = tex2D(_MainTex, i.uv);
			return color;
		}

					
	ENDCG
	
	SubShader {
	  ZTest Off Cull Off ZWrite Off Blend Off

	// 0
	Pass { 
	
		CGPROGRAM
		
		#pragma vertex vert4Tap
		#pragma fragment fragDownsample
		
		ENDCG
		 
		}

	// 1
	Pass {
		ZTest Always
		Cull Off
		
		CGPROGRAM 
		
		#pragma vertex vertBlurVertical
		#pragma fragment fragBlur8
		
		ENDCG 
		}	
		
	// 2
	Pass {		
		ZTest Always
		Cull Off
				
		CGPROGRAM
		
		#pragma vertex vertBlurHorizontal
		#pragma fragment fragBlur8
		
		ENDCG
		}	

	// alternate blur
	// 3
	Pass {
		ZTest Always
		Cull Off
		
		CGPROGRAM 
		
		#pragma vertex vertBlurVerticalSGX
		#pragma fragment fragBlurSGX
		
		ENDCG
		}	
		
	// 4
	Pass {		
		ZTest Always
		Cull Off
				
		CGPROGRAM
		
		#pragma vertex vertBlurHorizontalSGX
		#pragma fragment fragBlurSGX
		
		ENDCG
		}	

	// 5
	Pass {		
		ZTest Always
		Cull Off
				
		CGPROGRAM
		
		#pragma vertex vert_blur
		#pragma fragment frag_blur
		
		ENDCG
		}	
		  //6
		  Pass{
		  ZTest Always
		  Cull Off

		  CGPROGRAM

#pragma vertex vert_flip
#pragma fragment frag_flip

		  ENDCG
	  }

	  
	}	

	FallBack Off
}
