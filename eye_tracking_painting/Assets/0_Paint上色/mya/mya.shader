Shader "mya/terrainTextrueBlend" {

	Properties{
		_Control("Control (RGBA)", 2D) = "white" {}
	}

	SubShader{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}

		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 4.0

		struct Input
		{
			float2 uv_Control : TEXCOORD0;
		};

		sampler2D _Control;

		void surf(Input IN, inout SurfaceOutput o) {
			half4 splat_control = tex2D(_Control, IN.uv_Control).rgba;

			o.Alpha = 0.0;
			o.Albedo.rgb = splat_control;
		}
		ENDCG
	}
		FallBack "Specular"
}