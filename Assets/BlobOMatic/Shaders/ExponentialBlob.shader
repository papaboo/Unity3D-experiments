////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

Shader "BlobOMatic/ExponentialBlob" {
	Properties {
        _BlobFalloffExponent ("Exponent of shadow strength", Float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" "IgnoreProjector"="True" }
		LOD 200
		
        Pass {
            CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

            float _BlobFalloffExponent;

            struct appdata_t {
				float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : POSITION;
                float4 position : TEXCOORD;
            };

            v2f vert(appdata_t v) {
				v2f o;
				o.position = o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			half4 frag(v2f v) : COLOR {
#ifdef SHADER_API_D3D9
                float normZ = 1.0f - v.position.z / v.position.w;
#else
                float normZ = 1.0f - (v.position.z / v.position.w * 0.5 + 0.5);
#endif
                return half4(0,0,0, pow(normZ, _BlobFalloffExponent));
            }
            ENDCG
        }
	} 
}
