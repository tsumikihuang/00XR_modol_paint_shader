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

			uniform uint _EVRN;
			uniform uint vertice_count;
			uniform sampler2D array;

			uniform float _MinX;
			uniform float _RangeX;
			uniform float _MinY;
			uniform float _RangeY;
			uniform float _MinZ;
			uniform float _RangeZ;
			uniform float _MinW;
			uniform float _RangeW;

			struct a2v
			{
				float4 pos	: POSITION;
				float3 normal	: NORMAL;
				uint   id		: SV_VertexID;				//an unsigned 32-bit integer
			};

			struct VertexDataPass_v2g
			{
				float4 pos		: POSITION;
				float3 worldPos : TEXCOORD0;
				float3 normal	: NORMAL;
				uint   id		: TEXCOORD1;				//an unsigned 32-bit integer
			};
			struct VertexDataPass_g2f
			{
				float4 pos		: SV_POSITION;
				float3 normal	: NORMAL;
				float3 worldPos : TEXCOORD5;
				nointerpolation  uint v0_id : TEXCOORD2;
				nointerpolation  uint v1_id : TEXCOORD3;
				nointerpolation  uint v2_id : TEXCOORD4;
			};
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			VertexDataPass_v2g vert(a2v v)
			{
				VertexDataPass_v2g o;
				o.pos = v.pos;
				o.worldPos = mul(unity_ObjectToWorld, v.pos).xyz;
				o.normal = v.normal;
				o.id = v.id;
				return o;
			}
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			[maxvertexcount(3)]
			void geom(triangle VertexDataPass_v2g IN[3], inout TriangleStream<VertexDataPass_g2f> tristream)
			{
				float3 v0 = IN[0].pos.xyz;
				float3 v1 = IN[1].pos.xyz;
				float3 v2 = IN[2].pos.xyz;
				float3 vn = normalize(cross(v1 - v0, v2 - v0));

				VertexDataPass_g2f OUT;

				OUT.pos = UnityObjectToClipPos(IN[0].pos);
				OUT.normal = vn;
				OUT.worldPos = IN[0].worldPos;
				OUT.v0_id = IN[0].id;
				OUT.v1_id = IN[1].id;
				OUT.v2_id = IN[2].id;
				tristream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(IN[1].pos);
				OUT.normal = vn;
				OUT.worldPos = IN[1].worldPos;
				OUT.v0_id = IN[0].id;
				OUT.v1_id = IN[1].id;
				OUT.v2_id = IN[2].id;
				tristream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(IN[2].pos);
				OUT.normal = vn;
				OUT.worldPos = IN[2].worldPos;
				OUT.v0_id = IN[0].id;
				OUT.v1_id = IN[1].id;
				OUT.v2_id = IN[2].id;
				tristream.Append(OUT);

				//tristream.RestartStrip();
			}
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			half4 frag(VertexDataPass_g2f IN) :SV_Target
			{
				half heat = 0;
				uint width = vertice_count * _EVRN;
				uint height = 1;
				if (width > 10000) {
					width = 10000;
					height = (vertice_count * _EVRN) / 10000 + 1;
				}

				uint index;
				float index_x;
				float index_y;
				float4 value;
				float count;
				float dis;
				float3 temp_point;
				float ratio;
				//拿出 這個frag所在三角點(3個id)裡各自的_EVRN筆資料
				uint i;

				float4 allVert[30];	//保守給30
				for (i = 0; i < 30; i++)
					allVert[i] = float4(0,0,0,0);
				uint allVert_len = 0;
				bool isRepeat;
				
				for (i = 0; i < _EVRN; i++)
				{
					index = IN.v0_id * _EVRN + i;
					index_x = (index % 10000 + 0.5) / width;
					index_y = (index / 10000 + 0.5) / height;

					value = tex2D(array, float2(index_x, index_y)).rgba;

					//Count
					count = value.a*_RangeW + _MinW;
					if (count == 0.00f)continue;

					temp_point.x = value.r*_RangeX + _MinX;
					temp_point.y = value.g*_RangeY + _MinY;
					temp_point.z = value.b*_RangeZ + _MinZ;

					dis = distance(IN.worldPos, temp_point);

					if (dis < _Radius)
					{
						float4 thisP = float4(temp_point.x, temp_point.y, temp_point.z, count);
						isRepeat = false;
						for (uint j = 0; j < allVert_len; j++) {
							if ((allVert[j].x == thisP.x && allVert[j].y == thisP.y) && (allVert[j].z == thisP.z && allVert[j].w == thisP.w)) {
								isRepeat = true;
								break;
							}
						}
						if (!isRepeat) {
							allVert[allVert_len] = thisP;
							allVert_len++;
						}
					}
				}
				for (i = 0; i < _EVRN; i++)
				{
					index = IN.v1_id * _EVRN + i;
					index_x = (index % 10000 + 0.5) / width;
					index_y = (index / 10000 + 0.5) / height;

					value = tex2D(array, float2(index_x, index_y)).rgba;

					//Count
					count = value.a*_RangeW + _MinW;
					if (count == 0.00f)continue;

					temp_point.x = value.r*_RangeX + _MinX;
					temp_point.y = value.g*_RangeY + _MinY;
					temp_point.z = value.b*_RangeZ + _MinZ;

					dis = distance(IN.worldPos, temp_point);

					if (dis < _Radius)
					{
						float4 thisP = float4(temp_point.x, temp_point.y, temp_point.z, count);
						isRepeat = false;
						for (uint j = 0; j < allVert_len; j++) {
							if ((allVert[j].x == thisP.x && allVert[j].y == thisP.y) && (allVert[j].z == thisP.z && allVert[j].w == thisP.w)) {
								isRepeat = true;
								break;
							}
						}
						if (!isRepeat) {
							allVert[allVert_len] = thisP;
							allVert_len++;
						}
					}
				}
				for (i = 0; i < _EVRN; i++)
				{
					index = IN.v2_id * _EVRN + i;
					index_x = (index % 10000 + 0.5) / width;
					index_y = (index / 10000 + 0.5) / height;

					value = tex2D(array, float2(index_x, index_y)).rgba;

					//Count
					count = value.a*_RangeW + _MinW;
					if (count == 0.00f)continue;

					temp_point.x = value.r*_RangeX + _MinX;
					temp_point.y = value.g*_RangeY + _MinY;
					temp_point.z = value.b*_RangeZ + _MinZ;

					dis = distance(IN.worldPos, temp_point);

					if (dis < _Radius)
					{
						float4 thisP = float4(temp_point.x, temp_point.y, temp_point.z, count);
						isRepeat = false;
						for (uint j = 0; j < allVert_len; j++) {
							if (allVert[j].x == thisP.x && allVert[j].y == thisP.y && allVert[j].z == thisP.z && allVert[j].w == thisP.w) {
								isRepeat = true;
								break;
							}
						}
						if (!isRepeat) {
							allVert[allVert_len] = thisP;
							allVert_len++;
						}
					}
				}

				for (i = 0; i < allVert_len; i++) {
					dis = distance(IN.worldPos, float3(allVert[i].x, allVert[i].y, allVert[i].z));
					count = allVert[i].w;
					//ratio = 1 - saturate(dis / _Radius);				// ratio比例 ; saturate取 0 ~ 1。越近中心點 ratio為 1
					ratio = 1 / dis;
					heat += count * ratio/20;								// 熱度 = 亮度??(改成次數占比) * 距離比例
					//heat += count /10;								// 熱度 = 亮度??(改成次數占比) * 距離比例
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
				heat = clamp(heat, 0, 0.99);

				//tex2D在一张贴图中对一个点进行采样的方法，返回一个float4
				float3 color = (1 - heat)*(ambient + diffuse) + heat * tex2D(_HeatMapTex,fixed2(heat,0.5));			//_HeatMapTex是一個色階圖。 tex2D 二维纹理查询，此點彩色x看heat值，y為0.5不變

				//color.a = _Alpha;
				return fixed4(color, 1.0);
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}