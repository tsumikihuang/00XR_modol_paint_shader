Shader "UChart/HeatMap/Simple"
{
	Properties
	{
		_HeatMapTex("_HeatMapTex",2D) = "white"{}
		_Alpha("Alpha",range(0,1)) = 0.8 
	}

	SubShader
	{
		Tags {"RenderType"="Overlay" "Queue"="Transparent" } 
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest [unity_GUIZTestMode]
		ZWrite On
		// Cull off

		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma enable_d3d11_debug_symbols		//debug用!!!

			sampler2D _HeatMapTex;
			half _Alpha;

			//uniform变量是外部application程序传递给（vertex和fragment）shader的变量。
			//uniform变量就像是C语言里面的常量（const ），它不能被shader程序修改。（shader只能用，不能改）
			uniform float _Radius;
			uniform float _MaxCount;

			uniform int pixel_count;
			uniform sampler1D array;

			struct a2v
			{
				float4 pos : POSITION;
			};

			struct v2f
			{
				float4 pos : POSITION;
				float3 worldPos : TEXCOORD1;
			};

			struct _point
			{
				float3 pos: TEXCOORD3;
				float count : TEXCOORD4;
			};

			v2f vert(a2v input)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.worldPos = mul(unity_ObjectToWorld, input.pos).xyz;
				return o;
			}

			float4 frag(v2f input):SV_Target
			{
				float p = 1.0 / float(pixel_count - 1);
				float heat = 0;
				float4 value;
				for (int i = 0; i < pixel_count;)
				{
					_point now_point;
					float tempX, tempY, tempZ ;
					//讀取一個熱點的資料，前點的資訊(座標+次數)
					
					value = tex1D(array, (i ++)*p).rgba;			//tex1D 一维纹理查询
					tempX = floor(value.r * 10+0.5) * 10 + floor(value.g * 10+0.5) + floor(value.b * 10+0.5) * 0.1;
					if (floor(value.a * 10+0.5) == 10)
						tempX = tempX * (-1);

					value = tex1D(array, (i ++)*p).rgba;
					tempY = floor(value.r * 10 + 0.5) * 10 + floor(value.g * 10 + 0.5) + floor(value.b * 10 + 0.5) * 0.1;
					if (floor(value.a * 10+0.5) == 10)
						tempY = tempY * (-1);

					value = tex1D(array, (i ++)*p).rgba;
					tempZ = floor(value.r * 10 + 0.5) * 10 + floor(value.g * 10 + 0.5) + floor(value.b * 10 + 0.5) * 0.1;
					if (floor(value.a * 10+0.5) == 10)
						tempZ = tempZ * (-1);

					//return float4(1/ float(tempX), 1, 1 / tempZ, 1);

					value = tex1D(array, (i ++)*p).rgba;
					now_point.count = value.r * 1000 + value.g*100 + value.b*10 + value.a;

					now_point.pos = float3(tempX, tempY, tempZ);

					float dis = distance(input.worldPos, now_point.pos);	// 此點 和 重點點，兩點間距離
					//return float4(dis, dis, dis, 1);
					//若有時間可改用高斯函數
					float ratio = 1 - saturate(dis / _Radius) * saturate(dis / _Radius);	// ratio比例 ; saturate取 0 ~ 1。越近中心點 ratio為 1
					//heat += (now_point.count / _MaxCount) * ratio;						// 熱度 = 亮度??(改成次數占比) * 距離比例
					heat += now_point.count * ratio;						// 熱度 = 亮度??(改成次數占比) * 距離比例
					//heat += ratio;						// 熱度 = 亮度??(改成次數占比) * 距離比例
				}

				heat = clamp(heat, 0.05, 0.95);							// heat 介於 0 ~ 1

				//tex2D在一张贴图中对一个点进行采样的方法，返回一个float4
				float4 color = tex2D(_HeatMapTex,fixed2(heat,0.5));			//_HeatMapTex是一個色階圖。 tex2D 二维纹理查询，此點彩色x看heat值，y為0.5不變
				
				color.a = _Alpha;
				return color;
			}

			ENDCG
		}
	}

	 FallBack "Diffuse"
}