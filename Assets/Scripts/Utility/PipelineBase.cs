using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Functional;

public class PipelineBase : MonoBehaviour {
    public static List<PipeLine> onPreRenderEvents = new List<PipeLine>(10);
    private void OnPreRender()
    {
        PipeLine.projMatrix = GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, false);
        PipeLine.rtProjMatrix = GL.GetGPUProjectionMatrix(Camera.current.projectionMatrix, true);
        PipeLine.lastVPMatrix = PipeLine.rtProjMatrix * PipeLine.viewMatrix;
        PipeLine.viewMatrix = Camera.current.worldToCameraMatrix;
        foreach (var i in onPreRenderEvents)
        {
            i.OnPreRenderEvent();
        }
    }
}
