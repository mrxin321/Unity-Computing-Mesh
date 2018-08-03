using UnityEngine;
using System;
using UnityEngine.Rendering;
namespace GPUPipeline.Procedural
{
    public struct Point
    {
        public Vector3 vertex;
        public Vector4 tangent;
        public Vector3 normal;
        public Vector2 texcoord;
        public float index;
    }

    public struct Matrix
    {
        public Matrix4x4 MTW;
        public Matrix4x4 MVP;
        public Matrix4x4 MV;
    }

    public struct GeometryDrawCommand
    {
        public Action<CommandBuffer, CommandBuffer> drawWithMotionVectors;
        public Action<CommandBuffer> drawWithoutMotionVectors;
        public ProceduralGeometry parent;
    }
}
namespace GPUPipeline.Culling
{
    [System.Serializable]
    public struct CullingBuffers
    {
        /// <summary>
        /// All Boundings
        /// </summary>
        public ComputeBuffer allBoundBuffer;
        /// <summary>
        /// Buffer of size, size should be 1
        /// </summary>
        public ComputeBuffer sizeBuffer;
        /// <summary>
        /// Culling Results with MVP and ObjectToWorld Matrix
        /// </summary>
        public ComputeBuffer resultBuffer;
        /// <summary>
        /// 
        /// </summary>
        public ComputeShader cullingShader;
        /// <summary>
        /// Computer Shader Target Kernal
        /// </summary>
        [System.NonSerialized]
        public int kernal;
        /// <summary>
        /// Culling Target Count
        /// </summary>
        [System.NonSerialized]
        public int count;
    }
    [System.Serializable]
    public struct ProceduralInstance
    {
        public Material proceduralMaterial;
        public Material motionVectorMaterial;
        public ComputeBuffer vertexBuffer;
        public CommandBuffer geometryCommandBuffer;
        public CommandBuffer motionVectorsCommandBuffer;
    }
    public struct Point
    {
        public Vector3 vertex;  //12
        public Vector4 tangent; //16
        public Vector3 normal;  //12
        public Vector2 texcoord;    //8
    }; //48
    public struct Bounds
    {
        public Vector3 extent;  //12
        public Matrix4x4 localToWorldMatrix;    //64
    }; //76
}
namespace GPUPipeline.LOD
{
    public struct TransformInfo
    {
        public uint lodLevel;   //4
        public Vector3 position;        //12
        public Vector4 lodSize;         //16
        public const int SIZE = 32;
    };

    public struct UpdateInfo
    {
        public uint objIndex;   //4
        public Vector3 position;    //12
        public const int SIZE = 16;
    };
    [System.Serializable]
    public struct LodBuffers
    {
        public ComputeBuffer lodGroupBuffer;
        public ComputeBuffer changedBuffer;
        public ComputeBuffer sizeBuffer;
        public ComputeShader lodShader;
        [System.NonSerialized]
        public int calculateKernel;
        [System.NonSerialized]
        public int moveKernel;
        public const int CALCULATETHREADGROUP = 128;
        public const int MOVETHREADGROUP = 16;
    }

    public struct uint2
    {
        public uint x;
        public uint y;
        public const int SIZE = 8;
    }
}