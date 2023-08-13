using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.SinglePlayer
{
    public class SubSceneLoadTrigger : SoloCharacterTriggerZone
    {
        [SerializeField, Tooltip("The index of the scene within the SubSceneCollection to load.")]
        private int m_SubSceneIndex = -1;

        protected override void OnCharacterEntered(FpsSoloCharacter c)
        {
            base.OnCharacterEntered(c);

            if (m_SubSceneIndex != -1)
                SubSceneManager.LoadScene(m_SubSceneIndex);
        }
    }
}