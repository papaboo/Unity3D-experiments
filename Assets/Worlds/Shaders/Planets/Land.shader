Shader "Planets/Land" {
	Properties {
		_LandCube ("Land (Cube heightmap)", Cube) = "white" {}
        _InvLandDetail ("Inverse Land detail (1.0f / width of a face)", float) = 96
        _MinMaxHeight_SandPercentage ("Min height, Max height, Sand %", Vector) = (7, 15, 0.55,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
	    #pragma target 3.0
	    #pragma glsl
        #pragma surface surf Lambert vertex:vert addshadow

        samplerCUBE _LandCube;
        float _InvLandDetail;
        float4 _MinMaxHeight_SandPercentage;

		struct Input {
			float3 direction;
            float normHeight;
		};

        float3 GetPosition(float3 direction) {
            float hLerp = texCUBE(_LandCube, direction).r;
            float minHeight = _MinMaxHeight_SandPercentage.x;
            float maxHeight = _MinMaxHeight_SandPercentage.y;
            float height = lerp(minHeight, maxHeight, hLerp);
            return direction * height;
        }

        // Expensive, but easy for development. Precalculate at some point.
        // TODO Use the additional heights to calculate a slight ao term as well
        float3 GetNormal(float3 direction) {
            float3 x1, x2, y1, y2;
            if (abs(direction.x) > abs(direction.y) && abs(direction.x) > abs(direction.z)) {
                // x is largest
                x1 = normalize(direction + float3(0.0f, _InvLandDetail, 0.0f));
                x2 = normalize(direction - float3(0.0f, _InvLandDetail, 0.0f));

                y1 = normalize(direction + float3(0.0f, 0.0f, _InvLandDetail));
                y2 = normalize(direction - float3(0.0f, 0.0f, _InvLandDetail));

            } else if (abs(direction.y) > abs(direction.z)) {
                // y is largest
                x1 = normalize(direction + float3(_InvLandDetail, 0.0f, 0.0f));
                x2 = normalize(direction - float3(_InvLandDetail, 0.0f, 0.0f));

                y1 = normalize(direction + float3(0.0f, 0.0f, _InvLandDetail));
                y2 = normalize(direction - float3(0.0f, 0.0f, _InvLandDetail));

            } else {
                // z is largest
                x1 = normalize(direction + float3(_InvLandDetail, 0.0f, 0.0f));
                x2 = normalize(direction - float3(_InvLandDetail, 0.0f, 0.0f));

                y1 = normalize(direction + float3(0.0f, _InvLandDetail, 0.0f));
                y2 = normalize(direction - float3(0.0f, _InvLandDetail, 0.0f));
            }

            float3 dx = GetPosition(x1) - GetPosition(x2);
            float3 dy = GetPosition(y1) - GetPosition(y2);
            float3 normal = normalize(cross(dx, dy));
            // Make sure normal always points outwards
            normal = dot(normal, direction) >= 0.0f ? normal : -normal;

            normal = normalize(normal + direction);
            
            return normal;
        }

        void vert(inout appdata_full v, out Input o) {
            o.direction = v.vertex.xyz;
            o.normHeight = texCUBE(_LandCube, o.direction).r;
            float minHeight = _MinMaxHeight_SandPercentage.x;
            float maxHeight = _MinMaxHeight_SandPercentage.y;
            float height = lerp(minHeight, maxHeight, o.normHeight);
            // v.vertex.w = 1.0f / height; // Removes shadows for some reason /o\
            v.vertex = float4(o.direction * height, 1.0f);
            v.normal = GetNormal(o.direction);
            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

        // Grass colors: http://www.colourlovers.com/palette/264688/Grass_Green
		void surf(Input IN, inout SurfaceOutput surface) {
            // surface.Emission = GetNormal(IN.direction) * 0.5f + 0.5f;
            
			surface.Alpha = 1.0f;
            // sand fades to grass between 0.45 and 0.55
            float sandFactor = saturate((IN.normHeight - _MinMaxHeight_SandPercentage.z) * 10.0f);
            float3 sandColor = float3(237, 201, 175) / 255.0f;
            float3 grassColor = float3(1, 142, 14) / 255.0f;
            surface.Albedo = lerp(sandColor, grassColor, sandFactor);
		}
		ENDCG
	} 

	FallBack Off
}
