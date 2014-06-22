////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

Shader "BlobOMatic/MarginFalloff" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _MarginPercentage ("Margin begin and end Percentage", Vector) = (0.0, 0.0, 0.0, 0.0) // [begin.x, begin.y, end.x, end.y]
        _BlobStrength ("Blob strength", Float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
        ZTest Always Cull Off ZWrite Off
        Fog { Mode off }      
		
        Pass {
            CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

            sampler2D _MainTex;
            float4 _MarginPercentage;
            float _BlobStrength;

            struct v2f {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord;
				return o;
			}

            float InvLerp(float val, float begin, float end) {
                return (val - begin) / (end - begin);
            }

            float2 InvLerp(float2 val, float2 begin, float2 end) {
                return float2((val.x - begin.x) / (end.x - begin.x),
                              (val.y - begin.y) / (end.y - begin.y));
            }

			half4 frag(v2f v) : COLOR {
                float shadow = tex2D(_MainTex, v.uv).a;

                // return half4(0,0,0,shadow);

                float2 normUV = abs(v.uv * 2.0f - 1.0f);

                float2 marginPos = InvLerp(normUV, _MarginPercentage.xy, _MarginPercentage.zw);
                marginPos = max(marginPos, float2(0.0f, 0.0f));
                
                // Circular falloff area
                float marginFactor = length(marginPos);
                // return half4(0,0,0,marginFactor); // Debug
                
                // shadow = min(1.0f - marginFactor, shadow); // Only apply falloff when falloff is less than shadow. Is too noticable.
                shadow = shadow * (1.0f - marginFactor); // Apply falloff linearly.

                return half4(0,0,0,shadow * _BlobStrength);
            }
            ENDCG
        }
	} 
	Fallback off
}
