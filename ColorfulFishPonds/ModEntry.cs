using StardewModdingAPI;
using HarmonyLib;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using StardewValley;

namespace ColorfulFishPonds {
    internal class ModEntry : Mod {

        public static ModConfig Config;
        public static IMonitor SMonitor;

        static ModData ColorModel;

        public override void Entry(IModHelper helper) {
            Config = helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);
            
            harmony.Patch(
                original: AccessTools.Method(typeof(FishPond), "doFishSpecificWaterColoring"),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(FishPond_DoFishSpecificWaterColoring_Postfix))
            );
        }

        static void FishPond_DoFishSpecificWaterColoring_Postfix(FishPond __instance) {
            if (!Config.ModEnabled) return;
            
            Color? color = null;

            if (Config.UseContextColors) {
                color = ItemContextTagManager.GetColorFromTags(ItemRegistry.Create(ItemRegistry.type_object + __instance.fishType.Value));
            }
            
            foreach (var fish in ColorModel.FishPondColorOverrides) {
                string[] splitOverride = fish.Value.Split(" ");
                int reqPop = Int32.Parse(splitOverride[1]);
                Color? pondOverrideColor = new Color(int.Parse(splitOverride[2]), int.Parse(splitOverride[3]), int.Parse(splitOverride[4]));

                if (__instance.fishType.Value == splitOverride[0] && __instance.currentOccupants.Value > reqPop) {
                    color = pondOverrideColor;
                }
            }
            
            __instance.overrideWaterColor.Value = color.Value;
        }
        
        private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
            Helper.Data.WriteJsonFile("data.json", new ModData());
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned()) {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}", LogLevel.Info);

                if (!contentPack.HasFile("content.json")) {
                    Monitor.Log($"Content pack: {contentPack.Manifest.Name} missing required file: content.json, Skipping content pack.", LogLevel.Error);
                    continue;
                }

                ColorModel = Helper.Data.ReadJsonFile<ModData>("data.json") ?? new ModData();
                
                ModData? data = contentPack.ReadJsonFile<ModData>("content.json");
                Monitor.Log($"{data.FishPondColorOverrides.Count} overrides.", LogLevel.Info);

                foreach (var fish in data.FishPondColorOverrides) {
                    if (ColorModel.FishPondColorOverrides.ContainsKey(fish.Key)) {
                        Monitor.Log($"Content pack: {fish.Key} already present, skipping.\n", LogLevel.Warn);
                        continue;
                    }
                    Monitor.Log($"{fish.Key} color override found, applying.\n", LogLevel.Info);
                    ColorModel.FishPondColorOverrides.Add(fish.Key, fish.Value);
                    Helper.Data.WriteJsonFile("data.json", ColorModel);
                }
            }
        }

        private void OnSaveLoaded(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e) {
            var buildings = Game1.getFarm().buildings;
            foreach (var building in buildings) { 
                if (building.buildingType.Value == "Fish Pond") {
                    Helper.Reflection.GetMethod((FishPond)building, "doFishSpecificWaterColoring").Invoke();
                }
            }
        }

    }
}
