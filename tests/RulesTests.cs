using System;
using System.Collections.Generic;
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

    private const long Me = 111, Friend = 222, Other = 333;
    private static readonly long Second = TimeSpan.TicksPerSecond;

    /// <summary>Ticks every 2 s (real time) from <paramref name="from"/> to <paramref name="to"/> on one seal; true if a tick claims it.</summary>
    private static bool ClaimsBetween(TakeoverWatch watch, IDictionary<long, double> noMod, double from, double to, Func<double, (long owner, long beat)> seal)
    {
        for (var t = from; t <= to; t += 2)
        {
            var (owner, beat) = seal(t);
            if (watch.Decide(false, owner, beat, t, noMod) != TakeoverWatch.Step.Wait) return true;
        }
        return false;
    }

    private static void Test_Takeover_NeverFromAGameThatStamps()
    {
        var noMod = new Dictionary<long, double>();
        False(ClaimsBetween(new TakeoverWatch(), noMod, 0, 120, t => (Friend, 1 + (long)(t / 11))), "a modded runner stamps every 10-12 s: never taken");
        Eq(0, noMod.Count, "nobody mistaken for a game without the mod");
    }

    private static void Test_Takeover_NeverDuringASleepFastForward()
    {
        // Sleeping fast-forwards world time ~33x for 12 s: by the world clock every stamp looks old, but it changes every tick.
        False(ClaimsBetween(new TakeoverWatch(), new Dictionary<long, double>(), 0, 60, t => (Friend, 1000 + (long)t)), "stamps every tick: never taken");
    }

    private static void Test_Takeover_FromAGameThatNeverStampsAfter30Seconds()
    {
        var noMod = new Dictionary<long, double>();
        var watch = new TakeoverWatch();
        Eq(TakeoverWatch.Step.Wait, watch.Decide(false, Friend, 5, 100, noMod), "first sight");
        Eq(TakeoverWatch.Step.Wait, watch.Decide(false, Friend, 5, 129.9, noMod), "unchanged for 29.9 s");
        Eq(TakeoverWatch.Step.FoundNoMod, watch.Decide(false, Friend, 5, 130.1, noMod), "unchanged for 30.1 s: their game doesn't run the mod");
        True(noMod.ContainsKey(Friend), "remembered");
    }

    private static void Test_Takeover_RemembersAGameWithoutTheMod()
    {
        var noMod = new Dictionary<long, double> { [Friend] = 100 };
        Eq(TakeoverWatch.Step.Claim, new TakeoverWatch().Decide(false, Friend, 5, 100, noMod), "another seal they run: taken at once");
        var watch = new TakeoverWatch();
        Eq(TakeoverWatch.Step.Claim, watch.Decide(false, Friend, 0, 100, noMod), "taken at once");
        Eq(TakeoverWatch.Step.Claim, watch.Decide(false, Friend, 0, 102, noMod), "our takeover was undone: retried at the next tick");
        Eq(TakeoverWatch.Step.Claim, watch.Decide(false, Friend, 77, 104, noMod), "undone with our own stamp left on it: still retried");
        True(noMod.ContainsKey(Friend), "our own stamp isn't taken as theirs");
    }

    private static void Test_Takeover_ANewRunnerGetsTheFull30Seconds()
    {
        var noMod = new Dictionary<long, double>();
        var watch = new TakeoverWatch();
        False(ClaimsBetween(watch, noMod, 0, 24, t => (Friend, 5)), "a game without the mod, watched 24 s");
        // Handed to Other, still loading the area: its first stamp comes 8 s later.
        False(ClaimsBetween(watch, noMod, 26, 80, t => (Other, t < 34 ? 5 : 6 + (long)((t - 34) / 10))), "a new runner that stamps is never taken");
        False(noMod.ContainsKey(Other), "not mistaken for a game without the mod");
    }

    private static void Test_Takeover_ANewRunnerThatNeverStampsAfter30Seconds()
    {
        var noMod = new Dictionary<long, double>();
        var watch = new TakeoverWatch();
        False(ClaimsBetween(watch, noMod, 0, 24, t => (Friend, 5)), "watched 24 s");
        False(ClaimsBetween(watch, noMod, 26, 56, t => (Other, 5)), "handed over at 26 s: 30 s from then");
        Eq(TakeoverWatch.Step.FoundNoMod, watch.Decide(false, Other, 5, 56.1, noMod), "30.1 s after the handover");
    }

    private static void Test_Takeover_NotWhenUnownedOrOurs()
    {
        var noMod = new Dictionary<long, double> { [Friend] = 100 };
        Eq(TakeoverWatch.Step.Wait, new TakeoverWatch().Decide(false, 0, 0, 100, noMod), "no owner yet: vanilla hands it out");
        Eq(TakeoverWatch.Step.Wait, new TakeoverWatch().Decide(true, Me, 0, 100, noMod), "already ours");
        // While ours it forgets: losing the seal to a runner that never stamps waits a full 30 s from then.
        var fresh = new Dictionary<long, double>();
        var watch = new TakeoverWatch();
        False(ClaimsBetween(watch, fresh, 0, 20, t => (Other, 5)), "watching");
        Eq(TakeoverWatch.Step.Wait, watch.Decide(true, Me, 5, 22, fresh), "ours for a moment");
        False(ClaimsBetween(watch, fresh, 24, 54, t => (Other, 5)), "lost again at 24 s: 30 s from then");
    }

    private static void Test_Takeover_RemembersPerPlayer()
    {
        var noMod = new Dictionary<long, double> { [Friend] = 100 };
        var watch = new TakeoverWatch();
        False(ClaimsBetween(watch, noMod, 100, 130, t => (Other, 5)), "someone else's seal still waits 30 s");
        Eq(TakeoverWatch.Step.FoundNoMod, watch.Decide(false, Other, 5, 130.1, noMod), "then found without the mod too");
    }

    private static void Test_Takeover_NoOwnerResetsTheWatch()
    {
        var noMod = new Dictionary<long, double>();
        var watch = new TakeoverWatch();
        False(ClaimsBetween(watch, noMod, 0, 20, t => (Other, 5)), "watching");
        Eq(TakeoverWatch.Step.Wait, watch.Decide(false, 0, 5, 22, noMod), "released for a moment");
        False(ClaimsBetween(watch, noMod, 24, 54, t => (Other, 5)), "handed back at 24 s: 30 s from then");
    }

    private static void Test_Takeover_ForgetsAfterFiveMinutes()
    {
        // A modded game that froze for half a minute isn't taken from for the rest of the session.
        var noMod = new Dictionary<long, double> { [Friend] = 100 };
        var watch = new TakeoverWatch();
        Eq(TakeoverWatch.Step.Claim, watch.Decide(false, Friend, 5, 399.9, noMod), "found 4:59.9 ago: still taken at once");
        Eq(TakeoverWatch.Step.Wait, watch.Decide(false, Friend, 5, 400.1, noMod), "found over 5 min ago: watched again");
        False(noMod.ContainsKey(Friend), "forgotten");
        False(ClaimsBetween(watch, noMod, 402, 430, t => (Friend, 5)), "watched for 30 s again");
        Eq(TakeoverWatch.Step.FoundNoMod, watch.Decide(false, Friend, 5, 430.2, noMod), "still not stamping: found again");
    }

    private static void Test_Takeover_OwnerRestampsABeatFromTheFuture()
    {
        True(Takeover.ShouldBeat(Now + 60 * Second, Now), "the world clock went back: re-stamp, never stuck");
    }

    private static void Test_Takeover_OwnerBeatsEvery10Seconds()
    {
        True(Takeover.ShouldBeat(0, Now), "first beat");
        False(Takeover.ShouldBeat(Now - 9 * Second, Now), "not yet");
        True(Takeover.ShouldBeat(Now - 10 * Second, Now), "10 s since the last beat");
    }
}
