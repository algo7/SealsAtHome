using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SealsAtHome
{
    /// <summary>
    /// Which fields Unity serializes on a MonoBehaviour, i.e. what Instantiate copies into every instance: instance fields,
    /// not readonly or const, public without [NonSerialized] or non-public with [SerializeField]; never delegates.
    /// Needs no running engine (unit-tested against the game's types).
    /// </summary>
    internal static class UnitySerialization
    {
        public static bool IsSerialized(FieldInfo field)
        {
            if (field.IsStatic || field.IsInitOnly || field.IsLiteral) return false;
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) return false;
            if (field.IsPublic) return !field.IsNotSerialized;
            return field.GetCustomAttribute<SerializeField>() != null;
        }

        /// <summary>The serialized fields declared on <paramref name="type"/> and its bases, up to (not including) MonoBehaviour.</summary>
        public static IEnumerable<FieldInfo> SerializedFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (var t = type; t != null && t != typeof(MonoBehaviour) && t != typeof(object); t = t.BaseType)
                foreach (var field in t.GetFields(flags))
                    if (IsSerialized(field)) yield return field;
        }
    }
}
