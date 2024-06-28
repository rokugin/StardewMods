using StardewModdingAPI;
using HarmonyLib;
using StardewValley;
using System.Reflection.Emit;
using StardewValley.Companions;

namespace NoIndoorDismount {
    internal class ModEntry : Mod {

        //public static ModConfig Config;
        public static IMonitor? SMonitor;

        public override void Entry(IModHelper helper) {
            //Config = helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(FlyingCompanion), nameof(FlyingCompanion.Update)),
                transpiler: new HarmonyMethod(typeof(ModEntry), nameof(FlyingCompanion_Transpiler))
            );
        }

        static string GetAudioCueName() {
            string cueName = Game1.soundBank.Exists("parrotCompanion") ? "parrotCompanion" : "parrot_squawk";
            SMonitor!.Log($"Playing audio cue: ${cueName}.", LogLevel.Info);
            return cueName;
        }

        static IEnumerable<CodeInstruction> FlyingCompanion_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var method = AccessTools.Method(typeof(ModEntry), nameof(GetAudioCueName));

            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldstr, "parrot_squawk")
            ).ThrowIfNotMatch("Couldn't find match for parrot audio name");

            matcher.RemoveInstruction();
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, method)
            );

            return matcher.InstructionEnumeration();
        }

    }
}
