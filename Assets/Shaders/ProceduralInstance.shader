// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Maxwell/ProceduralInstance" {
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
};

struct Transform{ 
  float4x4 ObjectToWorld;
  float4x4 MVP;
};

StructuredBuffer<Point> VertexBuffer;
StructuredBuffer<Transform> Transforms;

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

float4 ProceduralStandardSpecular_Deferred (SurfaceOutputStandardSpecular s, float3 viewDir, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
{
    // energy conservation
    half oneMinusReflectivity;
    s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);
    // RT0: diffuse color (rgb), occlusion (a) - sRGB rendertarget
    outGBuffer0 = half4(s.Albedo, s.Occlusion);

    // RT1: spec color (rgb), smoothness (a) - sRGB rendertarget
    outGBuffer1 = half4(s.Specular, s.Smoothness);

    // RT2: normal (rgb), --unused, very low precision-- (a)
    outGBuffer2 = half4(s.Normal * 0.5f + 0.5f, 0);
    float4 emission = float4(s.Emission, 1);

    return emission;
}



ENDCG

  Pass
{
  Blend zero one
  ZTest less
  CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf

float4 vert_surf (uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID) : SV_POSITION{
  float4 vertex = float4(VertexBuffer[vertexID].vertex, 1);
  return mul(Transforms[instanceID].MVP, vertex);
}                                                                                                                                                

void frag_surf (
) {

}
ENDCG
}

Pass {
stencil{
  Ref 255
  comp always
  pass replace
}
ZWrite off
CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma exclude_renderers nomrt
#define UNITY_PASS_DEFERRED

struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float3 worldPos : TEXCOORD1;
  float3 worldTangent : TEXCOORD2;
  float3 worldBinormal : TEXCOORD3;
  float3 worldNormal : TEXCOORD4;
  float3 worldViewDir : TEXCOORD8;
};
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID) {
  Point v = VertexBuffer[vertexID];
  v2f_surf o;
  Transform mat = Transforms[instanceID];
  float4 vertex = float4(v.vertex, 1);
  o.pos = mul(mat.MVP, vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

  o.worldPos = mul(mat.ObjectToWorld, vertex).xyz;
  float3x3 o2wNormal = (float3x3)mat.ObjectToWorld;
  o.worldNormal = mul(o2wNormal, v.normal);
  o.worldTangent =  mul(o2wNormal, v.tangent.xyz);
  float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  o.worldBinormal = cross(o.worldNormal, o.worldTangent) * tangentSign;

  float3 viewDirForLight = (UnityWorldSpaceViewDir(o.worldPos));
  o.worldViewDir = viewDirForLight;
  return o;
}
float4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
) {
  // prepare and unpack data
  Input surfIN;
  surfIN.uv_MainTex = IN.pack0.xy;
  float3 worldPos = IN.worldPos;
  float3 worldViewDir = normalize(IN.worldViewDir);
  SurfaceOutputStandardSpecular o;
  float3x3 wdMatrix= float3x3(normalize(IN.worldTangent), normalize(IN.worldBinormal), normalize(IN.worldNormal));
  // call surface function
  surf (surfIN, o);
  o.Normal = normalize(mul(o.Normal, wdMatrix));
  outEmission = ProceduralStandardSpecular_Deferred (o, worldViewDir, outGBuffer0, outGBuffer1, outGBuffer2); //GI neccessary here!
}
ENDCG
}


}
CustomEditor "SpecularShaderEditor"
}

