using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Functional;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GPUPipeline.Culling
{
    public class GPUCullingTest : PipeLine
    {
        public Mesh mesh;
        public Transform parent;
        [HideInInspector]
        public Vector3[] allExtents;
        [HideInInspector]
        public Matrix4x4[] localToWorldMatrices;
        public CullingBuffers buffers;
        public Material proceduralMat;
        public enum MotionVectorState
        {
            Static,
            DynamicWithoutMotionVector,
            DynamicWithMotionVector,
        }
        public MotionVectorState motionVectorState;
        private Function drawFunction;

        private void Awake()
        {
            PipelineFunction.InitBuffers(ref buffers, allExtents, localToWorldMatrices, mesh);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            switch (motionVectorState)
            {
                case MotionVectorState.Static:
                    drawFunction = () => PipelineFunction.DrawNoMotionVectors(ref buffers, proceduralMat);
                    break;
                case MotionVectorState.DynamicWithMotionVector:
                    drawFunction = () =>
                    {
                        PipelineFunction.SetLastFrameMatrix(ref buffers, ref lastVPMatrix);
                        //Add Movement Operations here
                        PipelineFunction.Draw(ref buffers, proceduralMat);
                    };
                    break;
                case MotionVectorState.DynamicWithoutMotionVector:
                    //Add Movement Operations here
                    drawFunction = () => PipelineFunction.DrawNoMotionVectors(ref buffers, proceduralMat);
                    break;
            }
        }

        private void OnDestroy()
        {
            PipelineFunction.Dispose(ref buffers);
        }

        public override void OnPreRenderEvent()
        {
            PipelineFunction.SetCullingBuffer(ref buffers);
            PipelineFunction.RunCulling(ref viewMatrix, ref projMatrix, ref rtProjMatrix, ref buffers);
            drawFunction();
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(GPUCullingTest))]
    public class GPUCullingTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GPUCullingTest test = serializedObject.targetObject as GPUCullingTest;
            EditorGUILayout.LabelField("Transform Count: " + test.allExtents.Length);
            if (GUILayout.Button("Update Transforms"))
            {
                test.allExtents = new Vector3[test.parent.childCount];
                test.localToWorldMatrices = new Matrix4x4[test.parent.childCount];
                for (int i = 0; i < test.parent.childCount; ++i)
                {
                    var trans = test.parent.GetChild(i);
                    test.allExtents[i] = trans.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
                    test.localToWorldMatrices[i] = trans.localToWorldMatrix;
                }
            }
        }
    }
#endif
}