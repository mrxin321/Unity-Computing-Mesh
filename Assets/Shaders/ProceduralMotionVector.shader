// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Maxwell/ProceduralMotionVector" {
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
CGINCLUDE
#pragma target 5.0
#include "HLSLSupport.cginc"
#include "UnityCG.cginc"

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

ENDCG


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
  float4 currentPos : TEXCOORD1;
};
float4x4 LAST_VP_MATRIX;
v2f_motionVector vert_surf (uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID){
  Transform curt = Transforms[instanceID];
  v2f_motionVector o;
  float4 vertex = float4(VertexBuffer[vertexID].vertex, 1);
  o.pos = mul(curt.MVP, vertex);
  float4 worldPos = mul(curt.ObjectToWorld, vertex);
  o.currentPos = o.pos;
  o.lastPos = mul(LAST_VP_MATRIX, worldPos);
  return o;
}

// fragment shader
float2 frag_surf (v2f_motionVector i) : SV_Target {
    float4 hPos = float4(i.currentPos.xy, i.lastPos.xy) / float4(i.currentPos.ww, i.lastPos.ww);
    float4 vPos = hPos * 0.5 + 0.5;
    #if UNITY_UV_STARTS_AT_TOP
    vPos.yw = 1 - vPos.yw;
    #endif
    return vPos.xy - vPos.zw;
}
ENDCG
  }


}
CustomEditor "SpecularShaderEditor"
}

