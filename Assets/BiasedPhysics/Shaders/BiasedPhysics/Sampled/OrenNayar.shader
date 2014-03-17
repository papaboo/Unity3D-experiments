Shader "BiasedPhysics/Sampled/OrenNayar" {
	Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _Roughness ("Roughness", Range (0.0, 1.0)) = 0.3
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _EncodedSamples ("Encoded random samples", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 400

		CGPROGRAM
        #include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/OrenNayar.cginc"

	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf BiasedPhysics_OrenNayar vertex:vert

        fixed4 _Color;
		sampler2D _MainTex;
        sampler2D _BumpMap;
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
            surface.Alpha = 1.0f;

            // Apply IBL
            surface.Normal = normalize(float3(0,0,3) + UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap))); // Unpacks to tangent space (so basically N*2.0-1.0)
            float3 bumpedWorlNormal = WorldNormalVector(IN, surface.Normal); // Requires worldNormal and INTERNAL_DATA as Input members
            surface.Emission = BiasedPhysics_OrenNayar_SampledIBL(surface, normalize(IN.worldViewDir), normalize(bumpedWorlNormal), _EncodedSamples, 256);
		}

		ENDCG
	} 
	FallBack "BiasedPhysics/OrenNayar"
}
