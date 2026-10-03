using System;
using UnityEngine;

namespace SealsAtHome
{
    /// <summary>
    /// On every rebuilt seal (added to the Seal and Seal_Pup prefabs). Every 2 s:
    /// on every client, switch the MonsterAI between wild and tamed settings when the tamed state changes, and take the
    /// seal over when a player without the mod runs it (Takeover);
    /// on the game that owns the seal, copy the pet name into the vanilla override-name field (players without the mod
    /// see it) and, on pups, every 10 s, apply the grow-up rule (PupGrowth). Never throws.
    /// </summary>
    public sealed class SealCare : MonoBehaviour
    {
        private const float TickSeconds = 2f;
        private const int GrowEveryTicks = 5; // 10 s, like vanilla Growup

        /// <summary>True on the Seal_Pup prefab (serialized, so every pup has it).</summary>
        public bool m_isPup;

        private static readonly int s_tamedAtHash = SealSettings.TamedAtKey.GetStableHashCode();
        private static bool s_errorLogged;
        private static bool s_takeoverLogged;

        private ZNetView m_nview;
        private Character m_character;
        private MonsterAI m_ai;
        private bool? m_tamedApplied;
        private int m_ticks;
        private bool m_grown;

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
                if (m_grown || m_nview == null || !m_nview.IsValid() || m_character == null || m_ai == null) return;
                var tamed = m_character.IsTamed();
                if (m_tamedApplied != tamed)
                {
                    SettingsApplier.Apply(m_ai, tamed ? SealSettings.TamedSwitch : SealSettings.WildSwitch, null);
                    m_tamedApplied = tamed;
                }

                ModdedPlayers.MarkLocalPlayer();
                if (!m_nview.IsOwner())
                {
                    var owner = m_nview.GetZDO().GetOwner();
                    if (Takeover.ShouldClaim(false, owner, ModdedPlayers.HasMod(owner)))
                    {
                        m_nview.ClaimOwnership();
                        if (!s_takeoverLogged)
                        {
                            s_takeoverLogged = true;
                            Plugin.Log.LogInfo("Took over seals run by a player without SealsAtHome (logged once)");
                        }
                    }
                    return;
                }
                var zdo = m_nview.GetZDO();
                var name = zdo.GetString(ZDOVars.s_tamedName);
                if (NameCopy.ShouldWrite(name, zdo.GetString(ZDOVars.s_overrideHoverName)))
                {
                    var shown = NameCopy.Target(name);
                    zdo.Set(ZDOVars.s_overrideHoverName, shown);
                    Plugin.Log.LogInfo(shown.Length > 0
                        ? $"Pet name '{shown}' is now shown to players without the mod too"
                        : "Pet name cleared for players without the mod");
                }

                if (!m_isPup || ++m_ticks % GrowEveryTicks != 0 || ZNet.instance == null) return;
                var now = ZNet.instance.GetTime().Ticks;
                switch (PupGrowth.Decide(tamed, name, zdo.GetLong(s_tamedAtHash), now))
                {
                    case PupGrowth.Step.Stamp:
                        zdo.Set(s_tamedAtHash, now);
                        break;
                    case PupGrowth.Step.Grow:
                        GrowUp();
                        break;
                }
            }
            catch (Exception e)
            {
                if (s_errorLogged) return;
                s_errorLogged = true;
                Plugin.Log.LogError($"SealCare failed (logged once): {e}");
            }
        }

        /// <summary>As vanilla Growup: a tamed adult Seal at the same spot and star level, then the pup is removed.</summary>
        private void GrowUp()
        {
            var prefab = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(SealSettings.SealPrefab) : null;
            if (prefab == null) return;
            m_grown = true; // never twice, even if another tick comes before the pup is gone
            var adult = Instantiate(prefab, transform.position, transform.rotation).GetComponent<Character>();
            if (adult != null)
            {
                adult.SetTamed(true);
                adult.SetLevel(m_character.GetLevel());
            }
            m_nview.Destroy();
        }
    }
}
