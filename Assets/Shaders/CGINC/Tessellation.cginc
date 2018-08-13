		float _MinDist;
		float _MaxDist;
		float _Tessellation;
struct InternalTessInterp_appdata_full {
  float4 vertex : INTERNALTESSPOS;
  float4 texcoord : TEXCOORD0;
};


inline float3 UnityCalcTriEdgeTessFactors (float3 triVertexFactors)
{
    float3 tess;
    tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
    tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
    tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
    return tess;
}


inline float UnityCalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess)
{
    float3 wpos = mul(unity_ObjectToWorld,vertex).xyz;
    float dist = distance (wpos, _WorldSpaceCameraPos);
    float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
    return f;
}

inline float3 tessDist (float4 v0, float4 v1, float4 v2)
{
    float3 f;
    f.x = UnityCalcDistanceTessFactor (v0,_MinDist,_MaxDist,_Tessellation);
    f.y = UnityCalcDistanceTessFactor (v1,_MinDist,_MaxDist,_Tessellation);
    f.z = UnityCalcDistanceTessFactor (v2,_MinDist,_MaxDist,_Tessellation);
   	return UnityCalcTriEdgeTessFactors (f);

}

inline UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata_full,3> v) {
  UnityTessellationFactors o;
  float3 tf = (tessDist(v[0].vertex, v[1].vertex, v[2].vertex));
  float3 objCP = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1)).xyz;
  bool3 dir = 0 < float3(dot(normalize(objCP - v[0].vertex), float3(0,1,0)), dot(normalize(objCP - v[1].vertex), float3(0,1,0)), dot(normalize(objCP - v[2].vertex), float3(0,1,0)));
  tf = (dir.x + dir.y + dir.z) ? tf : 0;
  o.edge[0] = tf.x;
  o.edge[1] = tf.y;
  o.edge[2] = tf.z;
  o.inside = (tf.x + tf.y + tf.z) * 0.33333333;
  return o;
}

[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
inline InternalTessInterp_appdata_full hs_surf (InputPatch<InternalTessInterp_appdata_full,3> v, uint id : SV_OutputControlPointID) {
  
  return v[id];
}