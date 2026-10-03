using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SealsAtHome
{
    /// <summary>Writes SealSettings tables into live components (fields checked by SealSettings.Check beforehand).</summary>
    internal static class SettingsApplier
    {
        /// <param name="prefab">Looks up vanilla prefabs by name; may be null when every setting is a plain value.</param>
        public static void Apply(Component target, IEnumerable<FieldSetting> settings, Func<string, GameObject> prefab)
        {
            foreach (var setting in settings)
                setting.Component.GetField(setting.Field, BindingFlags.Instance | BindingFlags.Public)
                    .SetValue(target, Resolve(setting, prefab));
        }

        private static object Resolve(FieldSetting setting, Func<string, GameObject> prefab)
        {
            switch (setting.Kind)
            {
                case SettingKind.Effects:
                    return new EffectList
                    {
                        m_effectPrefabs = setting.PrefabNames.Select(n => new EffectList.EffectData { m_prefab = prefab(n) }).ToArray(),
                    };
                case SettingKind.Prefab:
                    return prefab((string)setting.Value);
                case SettingKind.Items:
                    return setting.PrefabNames.Select(n => prefab(n).GetComponent<ItemDrop>()).ToList();
                default:
                    return setting.Value;
            }
        }
    }
}
