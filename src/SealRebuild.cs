using System.Linq;
using BepInEx.Bootstrap;
using UnityEngine;

namespace SealsAtHome
{
    /// <summary>
    /// Rebuilds the vanilla Seal and Seal_Pup prefabs in place, so the spawner, breeding and saved seals all get them:
    /// Character → Humanoid, AnimalAI → MonsterAI (wild settings), plus Tameable, Procreation (adults) and SealCare.
    /// A pre-flight checks everything first; on any problem nothing changes and the seals stay vanilla.
    /// </summary>
    internal static class SealRebuild
    {
        private const string TameableSealsGuid = "pseudopulse.TameableSeals";

        public static bool Run(ZNetScene scene, out string message)
        {
            var seal = scene.GetPrefab(SealSettings.SealPrefab);
            var pup = scene.GetPrefab(SealSettings.PupPrefab);
            if (IsRebuilt(seal) && IsRebuilt(pup))
            {
                message = "Seals already rebuilt (the prefabs survived the scene change)";
                return true;
            }
            var problem = PreFlight(scene, seal, pup);
            if (problem != null)
            {
                message = "Seals stay vanilla: " + problem;
                return false;
            }
            foreach (var (prefab, isPup) in new[] { (seal, false), (pup, true) })
            {
                problem = Rebuild(scene, prefab, isPup);
                if (problem == null) continue;
                message = "Seals stay (partly) vanilla: " + problem;
                return false;
            }
            message = "Seal and Seal_Pup rebuilt: seals are tameable";
            return true;
        }

        private static bool IsRebuilt(GameObject prefab) => prefab != null && prefab.GetComponent<SealCare>() != null;

        /// <summary>Null when everything the rebuild needs is there and no other mod changed the seals; else why not.</summary>
        private static string PreFlight(ZNetScene scene, GameObject seal, GameObject pup)
        {
            if (Chainloader.PluginInfos.ContainsKey(TameableSealsGuid))
                return "TameableSeals (pseudopulse) is installed and changes seals too: remove one of the two mods";
            foreach (var name in SealSettings.RequiredPrefabs)
                if (scene.GetPrefab(name) == null) return $"the game has no prefab '{name}' (game update?)";
            if (scene.GetPrefab(SealSettings.RawFish).GetComponent<ItemDrop>() == null)
                return $"{SealSettings.RawFish} is not an item (game update?)";
            foreach (var setting in SealSettings.All)
            {
                var why = SealSettings.Check(setting);
                if (why != null) return why + " (game update?)";
            }
            foreach (var prefab in new[] { seal, pup })
            {
                if (prefab.GetComponent<Tameable>() != null || prefab.GetComponent<MonsterAI>() != null || prefab.GetComponent<Procreation>() != null)
                    return $"{prefab.name} was already changed by another mod (it has a Tameable, MonsterAI or Procreation)";
                var bodies = prefab.GetComponents<Character>();
                if (bodies.Length != 1 || bodies[0].GetType() != typeof(Character))
                    return $"{prefab.name}'s body isn't a plain Character (game update or another mod)";
                if (prefab.GetComponents<BaseAI>().Length != 1 || prefab.GetComponent<AnimalAI>() == null)
                    return $"{prefab.name}'s AI isn't a single AnimalAI (game update or another mod)";
            }
            return null;
        }

        /// <summary>Null on success; else what failed (a refused swap leaves its part of the prefab as it was).</summary>
        private static string Rebuild(ZNetScene scene, GameObject prefab, bool isPup)
        {
            GameObject Prefab(string name) => scene.GetPrefab(name);
            var body = ComponentSwap.Replace<Humanoid>(prefab, prefab.GetComponent<Character>(), typeof(Character));
            if (body == null) return $"Unity refused to swap {prefab.name}'s Character for a Humanoid";
            EmptyItemLists(body);
            var ai = ComponentSwap.Replace<MonsterAI>(prefab, prefab.GetComponent<AnimalAI>(), typeof(BaseAI));
            if (ai == null) return $"Unity refused to swap {prefab.name}'s AnimalAI for a MonsterAI";
            SettingsApplier.Apply(body, SealSettings.HumanoidFor(isPup), Prefab);
            SettingsApplier.Apply(ai, SealSettings.WildSwitch.Concat(SealSettings.AI), Prefab);
            SettingsApplier.Apply(prefab.AddComponent<Tameable>(), SealSettings.TameableFor(isPup), Prefab);
            if (!isPup) SettingsApplier.Apply(prefab.AddComponent<Procreation>(), SealSettings.Breeding, Prefab);
            prefab.AddComponent<SealCare>().m_isPup = isPup;
            return null;
        }

        /// <summary>A fresh Humanoid's item arrays are null, and Humanoid.GiveDefaultItems reads their Length unguarded.</summary>
        private static void EmptyItemLists(Humanoid body)
        {
            body.m_defaultItems = new GameObject[0];
            body.m_randomWeapon = new GameObject[0];
            body.m_randomArmor = new GameObject[0];
            body.m_randomShield = new GameObject[0];
            body.m_randomSets = new Humanoid.ItemSet[0];
            body.m_randomItems = new Humanoid.RandomItem[0];
        }
    }
}
