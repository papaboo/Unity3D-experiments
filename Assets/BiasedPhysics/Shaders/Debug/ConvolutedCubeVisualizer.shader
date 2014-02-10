﻿Shader "Debug/ConvolutedCubeVisualizer" {
	Properties {
        _Tex ("Cubemap", Cube) = "white" {}
        _mipmapBias ("Mipmap bias", Float) = 0.0
	}
    
    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off Fog { Mode Off }

        Pass {
		
            CGPROGRAM
		    #pragma vertex vert
		    #pragma fragment frag
		    #pragma fragmentoption ARB_precision_hint_fastest
		    #pragma target 3.0
		    #pragma glsl

		    #include "UnityCG.cginc"

            samplerCUBE _Tex;
            float _mipmapBias;
            
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
            
            fixed4 frag (v2f i) : COLOR {
                return texCUBEbias(_Tex, float4(i.texcoord, _mipmapBias));
            }
            
            ENDCG 
	    }
    }

	FallBack Off
}
