Shader "BiasedPhysics/LambertBlinn" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _SpecColor ("Clear Coat Color", Color) = (1,1,1,1)
        _Shininess ("Shininess (1.0 - roughness)", Range (0.03, 1)) = 0.078125
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _EnvironmentMap ("Convoluted IBL (RGB)", 2D) = "white" {}
        _EnvironmentMapMipmapCount ("Convoluted IBL mipmap count", Float) = 1
        _Fresnel_bias_scale_exponent ("Schlick fresnel bias, scale and exponent", Vector) = (0.06, 0.94, 5, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 500
		
		CGPROGRAM
        #define SCHLICK_FRESNEL

        #include "BiasedPhysicsLighting.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_LambertBlinn

		sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        half _Shininess;

		struct Input {
			float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 viewDir; // View space
            float3 worldRefl;
            float3 worldNormal;
            INTERNAL_DATA
		};

		void surf(Input IN, inout SurfaceOutput surface) {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            surface.Albedo = tex.rgb * _Color.rgb;
            surface.Gloss = tex.a;
            surface.Alpha = 1.0f; //tex.a * _Color.a;
            surface.Specular = _Shininess;

            // Apply IBL
            surface.Normal = normalize(UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlReflection = WorldReflectionVector(IN, surface.Normal); // Requires worldRefl and INTERNAL_DATA as Input members
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_LambertBlinn_IBL(surface, bumpedWorlNormal, normalize(bumpedWorlReflection));
		}
		ENDCG
	} 
	FallBack "Bumped Specular"
}
