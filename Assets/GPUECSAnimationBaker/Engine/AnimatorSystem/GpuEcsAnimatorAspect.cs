using Unity.Entities;

namespace GPUECSAnimationBaker.Engine.AnimatorSystem
{
    public readonly partial struct GpuEcsAnimatorAspect : IAspect
    {
        private readonly RefRW<GpuEcsAnimatorControlComponent> control;

        public void RunAnimation(int animationID,
            float blendFactor = 0f, float speedFactor = 1f, float startNormalizedTime = 0f, float transitionSpeed = 0f)
        {
            control.ValueRW = new GpuEcsAnimatorControlComponent()
            {
                animatorInfo = new AnimatorInfo()
                {
                    animationID = animationID,
                    blendFactor = blendFactor,
                    speedFactor = speedFactor
                },
                startNormalizedTime = startNormalizedTime,
                transitionSpeed = transitionSpeed
            };
        }
    }
}