﻿

Shader "UChart/Wireframe"
{
	Properties
	{
		_BottomColor("Bottom Color",COLOR) = (.4,.4,.4,.4)
		_TopColor("Top Color",COLOR) = (.8,.8,.8,.8)

		_NormalColor ("Normal Color",COLOR) = (1,1,1,1)
		_WireframeColor("Wireframe Color",COLOR) = (1,0,0,0)


		_WireframeWidth ("Wireframe Width",float) = 0.05
		_WireframeBorder ("Wireframe Border",FLOAT) = 0.1
		_WireframeBorderColor("Wireframe Border Color",COLOR) = (0,0,0,0)
	}

	SubShader
	{
		Tags{"RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _NormalColor;
			float4 _WireframeColor;
			float _WireframeWidth ;
			float _WireframeBorder;
			float4 _WireframeBorderColor;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			v2f vert(a2v IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;
				OUT.normal = IN.normal;
				return OUT;
			}

			float antialias( float minValue,float maxValue,float dis )
			{
				return smoothstep(minValue,maxValue,dis);
			}

			fixed4 frag( v2f IN ) : COLOR
			{
				fixed4 color = _NormalColor;
				float minUV = _WireframeWidth;
				float maxUV = 1 - _WireframeWidth; 
				if( IN.uv.x >= 0 && IN.uv.x < minUV )
				{
					color = _WireframeColor;
				}
				else if(IN.uv.x > maxUV && IN.uv.x < 1)
				{
					color = _WireframeColor;
				}
				else if(IN.uv.y >= 0 && IN.uv.y < minUV)
				{
					color = _WireframeColor;
				}
				else if(IN.uv.y > maxUV && IN.uv.y < 1)
				{
					color = _WireframeColor;
				}
				else
				{

				}
				return color;
			}

			ENDCG
		}
	}
}