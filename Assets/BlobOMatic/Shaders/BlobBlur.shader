////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

Shader "BlobOMatic/BlobBlur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _Factor ("BlurFactor", Float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
        ZTest Always Cull Off ZWrite Off
        Fog { Mode off }
		
        Pass {
            CGPROGRAM
			#include "UnityCG.cginc"
    		#pragma target 3.0
	    	#pragma glsl
			#pragma vertex vert
			#pragma fragment frag

            sampler2D _MainTex;
            float _Factor;
            
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

			half4 frag(v2f v) : COLOR {
                float shadowStr = tex2D(_MainTex, v.uv).a;
                float mipmap = (1.0f - shadowStr) * _Factor;
                float shadow = tex2Dbias(_MainTex, float4(v.uv, 0.0f, mipmap)).a;
                return half4(0,0,0, shadow);
            }
            ENDCG
        }
	} 
	Fallback off
}
