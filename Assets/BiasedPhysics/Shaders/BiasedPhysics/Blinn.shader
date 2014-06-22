Shader "BiasedPhysics/Blinn" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _Shininess ("Specularity", Range (0.0, 1)) = 0.3
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _RhoMap ("Rhomap (Hemispherical-Bidirectional distribution map)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 500
		
		CGPROGRAM
        #include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Blinn.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_Blinn vertex:vert

        fixed4 _Color;
		sampler2D _MainTex;
        half _Shininess; // Called shininess for compatibility with Unity shaders.
        sampler2D _BumpMap;
        sampler2D _RhoMap;

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
            surface.Specular = sqrt(sqrt(_Shininess));

            // Apply IBL
            surface.Normal = normalize(UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_Blinn_IBL(surface, normalize(IN.worldViewDir), bumpedWorlNormal, _RhoMap);
		}

		ENDCG
	} 
	FallBack "Bumped Specular"
}
