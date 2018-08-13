using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
namespace GPUPipeline.Culling
{
    #region STRUCTURE
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
        /// Last Frame's matrix buffer
        /// For Motion Vector
        /// </summary>
        public ComputeBuffer lastFrameMatricesBuffer;
        /// <summary>
        /// Culling Results with MVP and ObjectToWorld Matrix
        /// </summary>
        public ComputeBuffer resultBuffer;
        /// <summary>
        /// Main Culling Compute Shader
        /// </summary>
        public ComputeShader cullingShader;
        /// <summary>
        /// Computer Shader Target Kernal
        /// </summary>
        [System.NonSerialized]
        public int csmainKernel;
        [System.NonSerialized]
        public int lastMatricesKernel;
        /// <summary>
        /// Culling Target Count
        /// </summary>
        [System.NonSerialized]
        public int count;
        public ComputeBuffer vertexBuffer;
    }
    [System.Serializable]
    public struct ProceduralInstance
    {
        public Material proceduralMaterial;
        public CommandBuffer geometryCommandBuffer;
        public CommandBuffer motionVectorsCommandBuffer;
    }
    public struct Point
    {
        public Vector3 vertex;  //12
        public Vector4 tangent; //16
        public Vector3 normal;  //12
        public Vector2 texcoord;    //8
        public const int SIZE = 48;
    } //48
    public struct Bounds
    {
        public Vector3 extent;  //12
        public Matrix4x4 localToWorldMatrix;    //64
        public const int SIZE = 76;
    }//76
    #endregion
}