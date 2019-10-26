// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Kinect/ForegroundShader" 
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
		_ColorTex ("_ColorTex", 2D) = "white" {}
		_GradientTex("GradientTex (RGB)", 2D) = "white" {}
		_GradientColor("GradientColor", Color) = (1, 1, 1, 1)
	}

	SubShader 
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _ColorTex;
			uniform sampler2D _GradientTex;
			uniform half4 _GradientColor;

			float4 _MainTex_ST; 
			float4 _ColorTex_ST;
			float4 _GradientTex_ST;


			struct v2f 
			{
			   float4 pos : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 texMain = tex2D(_MainTex, i.uv);
				half4 texColor = tex2D(_ColorTex, i.uv);

				half gradA = tex2D(_GradientTex, i.uv).a;
				bool gradMask = gradA > 0.5 && _GradientColor.a > 0.0;
				half4 fgColor = texColor * (1.0 - gradMask) + _GradientColor * gradMask;

				half4 texForeground = half4(fgColor.rgb, texMain.a);

				return texForeground;
			}
			ENDCG

		}
	}

}
