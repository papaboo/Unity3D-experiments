Shader "BiasedPhysics/Debug/BlinnVisualizer" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Shininess ("Shininess", Float) = 1
        _ViewDir ("View direction", Vector) = (0, 1, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
        Cull off
		LOD 200
		
		CGPROGRAM
        #include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/DistributionSamplers.cginc"

		#pragma surface surf Lambert vertex:vert

		sampler2D _MainTex;
        float _Shininess;

		struct Input {
            float2 texcoord;
		};

        void vert(inout appdata_full v, out Input o) {
            DistributionSample distSample = PowerCosineDistribution_Sample(v.texcoord, _Shininess);
            v.vertex.xyz = distSample.Direction;// / distSample.PDF;
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.texcoord = v.texcoord.xy;
        }

		void surf(Input IN, inout SurfaceOutput o) {
            //o.Emission = float3(IN.texcoord, 0.0f);
            int2 checkers = IN.texcoord * 64;
            int checkerIndex = checkers.x * checkers.y;
            if (((checkers.x % 2) == 1 && (checkers.y % 2) == 0) ||
                ((checkers.x % 2) == 0 && (checkers.y % 2) == 1))
                o.Emission = float3(1,1,1);
            else
                o.Emission = float3(0,0,0);
		}

		ENDCG
	} 

	FallBack Off
}
