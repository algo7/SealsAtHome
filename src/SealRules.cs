using System;
using System.Collections.Generic;

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
    /// feed or command it, so a modded game nearby claims it (TakeoverWatch). Unity-free.
    /// </summary>
    internal static class Takeover
    {
        private static readonly long BeatTicks = (long)(SealSettings.BeatSeconds * TimeSpan.TicksPerSecond);

        /// <summary>Owner: stamp the heartbeat now (never stamped, 10 s since the last one, or a stamp from the future).</summary>
        public static bool ShouldBeat(long beatTicks, long nowTicks) =>
            beatTicks <= 0 || beatTicks > nowTicks || nowTicks - beatTicks >= BeatTicks;
    }

    /// <summary>
    /// One seal, on a game that doesn't run it: whether to take it over. A runner is found to have no mod when this game
    /// sees neither the runner nor the heartbeat change for StaleSeconds of real time (a modded runner stamps every 10 s,
    /// so within about 12 s of getting a seal; the world clock can't be used: sleeping fast-forwards it ~33x). Players
    /// found so (noMod: peer id, new on every connection → real time found) are taken from at once on every seal for
    /// ForgetSeconds, which also redoes a takeover the game undid (a runner still sending the seal's movement overwrites
    /// it); then they're watched again. Unity-free.
    /// </summary>
    internal sealed class TakeoverWatch
    {
        public enum Step
        {
            Wait,
            Claim,
            /// <summary>Claim; the runner was just found to have no mod (worth a log line).</summary>
            FoundNoMod,
        }

        private bool m_watching;
        private long m_owner;
        private long m_beat;
        private double m_since;

        /// <param name="owner">The seal's owner (peer id); 0 = none yet.</param>
        /// <param name="beat">The seal's heartbeat as last seen; its value only matters when it changes.</param>
        /// <param name="nowSeconds">Real time (seconds).</param>
        public Step Decide(bool isOwner, long owner, long beat, double nowSeconds, IDictionary<long, double> noMod)
        {
            if (isOwner || owner == 0)
            {
                m_watching = false;
                return Step.Wait;
            }
            if (noMod.TryGetValue(owner, out var found))
            {
                if (nowSeconds - found <= SealSettings.ForgetSeconds) return Step.Claim;
                noMod.Remove(owner);
            }
            if (!m_watching || owner != m_owner || beat != m_beat)
            {
                m_watching = true;
                m_owner = owner;
                m_beat = beat;
                m_since = nowSeconds;
                return Step.Wait;
            }
            if (nowSeconds - m_since <= SealSettings.StaleSeconds) return Step.Wait;
            noMod[owner] = nowSeconds;
            return Step.FoundNoMod;
        }
    }
}
