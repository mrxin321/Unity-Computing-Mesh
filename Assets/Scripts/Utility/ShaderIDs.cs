using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class ShaderIDs
{
    public static int _Count = Shader.PropertyToID("_Count");
    public static int planes = Shader.PropertyToID("planes");
    public static int allBounds = Shader.PropertyToID("allBounds");
    public static int sizeBuffer = Shader.PropertyToID("sizeBuffer");
    public static int Transforms = Shader.PropertyToID("Transforms");
    public static int _VPMatrix = Shader.PropertyToID("_VPMatrix");
    public static int LAST_VP_MATRIX = Shader.PropertyToID("LAST_VP_MATRIX");
    public static int VertexBuffer = Shader.PropertyToID("VertexBuffer");
    public static int lodGroupBuffer = Shader.PropertyToID("lodGroupBuffer");
    public static int changedBuffer = Shader.PropertyToID("changedBuffer");
    public static int newPositionBuffer = Shader.PropertyToID("newPositionBuffer");
    public static int _CameraPos = Shader.PropertyToID("_CameraPos");
}
