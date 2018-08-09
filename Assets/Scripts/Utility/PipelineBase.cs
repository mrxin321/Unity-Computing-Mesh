using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Functional;

public class PipelineBase : MonoBehaviour {
    public static List<Function> onPreRenderEvents = new List<Function>(10);
    private void OnPreRender()
    {
        foreach(var i in onPreRenderEvents)
        {
            i();
        }
    }
}
