using GpuEcsAnimationBaker.Engine.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace GPUECSAnimationBaker.Engine.AnimatorSystem
{
    [BurstCompile]
    public partial struct GpuEcsAnimatorSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            state.Dependency = new GpuEcsAnimatorJob()
            {
                deltaTime = deltaTime
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct GpuEcsAnimatorJob : IJobEntity
        {
            [ReadOnly] public float deltaTime;

            public void Execute(
                ref GpuEcsAnimatorShaderDataComponent gpuEcsAnimatorShaderData,
                ref GpuEcsAnimatorTransitionInfoComponent gpuEcsAnimatorTransitionInfo,
                ref GpuEcsAnimatorStateComponent gpuEcsAnimatorState,
                ref GpuEcsAnimatorInitializedComponent gpuEcsAnimatorInitialized,
                in GpuEcsAnimatorControlComponent gpuEcsAnimatorControl,
                in DynamicBuffer<GpuEcsAnimationDataBufferElement> gpuEcsAnimationDataBuffer)
            {
                if (!gpuEcsAnimatorInitialized.initialized )
                {
                    // We switch immediately to the first animation, no transition
                    gpuEcsAnimatorTransitionInfo = new GpuEcsAnimatorTransitionInfoComponent()
                    {
                        current = gpuEcsAnimatorControl.animatorInfo,
                        blendPreviousToCurrent = 1f
                    };
                    gpuEcsAnimatorState = new GpuEcsAnimatorStateComponent()
                    {
                        currentNormalizedTime = gpuEcsAnimatorControl.startNormalizedTime,
                    };
                    gpuEcsAnimatorInitialized.initialized = true;
                }
                else if(gpuEcsAnimatorControl.animatorInfo.animationID != gpuEcsAnimatorTransitionInfo.current.animationID)
                {
                    // A new animation (or animation combination) has been started, so we need to do a transition
                    // from the old one to the new
                    gpuEcsAnimatorTransitionInfo = new GpuEcsAnimatorTransitionInfoComponent()
                    {
                        current = gpuEcsAnimatorControl.animatorInfo,
                        previous = gpuEcsAnimatorTransitionInfo.current,
                        blendPreviousToCurrent = 0f
                    };
                    gpuEcsAnimatorState = new GpuEcsAnimatorStateComponent()
                    {
                        currentNormalizedTime = gpuEcsAnimatorControl.startNormalizedTime,
                        previousNormalizedTime = gpuEcsAnimatorState.currentNormalizedTime
                    };
                }
                else
                {
                    // The same animation (or animation combination) is still running, but the parameters might have changed
                    // (blendPrimaryToSecondary or speedFactor)
                    gpuEcsAnimatorTransitionInfo.current = gpuEcsAnimatorControl.animatorInfo;
                }

                UpdateAnimatorState(ref gpuEcsAnimatorState.currentNormalizedTime, 
                    gpuEcsAnimatorTransitionInfo.current, gpuEcsAnimationDataBuffer,
                    out float primaryBlendFactor, out float primaryTransitionToNextFrame, out int primaryFrameIndex,
                    out float secondaryBlendFactor, out float secondaryTransitionToNextFrame, out int secondaryFrameIndex);
                if (gpuEcsAnimatorTransitionInfo.blendPreviousToCurrent >= 1f)
                {
                    gpuEcsAnimatorShaderData.shaderData = new float4x4(
                        primaryBlendFactor, primaryTransitionToNextFrame, primaryFrameIndex, 0,
                        secondaryBlendFactor, secondaryTransitionToNextFrame, secondaryFrameIndex, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0);
                }
                else
                {
                    if(gpuEcsAnimatorControl.transitionSpeed == 0) gpuEcsAnimatorTransitionInfo.blendPreviousToCurrent = 1f;
                    else
                    {
                        gpuEcsAnimatorTransitionInfo.blendPreviousToCurrent += deltaTime / gpuEcsAnimatorControl.transitionSpeed;
                        if (gpuEcsAnimatorTransitionInfo.blendPreviousToCurrent > 1f) gpuEcsAnimatorTransitionInfo.blendPreviousToCurrent = 1f;
                    }
                    float previousToCurrent = gpuEcsAnimatorTransitionInfo.blendPreviousToCurrent;
                    float currentToPrevious = 1f - previousToCurrent;
                    UpdateAnimatorState(ref gpuEcsAnimatorState.previousNormalizedTime,
                        gpuEcsAnimatorTransitionInfo.previous, gpuEcsAnimationDataBuffer,
                        out float previousPrimaryBlendFactor, out float previousPrimaryTransitionToNextFrame, out int previousPrimaryFrameIndex,
                        out float previousSecondaryBlendFactor, out float previousSecondaryTransitionToNextFrame, out int previousSecondaryFrameIndex);

                    gpuEcsAnimatorShaderData.shaderData = new float4x4(
                        previousToCurrent * primaryBlendFactor, primaryTransitionToNextFrame, primaryFrameIndex, 0,
                        previousToCurrent * secondaryBlendFactor, secondaryTransitionToNextFrame, secondaryFrameIndex, 0,
                        currentToPrevious * previousPrimaryBlendFactor, previousPrimaryTransitionToNextFrame, previousPrimaryFrameIndex, 0,
                        currentToPrevious * previousSecondaryBlendFactor, previousSecondaryTransitionToNextFrame, previousSecondaryFrameIndex, 0);
                }
            }

            private void UpdateAnimatorState(ref float normalizedTime, AnimatorInfo animatorInfo,
                in DynamicBuffer<GpuEcsAnimationDataBufferElement> gpuEcsAnimationDataBuffer,
                out float primaryBlendFactor, out float primaryTransitionToNextFrame, out int primaryFrameIndex,
                out float secondaryBlendFactor, out float secondaryTransitionToNextFrame, out int secondaryFrameIndex)
            {
                GpuEcsAnimationDataBufferElement animationData = gpuEcsAnimationDataBuffer[animatorInfo.animationID];

                if (animationData.nbrOfInBetweenSamples == 1)
                {
                    float blendSpeedAdjustment = 1f;
                    UpdateAnimationNormalizedTime(ref normalizedTime, animatorInfo, animationData, blendSpeedAdjustment,
                        out float transitionToNextFrame, out int relativeFrameIndex);
                    primaryBlendFactor = 1;
                    primaryTransitionToNextFrame = transitionToNextFrame;
                    primaryFrameIndex = animationData.startFrameIndex + relativeFrameIndex;
                    secondaryBlendFactor = 0;
                    secondaryTransitionToNextFrame = 0;
                    secondaryFrameIndex = 0;
                }
                else
                {
                    float endBlend = (float)(animationData.nbrOfInBetweenSamples - 1);
                    float currentBlendSetFloat = animatorInfo.blendFactor * endBlend;
                    int currentBlendSet = (int)math.floor(currentBlendSetFloat);
                    float transitionToNextSet = currentBlendSetFloat - (float)currentBlendSet;
                    
                    float blendSpeedAdjustment = animatorInfo.blendFactor * animationData.blendTimeCorrection + (1f - animatorInfo.blendFactor);
                    UpdateAnimationNormalizedTime(ref normalizedTime, animatorInfo, animationData, blendSpeedAdjustment,
                        out float transitionToNextFrame, out int relativeFrameIndex);
                    primaryBlendFactor = 1f - transitionToNextSet;
                    primaryTransitionToNextFrame = transitionToNextFrame;
                    primaryFrameIndex = animationData.startFrameIndex + currentBlendSet * animationData.nbrOfFramesPerSample + relativeFrameIndex;
                    secondaryBlendFactor = transitionToNextSet;
                    secondaryTransitionToNextFrame = transitionToNextFrame;
                    secondaryFrameIndex = animationData.startFrameIndex + (currentBlendSet + 1) * animationData.nbrOfFramesPerSample + relativeFrameIndex;
                }
            }

            private void UpdateAnimationNormalizedTime(ref float normalizedTime, AnimatorInfo animatorInfo,
                GpuEcsAnimationDataBufferElement animationData, float blendSpeedAdjustment, 
                out float transitionToNextFrame, out int relativeFrameIndex)
            {
                float endFrame = (float)(animationData.nbrOfFramesPerSample - 1);
                float animationLength = endFrame / GlobalConstants.SampleFrameRate;
                float currentTime = normalizedTime * animationLength;
                currentTime += deltaTime * animatorInfo.speedFactor * blendSpeedAdjustment;
                normalizedTime = currentTime / animationLength;
                while (normalizedTime > 1f) normalizedTime -= 1f;
                
                float relativeFrameIndexFloat = normalizedTime * endFrame;
                relativeFrameIndex = (int)math.floor(relativeFrameIndexFloat);
                transitionToNextFrame = relativeFrameIndexFloat - (float)relativeFrameIndex;
            }

        }
        
    }
}