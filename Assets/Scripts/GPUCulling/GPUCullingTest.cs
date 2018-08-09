using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Functional;
using UnityEngine.Rendering;
namespace GPUPipeline.Culling
{
    public class GPUCullingTest : MonoBehaviour
    {
        public Transform[] transforms;
        public CullingBuffers buffers;
        public ProceduralInstance procedural;
        private Camera currentCamera;
        private Matrix4x4 view;
        public bool useMotionVector;
        public bool useOcclusion;
        private Function<CullingBuffers, ProceduralInstance> onPreRenderAction;

        private void Awake()
        {
            currentCamera = GetComponent<Camera>();
            currentCamera.depthTextureMode |= DepthTextureMode.MotionVectors;
            PipelineSystem.InitBuffers(ref buffers, transforms, transforms[0].GetComponent<MeshFilter>().sharedMesh);
            PipelineSystem.InitProceduralInstance(ref procedural);
        }

        private void OnEnable()
        {
            currentCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, procedural.geometryCommandBuffer);
            if (useMotionVector)
            {
                currentCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, procedural.motionVectorsCommandBuffer);
                if (useOcclusion)
                    onPreRenderAction = PipelineSystem.Draw;
                else
                    onPreRenderAction = PipelineSystem.DrawNoOcclusion;
            }
            else
            {
                if (useOcclusion)
                    onPreRenderAction = PipelineSystem.DrawNoMotionVectors;
                else
                    onPreRenderAction = PipelineSystem.DrawNoMotionVectorsNoOcclusion;
            }
            PipelineBase.onPreRenderEvents.Add(OnPreRenderEvent);
        }

        private void OnDisable()
        {
            currentCamera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, procedural.geometryCommandBuffer);
            if (useMotionVector) currentCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, procedural.motionVectorsCommandBuffer);
            PipelineBase.onPreRenderEvents.Remove(OnPreRenderEvent);
        }

        private void OnDestroy()
        {
            PipelineSystem.Dispose(ref buffers, ref procedural);
        }

        private void OnPreRenderEvent()
        {
            buffers.cullingShader.SetFloat(ShaderIDs._CurrentTime, Time.time * 5);
            PipelineSystem.SetLastFrameMatrix(ref buffers);
            PipelineSystem.SetCullingBuffer(ref buffers);
            Matrix4x4 proj = GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, false);
            Matrix4x4 rtProj = GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, true);
            Shader.SetGlobalMatrix(ShaderIDs.LAST_VP_MATRIX, rtProj * view);
            view = Camera.current.worldToCameraMatrix;
            PipelineSystem.RunCulling(ref view, ref proj, ref rtProj, ref buffers);
            PipelineSystem.ClearBuffer(ref procedural);
            onPreRenderAction(ref buffers, ref procedural);
        }
    }
}