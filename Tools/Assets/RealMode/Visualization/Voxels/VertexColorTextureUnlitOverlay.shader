Shader "Custom/VertexColorTextureUnlitOverlay" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags {"Queue"="Overlay"}
        LOD 300
        ZTest Off
        ZWrite Off
        CGPROGRAM

        #pragma surface surf NoLighting alpha fullforwardshadows
        #pragma target 3.0

        struct Input {
            float2 uv_MainTex;
            float4 color: Color;
        }; 

        sampler2D _MainTex;
            
        void surf (Input IN, inout SurfaceOutput o) {
            float4 tex = tex2D(_MainTex, IN.uv_MainTex).rgba;
            o.Albedo = tex * IN.color;
            o.Alpha = tex.w;
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
}

