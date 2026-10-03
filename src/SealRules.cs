using System;

namespace SealsAtHome
{
    /// <summary>When a pup grows up (decided by SealCare on the seal's owner). Unity-free, unit-tested.</summary>
    internal static class PupGrowth
    {
        public enum Step
        {
            Wait,
            Stamp,
            Grow,
        }

        /// <param name="tamedAtTicks">World time (ticks) when the pup was first seen tamed; 0 = not yet.</param>
        public static Step Decide(bool tamed, string name, long tamedAtTicks, long nowTicks)
        {
            if (!tamed) return Step.Wait;                                        // wild pups never grow
            if (tamedAtTicks <= 0 || tamedAtTicks > nowTicks) return Step.Stamp; // first seen tamed, or the world clock went back
            if (!string.IsNullOrWhiteSpace(name)) return Step.Wait;              // a named pup stays a baby
            return nowTicks - tamedAtTicks >= (long)(SealSettings.GrowSeconds * TimeSpan.TicksPerSecond) ? Step.Grow : Step.Wait;
        }
    }

    /// <summary>Copying the pet name into the vanilla override-name field, which players without the mod see. Unity-free.</summary>
    internal static class NameCopy
    {
        /// <summary>
        /// What the override field should hold: the name as players with the mod see it (vanilla strips formatting tags),
        /// or empty for none (a blank name counts as none).
        /// </summary>
        public static string Target(string tamedName)
        {
            var shown = tamedName?.RemoveRichTextTags();
            return string.IsNullOrWhiteSpace(shown) ? "" : shown;
        }

        /// <summary>Only when it differs, so the seal's data isn't re-sent every tick.</summary>
        public static bool ShouldWrite(string tamedName, string shown) => Target(tamedName) != (shown ?? "");
    }

    /// <summary>
    /// Taking seals over from games without the mod (decided by SealCare). The game that runs a seal stamps a hidden
    /// heartbeat on it every 10 s when it has the mod; a seal whose heartbeat stopped is run by a game that can't tame,
    /// feed or command it, so a modded game nearby claims it. Unity-free.
    /// </summary>
    internal static class Takeover
    {
        private static readonly long BeatTicks = (long)(SealSettings.BeatSeconds * TimeSpan.TicksPerSecond);
        private static readonly long StaleTicks = (long)(SealSettings.StaleSeconds * TimeSpan.TicksPerSecond);

        /// <summary>Owner: stamp the heartbeat now (never stamped, 10 s since the last one, or a stamp from the future).</summary>
        public static bool ShouldBeat(long beatTicks, long nowTicks) =>
            beatTicks <= 0 || beatTicks > nowTicks || nowTicks - beatTicks >= BeatTicks;

        /// <param name="owner">The seal's owner (peer id); 0 = none yet.</param>
        /// <param name="beatTicks">The seal's heartbeat (world time ticks); 0 = never stamped.</param>
        public static bool ShouldClaim(bool isOwner, long owner, long beatTicks, long nowTicks) =>
            !isOwner && owner != 0 && (beatTicks <= 0 || beatTicks > nowTicks || nowTicks - beatTicks > StaleTicks);
    }
}
