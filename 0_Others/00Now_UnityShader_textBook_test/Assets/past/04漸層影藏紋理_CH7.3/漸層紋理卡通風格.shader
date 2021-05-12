//與  單張紋理  做比較

Shader "Unity Shaders Book/Ch7/漸層紋理卡通風格"{
	Properties{
		_Specular("Specular",Color) = (1,1,1,1)	//高光反射顏色
		_Gloss("Gloss",Range(8.0,256)) = 20		//高光反射區域大小
		_Color("Color Tint",Color) = (1,1,1,1)
		
		///change name，主紋理
		_RampTex("Ramp Tex",2D) = "white"{}
		///
	}
	SubShader{
		Pass{
			Tags{ "LightMode" = "ForwardBase" }	//定義此Pass在Unity的光源管線中的腳色
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"			//為了使用Unity內建變數，如_LightColor0
			fixed4 _Specular;
			float _Gloss;
			fixed4 _Color;

			///change name
			sampler2D _RampTex;				//紋理
			float4 _RampTex_ST;				//需要用"紋理名_ST"的方式宣告某個紋理的屬性(Scale Translation)
			///

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord:TEXCOORD0;	//Unity會將第一組紋理座標儲存到該變數中，xy獲得紋理縮放值、zw獲得偏移值
			};
			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos:TEXCOORD1;
				float2 uv:TEXCOORD2;		//用於儲存紋理座標的變數，以便在片段著色器中使用該座標進行紋理取樣
			};

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);			//轉換頂點
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				///改用func
				//原本是 o.uv = v.texcoord.xy*_MainTex_ST.xy + _MainTex_ST.zw
				//儲存紋理座標的變數，在後面將使用該座標進行紋理取樣
				o.uv = TRANSFORM_TEX(v.texcoord,_RampTex);	
				///

				return o;
			}

			//change little more >> !delete! albedo
			fixed4 frag(v2f i) :SV_Target{
				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));//指向光源方向	

				//材質反射率
				//fixed3 albedo = tex2D(_MainTex, i.uv).rgb*_Color.rgb;	//i.uv是紋理座標

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;	//獲得環境光

				///new
				fixed halfLambert = 0.5*dot(worldNormal, worldLightDir) + 0.5;	//使用半蘭伯特，值對應到0~1
				
				//建置紋理座標，並對漸層紋理_RamTex取樣
				//又因為
				//_RampTex實際上是一個一維紋理(在縱軸方向上顏色不變)
				//因此紋理座標的u、v都使用halfLambert
				
				fixed3 diffuseColor = tex2D(_RampTex, fixed2(halfLambert, halfLambert)).rgb*_Color.rgb;

				//雖然理論上halfLambert會介於0~1  但  可能會有1.00001這樣的值
				//因此如果把漸層紋理的wrap mode設為  Repeat  ，此時會捨棄整數部分，得到0.00001，因此是黑色
				//可以看出在高光區域反而有黑點的情況
				//改為clamp即可改善
				///

				//fixed3 diffuse = _LightColor0.rgb * albedo * max(0,dot(worldNormal, worldLightDir));
				fixed3 diffuse = _LightColor0.rgb * diffuseColor;

				//眼睛方向 >> 攝影機位置-點的位置
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				fixed3 halfDir = normalize(worldLightDir + viewDir);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(worldNormal, halfDir)),_Gloss);

				return fixed4(ambient + diffuse + specular,1.0);
			}
			ENDCG
		}
	}
		Fallback "Specular"
}
