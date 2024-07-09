using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Minigames;
using SObject = StardewValley.Object;

namespace StandaloneCraneGame {
    internal class ModEntry : Mod {

        ModConfig Config = new();

        public override void Entry(IModHelper helper) {
            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        }

        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e) {
            if (!Context.IsPlayerFree) return;

            if (e.Pressed.Any(button => button.IsActionButton())
                && GetObjectAtCursor(e.Cursor) is SObject cursorObject
                && cursorObject.QualifiedItemId == "(BC)rokugin.cranecp_CraneGame") {
                Game1.currentLocation.createQuestionDialogue(
                    Config.Cost > 0 ? $"{Config.Cost} {Helper.Translation.Get("cost-not-free.text")}" : $"{Helper.Translation.Get("cost-free.text")}",
                                                    Game1.currentLocation.createYesNoResponses(),
                                                    TryToStartCraneGame);
            }
        }

        SObject? GetObjectAtCursor(ICursorPosition cursor) {
            var tile = Game1.currentCursorTile;

            if (!Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, 1, Game1.player)) {
                tile = Game1.player.GetGrabTile();
            }

            return Game1.currentLocation?.getObjectAtTile((int)tile.X, (int)tile.Y);
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
            if (!Helper.ModRegistry.IsLoaded("rokugin.cranecp")) {
                Monitor.Log("CP component not loaded, please check your installation.", LogLevel.Error);
            }

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
