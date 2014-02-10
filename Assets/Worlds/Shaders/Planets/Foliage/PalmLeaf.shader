Shader "Planets/Foliage/PalmLeaf" {
	Properties {
		_LeafColor ("Leaf Color", Color) = (0.137, 0.263, 0.067, 1)
        _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
        Cull Off
		LOD 200
		
		CGPROGRAM
	    #pragma target 3.0
	    #pragma glsl
        #include "LeafLighting.cginc"
		#pragma surface surf Leaf

        fixed4 _LeafColor;
        half _Shininess;

		struct Input {
            float uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = _LeafColor.rgb;
			o.Alpha = 1.0f;
            o.Specular = _Shininess;
            o.Gloss = 1.0f;
		}
		ENDCG
	} 

	FallBack "Diffuse"
}
