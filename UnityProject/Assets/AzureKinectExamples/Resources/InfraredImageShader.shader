// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Kinect/InfraredImageShader" {
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
			uniform float _MinValue;
			uniform float _MaxValue;

			StructuredBuffer<uint> _InfraredMap;

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

			half4 frag (v2f i) : COLOR
			{
				uint dx = (uint)(i.uv.x * _TexResX);
				uint dy = (uint)(i.uv.y * _TexResY);
				uint di = (dx + dy * _TexResX);
				
				uint ir2 = _InfraredMap[di >> 1];
				uint ir = di & 1 != 0 ? ir2 >> 16 : ir2 & 0xffff;
				half clr = saturate(((float)ir - _MinValue) / (_MaxValue - _MinValue));

				return half4(clr, clr, clr, clr != 0);
			}

			ENDCG
		}
	}

	Fallback Off
}