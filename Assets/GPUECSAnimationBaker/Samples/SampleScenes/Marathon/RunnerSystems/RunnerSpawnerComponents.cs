using Unity.Entities;

namespace GPUECSAnimationBaker.Samples.SampleScenes.Marathon.RunnerSystems
{
    public struct RunnerSpawnerComponent : IComponentData
    {
        public float fieldSizeZ;
        public float fieldSizeX;
        public int nbrOfRunners;
        public float speedWalking;
        public float speedRunning;
        public float speedSprinting;
        public float minSpeed;
        public float maxSpeed;
    }
    
    public struct RunnerSpawnerAnimatorPrefabBufferElement : IBufferElementData
    {
        public Entity gpuEcsAnimatorPrefab;
    }
    
}