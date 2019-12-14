Shader "UChart/HeatMap/LoadColor"
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
		uniform uint vertice_count;
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
			uint i;
			int VertInclude_id[300];
			//for (i = 0; i < 300; i++)
			//	VertInclude_id[i] = -1;
			uint VertInclude_len = 0;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			///圖一(頂點ID找多個id)1000*84
			uint width = floor(vertice_count * _EVRN/4.0+1);//無條件進位
			uint height = 1;
			if (width > SYSTEM_MAX_TEXTURE_SIZE) {
				height = width / SYSTEM_MAX_TEXTURE_SIZE + 1;
				width = SYSTEM_MAX_TEXTURE_SIZE;
			}

			for (uint j = 0; j < 3; j++) {		//3頂點跑三次
				for (i = 0; i < _EVRN; i++)
				{
					uint placeNO = 0;
					if (j == 0)placeNO = IN.v0_id * _EVRN + i;
					else if (j == 1)placeNO = IN.v1_id * _EVRN + i;
					else if (j == 2)placeNO = IN.v2_id * _EVRN + i;

					//STEP 1；用目前頂點的id，對應Group_array，找到多個simpleModel下的點id
					uint index = floor(placeNO / 4.0);
					float index_x = (index % SYSTEM_MAX_TEXTURE_SIZE + 0.5) / width;
					float index_y = (index / SYSTEM_MAX_TEXTURE_SIZE + 0.5) / height;

					float4 value = tex2D(M2M_array, float2(index_x, index_y)).rgba;

					int GetValue_idNum = 0;
					if (placeNO % 4 == 0) GetValue_idNum = floor(value.r*S_len + 0.5) - 1;
					else if (placeNO % 4 == 1) GetValue_idNum = floor(value.g*S_len + 0.5) - 1;
					else if (placeNO % 4 == 2) GetValue_idNum = floor(value.b*S_len + 0.5) - 1;
					else if (placeNO % 4 == 3) GetValue_idNum = floor(value.a*S_len + 0.5) - 1;
					if (GetValue_idNum == -1)break;

					
					if(VertInclude_len==0) {
						VertInclude_id[VertInclude_len] = GetValue_idNum;
						VertInclude_len= VertInclude_len+1;
					}
					else {
						bool isRepeat = false;
						for (uint k = 0; k < VertInclude_len; k++)
							if ((VertInclude_id[k] == GetValue_idNum)) {
								isRepeat = true;
								break;
							}
						if (!isRepeat) {
							VertInclude_id[VertInclude_len] = GetValue_idNum;
							VertInclude_len++;
						}
					}
				}
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			///圖二(對應vert pos、count)
			float heat = 0;
			width = S_len;
			height = 1;
			if (width > SYSTEM_MAX_TEXTURE_SIZE) {
				height = width / SYSTEM_MAX_TEXTURE_SIZE + 1;
				width = SYSTEM_MAX_TEXTURE_SIZE;
			}

			for (i = 0; i < VertInclude_len; i++)
			{
				uint index = VertInclude_id[i];
				float index_x = (index % SYSTEM_MAX_TEXTURE_SIZE + 0.5) / width;
				float index_y = (index / SYSTEM_MAX_TEXTURE_SIZE + 0.5) / height;

				float4 value = tex2D(SimpleModel_vertexINFO_array, float2(index_x, index_y)).rgba;

				//Count
				float count = value.a;
				if (count == 0.00f)continue;

				float3 temp_point;
				temp_point.x = value.r*_RangeX + _MinX;
				temp_point.y = value.g*_RangeY + _MinY;
				temp_point.z = value.b*_RangeZ + _MinZ;
				float dis = distance(IN.worldPos, temp_point);
				if (dis < _Radius)
				{
					//float ratio = 1 - saturate(dis / _Radius);				// ratio比例 ; saturate取 0 ~ 1。越近中心點 ratio為 1
					float ratio = 1 / dis;
					heat += count * ratio;							// 熱度 = 亮度??(改成次數占比) * 距離比例
				}
			}

			fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
			fixed3 worldNormal = normalize(UnityObjectToWorldNormal(IN.normal));
			fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
			//fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));
			fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir*0.5));

			fixed3 orign_color = ambient + diffuse;
			/*if (heat == 0) {
				return fixed4(orign_color, 1.0);
			}*/
			heat = clamp(heat, 0, 0.99);
			float3 color =(1- heat)* orign_color + heat * tex2D(_HeatMapTex,fixed2(heat,0.5));			//_HeatMapTex是一個色階圖。 tex2D 二维纹理查询，此點彩色x看heat值，y為0.5不變

			//color.a = _Alpha;
			return fixed4(color, 1.0);
		}
		ENDCG
	}
	}
		FallBack "Diffuse"
}