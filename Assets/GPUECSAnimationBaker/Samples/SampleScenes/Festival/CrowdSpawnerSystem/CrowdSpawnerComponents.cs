using Unity.Entities;

namespace GPUECSAnimationBaker.Samples.SampleScenes.Festival.CrowdSpawnerSystem
{
    public struct CrowdSpawnerComponent : IComponentData
    {
        public int cols;
        public int rows;
        public float spacing;
    }

    public struct CrowdSpawnerAnimatorPrefabBufferElement : IBufferElementData
    {
        public Entity gpuEcsAnimatorPrefab;
    }
}