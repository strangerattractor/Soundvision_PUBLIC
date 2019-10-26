Shader "Kinect/BackgroundRenderShader" {

     Properties{
          _Color ("Main Color", Color) = (1,1,1,1)
          _MainTex("Color (RGB)", 2D) = "white" {} 
          _AlphaTex("Alpha (RGB)", 2D) = "white" {} 
        
     }

     SubShader{
          Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

          CGPROGRAM
          #pragma surface surf NoLighting alpha
          fixed4 _Color;

          fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
               fixed4 c;
               //c.rgb = s.Albedo;
               c.rgb = s.Albedo*0.5f;
               c.a = s.Alpha;
               return c;
          }

          struct Input {
               float2 uv_MainTex;
               float2 uv_AlphaTex;
          };

          sampler2D _MainTex;
          sampler2D _AlphaTex;
          
          void surf(Input IN, inout SurfaceOutput o) {
               o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;
               o.Alpha = tex2D(_AlphaTex, IN.uv_AlphaTex).a;
          }

          ENDCG
     }

} 