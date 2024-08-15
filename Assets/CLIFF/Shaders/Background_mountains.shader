// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/2Bumped Specular" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_Mask("Mask (RGBA)", 2D) = "red" {}
	_Splat3("Snow (a)", 2D) = "black" {}
	_Splat2("Stone (B)", 2D) = "black" {}
	_Splat1("Cliff (G)", 2D) = "black" {}
	_Splat0("Grass (R)", 2D) = "white" {}

	_Normal3("SnowN (A)", 2D) = "bump" {}
	_Normal2("StonesN (B)", 2D) = "bump" {}
	_Normal1("CliffN (G)", 2D) = "bump" {}
	_Normal0("GrassN (R)", 2D) = "bump" {}

    _MainTex ("MainTex", 2D) = "white" {}
    _BumpMap ("Normalmap", 2D) = "bump" {}

	_Blur("Blur", Range(0.01, 1)) = 0.02
	_HeightSplatAll("Grass(R) Cliff(G) Stones(B) Snow(a)", 2D) = "black" {}
}

CGINCLUDE
half _Blur;
sampler2D _Control;
sampler2D _Mask;
sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
sampler2D _HeightSplatAll;
fixed4 _Color;
half _Shininess;

struct Input {
    float2 uv_MainTex : TEXCOORD0;
    float2 uv_BumpMap;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float2 uv_Splat2 : TEXCOORD3;
	float2 uv_Splat3 : TEXCOORD4;
};

void surf (Input IN, inout SurfaceOutput o) {

	half4 ColorTex = tex2D(_MainTex, IN.uv_MainTex);
	half4 MaskTex = tex2D(_Mask, IN.uv_MainTex);

	half4 Detail0 = tex2D(_Splat0, IN.uv_Splat0) * ColorTex * _Color;
	half4 Detail1 = tex2D(_Splat1, IN.uv_Splat1) * ColorTex * _Color;
	half4 Detail2 = tex2D(_Splat2, IN.uv_Splat2) * _Color;
	half4 Detail3 = tex2D(_Splat3, IN.uv_Splat3);

	half4 HeightSplatTex1 = tex2D(_HeightSplatAll, IN.uv_Splat0).r;
	half4 HeightSplatTex2 = tex2D(_HeightSplatAll, IN.uv_Splat1).g;
	half4 HeightSplatTex3 = tex2D(_HeightSplatAll, IN.uv_Splat2).b;
	half4 HeightSplatTex4 = tex2D(_HeightSplatAll, IN.uv_Splat3).a;

	float a0 = MaskTex.r;
	float a1 = MaskTex.g;
	float a2 = MaskTex.b;
	float a3 = MaskTex.a;

	float ma = (max(max(max(HeightSplatTex1.rgb + a0, HeightSplatTex2.rgb + a1), HeightSplatTex3.rgb + a2), HeightSplatTex4.rgb + a3)) - _Blur;

	float b0 = max(HeightSplatTex1.rgb + a0 - ma, 0);
	float b1 = max(HeightSplatTex2.rgb + a1 - ma, 0);
	float b2 = max(HeightSplatTex3.rgb + a2 - ma, 0);
	float b3 = max(HeightSplatTex4.rgb + a3 - ma, 0);

	float4 texture0 = Detail0;
	float4 texture1 = Detail1;
	float4 texture2 = Detail2;
	float4 texture3 = Detail3;
	fixed4 tex = (texture0 * b0 + texture1 * b1 + texture2 * b2 + texture3 * b3) / (b0 + b1 + b2 + b3);

	texture0 = tex2D(_Normal0, IN.uv_Splat0);
	texture1 = tex2D(_Normal1, IN.uv_Splat1);
	texture2 = tex2D(_Normal2, IN.uv_Splat2);
	texture3 = tex2D(_Normal3, IN.uv_Splat3);
	float4 mixnormal = (texture0 * b0 + texture1 * b1 + texture2 * b2 + texture3 * b3) / (b0 + b1 + b2 + b3);

	o.Normal = normalize(UnpackNormal(mixnormal)*1.5 + UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap)) * 3);

	o.Albedo = tex;
	//o.Alpha = tex.a * _Color.a;
	o.Gloss = tex.a;
	o.Specular = _Shininess;
	//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG


SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 400

    CGPROGRAM
    #pragma surface surf BlinnPhong nodynlightmap
	#pragma target 3.0
    ENDCG
}

FallBack "Legacy Shaders/Specular"
}
