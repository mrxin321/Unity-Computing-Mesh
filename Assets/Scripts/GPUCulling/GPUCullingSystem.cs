using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
namespace GPUPipeline.Culling
{
    public static partial class PipelineSystem
    {
        #region CONST_VALUE
        const int INTSIZE = 4;
        const int THREADGROUPCOUNT = 128;
        const int MATRIXSIZE = 64;
        const int TRANSFORMSIZE = 132;
        #endregion
        #region STATIC_FUNCTION
        public static uint[] currentDrawSize = new uint[] { 0 };
        private static Plane[] planes = new Plane[6];
        private static Vector4[] planesVector = new Vector4[6];
        public static RenderTargetIdentifier[] gBufferIdentifier = new RenderTargetIdentifier[]
            {
                    BuiltinRenderTextureType.GBuffer0,
                    BuiltinRenderTextureType.GBuffer1,
                    BuiltinRenderTextureType.GBuffer2,
                    BuiltinRenderTextureType.GBuffer3
            };
        public static void GetCullingPlanes(ref Matrix4x4 vp, Plane[] cullingPlanes)
        {
            Matrix4x4 invVp = vp.inverse;
            Vector3 nearLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 1));
            Vector3 nearLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 1));
            Vector3 nearRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 1));
            Vector3 nearRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 1));
            Vector3 farLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 0));
            Vector3 farLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 0));
            Vector3 farRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 0));
            Vector3 farRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 0));
            //Near
            cullingPlanes[0] = new Plane(nearRightTop, nearRightButtom, nearLeftButtom);
            //Up
            cullingPlanes[1] = new Plane(farLeftTop, farRightTop, nearRightTop);
            //Down
            cullingPlanes[2] = new Plane(nearRightButtom, farRightButtom, farLeftButtom);
            //Left
            cullingPlanes[3] = new Plane(farLeftButtom, farLeftTop, nearLeftTop);
            //Right
            cullingPlanes[4] = new Plane(farRightButtom, nearRightButtom, nearRightTop);
            //Far
            cullingPlanes[5] = new Plane(farLeftButtom, farRightButtom, farRightTop);
        }

        public static void SetCullingBuffer(ref CullingBuffers buffers)
        {
            buffers.cullingShader.SetBuffer(buffers.csmainKernel, ShaderIDs.allBounds, buffers.allBoundBuffer);
            buffers.cullingShader.SetBuffer(buffers.csmainKernel, ShaderIDs.Transforms, buffers.resultBuffer);
            buffers.cullingShader.SetBuffer(buffers.csmainKernel, ShaderIDs.sizeBuffer, buffers.sizeBuffer);
            buffers.sizeBuffer.SetData(ComputeShaderUtility.zero);
        }

        public static uint GetCullingBufferResult(ref CullingBuffers buffers)
        {
            buffers.sizeBuffer.GetData(currentDrawSize);
            return currentDrawSize[0];
        }

        /// <summary>
        /// Run Culling With Compute Shader
        /// </summary>
        /// <param name="view"></param> World To View Matrix
        /// <param name="proj"></param> View To NDC Matrix
        /// <param name="buffers"></param> Buffers
        public static void RunCulling(ref Matrix4x4 view, ref Matrix4x4 proj, ref Matrix4x4 rtProj, ref CullingBuffers buffers)
        {
            Matrix4x4 vp = proj * view;
            buffers.cullingShader.SetMatrix(ShaderIDs._VPMatrix, rtProj * view);
            GetCullingPlanes(ref vp, planes);
            for (int i = 0; i < 6; ++i)
            {
                Vector3 normal = planes[i].normal;
                float distance = planes[i].distance;
                planesVector[i] = new Vector4(normal.x, normal.y, normal.z, distance);
            }
            buffers.cullingShader.SetVectorArray(ShaderIDs.planes, planesVector);
            ComputeShaderUtility.Dispatch(buffers.cullingShader, buffers.csmainKernel, buffers.count, THREADGROUPCOUNT);
        }

        public static void Draw(ref CullingBuffers cullingBuffers, ref ProceduralInstance procedural)
        {
            int instanceCount = (int)GetCullingBufferResult(ref cullingBuffers);
            if (instanceCount == 0) return;
            procedural.geometryCommandBuffer.SetGlobalBuffer(ShaderIDs.Transforms, cullingBuffers.resultBuffer);
            procedural.geometryCommandBuffer.SetGlobalBuffer(ShaderIDs.VertexBuffer, cullingBuffers.vertexBuffer);
            procedural.motionVectorsCommandBuffer.SetGlobalBuffer(ShaderIDs.lastFrameMatrices, cullingBuffers.lastFrameMatricesBuffer);
            procedural.motionVectorsCommandBuffer.SetGlobalBuffer(ShaderIDs.Transforms, cullingBuffers.resultBuffer);
            procedural.motionVectorsCommandBuffer.SetGlobalBuffer(ShaderIDs.VertexBuffer, cullingBuffers.vertexBuffer);
            procedural.geometryCommandBuffer.DrawProcedural(Matrix4x4.identity, procedural.proceduralMaterial, 0, MeshTopology.Triangles, cullingBuffers.vertexBuffer.count, instanceCount);
            procedural.geometryCommandBuffer.DrawProcedural(Matrix4x4.identity, procedural.proceduralMaterial, 1, MeshTopology.Triangles, cullingBuffers.vertexBuffer.count, instanceCount);
            procedural.motionVectorsCommandBuffer.DrawProcedural(Matrix4x4.identity, procedural.proceduralMaterial, 3, MeshTopology.Triangles, cullingBuffers.vertexBuffer.count, instanceCount);
        }

        public static void DrawNoOcclusion(ref CullingBuffers cullingBuffers, ref ProceduralInstance procedural)
        {
            int instanceCount = (int)GetCullingBufferResult(ref cullingBuffers);
            if (instanceCount == 0) return;
            procedural.geometryCommandBuffer.SetGlobalBuffer(ShaderIDs.Transforms, cullingBuffers.resultBuffer);
            procedural.geometryCommandBuffer.SetGlobalBuffer(ShaderIDs.VertexBuffer, cullingBuffers.vertexBuffer);
            procedural.motionVectorsCommandBuffer.SetGlobalBuffer(ShaderIDs.lastFrameMatrices, cullingBuffers.lastFrameMatricesBuffer);
            procedural.motionVectorsCommandBuffer.SetGlobalBuffer(ShaderIDs.Transforms, cullingBuffers.resultBuffer);
            procedural.motionVectorsCommandBuffer.SetGlobalBuffer(ShaderIDs.VertexBuffer, cullingBuffers.vertexBuffer);
            procedural.geometryCommandBuffer.DrawProcedural(Matrix4x4.identity, procedural.proceduralMaterial, 2, MeshTopology.Triangles, cullingBuffers.vertexBuffer.count, instanceCount);
            procedural.motionVectorsCommandBuffer.DrawProcedural(Matrix4x4.identity, procedural.proceduralMaterial, 3, MeshTopology.Triangles, cullingBuffers.vertexBuffer.count, instanceCount);

        }

        public static void DrawNoMotionVectors(ref CullingBuffers cullingBuffers, ref ProceduralInstance procedural)
        {
            int instanceCount = (int)GetCullingBufferResult(ref cullingBuffers);
            if (instanceCount == 0) return;
            procedural.geometryCommandBuffer.SetGlobalBuffer(ShaderIDs.Transforms, cullingBuffers.resultBuffer);
            procedural.geometryCommandBuffer.SetGlobalBuffer(ShaderIDs.VertexBuffer, cullingBuffers.vertexBuffer);
            procedural.geometryCommandBuffer.DrawProcedural(Matrix4x4.identity, procedural.proceduralMaterial, 0, MeshTopology.Triangles, cullingBuffers.vertexBuffer.count, instanceCount);
            procedural.geometryCommandBuffer.DrawProcedural(Matrix4x4.identity, procedural.proceduralMaterial, 1, MeshTopology.Triangles, cullingBuffers.vertexBuffer.count, instanceCount);
        }

        public static void DrawNoMotionVectorsNoOcclusion(ref CullingBuffers cullingBuffers, ref ProceduralInstance procedural)
        {
            int instanceCount = (int)GetCullingBufferResult(ref cullingBuffers);
            if (instanceCount == 0) return;
            procedural.geometryCommandBuffer.SetGlobalBuffer(ShaderIDs.Transforms, cullingBuffers.resultBuffer);
            procedural.geometryCommandBuffer.SetGlobalBuffer(ShaderIDs.VertexBuffer, cullingBuffers.vertexBuffer);
            procedural.geometryCommandBuffer.DrawProcedural(Matrix4x4.identity, procedural.proceduralMaterial, 2, MeshTopology.Triangles, cullingBuffers.vertexBuffer.count, instanceCount);
        }

        public static void ClearBuffer(ref ProceduralInstance procedural)
        {
            procedural.geometryCommandBuffer.Clear();
            procedural.motionVectorsCommandBuffer.Clear();
            procedural.geometryCommandBuffer.SetRenderTarget(gBufferIdentifier, BuiltinRenderTextureType.CameraTarget);
            procedural.motionVectorsCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.MotionVectors, BuiltinRenderTextureType.CameraTarget);
        }

        public static void InitBuffers(ref CullingBuffers buffers, Transform[] transforms, Mesh mesh)
        {
            buffers.sizeBuffer = new ComputeBuffer(1, INTSIZE);
            buffers.allBoundBuffer = new ComputeBuffer(transforms.Length, Bounds.SIZE);
            buffers.resultBuffer = new ComputeBuffer(transforms.Length, TRANSFORMSIZE);
            buffers.csmainKernel = buffers.cullingShader.FindKernel("CSMain");
            buffers.lastMatricesKernel = buffers.cullingShader.FindKernel("GetLast");
            buffers.lastFrameMatricesBuffer = new ComputeBuffer(transforms.Length, MATRIXSIZE);
            buffers.count = transforms.Length;
            Bounds[] allBounds = new Bounds[transforms.Length];
            for (int i = 0; i < allBounds.Length; ++i)
            {
                allBounds[i].extent = Vector3.one * 0.5f;
                allBounds[i].localToWorldMatrix = transforms[i].localToWorldMatrix;
            }
            buffers.allBoundBuffer.SetData(allBounds);

            Vector3[] allVertex = mesh.vertices;
            int[] allIndices = mesh.triangles;
            Vector2[] allUVs = mesh.uv;
            Vector4[] tangents = mesh.tangents;
            Vector3[] normals = mesh.normals;
            Point[] points = new Point[allIndices.Length];
            for (int i = 0; i < allIndices.Length; ++i)
            {
                Point p;
                int index = allIndices[i];
                p.normal = normals[index];
                p.vertex = allVertex[index];
                p.texcoord = allUVs[index];
                p.tangent = tangents[index];
                points[i] = p;
            }
            buffers.vertexBuffer = new ComputeBuffer(points.Length, Point.SIZE);
            buffers.vertexBuffer.SetData(points);
        }

        public static void SetLastFrameMatrix(ref CullingBuffers cullingBuffers)
        {
            cullingBuffers.cullingShader.SetBuffer(cullingBuffers.lastMatricesKernel, ShaderIDs.allBounds, cullingBuffers.allBoundBuffer);
            cullingBuffers.cullingShader.SetBuffer(cullingBuffers.lastMatricesKernel, ShaderIDs.lastFrameMatrices, cullingBuffers.lastFrameMatricesBuffer);
            ComputeShaderUtility.Dispatch(cullingBuffers.cullingShader, cullingBuffers.lastMatricesKernel, cullingBuffers.lastFrameMatricesBuffer.count, THREADGROUPCOUNT);
        }

        public static void InitProceduralInstance(ref ProceduralInstance procedural)
        {
            procedural.geometryCommandBuffer = new CommandBuffer();
            procedural.motionVectorsCommandBuffer = new CommandBuffer();
        }

        public static void Dispose(ref CullingBuffers buffers, ref ProceduralInstance procedural)
        {
            buffers.allBoundBuffer.Dispose();
            buffers.resultBuffer.Dispose();
            buffers.sizeBuffer.Dispose();
            buffers.vertexBuffer.Dispose();
            procedural.geometryCommandBuffer.Dispose();
            procedural.motionVectorsCommandBuffer.Dispose();
            buffers.lastFrameMatricesBuffer.Dispose();
        }

        #endregion
    }
}