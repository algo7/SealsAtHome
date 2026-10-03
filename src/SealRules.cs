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
    /// Whether this game takes a seal over from the player whose game runs it (decided by SealCare on non-owners).
    /// A game without the mod can't tame, feed or command seals, so a modded player nearby runs them instead. Unity-free.
    /// </summary>
    internal static class Takeover
    {
        /// <param name="owner">The seal's owner (peer id); 0 = none yet.</param>
        /// <param name="ownerHasMod">Whether the owner's character carries the SealsAtHome mark; null = not loaded here.</param>
        public static bool ShouldClaim(bool isOwner, long owner, bool? ownerHasMod) =>
            !isOwner && owner != 0 && ownerHasMod == false; // never from a modded or unknown owner: no ping-pong
    }
}
