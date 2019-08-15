// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Merge RGB And Alpha"
{
	Properties
	{
		_MainTex("rgb tex", 2D) = "black" {}
		_AlphaTex("alpha tex",2D) = "white"{}
	}
	
	SubShader
	{
	LOD 100
	Tags
	{
	"Queue" = "Transparent"
	"IgnoreProjector" = "True"
	"RenderType" = "Transparent"
	}
	Cull Off
	Lighting Off
	ZWrite Off
	Fog { Mode Off }
	Offset -1, -1
	Blend SrcAlpha OneMinusSrcAlpha
	Pass
	{
	CGPROGRAM
	#pragma vertex vert  
	#pragma fragment frag  
	#include "UnityCG.cginc"  
	struct appdata_t
	{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	fixed4 color : COLOR;
	};
	struct v2f
	{
	float4 vertex : SV_POSITION;
	half2 texcoord : TEXCOORD0;
	fixed4 color : COLOR;
	};
	sampler2D _MainTex;
	float4 _MainTex_ST;
	sampler2D _AlphaTex;
	float4 _AlphaTex_ST;
	v2f vert(appdata_t v)
	{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.texcoord = v.texcoord;
	o.color = v.color;
	return o;
	}
	fixed4 frag(v2f i) : COLOR
	{
		//fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;  
		//return col;  
		fixed4 texcol = tex2D(_MainTex, i.texcoord);
		fixed4 result = texcol;
		result.a = tex2D(_AlphaTex,i.texcoord).r*i.color.a;
		//灰度计算  
		 if (i.color.r < 0.001)
		   {
			 //float grey  =  dot(result.rgb, float3(0.299, 0.587, 0.114));   
			 //整数运算速度更快  
			 float gray = (result.r * 299 + result.g * 587 + result.b * 114 + 500) / 1680;
			 result.rgb = float3(gray, gray, gray);
		 }
	  return result;
	  }
	  ENDCG
	  }
	}
		SubShader
	  {
	  LOD 100
	  Tags
	  {
	  "Queue" = "Transparent"
	  "IgnoreProjector" = "True"
	  "RenderType" = "Transparent"
	  }
	  Pass
	  {
	  Cull Off
	  Lighting Off
	  ZWrite Off
	  Fog { Mode Off }
	  Offset -1, -1
	  ColorMask RGB
	  AlphaTest Greater .01
	  Blend SrcAlpha OneMinusSrcAlpha
	  ColorMaterial AmbientAndDiffuse
	  SetTexture[_MainTex]
	  {
	  Combine Texture * Primary
	  }
	  }
	  }
}