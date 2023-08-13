using GPUECSAnimationBaker.Engine.AnimatorSystem;
using Unity.Entities;
using UnityEngine;

namespace GPUECSAnimationBaker.Samples.SampleScenes.Festival.CrowdSpawnerSystem
{
    public class CrowdSpawnerBehaviour : MonoBehaviour
    {
        public int cols;
        public int rows;
        public float spacing;
        public GameObject[] gpuEcsAnimatorPrefabs; 
    }

    public class CrowdSpawnerBaker : Baker<CrowdSpawnerBehaviour>
    {
        public override void Bake(CrowdSpawnerBehaviour authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CrowdSpawnerComponent()
            {
                cols = authoring.cols,
                rows = authoring.rows,
                spacing = authoring.spacing
            });

            DynamicBuffer<CrowdSpawnerAnimatorPrefabBufferElement> crowdSpawnerAnimatorPrefabs 
                = AddBuffer<CrowdSpawnerAnimatorPrefabBufferElement>(entity);
            foreach(GameObject gpuEcsAnimatorPrefab in authoring.gpuEcsAnimatorPrefabs)
            {
                crowdSpawnerAnimatorPrefabs.Add(new CrowdSpawnerAnimatorPrefabBufferElement()
                {
                    gpuEcsAnimatorPrefab = GetEntity(gpuEcsAnimatorPrefab, 
                        gpuEcsAnimatorPrefab.GetComponent<GpuEcsAnimatorBehaviour>().transformUsageFlags)
                });
            }
        }
    }
}