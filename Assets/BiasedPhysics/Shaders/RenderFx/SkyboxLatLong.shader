Shader "RenderFX/Skybox LatLong" {
	Properties {
		_Skybox ("LatLong (RGB)", 2D) = "white" {}
	}
	SubShader {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        Cull Off ZWrite Off Fog { Mode Off }
        Blend Off
		
        Pass {
		
            // Produces an edge between the poles along the seam for some reason, use the cubed version instead

            CGPROGRAM
		    #pragma vertex vert
    		#pragma fragment frag
                // #pragma fragmentoption ARB_precision_hint_fastest

		    #include "UnityCG.cginc"

            sampler2D _Skybox;
            
            #define PI 3.14159265359f
            
            struct appdata_t {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };
            
            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            float2 DirectionToSpherical(float3 direction) {
                // theta should be in the range [0; PI]
                float theta = acos(direction.y);
                theta += theta < 0.0f ? PI : 0.0f;
                
                // phi should be in the range [0; 2*PI]
                float phi = atan2(-direction.x, direction.z);
                phi += phi < 0.0f ? 2.0f * PI : 0.0f;
                return float2(theta, phi);
            }

            float2 DirectionToSphericalUV(float3 direction) {
                float2 spherical = DirectionToSpherical(direction);
                return float2(spherical.y / (2.0f * PI),
                              1.0f - spherical.x / PI);
            }
            
            fixed4 frag(v2f i) : COLOR {
                float3 direction = normalize(i.texcoord);
                return fixed4(direction, 1.0f);
                float2 sphereCoords = DirectionToSphericalUV(direction);
                return tex2D(_Skybox, sphereCoords);
            }
            ENDCG 
	    }
    } 	

	FallBack Off
}
