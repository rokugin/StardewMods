using StardewModdingAPI;
using HarmonyLib;
using StardewValley;

namespace NoIndoorDismount {
    internal class ModEntry : Mod {

        public static ModConfig Config;
        public static IMonitor SMonitor;

        public override void Entry(IModHelper helper) {
            Config = helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.ShouldDismountOnWarp)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Game1_ShouldDismountOnWarp_Postfix))
            );
        }

        static void Game1_ShouldDismountOnWarp_Postfix(ref bool __result) {
            __result = false;
        }

        private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
            
        }

    }
}
