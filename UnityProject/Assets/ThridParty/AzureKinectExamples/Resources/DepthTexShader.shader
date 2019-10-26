// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Kinect/DepthTexShader" {
	Properties {
		//_MainTex ("Base (RGB)", 2D) = "white" {}
	}
    
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
		
			CGPROGRAM
			#pragma target 5.0
			//#pragma enable_d3d11_debug_symbols

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			//uniform sampler2D _MainTex;
			StructuredBuffer<uint> _DepthMap;

			uint _TexResX;
			uint _TexResY;
			uint _MinDepth;
			uint _MaxDepth;

			struct v2f {
				float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = v.texcoord;
				
				return o;
			}

			float4 frag (v2f i) : COLOR
			{
				uint dx = (uint)(i.uv.x * _TexResX);
				uint dy = (uint)(i.uv.y * _TexResY);
				uint di = (dx + dy * _TexResX);
				
				//return float4((float)dx / (float)_TexResX, (float)dy / (float)_TexResY, 0, 1);

				uint depth2 = _DepthMap[di >> 1];
				uint depth = di & 1 != 0 ? depth2 >> 16 : depth2 & 0xffff;
				depth = (depth >= _MinDepth && depth <= _MaxDepth) * (depth << 2);

				float r = (float)(depth & 0xff) / 255.0;
				float g = (float)(depth >> 8) / 255.0;

				return float4(r, g, 0.0, 1.0);
			}

			ENDCG
		}
	}

	Fallback Off
}