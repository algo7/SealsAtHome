using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SealsAtHome.FieldSetting;

namespace SealsAtHome
{
    internal enum SettingKind
    {
        /// <summary>A float, bool or int, set as is.</summary>
        Value,

        /// <summary>An EffectList built from vanilla effect prefabs, by name.</summary>
        Effects,

        /// <summary>A vanilla prefab, by name.</summary>
        Prefab,

        /// <summary>A List&lt;ItemDrop&gt; of vanilla item prefabs, by name.</summary>
        Items,
    }

    /// <summary>
    /// One public field the rebuild sets on a vanilla component, with its value. Prefabs are names, so the tables need no
    /// running engine: the tests (and the runtime pre-flight) check every field against the game's types.
    /// </summary>
    internal sealed class FieldSetting
    {
        public readonly Type Component;
        public readonly string Field;
        public readonly SettingKind Kind;
        public readonly object Value;

        private FieldSetting(Type component, string field, SettingKind kind, object value)
        {
            Component = component;
            Field = field;
            Kind = kind;
            Value = value;
        }

        public static FieldSetting Of<T>(string field, object value) => new FieldSetting(typeof(T), field, SettingKind.Value, value);
        public static FieldSetting Effects<T>(string field, params string[] prefabs) => new FieldSetting(typeof(T), field, SettingKind.Effects, prefabs);
        public static FieldSetting Prefab<T>(string field, string prefab) => new FieldSetting(typeof(T), field, SettingKind.Prefab, prefab);
        public static FieldSetting Items<T>(string field, params string[] items) => new FieldSetting(typeof(T), field, SettingKind.Items, items);

        /// <summary>The type the game's field must have for this setting.</summary>
        public Type ExpectedFieldType
        {
            get
            {
                switch (Kind)
                {
                    case SettingKind.Effects: return typeof(EffectList);
                    case SettingKind.Prefab: return typeof(GameObject);
                    case SettingKind.Items: return typeof(List<ItemDrop>);
                    default: return Value.GetType();
                }
            }
        }

        /// <summary>The vanilla prefabs this setting needs.</summary>
        public IEnumerable<string> PrefabNames
        {
            get
            {
                switch (Kind)
                {
                    case SettingKind.Effects:
                    case SettingKind.Items: return (string[])Value;
                    case SettingKind.Prefab: return new[] { (string)Value };
                    default: return Enumerable.Empty<string>();
                }
            }
        }

        public override string ToString() => $"{Component.Name}.{Field}";
    }

    /// <summary>Everything SealsAtHome sets: vanilla numbers and vanilla prefabs only, not configurable.</summary>
    internal static class SealSettings
    {
        public const string SealPrefab = "Seal";
        public const string PupPrefab = "Seal_Pup";
        public const string RawFish = "FishRaw";
        public const string SealSound = "sfx_seal_idle";
        public const string PupSound = "sfx_babyseal_idle";
        public const string Hearts = "vfx_creature_soothed";
        public const string TamedEffect = "fx_creature_tamed";
        public const string LoveHearts = "vfx_boar_love";
        public const string BirthSplash = "vfx_boar_birth";

        /// <summary>World seconds from taming to growing up (vanilla cubs' Growup: 3000 s).</summary>
        public const double GrowSeconds = 3000;

        /// <summary>Hidden ZDO key: world time (ticks) when SealCare first saw a pup tamed.</summary>
        public const string TamedAtKey = "SealsAtHome_TamedAt";

        /// <summary>Wild seals run as soon as they notice you (~2 m).</summary>
        public static readonly FieldSetting[] WildSwitch =
        {
            Of<MonsterAI>("m_alertRange", 0f),
            Of<MonsterAI>("m_fleeIfNotAlerted", true),
        };

        /// <summary>Tamed seals keep following / staying; they care about enemies within 10 m of you or their stay spot.</summary>
        public static readonly FieldSetting[] TamedSwitch =
        {
            Of<MonsterAI>("m_alertRange", 10f),
            Of<MonsterAI>("m_fleeIfNotAlerted", false),
        };

        /// <summary>The other MonsterAI settings, wild and tamed alike. Shared BaseAI settings are copied from the seal's AnimalAI.</summary>
        public static readonly FieldSetting[] AI =
        {
            Of<MonsterAI>("m_fleeIfLowHealth", 1f),    // health below 100 %: any hit makes it run
            Of<MonsterAI>("m_fleeTimeSinceHurt", 15f), // the vanilla seal's 15 s to feel safe
            Of<MonsterAI>("m_attackPlayerObjects", false),
            Of<MonsterAI>("m_enableHuntPlayer", false),
            Of<MonsterAI>("m_sleeping", false),
            Of<MonsterAI>("m_avoidLand", false),
            Of<MonsterAI>("m_consumeRange", 1f),       // the Hen's
            Of<MonsterAI>("m_consumeSearchRange", 10f),
            Of<MonsterAI>("m_consumeSearchInterval", 10f),
            Items<MonsterAI>("m_consumeItems", RawFish),
        };

        public static FieldSetting[] TameableFor(bool pup) => new[]
        {
            Of<Tameable>("m_tamingTime", 1800f),
            Of<Tameable>("m_fedDuration", 600f),
            Of<Tameable>("m_commandable", true),
            Of<Tameable>("m_startsTamed", false),
            Of<Tameable>("m_unsummonDistance", 0f),
            Of<Tameable>("m_unsummonOnOwnerLogoutSeconds", 0f),
            Of<Tameable>("m_tamingSpeedMultiplierRange", 60f),
            Of<Tameable>("m_tamingBoostMultiplier", 2f),
            Effects<Tameable>("m_sootheEffect", Hearts),
            Effects<Tameable>("m_tamedEffect", TamedEffect),
            Effects<Tameable>("m_petEffect", Hearts, pup ? PupSound : SealSound),
        };

        /// <summary>Vanilla Boar / Moose values.</summary>
        public static readonly FieldSetting[] Breeding =
        {
            Of<Procreation>("m_updateInterval", 30f),
            Of<Procreation>("m_totalCheckRange", 10f),
            Of<Procreation>("m_maxCreatures", 5),
            Of<Procreation>("m_partnerCheckRange", 3f),
            Of<Procreation>("m_pregnancyChance", 0.33f),
            Of<Procreation>("m_pregnancyDuration", 60f),
            Of<Procreation>("m_requiredLovePoints", 3),
            Prefab<Procreation>("m_offspring", PupPrefab),
            Of<Procreation>("m_minOffspringLevel", 0),
            Of<Procreation>("m_spawnOffset", 1f),
            Effects<Procreation>("m_loveEffects", LoveHearts, SealSound),
            Effects<Procreation>("m_birthEffects", BirthSplash, PupSound),
        };

        /// <summary>There's no eating animation: eating plays the seal's sound.</summary>
        public static FieldSetting[] HumanoidFor(bool pup) => new[]
        {
            Effects<Humanoid>("m_consumeItemEffects", pup ? PupSound : SealSound),
        };

        public static IEnumerable<FieldSetting> All =>
            WildSwitch.Concat(TamedSwitch).Concat(AI)
                .Concat(TameableFor(false)).Concat(TameableFor(true))
                .Concat(Breeding)
                .Concat(HumanoidFor(false)).Concat(HumanoidFor(true));

        public static IEnumerable<string> RequiredPrefabs =>
            new[] { SealPrefab, PupPrefab }.Concat(All.SelectMany(s => s.PrefabNames)).Distinct();

        /// <summary>Null when the game's component has this public field with the expected type; else why not.</summary>
        public static string Check(FieldSetting setting)
        {
            var field = setting.Component.GetField(setting.Field, BindingFlags.Instance | BindingFlags.Public);
            if (field == null) return $"{setting} doesn't exist";
            if (field.FieldType != setting.ExpectedFieldType)
                return $"{setting} is a {field.FieldType.Name}, not a {setting.ExpectedFieldType.Name}";
            return null;
        }
    }
}
