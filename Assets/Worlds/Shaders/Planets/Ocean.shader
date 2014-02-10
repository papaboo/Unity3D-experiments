Shader "Planets/Ocean" {
	Properties {
		_LandCube ("Land (Cube heightmap)", Cube) = "white" {}
        _MinMaxHeight_OceanHeight ("Min height, Max height, Water height", Vector) = (7, 15, 0,0)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Geometry+449" }
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha
		LOD 200

            // ZPass. Needs to reflect vertex animation
        /* Pass { */
        /*     ColorMask 0 */
        /*     ZWrite On */
        /* } */

		CGPROGRAM
	    #pragma target 3.0
	    #pragma glsl
		#pragma surface surf Lambert vertex:vert

        samplerCUBE _LandCube;
        float4 _MinMaxHeight_OceanHeight;

		struct Input {
            float3 viewDir;
            float3 worldDirection;
		};

        void vert(inout appdata_full v, out Input o) {
            o.worldDirection = normalize(v.vertex.xyz);
            
            // Vertex wave offset
            /* float3 waveStrs = float3(sin(_Time.y + v.vertex.x * 3.0f), */
            /*                          sin(_Time.y + v.vertex.y * 3.0f), */
            /*                          sin(_Time.y + v.vertex.z * 3.0f)); */
            /* float waveStr = 0.5f * dot(waveStrs, o.worldDirection); */
            /* float oceanHeight = 1.0f - saturate(texCUBE(_LandCube, normalize(o.worldDirection)).r * 4.0f); */
            /* v.vertex.xyz += o.worldDirection * waveStr * oceanHeight; */
            
            /* // Normal */
            /* float3 dWaveStrs = float3(cos(_Time.y + v.vertex.x * 3.0f), */
            /*                           cos(_Time.y + v.vertex.y * 3.0f), */
            /*                           cos(_Time.y + v.vertex.z * 3.0f)) * o.worldDirection; */
            
            /* v.normal = normalize(v.normal + 0.2f * dWaveStrs); */
            
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

        float invLerp(float min, float max, float val) {
            return (min - val) / (min - max);
        }

        float GetHeight(float3 direction) {
            float hLerp = texCUBE(_LandCube, normalize(direction)).r;
            return hLerp;
            float minHeight = _MinMaxHeight_OceanHeight.x;
            float maxHeight = _MinMaxHeight_OceanHeight.y;
            return lerp(minHeight, maxHeight, hLerp);
        }

        // Compute highlight and skybox contribution first, then in post compute
        // the alpha based on highlight intensity and depth.

		void surf(Input IN, inout SurfaceOutput surface) {
            fixed3 waterColor = fixed3(57, 88, 121) / 255.0f;
			surface.Albedo = waterColor;

            float NdotV = dot(normalize(IN.viewDir), normalize(surface.Normal));
            float fresnel = lerp(1.0f, 0.06f, NdotV);
            //float heightLerp = 1.0f - saturate(texCUBE(_LandCube, normalize(IN.worldDirection)).r * 2.0f);
            //surface.Alpha = lerp(fresnel, 1.0f, heightLerp);
            surface.Alpha = lerp(1.2f, 0.8f, NdotV);
		}

		ENDCG
	} 

	FallBack Off
}
