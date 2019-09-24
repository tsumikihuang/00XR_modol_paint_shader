//高光影藏紋理，逐像素

//與 切線空間計算光源 做比較

Shader "Custom/影藏紋理"
{
	Properties{
		///new
		_SpecularMask("Specular Mask",2D) = "white"{}	//高光反射影藏紋理
		_SpecularScale("Specular Scale",float)=1.0		//控制 影藏影響度 係數
		///

		_Specular("Specular",Color) = (1,1,1,1)	//高光反射顏色
		_Gloss("Gloss",Range(8.0,256)) = 20		//高光反射區域大小
		_Color("Color Tint",Color) = (1,1,1,1)
		_MainTex("Main Tex",2D) = "white"{}
		_BumpMap("Normal Map",2D) = "bump"{}	//法線紋理，bump是unity內建法線紋理
		_BumpScale("Bump Scale",Float) = 1.0	//凹凸程度，0=無影響
	}

	SubShader{
		Pass{
			Tags{ "LightMode" = "ForwardBase" }	//定義此Pass在Unity的光源管線中的腳色
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"			//為了使用Unity內建變數，如_LightColor0

			///new
			sampler2D _SpecularMask;		//***影藏紋理
			float _SpecularScale;
			///
			fixed4 _Specular;
			float _Gloss;
			fixed4 _Color;
			sampler2D _MainTex;				//***主紋理

			//_MainTex、_BumpMap、_SpecularMask共同使用_MainTex_ST紋理屬性變數
			//表示，主紋理的延展和偏移係數 會同時影響3個紋理取樣
			float4 _MainTex_ST;				//需要用"紋理名_ST"的方式宣告某個紋理的屬性(Scale Translation)
			
			sampler2D _BumpMap;				//***法線紋理
			//float4 _BumpMap_ST;			//獲得紋理屬性(Scale Translation)	//del
			float _BumpScale;				//凹凸程度

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord:TEXCOORD0;	//Unity會將第一組紋理座標儲存到該變數中，xy獲得紋理縮放值、zw獲得偏移值
			
				//切線空間  是由  頂點的  "法線"和"切線"  建置出的座標空間
				//因此需要頂點的切線資訊
				float4 tangent:TANGENT;		//增加切線資訊
											//類型為float4 多一維 是為了 要使用w分量來決定切線空間中的第三個座標軸(副切線)的方向性
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv:TEXCOORD0;		//用於儲存紋理座標的變數，以便在片段著色器中使用該座標進行紋理取樣	//改型態
				float3 lightDir : TEXCOORD1;	//光源
				float3 viewDir : TEXCOORD2;		//視角
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);			//轉換頂點
				o.uv.xy = v.texcoord.xy*_MainTex_ST.xy + _MainTex_ST.zw;	//_MainTex紋理座標
				//o.uv.zw = v.texcoord.xy*_BumpMap_ST.xy + _BumpMap_ST.zw;	//_BumpMap紋理座標	//del

				TANGENT_SPACE_ROTATION;		//得到rotation轉換矩陣
				o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex)).xyz;	//模型空間的光源方向
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;	//模型空間的角度方向

				return o;
			}

			//使用影藏紋理!!!在片段著色器，控制模型表面的高光反射強度
			fixed4 frag(v2f i) :SV_Target{

				fixed3 tangentLightDir = normalize(i.lightDir);
				fixed3 tangentViewDir = normalize(i.viewDir);
				/*
				fixed4 packedNormal = tex2D(_BumpMap, i.uv.zw);		//BumpMap紋理座標
				fixed3 tangentNormal;
				tangentNormal = UnpackNormal(packedNormal);			//得到正確法線方向
				*/
				//簡化上述
				fixed3 tangentNormal= UnpackNormal(tex2D(_BumpMap, i.uv));

				tangentNormal.xy *= _BumpScale;
				//z = 1-(x^2+y^2)
				tangentNormal.z = sqrt(1.0 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));//重新計算z分量
				
				fixed3 albedo = tex2D(_MainTex, i.uv).rgb*_Color.rgb;//材質反射率
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz*albedo;	//獲得環境光
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0,dot(tangentNormal, tangentLightDir));

				fixed3 halfDir = normalize(tangentLightDir + tangentViewDir);

				///new
				//Get mask value
				//因為本書使用的影藏紋理中的每個文素的rgb都一樣(表明該點對應的高光反射強度)，在這我們選用r來計算影藏值
				fixed specularMask = tex2D(_SpecularMask, i.uv).r*_SpecularScale;
				///
				///fix
				fixed3 specular = specularMask*_LightColor0.rgb * _Specular.rgb * pow(max(0,dot(tangentNormal, halfDir)),_Gloss);
				///
				return fixed4(ambient + diffuse + specular,1.0);
			}
			ENDCG
		}
	}
		Fallback "Specular"
}
