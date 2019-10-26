// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Kinect/UserBodyImageShader" {
//    Properties {
//        _MainTex ("Base (RGB)", 2D) = "black" {}
//    }
    
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
			uniform uint _TexResX;
			uniform uint _TexResY;
			uniform uint _MinDepth;
			uniform uint _MaxDepth;

			StructuredBuffer<uint> _BodyIndexMap;
			//StructuredBuffer<uint> _DepthMap;

			//uniform float _TotalPoints;  // dummy
			//uniform uint _FirstUserIndex;  // dummy
			//StructuredBuffer<uint> _HistMap;  // dummy

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

				//uint depth2 = _DepthMap[di >> 1];
				//uint depth = di & 1 != 0 ? depth2 >> 16 : depth2 & 0xffff;
				//depth = (depth >= _MinDepth && depth <= _MaxDepth) * depth;

				uint bi4 = _BodyIndexMap[di >> 2];
				uint bi = 255;

				switch (di & 3)
				{
					case 0:
						bi = bi4 & 255;
						break;
					case 1:
						bi = (bi4 >> 8) & 255;
						break;
					case 2:
						bi = (bi4 >> 16) & 255;
						break;
					case 3:
						bi = (bi4 >> 24) & 255;
						break;
				}

				float clrPlayer = /**depth != 0 &&*/ bi != 255 ? 1.0 : 0.0;

				return float4(clrPlayer, clrPlayer, clrPlayer, clrPlayer);
			}

			ENDCG
		}
	}

	Fallback Off
}