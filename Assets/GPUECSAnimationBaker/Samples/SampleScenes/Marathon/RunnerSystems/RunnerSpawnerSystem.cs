using GPUECSAnimationBaker.Engine.AnimatorSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace GPUECSAnimationBaker.Samples.SampleScenes.Marathon.RunnerSystems
{
    [BurstCompile]
    public partial struct RunnerSpawnerSystem : ISystem
    {
        private ComponentLookup<LocalTransform> localTransformLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            localTransformLookup = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            localTransformLookup.Update(ref state);
            EndSimulationEntityCommandBufferSystem.Singleton ecbSystem =
                SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            state.Dependency = new RunnerSpawnerJob()
            {
                ecb = ecb,
                localTransformLookup = localTransformLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct RunnerSpawnerJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;
            
            public void Execute(in RunnerSpawnerComponent runnerSpawner, 
                in DynamicBuffer<RunnerSpawnerAnimatorPrefabBufferElement> runnerSpawnerAnimatorPrefabs,
                Entity runnerSpawnerEntity, [EntityIndexInQuery] int sortKey)
            {
                Random random = Random.CreateFromIndex((uint)sortKey);

                for (int i = 0; i < runnerSpawner.nbrOfRunners; i++)
                {
                    // Select a random runner prefab from the available buffer
                    Entity gpuEcsAnimatorPrefab = runnerSpawnerAnimatorPrefabs[
                        random.NextInt(0, runnerSpawnerAnimatorPrefabs.Length)].gpuEcsAnimatorPrefab;
                     
                    // Spawn a new runner
                    Entity gpuEcsAnimator = ecb.Instantiate(sortKey, gpuEcsAnimatorPrefab);

                    // Place it randomly in the running field
                    ecb.SetComponent(sortKey, gpuEcsAnimator, new LocalTransform()
                    {
                        Position = new float3(
                            random.NextFloat(-runnerSpawner.fieldSizeX / 2f, +runnerSpawner.fieldSizeX / 2f),
                            0,
                            random.NextFloat(-runnerSpawner.fieldSizeZ / 2f, +runnerSpawner.fieldSizeZ / 2f)),
                        Rotation = quaternion.identity,
                        Scale = localTransformLookup[gpuEcsAnimatorPrefab].Scale
                    });

                    // Select a random speed for the new runner between minimum & maximum speed
                    float speed = random.NextFloat(runnerSpawner.minSpeed, runnerSpawner.maxSpeed); 
                    ecb.SetComponent(sortKey, gpuEcsAnimator, new RunnerStateComponent()
                    {
                        speed = speed,
                        fieldSizeZ = runnerSpawner.fieldSizeZ
                    });
                    
                    // Calculate blendFactor, speedFactor & select animation
                    float blendFactor;
                    AnimationIdsRunnerMarathon animationID;
                    float speedFactor;
                    if (speed < runnerSpawner.speedWalking)
                    {
                        animationID = AnimationIdsRunnerMarathon.WalkToRun;
                        blendFactor = 0;
                        speedFactor = speed / runnerSpawner.speedWalking;
                    }
                    else if (speed < runnerSpawner.speedRunning)
                    {
                        animationID = AnimationIdsRunnerMarathon.WalkToRun;
                        blendFactor = (speed - runnerSpawner.speedWalking) 
                                      / (runnerSpawner.speedRunning - runnerSpawner.speedWalking);
                        speedFactor = 1f;
                    }
                    else if(speed < runnerSpawner.speedSprinting)
                    {
                        animationID = AnimationIdsRunnerMarathon.RunToSprint;
                        blendFactor = (speed - runnerSpawner.speedRunning) 
                                      / (runnerSpawner.speedSprinting - runnerSpawner.speedWalking);
                        speedFactor = 1f;
                    }
                    else
                    {
                        animationID = AnimationIdsRunnerMarathon.RunToSprint;
                        blendFactor = 1f;
                        speedFactor = speed / runnerSpawner.speedSprinting;
                    }
                    
                    // Kick off the correct animation with a random time offset so to avoid synchronized animations
                    ecb.SetComponent(sortKey, gpuEcsAnimator, new GpuEcsAnimatorControlComponent()
                    {
                        animatorInfo = new AnimatorInfo()
                        {
                            animationID = (int) animationID,
                            blendFactor = blendFactor,
                            speedFactor = speedFactor
                        },
                        startNormalizedTime = random.NextFloat(0f, 1f),
                        transitionSpeed = 0
                    });
                    
                }
                ecb.DestroyEntity(sortKey, runnerSpawnerEntity);
            }
        }
    }
}