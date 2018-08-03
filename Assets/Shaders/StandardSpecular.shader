// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Maxwell/Standard Specular" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_NormalScale("Normal Scale", float) = 1
		_SpecularMap("Specular Map", 2D) = "white"{}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_OcclusionMap("Occlusion Map", 2D) = "white"{}
		_Occlusion("Occlusion Scale", Range(0,1)) = 1
		_SpecularColor("Specular Color",Color) = (0.2,0.2,0.2,1)
		_EmissionColor("Emission Color", Color) = (0,0,0,1)

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
CGINCLUDE
#pragma target 5.0
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityMetaPass.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#pragma shader_feature USE_NORMAL
#pragma shader_feature USE_SPECULAR
#pragma shader_feature USE_OCCLUSION
#pragma shader_feature USE_ALBEDO
		struct Input {
			float2 uv_MainTex;
		};

struct Point{
    float3 vertex;
    float4 tangent;
    float3 normal;
    float2 texcoord;
    float index;
};

struct Matrix{
  float4x4 MTW;
  float4x4 MVP;
  float4x4 MV;
};

StructuredBuffer<Point> VertexBuffer;
StructuredBuffer<Matrix> MTWBuffer;
StructuredBuffer<float4x4> LastMTWBuffer;

    float4 _SpecularColor;
    float4 _EmissionColor;
		float _NormalScale;
		float _Occlusion;
		float _VertexScale;
		float _VertexOffset;
		sampler2D _BumpMap;
		sampler2D _SpecularMap;
		sampler2D _OcclusionMap;
		sampler2D _MainTex;



		half _Glossiness;
		float4 _Color;

		inline void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			// Albedo comes from a texture tinted by color
			float2 uv = IN.uv_MainTex;// - parallax_mapping(IN.uv_MainTex,IN.viewDir);
			#if USE_ALBEDO
			float4 c = tex2D (_MainTex, uv) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			#else
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
			#endif
			#if USE_OCCLUSION
			o.Occlusion = lerp(1, tex2D(_OcclusionMap, IN.uv_MainTex).r, _Occlusion);
			#else
			o.Occlusion = 1;
			#endif
			#if USE_SPECULAR
			float4 spec = tex2D(_SpecularMap, IN.uv_MainTex);
			o.Specular = _SpecularColor  * spec.rgb;
			o.Smoothness = _Glossiness * spec.a;
			#else
			o.Specular = _SpecularColor;
			o.Smoothness = _Glossiness;
			#endif
			#if USE_NORMAL
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			o.Normal.xy *= _NormalScale;
			#else
			o.Normal = float3(0,0,1);
			#endif
			o.Emission = _EmissionColor * 100;
		}

ENDCG

Pass {
  stencil{
  Ref 255
  comp always
  pass replace
}
CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma exclude_renderers nomrt
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_prepassfinal
#define UNITY_PASS_DEFERRED

struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float3 worldPos : TEXCOORD1;
  float3 worldTangent : TEXCOORD2;
  float3 worldBinormal : TEXCOORD3;
  float3 worldNormal : TEXCOORD4;
  half3 sh : TEXCOORD7; // SH
  float3 worldViewDir : TEXCOORD8;
};
float4 _MainTex_ST;

// vertex shader
inline v2f_surf vert_surf (uint vertexID : SV_VertexID) {
  Point v = VertexBuffer[vertexID];
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  Matrix mat = MTWBuffer[v.index];
  float4 vertex = float4(v.vertex, 1);
  o.pos = mul(mat.MVP, vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

  o.worldPos = mul(mat.MTW, vertex).xyz;
  o.worldNormal = mul((float3x3)mat.MTW, v.normal);
  o.worldTangent =  mul((float3x3)mat.MTW, v.tangent.xyz);
  float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  o.worldBinormal = cross(o.worldNormal, o.worldTangent) * tangentSign;

  float3 viewDirForLight = (UnityWorldSpaceViewDir(o.worldPos));
  o.sh = ShadeSHPerVertex (o.worldNormal, o.sh);

	o.worldViewDir = viewDirForLight;
  return o;
}
float4 unity_Ambient;

// fragment shader
inline void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
) {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  
  surfIN.uv_MainTex = IN.pack0.xy;

  float3 worldPos = IN.worldPos;
  float3 worldViewDir = normalize(IN.worldViewDir);
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
  #else
  SurfaceOutputStandardSpecular o;
  #endif
  float3x3 wdMatrix= float3x3( (IN.worldTangent), (IN.worldBinormal), (IN.worldNormal));    //Un normalized only for Cube!
  // call surface function
  surf (surfIN, o);
  o.Normal = normalize(mul(o.Normal, wdMatrix));
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
/*
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = 1;
  giInput.lightmapUV = 0.0;
  giInput.ambient = IN.sh;
  LightingStandardSpecular_GI(o, giInput, gi);
*/
  // call lighting function to output g-buffer
  outEmission = LightingStandardSpecular_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2); //GI neccessary here!
}
ENDCG
}

Pass {
ZWrite off
CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
// vertex shader
struct v2f_motionVector{
  float4 pos : SV_POSITION;
  float4 lastPos : TEXCOORD0;
  float4 worldPos : TEXCOORD1;
};
float4x4 LAST_VP_MATRIX;
v2f_motionVector vert_surf (uint vertexID : SV_VertexID){
  Point v = VertexBuffer[vertexID];
  float4x4 last = LastMTWBuffer[v.index];
  Matrix curt = MTWBuffer[v.index];
  v2f_motionVector o;
  float4 vertex = float4(v.vertex, 1);
  o.pos = mul(curt.MVP, vertex);
  o.worldPos = mul(curt.MTW, vertex);
  o.worldPos = mul(UNITY_MATRIX_VP, o.worldPos);
  o.lastPos = mul(last, vertex);
  o.lastPos = mul(LAST_VP_MATRIX, o.lastPos);
  return o;
}

// fragment shader
float4 frag_surf (v2f_motionVector i) : SV_Target {
    float4 hPos = float4(i.worldPos.xy, i.lastPos.xy) / float4(i.worldPos.ww, i.lastPos.ww);
    float4 vPos = hPos * 0.5 + 0.5;
    #if UNITY_UV_STARTS_AT_TOP
    vPos.yw = 1 - vPos.yw;
    #endif
    return float4(vPos.xy - vPos.zw , 0, 1);
}
ENDCG
  }
}
CustomEditor "SpecularShaderEditor"
}

