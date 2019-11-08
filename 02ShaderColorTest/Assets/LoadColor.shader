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

			uniform uint pixel_count;
			uniform sampler2D array;

			struct a2v
			{
				float4 vertex	: POSITION;
				float3 normal	: NORMAL;
				uint   id		: SV_VertexID;				//an unsigned 32-bit integer
			};

			struct VertexDataPass_v2g
			{
				float4 pos		: SV_POSITION;
				float3 normal	: NORMAL;
				uint   id		: TEXCOORD7;				//an unsigned 32-bit integer
			};
			struct VertexDataPass_g2f
			{
				float4 pos		: SV_POSITION;
				float3 normal	: NORMAL;
				float3 worldPos : TEXCOORD0;
				nointerpolation  float3 word_v0 : TEXCOORD1;
				nointerpolation  float3 word_v1 : TEXCOORD2;
				nointerpolation  float3 word_v2 : TEXCOORD3;
				nointerpolation  uint v0_id : TEXCOORD4;
				nointerpolation  uint v1_id : TEXCOORD5;
				nointerpolation  uint v2_id : TEXCOORD6;
			};

			VertexDataPass_v2g vert(a2v v)
			{
				float3 v0 = mul(unity_ObjectToWorld, v.vertex).xyz;
				v.vertex.xyz = mul((float3x3)unity_WorldToObject, v0);
				VertexDataPass_v2g o;
				o.pos = v.vertex;
				o.normal = v.normal;
				o.id = v.id;
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
				OUT.word_v0 = word_v0;
				OUT.word_v1 = word_v1;
				OUT.word_v2 = word_v2;
				OUT.v0_id = IN[0].id;
				OUT.v1_id = IN[1].id;
				OUT.v2_id = IN[2].id;
				tristream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(IN[1].pos);
				OUT.normal = vn;
				OUT.worldPos = mul(unity_ObjectToWorld, IN[1].pos.xyz);
				OUT.word_v0 = word_v0;
				OUT.word_v1 = word_v1;
				OUT.word_v2 = word_v2;
				OUT.v0_id = IN[0].id;
				OUT.v1_id = IN[1].id;
				OUT.v2_id = IN[2].id;
				tristream.Append(OUT);

				OUT.pos = UnityObjectToClipPos(IN[2].pos);
				OUT.normal = vn;
				OUT.worldPos = mul(unity_ObjectToWorld, IN[2].pos.xyz);
				OUT.word_v0 = word_v0;
				OUT.word_v1 = word_v1;
				OUT.word_v2 = word_v2;
				OUT.v0_id = IN[0].id;
				OUT.v1_id = IN[1].id;
				OUT.v2_id = IN[2].id;
				tristream.Append(OUT);


				tristream.RestartStrip();
			}

			half4 frag(VertexDataPass_g2f IN) :SV_Target
			{
				//return half4(1, 1, 1, 1);

				float All_area = length(cross(IN.word_v1 - IN.word_v0, IN.word_v2 - IN.word_v0)) / 2;
				float _area0 = length(cross(IN.word_v1 - IN.worldPos, IN.word_v2 - IN.worldPos)) / 2;
				float _area1 = length(cross(IN.word_v0 - IN.worldPos, IN.word_v2 - IN.worldPos)) / 2;
				float _area2 = length(cross(IN.word_v0 - IN.worldPos, IN.word_v1 - IN.worldPos)) / 2;

				float p = 1.0 / float(pixel_count - 1);
				//float4 value = tex2D(array, IN.v0_id*p).rgba;

				uint width = pixel_count;
				uint height = 1;
				if (width > 10000) {
					width = 10000;
					height = pixel_count / 10000 + 1;
				}
				float index_x = (IN.v0_id % 10000 + 0.5) / width;
				float index_y = (IN.v0_id / 10000 + 0.5) / height;
				float4 value = tex2D(array, float2(index_x, index_y)).rgba;
				float v0_count = floor(value.r * 10 + 0.5) + floor(value.g * 10 + 0.5) * 0.1 + floor(value.b * 10 + 0.5) * 0.01 + floor(value.a * 10 + 0.5) * 0.001;
				//value = tex2D(array, IN.v1_id*p).rgba;
				index_x = (IN.v1_id % 10000 + 0.5) / width;
				index_y = (IN.v1_id / 10000 + 0.5) / height;
				value = tex2D(array, float2(index_x, index_y)).rgba;
				float v1_count = floor(value.r * 10 + 0.5) + floor(value.g * 10 + 0.5) * 0.1 + floor(value.b * 10 + 0.5) * 0.01 + floor(value.a * 10 + 0.5) * 0.001;
				//value = tex2D(array, IN.v2_id*p).rgba;
				index_x = (IN.v2_id % 10000 + 0.5) / width;
				index_y = (IN.v2_id / 10000 + 0.5) / height;
				value = tex2D(array, float2(index_x, index_y)).rgba;
				float v2_count = floor(value.r * 10 + 0.5) + floor(value.g * 10 + 0.5) * 0.1 + floor(value.b * 10 + 0.5) * 0.01 + floor(value.a * 10 + 0.5) * 0.001;

				float heat = v0_count * _area0 / All_area + v1_count * _area1 / All_area + v2_count * _area2 / All_area;


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

				//color += (ambient + diffuse);

				//color.a = _Alpha;
				//return color;
				return fixed4(color, 1.0);
			}

		ENDCG
	}
		}
			FallBack "Diffuse"
}