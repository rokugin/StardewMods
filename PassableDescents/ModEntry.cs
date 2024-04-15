using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace PassableDescents {
    internal class ModEntry : Mod {

        static IMonitor StaticMonitor;
        static Vector2 tileVector2;

        public override void Entry(IModHelper helper) {
            StaticMonitor = Monitor;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.createLadderDown)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(CreateLadderDownPrefix))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.createLadderDown)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(CreateLadderDownPostfix))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.createLadderAt)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(CreateLadderAtPrefix))
                );
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.createLadderAt)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(CreateLadderAtPostfix))
                );
        }

        static void CreateLadderDownPrefix(int x, int y) {
            tileVector2 = new Vector2(x, y);
        }

        static void CreateLadderDownPostfix() {
            Game1.currentLocation.Map.GetLayer("Buildings").Tiles[(int)tileVector2.X, (int)tileVector2.Y].Properties.Add("Passable", "T");
        }

        static void CreateLadderAtPrefix(Vector2 p) {
            tileVector2 = p;
        }

        static void CreateLadderAtPostfix() {
            Game1.currentLocation.Map.GetLayer("Buildings").Tiles[(int)tileVector2.X, (int)tileVector2.Y].Properties.Add("Passable", "T");
        }

    }
}
