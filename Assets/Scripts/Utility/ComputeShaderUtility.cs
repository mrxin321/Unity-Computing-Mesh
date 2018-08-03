using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComputeShaderUtility
{
    public static uint[] zero = new uint[] { 0 };
    public static void Dispatch(ComputeShader shader, int kernal, int count, int threadGroupCount)
    {
        int threadPerGroup = count / threadGroupCount;
        if (threadPerGroup * threadGroupCount < count)
        {
            threadPerGroup++;
        }
        shader.SetInt(ShaderIDs._Count, count - 1);
        shader.Dispatch(kernal, threadPerGroup, 1, 1);
    }

    public static int GetThread(int count, int threadGroupCount)
    {
        int threadPerGroup = count / threadGroupCount;
        if (threadPerGroup * threadGroupCount < count)
        {
            threadPerGroup++;
        }
        return threadPerGroup;
    }
}