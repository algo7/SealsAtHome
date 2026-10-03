using System;
using SealsAtHome;

internal static partial class Tests
{
    private const long Minute = TimeSpan.TicksPerMinute;
    private static readonly long Now = 100_000 * Minute;

    private static void Test_PupGrowth_WildPupsNeverGrow()
    {
        Eq(PupGrowth.Step.Wait, PupGrowth.Decide(false, "", 0, Now), "wild, never stamped");
        Eq(PupGrowth.Step.Wait, PupGrowth.Decide(false, "", Now - 600 * Minute, Now), "wild, old stamp");
    }

    private static void Test_PupGrowth_FirstSeenTamedIsStamped()
    {
        Eq(PupGrowth.Step.Stamp, PupGrowth.Decide(true, "", 0, Now), "tamed, not stamped yet");
        Eq(PupGrowth.Step.Stamp, PupGrowth.Decide(true, "Snowball", 0, Now), "a named pup is stamped too, so clearing the name later counts from taming");
    }

    private static void Test_PupGrowth_ClockWentBackRestamps()
    {
        Eq(PupGrowth.Step.Stamp, PupGrowth.Decide(true, "", Now + 5 * Minute, Now), "stamp in the future: re-stamp, never stuck");
    }

    private static void Test_PupGrowth_NamedPupsStayBabies()
    {
        Eq(PupGrowth.Step.Wait, PupGrowth.Decide(true, "Snowball", Now - 600 * Minute, Now), "named: stays a baby");
        Eq(PupGrowth.Step.Grow, PupGrowth.Decide(true, "   ", Now - 600 * Minute, Now), "a blank name is no name");
        Eq(PupGrowth.Step.Grow, PupGrowth.Decide(true, null, Now - 600 * Minute, Now), "no name");
    }

    private static void Test_PupGrowth_GrowsAfter3000Seconds()
    {
        var almost = TimeSpan.FromSeconds(2999).Ticks;
        var exactly = TimeSpan.FromSeconds(3000).Ticks;
        Eq(PupGrowth.Step.Wait, PupGrowth.Decide(true, "", Now - almost, Now), "49:59 after taming");
        Eq(PupGrowth.Step.Grow, PupGrowth.Decide(true, "", Now - exactly, Now), "50:00 after taming");
    }

    private static void Test_NameCopy_WritesOnlyChanges()
    {
        True(NameCopy.ShouldWrite("Snowball", ""), "new name");
        Eq("Snowball", NameCopy.Target("Snowball"), "copied as is");
        False(NameCopy.ShouldWrite("Snowball", "Snowball"), "already shown: no ZDO write");
        False(NameCopy.ShouldWrite("", ""), "wild seal: nothing to do");
        False(NameCopy.ShouldWrite(null, null), "missing values");
    }

    private static void Test_NameCopy_ClearedOrBlankNamesClearTheField()
    {
        True(NameCopy.ShouldWrite("", "Snowball"), "name cleared: clear the field");
        True(NameCopy.ShouldWrite("  ", "Snowball"), "blank name: clear the field");
        Eq("", NameCopy.Target("  "), "blank counts as none");
        False(NameCopy.ShouldWrite("  ", ""), "blank and empty are the same");
    }

    private static void Test_NameCopy_CatchesUpOnAStaleName()
    {
        True(NameCopy.ShouldWrite("Snowball II", "Snowball"), "renamed while a player without the mod owned it: fixed later");
    }

    private static void Test_NameCopy_StripsFormattingTagsLikeVanilla()
    {
        Eq("Snowball", NameCopy.Target("<b>Snowball</b>"), "shown as players with the mod see it (Tameable.GetHoverName strips tags)");
        Eq("", NameCopy.Target("<size=99></size>"), "tags only: no name");
        False(NameCopy.ShouldWrite("<color=red>Bob</color>", "Bob"), "already shown without its tags");
    }

    private const long Me = 111, Friend = 222;
    private static readonly long Second = TimeSpan.TicksPerSecond;

    private static void Test_Takeover_ClaimsSealsWhoseRunnerHasNoMod()
    {
        True(Takeover.ShouldClaim(false, Friend, 0, Now), "never beaten: a game without the mod runs it");
        True(Takeover.ShouldClaim(false, Friend, Now - 31 * Second, Now), "beat stopped over 30 s ago: runner has no mod now");
    }

    private static void Test_Takeover_NeverFromARunnerWithTheMod()
    {
        False(Takeover.ShouldClaim(false, Friend, Now - 5 * Second, Now), "fresh beat: a modded game runs it, no ping-pong");
        False(Takeover.ShouldClaim(false, Friend, Now - 30 * Second, Now), "up to 30 s (3 missed beats) still counts as modded");
    }

    private static void Test_Takeover_NotWhenUnownedOrOurs()
    {
        False(Takeover.ShouldClaim(false, 0, 0, Now), "no owner yet: vanilla hands it out");
        False(Takeover.ShouldClaim(true, Me, 0, Now), "already ours");
    }

    private static void Test_Takeover_ClockWentBackCountsAsStale()
    {
        True(Takeover.ShouldClaim(false, Friend, Now + 60 * Second, Now), "beat in the future: never trust it forever");
        True(Takeover.ShouldBeat(Now + 60 * Second, Now), "owner re-stamps a beat from the future");
    }

    private static void Test_Takeover_OwnerBeatsEvery10Seconds()
    {
        True(Takeover.ShouldBeat(0, Now), "first beat");
        False(Takeover.ShouldBeat(Now - 9 * Second, Now), "not yet");
        True(Takeover.ShouldBeat(Now - 10 * Second, Now), "10 s since the last beat");
    }
}
