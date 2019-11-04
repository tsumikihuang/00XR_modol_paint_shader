Shader "UChart/HeatMap/LoadColor"
{
	Properties
	{
		_Diffuse("Diffuse",Color) = (1,1,1,1)
		_HeatMapTex("_HeatMapTex",2D) = "white"{}
		_Alpha("Alpha",range(0,1)) = 1
	}

		SubShader
		{
			Tags {"RenderType" = "Overlay" "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			
			ZTest[unity_GUIZTestMode]
			ZWrite On
			
			//Cull off		//開啟即有雙面

			Pass
			{
				Tags{ "LightMode" = "ForwardBase" }

				CGPROGRAM
				#include "Lighting.cginc"
				#include "UnityCG.cginc"
				#pragma vertex vert
				#pragma require geometry
				#pragma geometry geom
				#pragma fragment frag

				//#pragma target 4.0
				#pragma enable_d3d11_debug_symbols		//debug用!!!
				

				fixed4 _Diffuse;
				sampler2D _HeatMapTex;
				half _Alpha;

				uniform float _Radius;
				//uniform float _MaxCount;

				uniform int pixel_count;
				uniform sampler1D array;

				struct a2v
				{
					float4 pos : POSITION;
					float3 normal : NORMAL;
				};

				struct VertexDataPass_v2g
				{
					float4 pos : SV_POSITION;
					float3 normal : NORMAL;
				};
				struct VertexDataPass_g2f
				{
					float4 pos : SV_POSITION;
					float3 normal : NORMAL;
					float3 worldPos : TEXCOORD3;
					nointerpolation  float3 v0 : TEXCOORD4;
					nointerpolation  float3 v1 : TEXCOORD5;
					nointerpolation  float3 v2 : TEXCOORD6;
				};

				VertexDataPass_v2g vert(appdata_full v)
				{
					float3 v0 = mul(unity_ObjectToWorld, v.vertex).xyz;
					v.vertex.xyz = mul((float3x3)unity_WorldToObject, v0);
					VertexDataPass_v2g o;
					o.pos = v.vertex;
					o.normal = v.normal;
					return o;
				}

				[maxvertexcount(3)]
				void geom(triangle VertexDataPass_v2g IN[3], inout TriangleStream<VertexDataPass_g2f> tristream)
				{
					float3 v0 = IN[0].pos.xyz;
					float3 v1 = IN[1].pos.xyz;
					float3 v2 = IN[2].pos.xyz;
					float3 word_v0 = mul(unity_ObjectToWorld, IN[0].pos.xyz);
					float3 word_v1 = mul(unity_ObjectToWorld, IN[1].pos.xyz);
					float3 word_v2 = mul(unity_ObjectToWorld, IN[2].pos.xyz);
					float3 vn = normalize(cross(v1 - v0, v2 - v0));

					VertexDataPass_g2f OUT;

					OUT.pos = UnityObjectToClipPos(IN[0].pos);
					OUT.normal = vn;
					OUT.worldPos = mul(unity_ObjectToWorld, IN[0].pos.xyz);
					OUT.v0 = word_v0;
					OUT.v1 = word_v1;
					OUT.v2 = word_v2;
					tristream.Append(OUT);

					OUT.pos = UnityObjectToClipPos(IN[1].pos);
					OUT.normal = vn;
					OUT.worldPos = mul(unity_ObjectToWorld, IN[1].pos.xyz);
					OUT.v0 = word_v0;
					OUT.v1 = word_v1;
					OUT.v2 = word_v2;
					tristream.Append(OUT);

					OUT.pos = UnityObjectToClipPos(IN[2].pos);
					OUT.normal = vn;
					OUT.worldPos = mul(unity_ObjectToWorld, IN[2].pos.xyz);
					OUT.v0 = word_v0;
					OUT.v1 = word_v1;
					OUT.v2 = word_v2;
					tristream.Append(OUT);


					tristream.RestartStrip();
				}

				half4 frag(VertexDataPass_g2f IN) :SV_Target
				{
					//return half4(1, 1, 1, 1);
					float p = 1.0 / float(pixel_count - 1);
					float heat = 0;
					float4 value;
					for (int i = 0; i < pixel_count;)
					{
						float tempX, tempY, tempZ;
						//讀取一個熱點的資料，前點的資訊(座標+次數)

						value = tex1D(array, (i++)*p).rgba;			//tex1D 一维纹理查询

						tempX = floor(value.r * 10 + 0.5) * 10 + floor(value.g * 10 + 0.5) + floor(value.b * 10 + 0.5) * 0.1;
						if (floor(value.a * 10 + 0.5) == 10)
							tempX = tempX * (-1);

						value = tex1D(array, (i++)*p).rgba;
						tempY = floor(value.r * 10 + 0.5) * 10 + floor(value.g * 10 + 0.5) + floor(value.b * 10 + 0.5) * 0.1;
						if (floor(value.a * 10 + 0.5) == 10)
							tempY = tempY * (-1);

						value = tex1D(array, (i++)*p).rgba;
						tempZ = floor(value.r * 10 + 0.5) * 10 + floor(value.g * 10 + 0.5) + floor(value.b * 10 + 0.5) * 0.1;
						if (floor(value.a * 10 + 0.5) == 10)
							tempZ = tempZ * (-1);

						value = tex1D(array, (i++)*p).rgba;
						float count = floor(value.r * 10 + 0.5) * 1000 + floor(value.g * 10 + 0.5) * 100 + floor(value.b * 10 + 0.5) * 10 + floor(value.a * 10 + 0.5);

						float3 n_pos = float3(tempX, tempY, tempZ);
						
						//float dis = distance(mul(unity_ObjectToWorld, IN.pos), n_pos);	// 此點 和 重點點，兩點間距離
						//IN.pos = mul(unity_ObjectToWorld, IN.pos);
						float dis = distance(IN.worldPos, n_pos);	// 此點 和 重點點，兩點間距離

						if (dis < _Radius)
						//if (true)
						{
							//若有時間可改用高斯函數
							float ratio = 1 - saturate(dis / _Radius);				// ratio比例 ; saturate取 0 ~ 1。越近中心點 ratio為 1

							//heat += (now_point.count / _MaxCount) * ratio;		// 熱度 = 亮度??(改成次數占比) * 距離比例
							heat += count * ratio * ratio;				// 熱度 = 亮度??(改成次數占比) * 距離比例
						}
					}

					fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
					fixed3 worldNormal = normalize(UnityObjectToWorldNormal(IN.normal));
					fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
					//fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));
					fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir*0.5));

					if (heat == 0) {

						fixed3 color = ambient + diffuse;

						return fixed4(color, 1.0);
					}
					heat = clamp(heat, 0.05, 0.95);

					//tex2D在一张贴图中对一个点进行采样的方法，返回一个float4
					float3 color = tex2D(_HeatMapTex,fixed2(heat,0.5));			//_HeatMapTex是一個色階圖。 tex2D 二维纹理查询，此點彩色x看heat值，y為0.5不變

					color += (ambient + diffuse);
					//color.a = _Alpha;
					//return color;
					return fixed4(color, 1.0);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}