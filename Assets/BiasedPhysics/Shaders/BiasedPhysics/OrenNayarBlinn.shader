Shader "BiasedPhysics/OrenNayarBlinn" {
	Properties {
        _Color ("Base Color", Color) = (1,1,1,1)
        _Roughness ("Roughness of the base", Range (0.0, 1.0)) = 0.3
        _SpecColor ("Clear Coat Color", Color) = (1,1,1,1)
        _Shininess ("Specularity of the clear coat", Range (0.0, 1)) = 0.3
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _OrenNayarRhoMap ("OrenNayar Rhomap (Hemispherical-Bidirectional distribution map)", 2D) = "white" {}
        _BlinnRhoMap ("Blinn Rhomap (Hemispherical-Bidirectional distribution map)", 2D) = "white" {}
        _UE4_Fresnel_bias ("UE4 fresnel bias", Float) = 0.06
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 400

		CGPROGRAM
        #define UE4_FRESNEL

        #include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/OrenNayarBlinn.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_OrenNayarBlinn vertex:vert

        fixed4 _Color;
        half _Shininess; // Called shininess for compatibility with Unity shaders.
		sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _OrenNayarRhoMap;
        sampler2D _BlinnRhoMap;

		struct Input {
			float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldViewDir;
            float3 worldNormal;
            INTERNAL_DATA
		};

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.worldViewDir = WorldSpaceViewDir(v.vertex);
        }

		void surf(Input IN, inout SurfaceOutput surface) {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            surface.Albedo = tex.rgb * _Color.rgb;
            surface.Gloss = tex.a;
            surface.Alpha = 1.0f;
            surface.Specular = sqrt(_Shininess);

            // Apply IBL
            surface.Normal = normalize(UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_OrenNayarBlinn_IBL(surface, normalize(IN.worldViewDir), bumpedWorlNormal, _OrenNayarRhoMap, _BlinnRhoMap);
		}

		ENDCG
	} 
	FallBack "Bumped Diffuse"
}
