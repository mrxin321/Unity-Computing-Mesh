using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GPUPipeline.LOD
{
    public static class LodSystem
    {
        public static void InitLodBuffers(ref LodBuffers buffers, int size)
        {
            buffers.calculateKernel = buffers.lodShader.FindKernel("Calculate");
            buffers.moveKernel = buffers.lodShader.FindKernel("Move");
            buffers.sizeBuffer = new ComputeBuffer(1, 4);
            buffers.lodGroupBuffer = new ComputeBuffer(size, TransformInfo.SIZE);
            buffers.changedBuffer = new ComputeBuffer(size, uint2.SIZE);
        }

        public static void DisposeLodBuffers(ref LodBuffers buffers)
        {
            buffers.changedBuffer.Dispose();
            buffers.lodGroupBuffer.Dispose();
            buffers.sizeBuffer.Dispose();
        }

        public static void DispatchLodCalculate(ref LodBuffers buffers, Vector3 cameraPosition)
        {
            buffers.sizeBuffer.SetData(ComputeShaderUtility.zero);
            buffers.lodShader.SetVector(ShaderIDs._CameraPos, cameraPosition);
            buffers.lodShader.SetBuffer(buffers.calculateKernel, ShaderIDs.lodGroupBuffer, buffers.lodGroupBuffer);
            buffers.lodShader.SetBuffer(buffers.calculateKernel, ShaderIDs.sizeBuffer, buffers.sizeBuffer);
            buffers.lodShader.SetBuffer(buffers.calculateKernel, ShaderIDs.changedBuffer, buffers.changedBuffer);
            ComputeShaderUtility.Dispatch(buffers.lodShader, buffers.calculateKernel, buffers.lodGroupBuffer.count, LodBuffers.CALCULATETHREADGROUP);
        }
    }
}