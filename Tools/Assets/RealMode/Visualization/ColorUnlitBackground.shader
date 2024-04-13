Shader "Custom/ColorUnlitBackground" {
    Properties {
        _Color("Color", Color) = (.2, .2, .2, 1)
    }

    SubShader {
        Tags { "Queue"="Background" }
		Lighting Off
		Cull Off
		Zwrite Off
		Ztest Less

        CGPROGRAM

        #pragma surface surf NoLighting
        #pragma target 3.0

        struct Input {
            float2 uv_MainTex;
            float4 color: Color;
        }; 

        float4 _Color;
            
        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = _Color;
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
