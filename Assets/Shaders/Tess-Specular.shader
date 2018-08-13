 Shader "Tessellation/Standard Specular" {
	Properties {
		_MinDist("Tess Min Distance", float) = 10
		_MaxDist("Tess Max Distance", float) = 25
		_Tessellation("Tessellation", Range(1,63)) = 1
		_Phong ("Phong Strengh", Range(0,1)) = 0.5
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_NormalScale("Normal Scale", float) = 1
		_SpecularMap("Specular Map", 2D) = "white"{}
		_HeightMap("Vertex Map", 2D) = "black"{}
		//_HeightScale("Height Scale", Range(-4,4)) = 1
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_OcclusionMap("Occlusion Map", 2D) = "white"{}
		_Occlusion("Occlusion Scale", Range(0,1)) = 1
		_SpecularColor("Specular Color",Color) = (0.2,0.2,0.2,1)
		_EmissionColor("Emission Color", Color) = (0,0,0,1)
		_VertexScale("Vertex Scale", Range(0,3)) = 0.1
		_VertexOffset("Vertex Offset", Range(-1,1)) = 0
		_DetailAlbedo("Detail Albedo(RGB) Mask(A)", 2D) = "black"{}
		_AlbedoBlend("Albedo Blend Rate", Range(0,1)) = 0.3
		_DetailBump("Detail Bump(RGB) Mask(A)", 2D) = "bump"{}
		_BumpBlend("Bump Blend Rate", Range(0,1)) = 0.3

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
CGINCLUDE

#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityMetaPass.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#include "CGINC/Tessellation.cginc"

#pragma shader_feature USE_NORMAL
#pragma shader_feature USE_SPECULAR
#pragma shader_feature USE_VERTEX
#pragma shader_feature USE_PHONG
#pragma shader_feature USE_OCCLUSION
#pragma shader_feature USE_ALBEDO
#pragma shader_feature USE_DETAILALBEDO
#pragma shader_feature USE_DETAILNORMAL
		struct Input {
			float2 uv_MainTex;
			#if USE_DETAILALBEDO
			float2 uv_DetailAlbedo;
			#endif
			#if USE_DETAILNORMAL
			float2 uv_DetailNormal;
			#endif
		};
		
struct appdata_tess{
    float4 vertex : POSITION;
    float4 texcoord : TEXCOORD0;
};

        float4 _SpecularColor;
        float4 _EmissionColor;

		float _HeightScale;
		float _Phong;
		float _NormalScale;
		float _Occlusion;
		float _VertexScale;
		float _VertexOffset;
		sampler2D _DetailAlbedo;
		float _AlbedoBlend;
		sampler2D _DetailBump;
		float _BumpBlend;
		float4 _DetailAlbedo_ST;
		float4 _DetailBump_ST;

		sampler2D _BumpMap;
		sampler2D _SpecularMap;
		sampler2D _HeightMap;
		sampler2D _OcclusionMap;

		
		sampler2D _MainTex;



		half _Glossiness;
		float4 _Color;

		inline void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			// Albedo comes from a texture tinted by color
			float2 uv = IN.uv_MainTex;// - parallax_mapping(IN.uv_MainTex,IN.viewDir);
			#if USE_ALBEDO
			float4 c = tex2D (_MainTex, uv) * _Color;

			#if USE_DETAILALBEDO
			float4 dA = tex2D(_DetailAlbedo, IN.uv_DetailAlbedo);
			c.rgb = lerp(c.rgb, dA.rgb, _AlbedoBlend);
			#endif
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			#else
			#if USE_DETAILALBEDO
			float4 dA = tex2D(_DetailAlbedo, IN.uv_DetailAlbedo);
			o.Albedo.rgb = lerp(1, dA.rgb, _AlbedoBlend) * _Color;
			#else
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
			#endif
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
			#if USE_DETAILNORMAL
			float4 dN =  tex2D(_DetailBump,IN.uv_DetailNormal);
			o.Normal = lerp(o.Normal, UnpackNormal(dN), _BumpBlend);
			#endif
			o.Normal.xy *= _NormalScale;

			#else
			o.Normal = float3(0,0,1);
			#endif
			o.Emission = _EmissionColor;

		}

inline InternalTessInterp_appdata_full tessvert_surf (appdata_tess v) {
  InternalTessInterp_appdata_full o;
  o.vertex = v.vertex;
  o.texcoord = v.texcoord;
  return o;
}

inline void vert(inout appdata_tess v){
	v.vertex.xyz += ((tex2Dlod(_HeightMap, v.texcoord).r + _VertexOffset) * _VertexScale) * float3(0,1,0);

}
ENDCG
	
	
Pass {
ZTest less

CGPROGRAM
// compile directives
#pragma vertex tessvert_surf
#pragma fragment frag_surf
#pragma hull hs_surf
#pragma domain ds_surf
#pragma target 5.0

#pragma exclude_renderers nomrt
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_prepassfinal
#define UNITY_PASS_DEFERRED

struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float3 worldPos : TEXCOORD1;

    #if USE_DETAILALBEDO
  float2 pack1 : TEXCOORD2;
  #endif

  #if USE_DETAILNORMAL
  float2 pack2 : TEXCOORD3;
  #endif
  float3 worldViewDir : TEXCOORD4;
};
float4 _MainTex_ST;

// vertex shader
inline v2f_surf vert_surf (appdata_tess v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);

  o.pos = mul(UNITY_MATRIX_VP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    #if USE_DETAILALBEDO
 o.pack1 = TRANSFORM_TEX(v.texcoord,_DetailAlbedo);
  #endif
  #if USE_DETAILNORMAL
  o.pack2 = TRANSFORM_TEX(v.texcoord, _DetailBump);
  #endif
  o.worldPos = v.vertex.xyz;
  float3 viewDirForLight = (UnityWorldSpaceViewDir(o.worldPos));
	o.worldViewDir = viewDirForLight;
  return o;
}

// tessellation domain shader
[UNITY_domain("tri")]
inline v2f_surf ds_surf (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata_full,3> vi, float3 bary : SV_DomainLocation) {
  appdata_tess v;
  v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
  v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
    #if USE_VERTEX
  vert(v);
  #endif
  v2f_surf o = vert_surf (v);
  return o;
}

// fragment shader
inline void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
) {
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  
  surfIN.uv_MainTex = IN.pack0.xy;
    #if USE_DETAILALBEDO
  surfIN.uv_DetailAlbedo = IN.pack1;
  #endif

  #if USE_DETAILNORMAL
  surfIN.uv_DetailNormal = IN.pack2;
  #endif
  float3 worldPos = IN.worldPos;
  float3 worldViewDir = normalize(IN.worldViewDir);
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
  #else
  SurfaceOutputStandardSpecular o;
  #endif
  // call surface function
  surf (surfIN, o);
  o.Normal.y *= unity_WorldTransformParams.w;
  o.Normal = normalize(o.Normal);


  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  
  outEmission = LightingStandardSpecular_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);

  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}

ENDCG

}

	}
CustomEditor "SpecularShaderEditor"
}

