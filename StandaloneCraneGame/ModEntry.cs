using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Minigames;

namespace StandaloneCraneGame {
    internal class ModEntry : Mod {

        ModConfig Config = new();

        public override void Entry(IModHelper helper) {
            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        void OnButtonPressed(object? sender, ButtonPressedEventArgs e) {
            if (!Context.IsPlayerFree) return;
            if (Game1.activeClickableMenu != null) return;

            if (e.Button.IsActionButton()) {
                Vector2 cursorTile = e.Cursor.GrabTile;
                GameLocation location = Game1.player.currentLocation;
                var obj = location.getObjectAtTile((int)cursorTile.X, (int)cursorTile.Y);

                if (obj is null) {
                    if (Config.DebugLogging) Monitor.Log($"Object is null", LogLevel.Debug);
                    return;
                }
                if (obj.ItemId != "rokugin.cranecp_CraneGame") {
                    if (Config.DebugLogging) Monitor.Log($"Object is not crane game", LogLevel.Debug);
                    return;
                }
                if (Config.DebugLogging) Monitor.Log($"{obj.ItemId} found at tile ({(int)cursorTile.X}, {(int)cursorTile.Y})", LogLevel.Debug);
                location.createQuestionDialogue(
                    Config.Cost > 0 ? $"{Config.Cost} {Helper.Translation.Get("cost-not-free.text")}" : $"{Helper.Translation.Get("cost-free.text")}",
                                                    location.createYesNoResponses(),
                                                    TryToStartCraneGame);
            }
        }

        void TryToStartCraneGame(Farmer who, string whichAnswer) {
            if (!(whichAnswer.ToLower() == "yes")) {
                return;
            }
            if (Game1.player.Money >= Config.Cost) {
                Game1.player.Money -= Config.Cost;
                Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);
                Game1.globalFadeToBlack(delegate {
                    Game1.currentMinigame = new CraneGame();
                }, 0.008f);
            } else {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("crane-game-section.text")
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.Cost,
                setValue: value => Config.Cost = value,
                name: () => Helper.Translation.Get("cost.name"),
                tooltip: () => Helper.Translation.Get("cost.tooltip"),
                min: 0
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("debug-section.text")
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.DebugLogging,
                setValue: value => Config.DebugLogging = value,
                name: () => Helper.Translation.Get("enabled.name"),
                tooltip: () => Helper.Translation.Get("debug-enabled.tooltip")
            );

        }

    }
}
