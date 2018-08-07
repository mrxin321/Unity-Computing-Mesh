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