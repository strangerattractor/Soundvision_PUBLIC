   Shader "Vertex Modifier" {
     Properties {
       _MainTex ("Texture", 2D) = "depth" {}
       _Scale ("Scale", Float) = 1.0
     }
     SubShader {
       Tags { "RenderType" = "Opaque" }
       
       CGPROGRAM
       #pragma surface surf Lambert vertex:vert
       struct Input {
           float2 uv_MainTex;
       };
 
       float _Scale;
       sampler2D _MainTex;
 
       void vert (inout appdata_full v){
           v.vertex.z += _Scale * tex2Dlod(_MainTex, v.texcoord).r;
       }
 
       void surf (Input IN, inout SurfaceOutput o) {
           o.Albedo = float4(1.0f, 1.0f, 1.0f, 1.0f);
       }
       ENDCG
     }
   }
