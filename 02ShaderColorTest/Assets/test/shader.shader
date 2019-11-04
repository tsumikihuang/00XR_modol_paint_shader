// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TexturedColor2"
{
	Properties{
		_HeatMapTex("_HeatMapTex",2D) = "white"{}
	}

		SubShader
	{

		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _HeatMapTex;

			struct vertexInput
			{
				 float4 vertex   : POSITION;  // The vertex position in model space.
				 float3 normal   : NORMAL;    // The vertex normal in model space.
				 float4 color     : COLOR;     // Per-vertex color
				 float4 texcoord : TEXCOORD0; // The first UV coordinate.
			};

			struct vertexOutput
			{
				float4 pos      : SV_POSITION;
				float4 color    : COLOR;
				float2 tex      : TEXCOORD0;
				float3 normal   : TEXCOORD1;
			};

			vertexOutput vert(vertexInput v)
			{
				vertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.tex = v.texcoord;
				o.normal = v.normal;
				return o;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				float heat = clamp(fixed2(input.color.r, 0.5), 0.05, 0.95);
				float3 color = tex2D(_HeatMapTex, heat);
				return fixed4(color, 1.0);
			}

			ENDCG
		}
	}
		FallBack "Diffuse"
}