﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-instantdamagezone.html")]
    public class InstantDamageZone : CharacterTriggerZone, IDamageSource
    {
        [SerializeField, Tooltip("The amount of damage to apply to the player character per second.")]
        private float m_Damage = 10f;
        [SerializeField, Tooltip("The type of damage to apply.")]
        private DamageType m_DamageType = DamageType.Default;
        [SerializeField, Tooltip("A description of the damage to use in logs, etc.")]
        private string m_DamageDescription = "Damage Zone";

        private DamageFilter m_OutDamageFilter = DamageFilter.AllDamageAllTeams;

        protected override void OnCharacterEntered(ICharacter c)
        {
            base.OnCharacterEntered(c);

            var hm = c.GetComponent<IHealthManager>();
            if (hm != null)
                hm.AddDamage(m_Damage, false, this);
        }

        protected void Awake()
        {
            m_OutDamageFilter.SetDamageType(m_DamageType);
        }

        #region IDamageSource IMPLEMENTATION

        public DamageFilter outDamageFilter
        {
            get { return new DamageFilter(m_DamageType, DamageTeamFilter.All); }
            set { m_OutDamageFilter = value; }
        }

        public IController controller
        {
            get { return null; }
        }

        public Transform damageSourceTransform
        {
            get { return transform; }
        }

        public string description
        {
            get { return m_DamageDescription; }
        }

        #endregion
    }
}