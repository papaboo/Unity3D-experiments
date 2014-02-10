Shader "Planets/Clouds" {
	Properties {
		_Clouds ("Clouds (Cube intensity map)", Cube) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+500" }
        Cull Off
        ZTest Less
		LOD 200
		
		CGPROGRAM
	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf Lambert alpha vertex:vert

		samplerCUBE _Clouds;

		struct Input {
			float3 worldDirection;
            float4 screenPos;
		};

        void vert(inout appdata_full v, out Input o) {
            o.worldDirection = normalize(v.vertex.xzy);
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

		void surf (Input IN, inout SurfaceOutput o) {
            float fadeOut = IN.screenPos.z / IN.screenPos.w;
            
			float intensity = texCUBE(_Clouds, IN.worldDirection).a * fadeOut;
            intensity = smoothstep(0.0f, 1.0f, intensity);
			o.Albedo = half3(intensity);
			o.Alpha = intensity;
		}
		ENDCG
	} 

	FallBack Off
}
