using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Functional;
using UnityEngine.Rendering;
namespace GPUPipeline.Culling
{
    public class GPUCullingTest : PipeLine
    {
        public MeshFilter[] transforms;
        public CullingBuffers buffers;
        public ProceduralInstance procedural;
        private Camera currentCamera;
        public bool useMotionVector;
        private Function<CullingBuffers, ProceduralInstance> onPreRenderAction;

        private void Awake()
        {
            currentCamera = GetComponent<Camera>();
            currentCamera.depthTextureMode |= DepthTextureMode.MotionVectors;
            PipelineSystem.InitBuffers(ref buffers, transforms);
            PipelineSystem.InitProceduralInstance(ref procedural);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            currentCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, procedural.geometryCommandBuffer);
            if (useMotionVector)
            {
                currentCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, procedural.motionVectorsCommandBuffer);
                onPreRenderAction = PipelineSystem.Draw;
            }
            else
            {
                onPreRenderAction = PipelineSystem.DrawNoMotionVectors;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            currentCamera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, procedural.geometryCommandBuffer);
            if (useMotionVector) currentCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, procedural.motionVectorsCommandBuffer);
        }

        private void OnDestroy()
        {
            PipelineSystem.Dispose(ref buffers, ref procedural);
        }

        public override void OnPreRenderEvent()
        {
            buffers.cullingShader.SetFloat(ShaderIDs._CurrentTime, Time.time * 5);
            PipelineSystem.SetLastFrameMatrix(ref buffers, ref lastVPMatrix);
            PipelineSystem.SetCullingBuffer(ref buffers);
            PipelineSystem.RunCulling(ref viewMatrix, ref projMatrix, ref rtProjMatrix, ref buffers);
            PipelineSystem.ClearBuffer(ref procedural);
            onPreRenderAction(ref buffers, ref procedural);
        }
    }
}