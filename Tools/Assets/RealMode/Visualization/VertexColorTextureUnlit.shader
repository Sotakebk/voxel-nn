Shader "Custom/VertexColorTextureUnlit" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf NoLighting
        #pragma target 3.0

        struct Input {
            float2 uv_MainTex;
            float4 color: Color;
        }; 

        sampler2D _MainTex;
            
        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * IN.color;
        }

        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            fixed4 c;
            c.rgb = s.Albedo; 
            c.a = s.Alpha;
            return c;
        }

        ENDCG
    }
    Fallback "Diffuse"
}
