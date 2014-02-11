Shader "BiasedPhysics/OrenNayarBlinn" {
	Properties {
        _Color ("Base Color", Color) = (1,1,1,1)
        _Roughness ("Roughness of the base", Range (0.0, 1.0)) = 0.3
        _SpecColor ("Clear Coat Color", Color) = (1,1,1,1)
        _Shininess ("Shininess of the clear coat (1.0 - roughness)", Range (0.03, 1)) = 0.078125
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _UE4_Fresnel_bias ("UE4 fresnel bias", Float) = 0.06
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 400

		CGPROGRAM
        #define UE4_FRESNEL

        #include "BiasedPhysicsLighting.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_OrenNayarBlinn

        fixed4 _Color;
		sampler2D _MainTex;
        sampler2D _BumpMap;
        half _Shininess;

		struct Input {
			float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldRefl;
            float3 worldNormal;
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
            surface.Emission = BiasedPhysics_OrenNayarBlinn_IBL(surface, bumpedWorlNormal, normalize(bumpedWorlReflection));

            /* surface.Normal = normalize(float3(0,0,3) + UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0) */
            /* float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members */
            /* surface.Emission = BiasedPhysics_OrenNayar_IBL(surface, normalize(bumpedWorlNormal)); // Useless normalize? */
		}

		ENDCG
	} 
	FallBack "Bumped Diffuse"
}
