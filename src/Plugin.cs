using BepInEx;
using BepInEx.Logging;

namespace SealsAtHome
{
    [BepInPlugin(Guid, Name, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "algo7.sealsathome";
        public const string Name = "SealsAtHome";
        public const string PluginVersion = PluginInfo.Version; // from the git tag, generated at build time (MinVer)

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{Name} loaded (v{PluginVersion})");
        }
    }
}
