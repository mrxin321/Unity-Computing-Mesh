using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace GPUPipeline.Procedural
{
    /**
     * Implements of separate mesh drawing with one procedural
     */
    public struct ComputeMeshes
    {
        public List<ComputeBuffer> vertexBuffer;
        public ComputeBuffer mvpBuffer;
        public ComputeBuffer lastMvpBuffer;
        public ComputeShader matrixComputeShader;
        public List<Material> materials;
        private List<Matrix> matrices;
        private List<Matrix4x4> lastMatrices;
        public Transform[] transforms;

        private int shaderKernal;
        public ComputeMeshes(Transform[] transforms, ComputeShader matrixShader, int shaderKernal)
        {
            matrixComputeShader = matrixShader;
            this.transforms = transforms;
            this.shaderKernal = shaderKernal;
            matrices = new List<Matrix>();
            lastMatrices = new List<Matrix4x4>();
            /*
            vertexBuffer = vertex;
            mvpBuffer = mvp;
            lastMvpBuffer = last;
            material.SetBuffer("MTWBuffer", mvpBuffer);
            material.SetBuffer("VertexBuffer", vertexBuffer);
            material.SetBuffer("LastMTWBuffer", lastMvpBuffer);
            */
            vertexBuffer = new List<ComputeBuffer>();
            mvpBuffer = new ComputeBuffer(transforms.Length, Marshal.SizeOf(typeof(Matrix)), ComputeBufferType.Default);
            lastMvpBuffer = new ComputeBuffer(transforms.Length, Marshal.SizeOf(typeof(Matrix4x4)), ComputeBufferType.Default);
            materials = new List<Material>();
            Dictionary<Material, List<Point>> vertDict = new Dictionary<Material, List<Point>>();
            for (int i = 0; i < transforms.Length; ++i)
            {
                MeshRenderer renderer = transforms[i].GetComponent<MeshRenderer>();
                renderer.enabled = false;
                MeshFilter filter = transforms[i].GetComponent<MeshFilter>();
                Mesh mesh = filter.sharedMesh;
                for (int a = 0; a < mesh.subMeshCount; ++a)
                {
                    List<Point> points;
                    if (!vertDict.TryGetValue(renderer.sharedMaterials[a], out points))
                    {
                        points = new List<Point>();
                        vertDict[renderer.sharedMaterials[a]] = points;
                    }
                    int[] triangles = mesh.GetTriangles(a);
                    Vector3[] vertices = mesh.vertices;
                    Vector3[] normals = mesh.normals;
                    Vector2[] uv = mesh.uv;
                    Vector2[] uv2 = mesh.uv2;
                    Vector2[] uv3 = mesh.uv3;
                    Vector4[] tangents = mesh.tangents;
                    for (int z = 0; z < triangles.Length; ++z)
                    {
                        int j = triangles[z];
                        Point p;
                        p.vertex = vertices[j];
                        p.index = i + 0.01f;
                        p.normal = normals[j];
                        p.tangent = tangents[j];
                        p.texcoord = uv[j];
                        points.Add(p);
                    }
                }

            }
            foreach (var mat in vertDict.Keys)
            {
                materials.Add(mat);
                List<Point> vert = vertDict[mat];
                ComputeBuffer newVertexBuffer = new ComputeBuffer(vert.Count, Marshal.SizeOf(typeof(Point)), ComputeBufferType.Default);
                newVertexBuffer.SetData(vert);
                vertexBuffer.Add(newVertexBuffer);
            }
        }

        static int MTWBuffer = Shader.PropertyToID("MTWBuffer");
        static int VertexBuffer = Shader.PropertyToID("VertexBuffer");
        static int LastMTWBuffer = Shader.PropertyToID("LastMTWBuffer");
        static int _Matrices = Shader.PropertyToID("_Matrices");
        static int _ViewMatrix = Shader.PropertyToID("_ViewMatrix");
        static int _VPMatrix = Shader.PropertyToID("_VPMatrix");
        public void SetCurrentData(ref Matrix4x4 vp, ref Matrix4x4 v, bool updatePos)
        {
            if (updatePos)
            {
                lastMatrices.Clear();
                for (int i = 0; i < matrices.Count; ++i)
                {
                    lastMatrices.Add(matrices[i].MTW);
                }
                lastMvpBuffer.SetData(lastMatrices);
                matrices.Clear();
                for (int i = 0; i < transforms.Length; ++i)
                {
                    Matrix matrix;
                    matrix.MTW = transforms[i].localToWorldMatrix;
                    matrix.MV = new Matrix4x4();
                    matrix.MVP = new Matrix4x4();
                    matrices.Add(matrix);
                }
                mvpBuffer.SetData(matrices);
            }
            matrixComputeShader.SetBuffer(shaderKernal, _Matrices, mvpBuffer);
            matrixComputeShader.SetMatrix(_ViewMatrix, v);
            matrixComputeShader.SetMatrix(_VPMatrix, vp);
            matrixComputeShader.Dispatch(shaderKernal, mvpBuffer.count, 1, 1);
        }

        public void Draw(CommandBuffer beforeGBuffer, CommandBuffer beforeImageOpaque)
        {
            for (int i = 0; i < materials.Count; ++i)
            {
                var vertexBuf = vertexBuffer[i];
                int count = vertexBuf.count;
                var mat = materials[i];
                beforeGBuffer.SetGlobalBuffer(MTWBuffer, mvpBuffer);
                beforeGBuffer.SetGlobalBuffer(VertexBuffer, vertexBuf);
                beforeGBuffer.DrawProcedural(Matrix4x4.zero, mat, 0, MeshTopology.Triangles, count);
                beforeImageOpaque.SetGlobalBuffer(MTWBuffer, mvpBuffer);
                beforeImageOpaque.SetGlobalBuffer(VertexBuffer, vertexBuffer[i]);
                beforeImageOpaque.SetGlobalBuffer(LastMTWBuffer, lastMvpBuffer);
                beforeImageOpaque.DrawProcedural(Matrix4x4.zero, mat, 1, MeshTopology.Triangles, count);
            }
        }

        public void Draw(CommandBuffer beforeGBuffer)
        {
            for (int i = 0; i < materials.Count; ++i)
            {
                var vertexBuf = vertexBuffer[i];
                int count = vertexBuf.count;
                var mat = materials[i];
                beforeGBuffer.SetGlobalBuffer(MTWBuffer, mvpBuffer);
                beforeGBuffer.SetGlobalBuffer(VertexBuffer, vertexBuf);
                beforeGBuffer.DrawProcedural(Matrix4x4.zero, mat, 0, MeshTopology.Triangles, count);
            }
        }

        public void Dispose()
        {
            foreach (var i in vertexBuffer)
            {
                i.Dispose();
            }
            mvpBuffer.Dispose();
            lastMvpBuffer.Dispose();
        }
    }
}