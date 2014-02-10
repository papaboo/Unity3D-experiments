Shader "Debug/ConvolutedLatlongVisualizer" {
	Properties {
		_Skybox ("LatLong (RGB)", 2D) = "white" {}
        _mipmapBias ("Mipmap bias", Float) = 0.0
	}
	SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off Fog { Mode Off }
        Blend Off
		
        Pass {
		
            CGPROGRAM
		    #pragma vertex vert
    		#pragma fragment frag
	    	#pragma fragmentoption ARB_precision_hint_fastest
		    #pragma target 3.0
		    #pragma glsl

		    #include "UnityCG.cginc"

            sampler2D _Skybox;
            float _mipmapBias;
            
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
                float theta = acos(direction.y);
                theta += theta < 0.0f ? PI : 0.0f;
                float phi = atan2(-direction.x, direction.z);
                phi += phi < 0.0f ? 2.0f * PI : 0.0;
                return float2(theta, phi);
            }

            float2 DirectionToSphericalUV(float3 direction) {
                float2 spherical = DirectionToSpherical(direction);
                return float2(spherical.y / (2.0f * PI),
                              1.0f - spherical.x / PI);
            }
            
            fixed4 frag(v2f i) : COLOR {
                float3 direction = normalize(i.texcoord);
                float2 sphereCoords = DirectionToSphericalUV(direction);
                // return fixed4(sphereCoords, 0.0, 1.0f);
                return tex2Dbias(_Skybox, float4(sphereCoords, 0.0f, _mipmapBias));
            }
            ENDCG 
	    }
    } 	

	FallBack Off
}
