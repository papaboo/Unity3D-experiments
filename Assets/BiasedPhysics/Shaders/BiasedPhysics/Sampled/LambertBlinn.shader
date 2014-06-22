Shader "BiasedPhysics/Sampled/LambertBlinn" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _SpecColor ("Clear Coat Color", Color) = (1,1,1,1)
        _Shininess ("Specularity", Range (0.0, 1.0)) = 0.3
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _Fresnel_bias_scale_exponent ("Schlick fresnel bias, scale and exponent", Vector) = (0.06, 0.94, 5, 0)
        _SamplesDrawn ("Samples drawn", Float) = 64
        _EncodedSamples ("Encoded random samples (must contain a multiple of 2048 samples)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 500

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
        half _SamplesDrawn;
        sampler2D _EncodedSamples;

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
            surface.Alpha = 1.0f; //tex.a * _Color.a;
            surface.Gloss = tex.a;
            surface.Specular = _Shininess;

            // Apply IBL
            surface.Normal = normalize(float3(0,0,3) + UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_LambertBlinn_SampledIBL(surface, normalize(IN.worldViewDir), normalize(bumpedWorlNormal), _SamplesDrawn, _EncodedSamples, 1.0f / 2048);
		}

		ENDCG
	} 

	FallBack "BiasedPhysics/Blinn"
}