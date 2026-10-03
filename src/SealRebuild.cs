using UnityEngine;

namespace SealsAtHome
{
    /// <summary>Spike: swaps the Seal and Seal_Pup prefabs' body and brain (Task 4 adds settings and taming).</summary>
    internal static class SealRebuild
    {
        public static bool Run(ZNetScene scene, out string message)
        {
            var seal = scene.GetPrefab(SealSettings.SealPrefab);
            var pup = scene.GetPrefab(SealSettings.PupPrefab);
            if (seal == null || pup == null)
            {
                message = "Seals stay vanilla: Seal or Seal_Pup prefab not found";
                return false;
            }
            if (seal.GetComponent<MonsterAI>() != null)
            {
                message = "Seals already rebuilt (the prefabs survived the scene change)";
                return true;
            }
            foreach (var prefab in new[] { seal, pup })
            {
                var body = ComponentSwap.Replace<Humanoid>(prefab, prefab.GetComponent<Character>(), typeof(Character));
                if (body == null)
                {
                    message = $"Seals stay vanilla: Unity refused to swap {prefab.name}'s Character for a Humanoid";
                    return false;
                }
                EmptyItemLists(body);
                if (ComponentSwap.Replace<MonsterAI>(prefab, prefab.GetComponent<AnimalAI>(), typeof(BaseAI)) == null)
                {
                    message = $"Seals stay vanilla: Unity refused to swap {prefab.name}'s AnimalAI for a MonsterAI";
                    return false;
                }
            }
            message = $"Seal and Seal_Pup rebuilt (body and brain): {Describe(seal)} / {Describe(pup)}";
            return true;
        }

        private static string Describe(GameObject prefab) =>
            $"{prefab.GetComponents<Character>().Length} Character ({prefab.GetComponent<Character>().GetType().Name}), " +
            $"{prefab.GetComponents<BaseAI>().Length} BaseAI ({prefab.GetComponent<BaseAI>().GetType().Name})";

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
