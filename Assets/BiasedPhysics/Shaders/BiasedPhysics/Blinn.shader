﻿Shader "BiasedPhysics/FunkyBlinn" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _Fresnel_bias_scale_exponent ("Schlick fresnel bias, scale and exponent", Vector) = (0.06, 0.94, 5, 0)
        _BumpMap ("Normalmap", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 500
		
		CGPROGRAM
        #define SCHLICK_FRESNEL

        #include "BiasedPhysicsLighting.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_FunkyBlinn vertex:vert

		sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        half _Shininess;

		struct Input {
			float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldViewDir;
            float3 worldNormal;
            float3 worldRefl;
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
            surface.Alpha = 1.0f; //tex.a * _Color.a;
            surface.Specular = _Shininess;

            // Apply IBL
            surface.Normal = normalize(float3(0,0,3) + UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlReflection = WorldReflectionVector(IN, surface.Normal); // Requires worldRefl and INTERNAL_DATA as Input members
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_FunkyBlinn_IBL(surface, normalize(bumpedWorlNormal), normalize(bumpedWorlReflection)); // Useless normalize?
		}

		ENDCG
	} 
	FallBack "Bumped Specular"
}
