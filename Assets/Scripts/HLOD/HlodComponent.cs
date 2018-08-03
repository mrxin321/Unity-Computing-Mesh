using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
namespace Terrain.HLOD
{
    [System.Serializable]
    public struct Lod3Component
    {
        public const float distance = 800;
    }

    [System.Serializable]
    public struct Lod2Component
    {
        public const float distance = 500;
    }

    [System.Serializable]
    public struct Lod1Component
    {
        public const float distance = 300;
    }

    [System.Serializable]
    public struct Lod0Component
    {
        public const float distance = 100;
    }
}