Shader "BiasedPhysics/LambertBlinn" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _SpecColor ("Clear Coat Color", Color) = (1,1,1,1)
        _Shininess ("Specularity", Range (0.0, 1)) = 0.3
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _BlinnRhoMap ("Blinn Rhomap (Hemispherical-Bidirectional distribution map)", 2D) = "white" {}
        _Fresnel_bias_scale_exponent ("Schlick fresnel bias, scale and exponent", Vector) = (0.06, 0.94, 5, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 400
		
		CGPROGRAM
        #define SCHLICK_FRESNEL

        #include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/LambertBlinn.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_LambertBlinn vertex:vert

        fixed4 _Color;
        half _Shininess; // Called shininess for compatibility with Unity shaders.
		sampler2D _MainTex;
        sampler2D _BumpMap;
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
            surface.Alpha = 1.0f; //tex.a * _Color.a;
            surface.Specular = _Shininess;

            // Apply IBL
            surface.Normal = normalize(UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_LambertBlinn_IBL(surface, normalize(IN.worldViewDir), bumpedWorlNormal, _BlinnRhoMap);
		}
		ENDCG
	} 
	FallBack "Bumped Specular"
}
