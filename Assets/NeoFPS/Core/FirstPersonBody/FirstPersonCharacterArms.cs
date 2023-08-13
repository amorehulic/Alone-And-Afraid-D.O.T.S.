using UnityEngine;

namespace NeoFPS
{
    [RequireComponent (typeof(Animator))]
    public class FirstPersonCharacterArms : MonoBehaviour
    {
        [SerializeField, Tooltip("An optional arms transform that should be matched to the weapon geometry. Use this to match arm animations to weapon animations after the weapon has been affected by procedural animation effects such as bob or poses.")]
        private Transform m_ArmsRootTransform = null;

        private Animator m_Animator = null;
        private Vector3 m_RootNeutralPosition = Vector3.zero;
        private Quaternion m_RootNeutralRotation = Quaternion.identity;

        private WieldableItemKinematics m_WieldableKinematics = null;
        public WieldableItemKinematics wieldableKinematics
        {
            get { return m_WieldableKinematics; }
            set
            {
                m_WieldableKinematics = value;

                if (m_ArmsRootTransform != null)
                {
                    if (m_WieldableKinematics == null)
                    {
                        m_ArmsRootTransform.localPosition = m_RootNeutralPosition;
                        m_ArmsRootTransform.localRotation = m_RootNeutralRotation;
                        gameObject.SetActive(false);
                    }
                    else
                        gameObject.SetActive(true);
                }
            }
        }

        protected void Awake()
        {
            if (m_ArmsRootTransform != null)
            {
                m_RootNeutralPosition = m_ArmsRootTransform.localPosition;
                m_RootNeutralRotation = m_ArmsRootTransform.localRotation;
            }

            m_Animator = GetComponent<Animator>();
            if (m_Animator == null)
                enabled = false;

            if (m_ArmsRootTransform != null)
                gameObject.SetActive(m_WieldableKinematics != null);
        }

        protected void OnAnimatorIK(int layerIndex)
        {
            if (wieldableKinematics != null)
            {
                bool matchLeftFingers = false;
                bool matchRightFingers = false;

                // Reposition to match weapon (if required)
                if (m_ArmsRootTransform != null)
                {
                    if (wieldableKinematics.viewModelTransform != null)
                    {
                        m_ArmsRootTransform.position = wieldableKinematics.viewModelTransform.position;
                        m_ArmsRootTransform.rotation = wieldableKinematics.viewModelTransform.rotation;
                    }
                }

                // Arm IK goals
                Vector3 goalPosition;
                Quaternion goalRotation;

                // Match left hand
                if (wieldableKinematics.GetLeftHandGoals(out goalPosition, out goalRotation))
                {
                    SetIKGoals(AvatarIKGoal.LeftHand, goalPosition, goalRotation);
                    matchLeftFingers = wieldableKinematics.matchFingers;
                }
                else
                {
                    ResetIKGoals(AvatarIKGoal.LeftHand);
                    matchLeftFingers = false;
                }

                // Match right hand
                if (wieldableKinematics.GetRightHandGoals(out goalPosition, out goalRotation))
                {
                    SetIKGoals(AvatarIKGoal.RightHand, goalPosition, goalRotation);
                    matchRightFingers = wieldableKinematics.matchFingers;
                }
                else
                {
                    ResetIKGoals(AvatarIKGoal.RightHand);
                    matchRightFingers = false;
                }

                // Finger matching (left hand)
                if (matchLeftFingers)
                {
                    for (int i = 0; i < 15; ++i)
                    {
                        int index = 24 + i; // 24 = left thumb proximal
                        AnimatorMatchFingerRotation((HumanBodyBones)index);
                    }
                }

                // Finger matching (right hand)
                if (matchRightFingers)
                {
                    for (int i = 0; i < 15; ++i)
                    {
                        int index = 39 + i; // 39 = right thumb proximal
                        AnimatorMatchFingerRotation((HumanBodyBones)index);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (wieldableKinematics != null && m_ArmsRootTransform != null && wieldableKinematics.viewModelTransform != null)
            {
                m_ArmsRootTransform.position = wieldableKinematics.viewModelTransform.position;
                m_ArmsRootTransform.rotation = wieldableKinematics.viewModelTransform.rotation;
            }
        }

        void SetIKGoals (AvatarIKGoal goal, Vector3 position, Quaternion rotation)
        {
            m_Animator.SetIKPosition(goal, position);
            m_Animator.SetIKRotation(goal, rotation );
            m_Animator.SetIKPositionWeight(goal, 1f);
            m_Animator.SetIKRotationWeight(goal, 1f);
        }

        void ResetIKGoals(AvatarIKGoal goal)
        {
            m_Animator.SetIKPositionWeight(goal, 0f);
            m_Animator.SetIKRotationWeight(goal, 0f);
        }

        void TransformMatchFingerRotation(HumanBodyBones bone)
        {
            Transform matchBone = m_Animator.GetBoneTransform(bone);
            if (matchBone != null)
            {
                Quaternion rotation;
                if (wieldableKinematics.GetFingerLocalRotation(bone, out rotation))
                    matchBone.localRotation = rotation;
            }
        }

        void AnimatorMatchFingerRotation(HumanBodyBones bone)
        {
            Quaternion rotation;
            if (wieldableKinematics.GetFingerLocalRotation(bone, out rotation))
                m_Animator.SetBoneLocalRotation(bone, rotation);
        }
    }
}