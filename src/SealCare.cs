using System;
using UnityEngine;

namespace SealsAtHome
{
    /// <summary>
    /// On every rebuilt seal (added to the Seal and Seal_Pup prefabs). Every 2 s, on every client: when the seal's tamed
    /// state changes, switch its MonsterAI between the wild and tamed settings. Never throws.
    /// </summary>
    public sealed class SealCare : MonoBehaviour
    {
        private const float TickSeconds = 2f;

        /// <summary>True on the Seal_Pup prefab (serialized, so every pup has it).</summary>
        public bool m_isPup;

        private static bool s_errorLogged;

        private ZNetView m_nview;
        private Character m_character;
        private MonsterAI m_ai;
        private bool? m_tamedApplied;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_character = GetComponent<Character>();
            m_ai = GetComponent<MonsterAI>();
            InvokeRepeating(nameof(Tick), UnityEngine.Random.Range(0.5f, TickSeconds), TickSeconds);
        }

        private void Tick()
        {
            try
            {
                if (m_nview == null || !m_nview.IsValid() || m_character == null || m_ai == null) return;
                var tamed = m_character.IsTamed();
                if (m_tamedApplied != tamed)
                {
                    SettingsApplier.Apply(m_ai, tamed ? SealSettings.TamedSwitch : SealSettings.WildSwitch, null);
                    m_tamedApplied = tamed;
                }
            }
            catch (Exception e)
            {
                if (s_errorLogged) return;
                s_errorLogged = true;
                Plugin.Log.LogError($"SealCare failed (logged once): {e}");
            }
        }
    }
}
