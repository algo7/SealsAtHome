using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SealsAtHome
{
    /// <summary>Swaps one component of a prefab for another, keeping its settings and everything that pointed at it.</summary>
    internal static class ComponentSwap
    {
        /// <summary>
        /// Adds a <typeparamref name="TNew"/>, copies the serialized fields declared on <paramref name="copyFieldsOf"/> (and
        /// its bases below MonoBehaviour) from <paramref name="old"/>, points every serialized reference to the old component
        /// inside the prefab at the new one, then destroys the old one. The new component goes in first: CharacterDrop
        /// requires a Character, and a Humanoid is one. If Unity refuses to remove the old component, everything is undone
        /// and null is returned (the prefab is as it was).
        /// </summary>
        public static TNew Replace<TNew>(GameObject prefab, Component old, Type copyFieldsOf) where TNew : Component
        {
            var replacement = prefab.AddComponent<TNew>();
            foreach (var field in UnitySerialization.SerializedFields(copyFieldsOf))
                field.SetValue(replacement, field.GetValue(old));
            Repoint(prefab, old, replacement);
            var oldName = old.GetType().Name;
            try
            {
                Object.DestroyImmediate(old, true); // prefab loaded from an asset bundle: an asset, so allow it
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Could not remove {oldName} from {prefab.name}: {e.Message}");
            }
            if (old == null) return replacement; // Unity's ==: true once destroyed
            Repoint(prefab, replacement, old);
            Object.DestroyImmediate(replacement, true);
            return null;
        }

        private static void Repoint(GameObject prefab, Component from, Component to)
        {
            foreach (var behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null) continue;
                foreach (var field in UnitySerialization.SerializedFields(behaviour.GetType()))
                    if (field.FieldType.IsInstanceOfType(to) && ReferenceEquals(field.GetValue(behaviour), from))
                        field.SetValue(behaviour, to);
            }
        }
    }
}
