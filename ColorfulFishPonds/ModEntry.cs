using StardewModdingAPI;
using HarmonyLib;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Diagnostics;

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

            Color? color = Color.White;

            if (Config.UseContextColors) {
                if (__instance.fishType.Value == "162" && __instance.currentOccupants.Value >= Config.RequiredPopulation) {
                    color = new Color(250, 30, 30);
                } else if (__instance.fishType.Value == "796" && __instance.currentOccupants.Value >= Config.RequiredPopulation) {
                    color = new Color(60, 255, 60);
                } else if (__instance.fishType.Value == "795" && __instance.currentOccupants.Value >= Config.RequiredPopulation) {
                    color = new Color(120, 20, 110);
                } else if (__instance.fishType.Value == "155" && __instance.currentOccupants.Value >= Config.RequiredPopulation) {
                    color = new Color(150, 100, 200);
                } else {
                    if (__instance.currentOccupants.Value >= Config.RequiredPopulation) {
                        color = ItemContextTagManager.GetColorFromTags(ItemRegistry.Create(ItemRegistry.type_object + __instance.fishType.Value));
                    }
                }
            }
            
            foreach (var fish in ColorModel.Changes) {
                Color? pondOverrideColor = new Color(fish.Value.PondColor.GetValueOrDefault("R"),
                                                     fish.Value.PondColor.GetValueOrDefault("G"),
                                                     fish.Value.PondColor.GetValueOrDefault("B"));

                if (__instance.fishType.Value == fish.Value.FishID && __instance.currentOccupants.Value >= fish.Value.RequiredPopulation) {
                    color = pondOverrideColor;
                }
            }

            __instance.overrideWaterColor.Value = color.Value;
        }

        private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
            SetUpGMCM();
            Helper.Data.WriteJsonFile("fishPondColorData.json", new ModData());
            Monitor.Log("Loading content packs started.\n", LogLevel.Debug);
            Stopwatch sw = Stopwatch.StartNew();
            ColorModel = Helper.Data.ReadJsonFile<ModData>("fishPondColorData.json");

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned()) {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");

                if (!contentPack.HasFile("content.json")) {
                    Monitor.Log($"Content pack: {contentPack.Manifest.Name} missing required file: content.json, Skipping content pack.", LogLevel.Error);
                    continue;
                }

                ModData? data = contentPack.ReadJsonFile<ModData>("content.json");
                Monitor.Log($"{data.Changes.Count} overrides.");

                foreach (var fish in data.Changes) {
                    if (ColorModel.Changes.ContainsKey(fish.Key)) {
                        Monitor.Log($"Content pack: {fish.Key} already present, skipping.\n", LogLevel.Warn);
                        continue;
                    }
                    Monitor.Log($"{fish.Key} color override found, applying.\n");
                    ColorModel.Changes.Add(fish.Key, fish.Value);
                    Helper.Data.WriteJsonFile("fishPondColorData.json", ColorModel);
                }
            }

            sw.Stop();
            Monitor.Log($"Loading content packs finished. [{sw.ElapsedMilliseconds} ms]\n", LogLevel.Debug);
        }

        private void SetUpGMCM() {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("mod-enabled.label"),
                tooltip: () => Helper.Translation.Get("mod-enabled.tooltip"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("default-colors.label"),
                tooltip: () => Helper.Translation.Get("default-colors.tooltip"),
                getValue: () => Config.UseContextColors,
                setValue: value => Config.UseContextColors = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("required-population.label"),
                tooltip: () => Helper.Translation.Get("required-population.tooltip"),
                getValue: () => Config.RequiredPopulation,
                setValue: value => Config.RequiredPopulation = value,
                min: 1,
                max: 10
            );
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
