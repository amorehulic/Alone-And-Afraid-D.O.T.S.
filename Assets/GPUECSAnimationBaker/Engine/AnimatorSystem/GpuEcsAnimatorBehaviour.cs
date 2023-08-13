using GpuEcsAnimationBaker.Engine.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GPUECSAnimationBaker.Engine.AnimatorSystem
{
    public class GpuEcsAnimatorBehaviour : MonoBehaviour
    {
        public GpuEcsAnimationData[] animations;
        public TransformUsageFlags transformUsageFlags = TransformUsageFlags.Dynamic;
    }

    public class GpuEcsAnimatorBaker : Baker<GpuEcsAnimatorBehaviour>
    {
        public override void Bake(GpuEcsAnimatorBehaviour authoring)
        {
            Entity entity = GetEntity(authoring.transformUsageFlags);
            DynamicBuffer<GpuEcsAnimationDataBufferElement> buffer = AddBuffer<GpuEcsAnimationDataBufferElement>(entity);
            for (int animationIndex = 0; animationIndex < authoring.animations.Length; animationIndex++)
            {
                GpuEcsAnimationData gpuEcsAnimationData = authoring.animations[animationIndex];
                buffer.Add(new GpuEcsAnimationDataBufferElement()
                {
                    startFrameIndex = gpuEcsAnimationData.startFrameIndex,
                    nbrOfFramesPerSample = gpuEcsAnimationData.nbrOfFramesPerSample,
                    nbrOfInBetweenSamples = gpuEcsAnimationData.nbrOfInBetweenSamples,
                    blendTimeCorrection = gpuEcsAnimationData.blendTimeCorrection
                });
            }
            
            AddComponent(entity, new GpuEcsAnimatorShaderDataComponent()
            {
                shaderData = new float4x4(
                    1f, 0, 0, 0, 
                    0, 0, 0, 0, 
                    0, 0, 0, 0,
                    0, 0, 0, 0)
            });

            int initialAnimationID = 0;
            GpuEcsAnimatorInitializerBehaviour initializer = authoring.GetComponent<GpuEcsAnimatorInitializerBehaviour>();
            if (initializer != null) initialAnimationID = initializer.GetInitialAnimationID();

            AddComponent(entity, new GpuEcsAnimatorInitializedComponent()
            {
                initialized = false
            });
            
            AddComponent(entity, new GpuEcsAnimatorControlComponent()
            {
                animatorInfo = new AnimatorInfo()
                {
                    animationID = initialAnimationID,
                    blendFactor = 0,
                    speedFactor = 1
                },
                transitionSpeed = 0,
                startNormalizedTime = 0
            });
                
            AddComponent<GpuEcsAnimatorTransitionInfoComponent>(entity);
            AddComponent<GpuEcsAnimatorStateComponent>(entity);
        }
    }
}