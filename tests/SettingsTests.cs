using System.Linq;
using SealsAtHome;

internal static partial class Tests
{
    private static FieldSetting Setting(FieldSetting[] table, string field) => table.Single(s => s.Field == field);

    private static void Test_Settings_EveryFieldExistsWithTheRightType()
    {
        foreach (var setting in SealSettings.All)
            Eq(null, SealSettings.Check(setting), setting.ToString());
    }

    private static void Test_Settings_CheckReportsMistakes()
    {
        True(SealSettings.Check(FieldSetting.Of<MonsterAI>("m_nope", 1f)) != null, "a missing field");
        True(SealSettings.Check(FieldSetting.Of<MonsterAI>("m_alertRange", 1)) != null, "int for a float field");
        True(SealSettings.Check(FieldSetting.Effects<Tameable>("m_tamingTime", "x")) != null, "effects for a float field");
    }

    private static void Test_Settings_RequiredPrefabs()
    {
        var names = SealSettings.RequiredPrefabs.ToList();
        Eq("Seal,Seal_Pup,FishRaw,vfx_creature_soothed,fx_creature_tamed,sfx_seal_idle,sfx_babyseal_idle,vfx_boar_love,vfx_boar_birth",
            string.Join(",", names), "every prefab the rebuild uses, once");
    }

    private static void Test_Settings_VanillaNumbers()
    {
        var tame = SealSettings.TameableFor(false);
        Eq(1800f, (float)Setting(tame, "m_tamingTime").Value, "taming 30 min");
        Eq(600f, (float)Setting(tame, "m_fedDuration").Value, "fed 10 min");
        Eq(true, (bool)Setting(tame, "m_commandable").Value, "commandable like the wolf");
        Eq(3000d, SealSettings.GrowSeconds, "grow-up time of vanilla cubs");
        var b = SealSettings.Breeding;
        Eq("30,10,5,3,0.33,60,3,0,1",
            string.Join(",", new object[]
            {
                Setting(b, "m_updateInterval").Value, Setting(b, "m_totalCheckRange").Value, Setting(b, "m_maxCreatures").Value,
                Setting(b, "m_partnerCheckRange").Value, Setting(b, "m_pregnancyChance").Value, Setting(b, "m_pregnancyDuration").Value,
                Setting(b, "m_requiredLovePoints").Value, Setting(b, "m_minOffspringLevel").Value, Setting(b, "m_spawnOffset").Value,
            }.Select(v => System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture))),
            "Boar / Moose breeding values");
        Eq("Seal_Pup", (string)Setting(b, "m_offspring").Value, "offspring");
    }

    private static void Test_Settings_SealSoundsNeverTheWolf()
    {
        Eq("vfx_creature_soothed,sfx_seal_idle", string.Join(",", Setting(SealSettings.TameableFor(false), "m_petEffect").PrefabNames), "adult pet effect");
        Eq("vfx_creature_soothed,sfx_babyseal_idle", string.Join(",", Setting(SealSettings.TameableFor(true), "m_petEffect").PrefabNames), "pup pet effect");
        Eq("sfx_babyseal_idle", string.Join(",", Setting(SealSettings.HumanoidFor(true), "m_consumeItemEffects").PrefabNames), "pup eating sound");
        False(SealSettings.RequiredPrefabs.Any(n => n.ToLowerInvariant().Contains("wolf")), "no wolf effect anywhere");
    }

    private static void Test_Settings_WildAndTamedSwitchTheSameFields()
    {
        Eq(string.Join(",", SealSettings.WildSwitch.Select(s => s.Field)), string.Join(",", SealSettings.TamedSwitch.Select(s => s.Field)), "same fields");
        Eq(0f, (float)Setting(SealSettings.WildSwitch, "m_alertRange").Value, "wild: run as soon as they notice you");
        Eq(10f, (float)Setting(SealSettings.TamedSwitch, "m_alertRange").Value, "tamed: enemies within 10 m of you");
        Eq(true, (bool)Setting(SealSettings.WildSwitch, "m_fleeIfNotAlerted").Value, "wild flee");
        Eq(false, (bool)Setting(SealSettings.TamedSwitch, "m_fleeIfNotAlerted").Value, "tamed: keep following");
        False(SealSettings.AI.Any(s => SealSettings.WildSwitch.Any(w => w.Field == s.Field)), "switch fields aren't in the shared table");
    }
}
