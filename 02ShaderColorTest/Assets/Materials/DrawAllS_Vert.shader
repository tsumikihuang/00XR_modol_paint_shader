﻿Shader "UChart/HeatMap/All"
{
	Properties
	{
		_Diffuse("Diffuse",Color) = (1,1,1,1)
		_HeatMapTex("_HeatMapTex",2D) = "white"{}
	//_Alpha("Alpha",range(0,1)) = 1

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
		//half _Alpha;


		uniform sampler2D SimpleModel_vertexINFO_array;
		uniform sampler2D M2M_array;
		uniform uint SYSTEM_MAX_TEXTURE_SIZE;
		uniform uint S_len;
		uniform uint O_vertice_count;
		uniform uint _EVRN;

		uniform float _Radius;
		//uniform float _MaxCount;

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
			nointerpolation float3 id : TEXCOORD2;
			/*nointerpolation uint v0_id : TEXCOORD2;
			nointerpolation uint v1_id : TEXCOORD3;
			nointerpolation uint v2_id : TEXCOORD4;*/
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
			OUT.id = float3(IN[0].id, IN[1].id, IN[2].id);
			/*OUT.v0_id = IN[0].id;
			OUT.v1_id = IN[1].id;
			OUT.v2_id = IN[2].id;*/
			tristream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(IN[1].pos);
			OUT.normal = vn;
			OUT.worldPos = IN[1].worldPos;
			OUT.id = float3(IN[0].id, IN[1].id, IN[2].id);
			/*OUT.v0_id = IN[0].id;
			OUT.v1_id = IN[1].id;
			OUT.v2_id = IN[2].id;*/
			tristream.Append(OUT);

			OUT.pos = UnityObjectToClipPos(IN[2].pos);
			OUT.normal = vn;
			OUT.worldPos = IN[2].worldPos;
			OUT.id = float3(IN[0].id, IN[1].id, IN[2].id);
			/*OUT.v0_id = IN[0].id;
			OUT.v1_id = IN[1].id;
			OUT.v2_id = IN[2].id;*/
			tristream.Append(OUT);

			//tristream.RestartStrip();
		}
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		half4 frag(VertexDataPass_g2f IN) :SV_Target
		{
			///圖二(對應vert pos、count)
			float heat = 0;
			uint width = S_len;
			uint height = 1;
			if (width > SYSTEM_MAX_TEXTURE_SIZE) {
				height = width / SYSTEM_MAX_TEXTURE_SIZE + 1;
				width = SYSTEM_MAX_TEXTURE_SIZE;
			}
			for (uint i = 0; i < 512; i++)
			{
				uint index = i;
				float index_x = (index % SYSTEM_MAX_TEXTURE_SIZE + 0.5) / width;
				float index_y = (index / SYSTEM_MAX_TEXTURE_SIZE + 0.5) / height;

				float4 value = tex2D(SimpleModel_vertexINFO_array, float2(index_x, index_y)).rgba;

				float3 temp_point;
				temp_point.x = value.r*_RangeX + _MinX;
				temp_point.y = value.g*_RangeY + _MinY;
				temp_point.z = value.b*_RangeZ + _MinZ;
				float dis = distance(IN.worldPos, temp_point);
				if (dis < 0.5f) {
					heat = 1;
					break;
				}
			}

			fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
			fixed3 worldNormal = normalize(UnityObjectToWorldNormal(IN.normal));
			fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
			//fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));
			fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir*0.5));

			fixed3 orign_color = ambient + diffuse;

			heat = clamp(heat, 0, 0.99);
			float3 color = (1 - heat)* orign_color + heat * tex2D(_HeatMapTex,fixed2(heat,0.5));			//_HeatMapTex是一個色階圖。 tex2D 二维纹理查询，此點彩色x看heat值，y為0.5不變

			return fixed4(color, 1.0);
		}
		ENDCG
	}
	}
		FallBack "Diffuse"
}