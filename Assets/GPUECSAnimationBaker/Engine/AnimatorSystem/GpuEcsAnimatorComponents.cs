using Unity.Entities;
using Unity.Mathematics;

namespace GPUECSAnimationBaker.Engine.AnimatorSystem
{
    public struct GpuEcsAnimationDataBufferElement : IBufferElementData
    {
        public int startFrameIndex;
        public int nbrOfFramesPerSample;
        public int nbrOfInBetweenSamples;
        public float blendTimeCorrection;
    }
    
    public struct GpuEcsAnimatorShaderDataComponent : IComponentData
    {
        public float4x4 shaderData;
    }

    public struct GpuEcsAnimatorInitializedComponent : IComponentData
    {
        public bool initialized;
    }
        
    
    public struct GpuEcsAnimatorTransitionInfoComponent : IComponentData
    {
        public AnimatorInfo current;
        public float blendPreviousToCurrent; // 0 when only previous is running, 1 when only current is running
        public AnimatorInfo previous;
    }

    public struct GpuEcsAnimatorStateComponent : IComponentData
    {
        public float currentNormalizedTime;
        public float previousNormalizedTime;
    }
}