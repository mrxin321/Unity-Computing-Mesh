using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GPUPipeline.LOD
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class LODObject : MonoBehaviour
    {
        [System.Serializable]
        public struct LodAction
        {
            public Mesh mesh;
            public float range;
        }
        public MeshRenderer renderer;
        public LodAction[] allActions = new LodAction[4];
        public MeshFilter filter;
        
        private void Awake()
        {
            filter = GetComponent<MeshFilter>();
            renderer = GetComponent<MeshRenderer>();
        }
        public void SetLevel(uint level)
        {
            if(level > 4)
            {
                renderer.enabled = false;
            }else
            {
                renderer.enabled = true;
                filter.sharedMesh = allActions[level].mesh;
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(LODObject))]
    public class LodObjectEditor : Editor
    {
        LODObject target;
        private void OnEnable()
        {
            target = serializedObject.targetObject as LODObject;
        }

        public override void OnInspectorGUI()
        {
            float max = 0;
            for(int i = 0; i < 4; ++i)
            {
                string str = "LOD" + i.ToString();
                target.allActions[i].mesh = EditorGUILayout.ObjectField(str + " Mesh: ", target.allActions[i].mesh, typeof(Mesh), false) as Mesh;
                max = Mathf.Max(target.allActions[i].range, max);
                max = EditorGUILayout.Slider(str + " Distance: ", max, 0, 1000);
                target.allActions[i].range = max;
                if(GUILayout.Button("Set Renderer to " + str + " Mesh") && target.allActions[i].mesh != null)
                {
                    if(!target.filter)
                    {
                        target.filter = target.GetComponent<MeshFilter>();
                    }
                    target.filter.sharedMesh = target.allActions[i].mesh;
                }
            }
        }
    }
#endif
}