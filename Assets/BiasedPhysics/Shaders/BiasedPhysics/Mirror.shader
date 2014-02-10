Shader "BiasedPhysics/Mirror" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _EnvironmentMap ("Reflection (RGB)", CUBE) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
        #include "Utils/LatLong.cginc"
		#pragma surface surf Lambert
	    #pragma target 3.0
	    #pragma glsl

		sampler2D _MainTex;
        samplerCUBE _GlobalEnvironment;
        samplerCUBE _EnvironmentMap;

		struct Input {
			float2 uv_MainTex;
            float3 worldRefl;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Alpha = 1.0f;
            o.Emission = c.rgb * texCUBE(_GlobalEnvironment, normalize(IN.worldRefl));
            //o.Emission = c.rgb * texCUBE(_EnvironmentMap, normalize(IN.worldRefl));
		}
		ENDCG
	} 
}
