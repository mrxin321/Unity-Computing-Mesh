using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Terrain.HLOD
{
    public static class HlodSystem
    {
        public static NativeList<NativeList<Lod3Component>> lod3Component;
        public static NativeList<NativeList<Lod2Component>> lod2Component;
        public static NativeList<NativeList<Lod1Component>> lod1Component;
        public static NativeList<NativeList<Lod0Component>> lod0Component;
    }
}