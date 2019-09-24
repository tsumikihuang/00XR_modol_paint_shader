// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unity Shaders Book/Ch6/半蘭伯特漫射"{
	Properties{
		_Diffuse("Diffuse",Color) = (1,1,1,1)
	}
		SubShader{
		Pass{
		Tags{ "LightMode" = "ForwardBase" }
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "Lighting.cginc"
		fixed4 _Diffuse;
	struct a2v {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};
	struct v2f {
		float4 pos : SV_POSITION;
		float3 worldNormal : TEXCOORD0;
	};
	v2f vert(a2v v) {
		v2f o;	//回傳值
		o.pos = UnityObjectToClipPos(v.vertex);//Unity換
		o.worldNormal = UnityObjectToWorldNormal(v.normal);
		return o;
	}
	fixed4 frag(v2f i) :SV_Target{
		fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
	fixed3 worldNormal = normalize(i.worldNormal);
	fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);


	//和逐像素相差的部分 (改的部分)
	fixed halfLambert = dot(worldNormal, worldLightDir)*0.5 + 0.5;
	fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * halfLambert;

	fixed3 color = ambient + diffuse;


	return fixed4(color,1.0);
	}
		ENDCG
	}
	}
		Fallback "Diffuse"
}
