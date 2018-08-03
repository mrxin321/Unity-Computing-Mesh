using UnityEngine;
using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
namespace GPUPipeline.Procedural
{
    public class ProceduralGeometry : MonoBehaviour
    {
        protected GeometryDrawCommand currentCommand;
        private int currentIndexInAllInstances;
        private Transform[] allTrans;
        public ComputeShader shader;
        public ComputeMeshes strct;
        public bool enableUpdate = true;
        void Awake()
        {
            allTrans = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; ++i)
            {
                allTrans[i] = transform.GetChild(i);
            }
            strct = new ComputeMeshes(allTrans, shader, shader.FindKernel("CSMain"));
            currentCommand.drawWithMotionVectors = SetBuffer;
            currentCommand.drawWithoutMotionVectors = SetBuffer;
            currentCommand.parent = this;
        }

        private void OnEnable()
        {
            currentIndexInAllInstances = ProceduralCamera.allInstances.Count;
            ProceduralCamera.allInstances.Add(currentCommand);
        }

        private void OnDisable()
        {
            GeometryDrawCommand lastOne = ProceduralCamera.allInstances[ProceduralCamera.allInstances.Count - 1];
            lastOne.parent.currentIndexInAllInstances = currentIndexInAllInstances;
            ProceduralCamera.allInstances[currentIndexInAllInstances] = lastOne;
            ProceduralCamera.allInstances.RemoveAt(ProceduralCamera.allInstances.Count - 1);
        }

        private void SetBuffer(CommandBuffer buffer, CommandBuffer beforeImageBuffer)
        {
            strct.SetCurrentData(ref ProceduralCamera.vp, ref ProceduralCamera.v, enableUpdate);
            buffer.SetRenderTarget(ProceduralCamera.renderTargetIdentifiers, BuiltinRenderTextureType.CameraTarget);
            beforeImageBuffer.SetRenderTarget(BuiltinRenderTextureType.MotionVectors, BuiltinRenderTextureType.CameraTarget);
            strct.Draw(buffer, beforeImageBuffer);
            strct.Draw(buffer);
        }

        private void SetBuffer(CommandBuffer buffer)
        {
            strct.SetCurrentData(ref ProceduralCamera.vp, ref ProceduralCamera.v, enableUpdate);
            buffer.SetRenderTarget(ProceduralCamera.renderTargetIdentifiers, BuiltinRenderTextureType.CameraTarget);
            strct.Draw(buffer);
        }

        void OnDestroy()
        {
            strct.Dispose();
        }

    }
}