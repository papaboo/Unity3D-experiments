Shader "BiasedPhysics/Blinn" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _BumpMap ("Normalmap", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 500
		
		CGPROGRAM
        #include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Blinn.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_Blinn

		sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        half _Shininess;

		struct Input {
			float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldNormal;
            float3 worldRefl;
            INTERNAL_DATA
		};

		void surf(Input IN, inout SurfaceOutput surface) {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            surface.Albedo = tex.rgb * _Color.rgb;
            surface.Gloss = tex.a;
            surface.Alpha = 1.0f;
            surface.Specular = _Shininess;

            // Apply IBL
            surface.Normal = normalize(UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlReflection = WorldReflectionVector(IN, surface.Normal); // Requires worldRefl and INTERNAL_DATA as Input members
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_Blinn_IBL(surface, normalize(bumpedWorlNormal), normalize(bumpedWorlReflection)); // Useless normalize?
		}

		ENDCG
	} 
	FallBack "Bumped Specular"
}
