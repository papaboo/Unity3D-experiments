﻿Shader "BiasedPhysics/OrenNayar" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _Roughness ("Roughness", Range (0.0, 1.0)) = 0.3
        _BumpMap ("Normalmap", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 400

		CGPROGRAM
        #include "BiasedPhysicsLighting.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_OrenNayar vertex:vert

        fixed4 _Color;
		sampler2D _MainTex;
        sampler2D _BumpMap;

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
            surface.Alpha = 1.0f;

            // Apply IBL
            surface.Normal = normalize(float3(0,0,3) + UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_OrenNayar_IBL(surface, normalize(bumpedWorlNormal)); // Useless normalize?
		}

		ENDCG
	} 
	FallBack "Bumped Diffuse"
}
