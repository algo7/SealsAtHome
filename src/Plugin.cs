using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
            Log.LogInfo($"{Name} loaded (v{PluginVersion})");
        }

        private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        /// <summary>
        /// The game scene: ZNetScene.Awake has registered every prefab, and creatures are only created later in
        /// ZNetScene.Update, so the seal prefabs are rebuilt before any seal exists. Never throws.
        /// </summary>
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {
                if (Application.isBatchMode) return; // dedicated server: nothing to do there
                var netScene = ZNetScene.instance;
                if (netScene == null) return; // main menu
                var seals = Character.GetAllCharacters()
                    .Count(c => c != null && c.m_name.StartsWith("$enemy_seal", StringComparison.Ordinal));
                Log.LogInfo($"Scene '{scene.name}': prefabs registered, {seals} seals exist yet (expected 0)");
                if (SealRebuild.Run(netScene, out var message)) Log.LogInfo(message);
                else Log.LogWarning(message);
            }
            catch (Exception e)
            {
                Log.LogError($"Seal rebuild failed, seals stay vanilla: {e}");
            }
        }
    }
}
