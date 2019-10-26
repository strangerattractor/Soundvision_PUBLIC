// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Kinect/UserBlendShader" 
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}
		_BackTex ("BackTex", 2D) = "white" {}
		_ColorTex ("ColorTex", 2D) = "white" {}
		_Threshold ("Depth Threshold", Range(0, 0.5)) = 0.1
		_BlurOffset("Blur Offset", Range(0, 10)) = 2
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }

		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
		
			CGPROGRAM
			#pragma target 5.0
			//#pragma enable_d3d11_debug_symbols

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			//float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			sampler2D _CameraDepthTexture;

			uniform sampler2D _BackTex;
			uniform sampler2D _ColorTex;
			uniform float _Threshold;
			uniform int _BlurOffset;

			uniform float _ColorResX;
			uniform float _ColorResY;
			uniform float _ColorScaleX;

			uniform float _ColorOfsX;
			uniform float _ColorMulX;
			uniform float _ColorOfsY;
			uniform float _ColorMulY;

			StructuredBuffer<uint> _DepthMap;


			struct v2f 
			{
			   float4 pos : SV_POSITION;
			   float2 uv : TEXCOORD0;
			   float2 uv2 : TEXCOORD1;
			   float4 scrPos : TEXCOORD2;
			};

			v2f vert (appdata_base v)
			{
			   v2f o;
			   
			   o.pos = UnityObjectToClipPos (v.vertex);
			   o.uv = v.texcoord;

			   o.uv2.x = o.uv.x;
			   o.uv2.y = 1 - o.uv.y;

			   o.scrPos = ComputeScreenPos(o.pos);

			   return o;
			}

			half getKinectAlpha(int2 cxy, float camDepth)
			{
				int rcCount = 2 * _BlurOffset + 1;
				int maxCount = rcCount * rcCount;

				int ci0 = (int)((cxy.x - _BlurOffset) + (cxy.y - _BlurOffset) * _ColorResX);
				int pixCount = 0;

				for (int iY = -_BlurOffset; iY <= _BlurOffset; iY++)
				{
					uint ci = ci0;

					for (int iX = -_BlurOffset; iX <= _BlurOffset; iX++, ci++)
					{
						uint depth2 = _DepthMap[ci >> 1];
						//uint depth = (ci % 2 == 0 ? depth2 <<= 16 : depth2) >> 16;
						uint depth = ci & 1 != 0 ? depth2 >> 16 : depth2 & 0xffff;

						if (depth != 0)
						{
							float kinDepth = depth / 1000.0;
							if ((camDepth < 0.1 || camDepth >= 10.0) || (kinDepth >= 0.1 && camDepth > (kinDepth + _Threshold)))
							{
								pixCount++;
							}
						}
						else
						{
							if (camDepth < 0.1 || camDepth >= 10.0)
							{
								pixCount++;
							}
						}
					}

					ci0 += _ColorResX;
				}

				half alpha = (half)pixCount / (half)maxCount;

				return alpha;
			}

			half4 frag (v2f i) : COLOR
			{
			    float camDepth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
				//float camDepth01 = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);

				float2 ctUv = float2(_ColorOfsX + i.uv.x * _ColorMulX, 1.0 - i.uv.y /**_ColorOfsY + i.uv.y * _ColorMulY*/);
#if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0.0)
                {
                    ctUv.y = 1.0 - ctUv.y;
                }
#endif
				// for non-flipped textures
				float2 ctUv2 = float2(ctUv.x, 1.0 - ctUv.y);

				if (_ColorScaleX < 0.0)
				{
					ctUv.x = 1.0 - ctUv.x;
				}

				uint cx = (int)(ctUv.x * _ColorResX);
				uint cy = (int)(ctUv.y * _ColorResY);

				half4 clrBack = tex2D(_BackTex, ctUv2);
				half4 clrFront = tex2D(_ColorTex, ctUv);
				half3 clrBlend = clrBack.rgb * (1.0 - clrFront.a) + clrFront.rgb * clrFront.a;

				half4 clrMain = tex2D(_MainTex, i.uv);
				half kinAlpha = getKinectAlpha(int2(cx, cy), camDepth);
				//clrBlend = lerp(clrMain.rgb, clrBlend.rgb, kinAlpha);
				clrBlend = clrMain.rgb * (1.0 - kinAlpha) + clrBlend * kinAlpha;

				//uint ci = cx + cy * _ColorResX;
				//uint depth2 = _DepthMap[ci >> 1];
				//uint depth = ci & 1 != 0 ? depth2 >> 16 : depth2 & 0xffff;
				//float kinDepth = (float)depth / 4000.0;

				//return half4(kinDepth, kinDepth, kinDepth, 1.0);
				//bool mask = depth <= 1000;
				//clrBlend = half4(kinDepth, kinDepth, kinDepth, 1.0);
				//clrBlend = clrMain.rgb * (1 - mask) + clrBlend * mask;

				return half4(clrBlend, 1.0);
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}
