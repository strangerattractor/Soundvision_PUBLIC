   Shader "Vertex Modifier" {
     Properties {
       _MainTex ("Texture", 2D) = "depth" {}
       _Amount ("Height Adjustment", Float) = 1.0
     }
     SubShader {
       Tags { "RenderType" = "Opaque" }
       CGPROGRAM
       #pragma surface surf Lambert vertex:vert
       struct Input {
           float2 uv_MainTex;
       };
 
       // Access the shaderlab properties
       float _Amount;
       sampler2D _MainTex;
 
       // Vertex modifier function
       void vert (inout appdata_full v){
           v.vertex.z += _Amount * tex2Dlod(_MainTex, v.texcoord).r;
       }
 
       // Surface shader function
       void surf (Input IN, inout SurfaceOutput o) {
           o.Albedo = float4(1.0f, 1.0f, 1.0f, 1.0f);
       }
       ENDCG
     }
   }
