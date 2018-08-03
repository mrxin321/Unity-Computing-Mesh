using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUPipeline.LOD
{
    public class LODManager : MonoBehaviour
    {
        public LodBuffers buffers;
        public LODObject[] lodObjects;
        private TransformInfo[] infos;
        private uint2[] changedList;
        private uint[] size = new uint[] { 0 };
        private void Awake()
        {
            LodSystem.InitLodBuffers(ref buffers, lodObjects.Length);
            infos = new TransformInfo[lodObjects.Length];
            changedList = new uint2[lodObjects.Length];
            for(int i = 0; i < infos.Length; ++i)
            {
                infos[i].lodSize = new Vector4(lodObjects[i].allActions[0].range, lodObjects[i].allActions[1].range, lodObjects[i].allActions[2].range, lodObjects[i].allActions[3].range);
                infos[i].position = lodObjects[i].transform.position;
                infos[i].lodLevel = 0;
            }
            buffers.lodGroupBuffer.SetData(infos);
        }

        private void OnDestroy()
        {
            LodSystem.DisposeLodBuffers(ref buffers);
        }

        private void OnPreCull()
        {
            LodSystem.DispatchLodCalculate(ref buffers, transform.position);
            buffers.sizeBuffer.GetData(size);
            if(size[0] > 0)
            {
                buffers.changedBuffer.GetData(changedList);
            }
            for(int i = 0; i < size[0]; ++i)
            {
                uint index = changedList[i].x;
                uint lodLevel = changedList[i].y;
                lodObjects[index].SetLevel(lodLevel);
            }
        }
    }
}
