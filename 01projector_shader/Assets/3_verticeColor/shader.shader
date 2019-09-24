// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TexturedColor2"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
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

			uniform sampler2D _MainTex;

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
				float4 outcolor = input.color;
				float4 lMainTex = tex2D(_MainTex, input.tex);
				outcolor = outcolor * lMainTex;
				return outcolor;
			}

			ENDCG
		}
	}
		FallBack "Diffuse"
}