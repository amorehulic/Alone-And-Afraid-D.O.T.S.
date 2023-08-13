using System;
using GPUECSAnimationBaker.Engine.AnimatorSystem;
using Unity.Entities;
using UnityEngine;

namespace GPUECSAnimationBaker.Samples.SampleScenes.Marathon.RunnerSystems
{
    public class RunnerSpawnerBehaviour : MonoBehaviour
    {
        public float fieldSizeZ;
        public float fieldSizeX;
        public int nbrOfRunners;
        public GameObject[] gpuEcsAnimatorPrefabs; 
        public float walkCycleDistance;
        public AnimationClip walkAnimation;
        public float runCycleDistance;
        public AnimationClip runAnimation;
        public float sprintCycleDistance;
        public AnimationClip sprintAnimation;
        public float minSpeed;
        public float maxSpeed;
    }

    public class RunnerSpawnerBaker : Baker<RunnerSpawnerBehaviour>
    {
        public override void Bake(RunnerSpawnerBehaviour authoring)
        {
            float speedWalking = authoring.walkCycleDistance / authoring.walkAnimation.length;
            float speedRunning = authoring.runCycleDistance / authoring.runAnimation.length;
            float speedSprinting = authoring.sprintCycleDistance / authoring.sprintAnimation.length;
            float minSpeed = Math.Min(authoring.minSpeed, speedWalking);
            float maxSpeed = Math.Max(authoring.maxSpeed, speedSprinting);
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new RunnerSpawnerComponent()
            {
                fieldSizeZ = authoring.fieldSizeZ,
                fieldSizeX = authoring.fieldSizeX,
                nbrOfRunners = authoring.nbrOfRunners,
                speedWalking = speedWalking,
                speedRunning = speedRunning,
                speedSprinting = speedSprinting,
                minSpeed = minSpeed,
                maxSpeed = maxSpeed
            });
            
            DynamicBuffer<RunnerSpawnerAnimatorPrefabBufferElement> crowdSpawnerAnimatorPrefabs 
                = AddBuffer<RunnerSpawnerAnimatorPrefabBufferElement>(entity);
            foreach(GameObject gpuEcsAnimatorPrefab in authoring.gpuEcsAnimatorPrefabs)
            {
                crowdSpawnerAnimatorPrefabs.Add(new RunnerSpawnerAnimatorPrefabBufferElement()
                {
                    gpuEcsAnimatorPrefab = GetEntity(gpuEcsAnimatorPrefab, 
                        gpuEcsAnimatorPrefab.GetComponent<GpuEcsAnimatorBehaviour>().transformUsageFlags)
                });
            }
            
        }
    }
}