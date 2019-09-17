Shader "ColorTest/LoadColorValue"
{
	Properties
	{
		_HeatMapTex("_HeatMapTex",2D) = "white"{}
	}

		SubShader
	{
		Tags {"RenderType" = "Overlay" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest[unity_GUIZTestMode]
		ZWrite On
		//Cull off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma enable_d3d11_debug_symbols		//debug用

			struct a2v
			{
				float4 pos : POSITION;
			};

			struct v2f
			{
				float4 pos : POSITION;
				float3 worldPos : TEXCOORD1;
			};

			v2f vert(a2v input)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.worldPos = mul(unity_ObjectToWorld, input.pos).xyz;
				return o;
			}

			struct _point
			{
				float3 pos: TEXCOORD3;
				float count : TEXCOORD4;
			};

			sampler2D _HeatMapTex;
			uniform sampler1D array;
			uniform int pixel_count;

			float4 frag(v2f input) :SV_Target
			{
				float p = 1.0 / float(pixel_count - 1);
				float heat = 0;
				for (int i = 0; i < pixel_count; i++)
				{
					_point now_point;
					float tempX, tempY, tempZ;

					float4 value = tex1D(array, i*p).rgba;				//tex1D 一维纹理查询
						//return float4(value.r, value.g, value.b,1);

					tempX = floor(value.r * 10 + 0.5) * 10 + floor(value.g * 10 + 0.5) + floor(value.b * 10 + 0.5) * 0.1;
					if (floor(value.a * 10 + 0.5) == 10)
						tempX = tempX * (-1);

					i += 1;
					value = tex1D(array, i*p).rgba;
					tempY = floor(value.r * 10 + 0.5) * 10 + floor(value.g * 10 + 0.5) + floor(value.b * 10 + 0.5) * 0.1;
					if (floor(value.a * 10 + 0.5) == 10)
						tempY = tempY * (-1);

					i += 1;
					value = tex1D(array, i*p).rgba;
					tempZ = floor(value.r * 10 + 0.5) * 10 + floor(value.g * 10 + 0.5) + floor(value.b * 10 + 0.5) * 0.1;
					if (floor(value.a * 10 + 0.5) == 10)
						tempZ = tempZ * (-1);

					i += 1;
					value = tex1D(array, i*p).rgba;
					now_point.count = value.r * 1000 + value.g * 100 + value.b * 10 + value.a;

					now_point.pos = float3(tempX, tempY, tempZ);

					float dis = distance(input.worldPos, now_point.pos);

					float ra = saturate(dis / 10);

					float ratio = 1 - ra;

					heat += now_point.count * ratio;

				}
				heat = clamp(heat, 0.05, 0.95);

				float4 color = tex2D(_HeatMapTex, fixed2(heat, 0.5));

				return color;
			}

			ENDCG
		}
	}
		FallBack "Diffuse"
}