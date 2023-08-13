using System;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;

namespace GpuEcsAnimationBaker.Engine.Data
{
    [Serializable]
    public struct GpuEcsAnimationBakerData
    {
        public AnimationData[] animations;
        [Tooltip("Specifies if an animation Ids enum code file needs to be generated")]
        public bool generateAnimationIdsEnum;
        [Tooltip("Specifies the name of the enum to be generated")]
        public string animationIdsEnumName;
        public BoneUsage boneUsage;
        [Tooltip("Specifies the TransformUsageFlags to be used when converting the parent animator to an ECS entity")]
        public TransformUsageFlags transformUsageFlagsParent;
        [Tooltip("Specifies the TransformUsageFlags to be used when converting the child meshes to an ECS entity")]
        public TransformUsageFlags transformUsageFlagsChildren;
    }
}