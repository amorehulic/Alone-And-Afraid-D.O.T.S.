using GPUECSAnimationBaker.Engine.AnimatorSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace GPUECSAnimationBaker.Samples.SampleScenes.Festival.CrowdSpawnerSystem
{
    [BurstCompile]
    public partial struct CrowdSpawnerSystem : ISystem
    {
        private BufferLookup<GpuEcsAnimationDataBufferElement> gpuEcsAnimationDataBufferLookup;
        private ComponentLookup<LocalTransform> localTransformLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            gpuEcsAnimationDataBufferLookup = state.GetBufferLookup<GpuEcsAnimationDataBufferElement>(isReadOnly: true);
            localTransformLookup = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            gpuEcsAnimationDataBufferLookup.Update(ref state);
            localTransformLookup.Update(ref state);
            EndSimulationEntityCommandBufferSystem.Singleton ecbSystem =
                SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            state.Dependency = new CrowdSpawnerJob()
            {
                ecb = ecb,
                gpuEcsAnimationDataBufferLookup = gpuEcsAnimationDataBufferLookup,
                localTransformLookup = localTransformLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct CrowdSpawnerJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;
            [ReadOnly] public BufferLookup<GpuEcsAnimationDataBufferElement> gpuEcsAnimationDataBufferLookup;
            [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;
            
            public void Execute(in CrowdSpawnerComponent crowdSpawner, 
                in DynamicBuffer<CrowdSpawnerAnimatorPrefabBufferElement> crowdSpawnerAnimatorPrefabs,
                Entity crowdSpawnerEntity, [EntityIndexInQuery] int sortKey)
            {
                Random random = Random.CreateFromIndex((uint)sortKey);

                // Calculate the base offset so that the square of entities is centered around the origin
                float3 baseOffset = new float3(
                    -(crowdSpawner.cols - 1) * crowdSpawner.spacing / 2f,
                    0f,
                    -(crowdSpawner.rows - 1) * crowdSpawner.spacing / 2f);

                
                for (int col = 0; col < crowdSpawner.cols; col++)
                {
                    for (int row = 0; row < crowdSpawner.rows; row++)
                    {
                        // Select a random prefab from the available buffer
                        Entity gpuEcsAnimatorPrefab = crowdSpawnerAnimatorPrefabs[
                            random.NextInt(0, crowdSpawnerAnimatorPrefabs.Length)].gpuEcsAnimatorPrefab;
                        // Spawn a character
                        Entity gpuEcsAnimator = ecb.Instantiate(sortKey, gpuEcsAnimatorPrefab);
                        
                        // set the position according to column, row & spacing values
                        // Preserve the scale that was set in the prefab
                        ecb.SetComponent(sortKey, gpuEcsAnimator, new LocalTransform()
                        {
                            Position = baseOffset + new float3(
                                col * crowdSpawner.spacing + random.NextFloat(-crowdSpawner.spacing / 4f, crowdSpawner.spacing / 4f)
                                , 0, 
                                row * crowdSpawner.spacing + random.NextFloat(-crowdSpawner.spacing / 4f, crowdSpawner.spacing / 4f)
                                ),
                            Rotation = quaternion.Euler(0, random.NextFloat(-math.PI, math.PI), 0),
                            Scale = localTransformLookup[gpuEcsAnimatorPrefab].Scale
                        });

                        // Pick a random animation ID from the available animations
                        DynamicBuffer<GpuEcsAnimationDataBufferElement> animationDataBuffer = gpuEcsAnimationDataBufferLookup[gpuEcsAnimatorPrefab];
                        int animationID = random.NextInt(0, animationDataBuffer.Length);
                        
                        // Kick off the correct animation with a random time offset so to avoid synchronized animations
                        ecb.SetComponent(sortKey, gpuEcsAnimator, new GpuEcsAnimatorControlComponent()
                        {
                            animatorInfo = new AnimatorInfo()
                            {
                                animationID = animationID,
                                blendFactor = 0,
                                speedFactor = 1f
                            },
                            startNormalizedTime = random.NextFloat(0f, 1f),
                            transitionSpeed = 0
                        });
                    }
                }
                ecb.DestroyEntity(sortKey, crowdSpawnerEntity);
            }
        }
    }
}