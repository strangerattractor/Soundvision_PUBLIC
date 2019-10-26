// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Kinect/InvertShader" {
    Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
	}

    SubShader {
        Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
		
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			uniform sampler2D _MainTex;    
			uniform float _TexResX;
			uniform float _TexResY;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

			float4 _MainTex_ST; 

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 texColor = tex2D(_MainTex, i.uv);
				return fixed4(texColor.rgb, 1.0 - texColor.a);
            }
            ENDCG

        }
    }
}
