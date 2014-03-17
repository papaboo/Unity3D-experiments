//==============================================================================
// Blinn Rho computation.
// Approximates the Blinn Directional-Hemispherical-distribution function by 
// sampling the hemisphere.
//==============================================================================
Shader "BiasedPhysics/Rho/Blinn" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
        _SamplesDrawn ("Samples drawn", Float) = 64
        _EncodedSamples ("Encoded random samples (must contain a multiple of 2048 samples)", 2D) = "white" {}
	}
	
    Subshader {
        Pass {
	        ZTest Always Cull Off ZWrite Off
            Fog { Mode off }      

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
		    #pragma target 3.0
		    #pragma glsl

            #include "UnityCG.cginc"
            #include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Blinn.cginc"
                
            sampler2D _MainTex;
            half _SamplesDrawn;
            sampler2D _EncodedSamples;

            struct v2f {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };
	
            v2f vert(appdata_img v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }
            
            half4 frag(v2f i) : COLOR  {
                // Texcoord u is the angle between the normal (0,1,0) and incoming
                // direction from which we can compute the incoming direction. 
                // (Remember that Blinn is isotropic)
                // The view direction vector (x, u, z) must satisfy that 
                // x * 0 + y * 0 + u * 1 = 1 and x*x + u*u + z*z = 1
                // Since Blinn is isotropic we are free to choose x and z as we
                // please and can therefore set z to 0, so the view dir is (x,u,0), 
                // where x*x + u*u = 1
                
                float x = sqrt(1.0f - i.uv.x * i.uv.x);
                float3 viewDir = float3(x, i.uv.x, 0);
                
                float3 normal = float3(0,1,0);
                
                // Texcoord v is the specularity
                float specularity = i.uv.y;

                return fixed4(viewDir, 1.0f);

                //return fixed4(0,1,0,1);
            }
        
            ENDCG
        }
    }

    Fallback off
}
