Shader "BiasedPhysics/Sampled/Lambert" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _SamplesDrawn ("Samples drawn", Float) = 64
        _EncodedSamples ("Encoded random samples", 2D) = "gray" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 500
		
		CGPROGRAM
        #include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Lambert.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_Lambert

		sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        half _SamplesDrawn;
        sampler2D _EncodedSamples;

		struct Input {
			float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldNormal;
            INTERNAL_DATA
		};

		void surf(Input IN, inout SurfaceOutput surface) {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            surface.Albedo = tex.rgb * _Color.rgb;
            surface.Alpha = 1.0f;

            // Apply IBL
            surface.Normal = normalize(float3(0,0,3) + UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_Lambert_SampledIBL(surface, normalize(bumpedWorlNormal), _SamplesDrawn, _EncodedSamples, 1.0f / 2048.0f);
		}
		ENDCG
	} 

	FallBack "BiasedPhysics/Lambert"
}
