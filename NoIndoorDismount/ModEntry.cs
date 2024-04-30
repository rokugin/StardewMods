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

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.ShouldDismountOnWarp)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Game1_ShouldDismountOnWarp_Prefix)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Game1_ShouldDismountOnWarp_Postfix))
            );
        }

        static void Game1_ShouldDismountOnWarp_Prefix(GameLocation new_location, out GameLocation __state) {
            __state = new_location; // set the passthrough GameLocation argument to the original method's new_location argument
        }

        static void Game1_ShouldDismountOnWarp_Postfix(ref bool __result, GameLocation __state) {
            bool dismount = __state.IsOutdoors || __state.treatAsOutdoors.Value; // check if new_location is outdoors or treated as outdoors
            __result = !dismount; // set the result that's passed back to the original call
        }                         // true causes dismount and we don't want to dismount in places treated as outdoors

    }
}
