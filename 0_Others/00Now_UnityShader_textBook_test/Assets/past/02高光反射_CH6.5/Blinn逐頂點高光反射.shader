// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unity Shaders Book/Ch6/Blinn逐頂點高光反射"{
	Properties{
		_Diffuse("Diffuse",Color) = (1,1,1,1)
		_Specular("Specular",Color)=(1,1,1,1)	//高光反射顏色
		_Gloss("Gloss",Range(8.0,256))=20		//高光反射區域大小
	}
	SubShader{
		Pass{
			Tags{ "LightMode" = "ForwardBase" }	//定義此Pass在Unity的光源管線中的腳色
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"			//為了使用Unity內建變數，如_LightColor0
			fixed4 _Diffuse;
			fixed4 _Specular;
			float _Gloss;
			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			struct v2f {
				float4 pos : SV_POSITION;
				float3 color : COLOR;
			};
			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);			//轉換頂點
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;	//獲得環境光
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);	//指向光源方向
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));

				//////////////////////////////////////
				fixed3 reflectDir = normalize(reflect(-worldLightDir, worldNormal));
				//眼睛方向 >> 攝影機位置-點的位置
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
				fixed3 specular= _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(reflectDir,viewDir)),_Gloss);
				
				o.color = ambient + diffuse + specular;
				return o;
			}
			fixed4 frag(v2f i) :SV_Target{
				return fixed4(i.color,1.0);
			}
			ENDCG
		}
	}
	Fallback "Specular"
}
