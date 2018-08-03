using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace GPUPipeline.Procedural
{
    public class ProceduralCamera : MonoBehaviour
    {
        /**
 * Enable and draw mesh to motion vector texture
 * Active this option only if post-processing need pixel's motion vector
 */
        public static List<GeometryDrawCommand> allInstances = new List<GeometryDrawCommand>();
        public bool useMotionVector = true;
        private Camera targetCamera;
        private CommandBuffer beforeImageBuffer;
        private CommandBuffer buffer;
        public static Matrix4x4 vp;
        public static Matrix4x4 v;
        public static RenderTargetIdentifier[] renderTargetIdentifiers = new RenderTargetIdentifier[]
                {
                    BuiltinRenderTextureType.GBuffer0,
                    BuiltinRenderTextureType.GBuffer1,
                    BuiltinRenderTextureType.GBuffer2,
                    BuiltinRenderTextureType.GBuffer3
                };
        // Use this for initialization
        private void Awake()
        {
            buffer = new CommandBuffer();
            buffer.name = "Geometry Buffer";
            targetCamera = GetComponent<Camera>();
            beforeImageBuffer = new CommandBuffer();
            beforeImageBuffer.name = "Motion Vectors Buffer";
        }

        private void OnDestroy()
        {
            buffer.Dispose();
            beforeImageBuffer.Dispose();
        }

        private void OnEnable()
        {
            targetCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, buffer);
            if (useMotionVector)
            {
                targetCamera.depthTextureMode |= DepthTextureMode.Depth;
                targetCamera.depthTextureMode |= DepthTextureMode.MotionVectors;
                targetCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, beforeImageBuffer);
            }
            v = targetCamera.worldToCameraMatrix;
            vp = GL.GetGPUProjectionMatrix(targetCamera.projectionMatrix, true) * v;
        }

        private void OnDisable()
        {
            targetCamera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, buffer);
            if (useMotionVector)
            {
                targetCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, beforeImageBuffer);
            }
        }
        static int LAST_VP_MATRIX = Shader.PropertyToID("LAST_VP_MATRIX");
        private void OnPreRender()
        {
            if (!enabled) return;
            Matrix4x4 p = GL.GetGPUProjectionMatrix(targetCamera.projectionMatrix, true);
            vp = p * v;
            Shader.SetGlobalMatrix(LAST_VP_MATRIX, vp);
            v = targetCamera.worldToCameraMatrix;
            vp = p * v;
            if (useMotionVector)
            {
                buffer.Clear();
                beforeImageBuffer.Clear();
                foreach (var i in allInstances)
                {
                    i.drawWithMotionVectors(buffer, beforeImageBuffer);
                }
            }
            else
            {
                buffer.Clear();
                foreach (var i in allInstances)
                {
                    i.drawWithoutMotionVectors(buffer);
                }
            }
        }
    }
}