Shader "Kinect/SceneMeshShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ColorTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Opaque" "IgnoreProjector" = "True" }
        //LOD 200

		Cull Off
		Blend Off

        CGPROGRAM

		//#include "UnityCG.cginc"
		#pragma target 5.0

		#pragma surface surf Lambert vertex:vert addshadow
		#pragma exclude_renderers d3d11_9x

        sampler2D _ColorTex;
		float4 _ColorTex_TexelSize;
		//float4 _ColorTex_ST;

#ifdef SHADER_API_D3D11
		StructuredBuffer<uint> _DepthMap;
		StructuredBuffer<float> _SpaceTable;
#endif

		int _CoarseFactor;
		float2 _SpaceScale;

		float2 _TexRes;
		float2 _Cxy;
		float2 _Fxy;

		float4x4 _Transform;
		float3 _PosMin;
		float3 _PosMax;


        struct Input
        {
			float2 uv_ColorTex : TEXCOORD0;
			float4 vertexPos : TEXCOORD2;
		};

        //half _Glossiness;
        //half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


		uint getDepthAt(float2 ui)
		{
			uint dx = (uint)(ui.x * _TexRes.x);
			uint dy = (uint)(ui.y * _TexRes.y);
			uint di = (dx + dy * _TexRes.x);

			uint depth2 = 0;
#ifdef SHADER_API_D3D11
			depth2 = _DepthMap[di >> 1];
#endif
			uint depth = di & 1 != 0 ? depth2 >> 16 : depth2 & 0xffff;

			return depth;
		}


		float4 getSpacePos(float2 ui, float fDepth, float fAplha)
		{
			uint dx = (uint)(ui.x * _TexRes.x);
			uint dy = (uint)(ui.y * _TexRes.y);
			uint di = (dx + dy * _TexRes.x) * 3;

#ifdef SHADER_API_D3D11
			float4 spacePos = float4(_SpaceTable[di] * fDepth * _SpaceScale.x, _SpaceTable[di + 1] * fDepth * _SpaceScale.y, fDepth, fAplha);
#else
			float4 spacePos = float4(0, 0, 0, fAplha);
#endif

			return spacePos;
		}


		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			float2 posDepth = (float2)v.vertex;
			uint depth = getDepthAt(posDepth);

			float dTexX = _ColorTex_TexelSize.x * _CoarseFactor;
			float dTexY = _ColorTex_TexelSize.y * _CoarseFactor;

			const float2 ofsToCheck[8] = {
				float2(-dTexX, -dTexY), float2(0, -dTexY), float2(dTexX, -dTexY),
				float2(-dTexX, 0), float2(dTexX, 0),
				float2(-dTexX, dTexY), float2(0, dTexY), float2(dTexX, dTexY)
			};

			if (depth == 0)
			{
				uint maxDepth = 0;
				uint numValid = 0;

				for (int i = 0; i < 8; i++)
				{
					uint depth2 = getDepthAt(posDepth + ofsToCheck[i]);
					maxDepth = max(maxDepth, depth2);
					numValid += (depth2 != 0);
				}

				depth = numValid > 0 ? maxDepth : 0;
			}

			float fDepth = (float)depth * 0.001;
			//float2 oPos = posDepth * _TexRes - _Cxy; // float2(posDepth.x * _TexRes.x - _Cxy.x, posDepth.y * _TexRes.y - _Cxy.y);

			//float2 pPos = float2(oPos.x * fDepth * _SpaceScale.x / _Fxy.x, oPos.y * fDepth * _SpaceScale.y / _Fxy.y);
			//float4 vPos = float4(pPos.x, pPos.y, fDepth, v.vertex.w);
			float4 vPos = getSpacePos(posDepth, fDepth, v.vertex.w);

			float4 wPos = mul(_Transform, vPos);
			bool mask = wPos.x >= _PosMin.x && wPos.x <= _PosMax.x && wPos.y >= _PosMin.y && wPos.y <= _PosMax.y && wPos.z >= _PosMin.z && wPos.z <= _PosMax.z;

			if (mask)
			{
				for (int i = 0; i < 8; i++)
				{
					float2 posDepth2 = posDepth + ofsToCheck[i];
					uint depth2 = getDepthAt(posDepth2);

					float fDepth2 = (float)depth2 * 0.001;
					//float2 oPos2 = posDepth2 * _TexRes - _Cxy;

					//float2 pPos2 = float2(oPos2.x * fDepth2 * _SpaceScale.x / _Fxy.x, oPos2.y * fDepth2 * _SpaceScale.y / _Fxy.y);
					//float4 vPos2 = float4(pPos2.x, pPos2.y, fDepth2, v.vertex.w);
					float4 vPos2 = getSpacePos(posDepth2, fDepth2, v.vertex.w);

					float4 wPos2 = mul(_Transform, vPos2);
					bool mask2 = abs(wPos2.z - wPos.z) < 0.1 &&
						wPos2.x >= _PosMin.x && wPos2.x <= _PosMax.x && wPos2.y >= _PosMin.y && wPos2.y <= _PosMax.y && wPos2.z >= _PosMin.z && wPos2.z <= _PosMax.z;
					mask = mask && mask2;
				}
			}

			float4 vertexPos = vPos;

			v.vertex = vertexPos;
			v.texcoord.xy = posDepth;

			o.vertexPos = float4(vertexPos.xyz, mask);  // vertexPos * mask;  //
		}


		void surf(Input IN, inout SurfaceOutput o)
		{
			float4 col = tex2D(_ColorTex, IN.uv_ColorTex);
			//float4 col = float3(1, 0, 0);

			if (IN.vertexPos.w == 0)
			{
				discard;
			}

			//o.Emission = col * _Color;
			o.Albedo = col * _Color;
			o.Alpha = col.a;

			//float3 nPos = IN.vertexPos.xyz;
			//nPos.xy = nPos.xy / nPos.z;

			//float3 dx = ddx(nPos);
			//float3 dy = ddy(nPos);

			//float3 n = normalize(cross(dx, dy));
			//o.Normal = n;
		}

		ENDCG
    }

    FallBack "Diffuse"
}

