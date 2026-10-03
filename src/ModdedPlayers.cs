namespace SealsAtHome
{
    /// <summary>
    /// Which players run SealsAtHome: each modded game marks its own character's ZDO with a hidden bool (vanilla ignores
    /// unknown keys), so other modded games can tell whether a seal's owner can tame, feed and command it.
    /// </summary>
    internal static class ModdedPlayers
    {
        private static readonly int s_markHash = SealSettings.ModdedPlayerKey.GetStableHashCode();

        /// <summary>Marks the local player's character once (a new character object after a respawn gets marked too).</summary>
        public static void MarkLocalPlayer()
        {
            var player = Player.m_localPlayer;
            if (player == null) return;
            var nview = player.GetComponent<ZNetView>();
            var zdo = nview != null ? nview.GetZDO() : null;
            if (zdo == null || !zdo.IsOwner() || zdo.GetBool(s_markHash)) return;
            zdo.Set(s_markHash, true);
        }

        /// <summary>
        /// Whether the player whose game owns ZDOs as <paramref name="owner"/> runs SealsAtHome; null when no loaded
        /// player's character has that owner (far away, or a server).
        /// </summary>
        public static bool? HasMod(long owner)
        {
            foreach (var player in Player.GetAllPlayers())
            {
                if (player == null) continue;
                var nview = player.GetComponent<ZNetView>();
                var zdo = nview != null ? nview.GetZDO() : null;
                if (zdo != null && zdo.GetOwner() == owner) return zdo.GetBool(s_markHash);
            }
            return null;
        }
    }
}
