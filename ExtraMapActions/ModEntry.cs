using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Minigames;
using StardewValley.SpecialOrders;

namespace ExtraMapActions {
    internal class ModEntry : Mod {

        ModConfig Config = new();

        public override void Entry(IModHelper helper) {
            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        }

        private void OnButtonsChanged(object? sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e) {
            if (!Context.IsPlayerFree) return;

            if (e.Pressed.Any(button => button.IsActionButton())) {
                var tile = Game1.currentCursorTile;
                
                if (!Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, 1, Game1.player)) {
                    tile = Game1.player.GetGrabTile();
                }

                switch (Game1.currentLocation.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "Action", "Buildings").ToLower()) {
                    case "ema_cranegame":
                        if (Config.DebugLogging) Monitor.Log("Crane game action found, attempting to open prompt.", LogLevel.Debug);
                        Game1.currentLocation.createQuestionDialogue(
                            Config.CraneGameCost > 0
                            ? $"{Config.CraneGameCost} {Helper.Translation.Get("start-play-cost.text")}"
                            : $"{Helper.Translation.Get("start-play-free.text")}",
                        Game1.currentLocation.createYesNoResponses(),
                        TryToStartCraneGame);
                        break;
                    case "ema_lostandfound":
                        if (Config.DebugLogging) Monitor.Log("Lost and found action found, checking if lost and found can be opened.", LogLevel.Debug);
                        if (Game1.player.team.returnedDonations.Count > 0 && !Game1.player.team.returnedDonationsMutex.IsLocked()) {
                            if (Config.DebugLogging) Monitor.Log("Lost and found can be opened, attempting to open prompt.", LogLevel.Debug);
                            Game1.currentLocation.createQuestionDialogue(
                                Helper.Translation.Get("lost-and-found-question"),
                                Game1.currentLocation.createYesNoResponses(),
                                OpenLostAndFound);
                        } else {
                            if (Config.DebugLogging) Monitor.Log("Lost and found cannot be opened, attempting to open info dialogue.", LogLevel.Debug);
                            string prompt = 
                                SpecialOrder.IsSpecialOrdersBoardUnlocked() 
                                ? Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check_OrdersUnlocked") 
                                : Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check");
                            Game1.drawObjectDialogue(prompt);
                        }
                        break;
                    case "ema_offlinefarmhandinventory":
                        if (Config.DebugLogging) 
                            Monitor.Log("Offline farmhand inventory action found, checking for offline farmhand inventories.", LogLevel.Debug);
                        List<Response> choices = new List<Response>();

                        foreach (Farmer retrievableFarmer in GetRetrievableFarmers()) {
                            string key = retrievableFarmer.UniqueMultiplayerID.ToString() ?? "";
                            string name = retrievableFarmer.Name;

                            if (retrievableFarmer.Name == "") {
                                name = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
                            }

                            choices.Add(new Response(key, name));
                        }
                        if (Config.DebugLogging) Monitor.Log($"{choices.Count} farmhand inventories found.", LogLevel.Debug);
                        choices.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
                        if (Config.DebugLogging) Monitor.Log("Attempting to open prompt.", LogLevel.Debug);
                        Game1.currentLocation.createQuestionDialogue(
                            Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_FarmhandItemsQuestion"), 
                            choices.ToArray(), 
                            OpenFarmhandInventory);
                        break;
                }
            }
        }

        void TryToStartCraneGame(Farmer who, string whichAnswer) {
            if (!(whichAnswer.ToLower() == "yes")) return;

            if (Game1.player.Money >= Config.CraneGameCost) {
                Game1.player.Money -= Config.CraneGameCost;
                Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);

                Game1.globalFadeToBlack(delegate {
                    Game1.currentMinigame = new CraneGame();
                }, 0.008f);
            } else {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
            }
        }

        void OpenLostAndFound(Farmer who, string answer) {
            if (answer.ToLower() == "yes") Game1.player.team.CheckReturnedDonations();
        }

        void OpenFarmhandInventory(Farmer who, string answer) {
            if (answer.ToLower() == "cancel") return;

            if (long.TryParse(answer.Split('_')[0], out var id)) {
                Farmer farmhand = Game1.getFarmerMaybeOffline(id);
                if (farmhand != null && Utility.getHomeOfFarmer(farmhand) is Cabin home && !farmhand.isActive()) {
                    home.inventoryMutex.RequestLock(home.openFarmhandInventory);
                }
            }
        }

        List<Farmer> GetRetrievableFarmers() {
            List<Farmer> offline_farmers = new List<Farmer>(Game1.getAllFarmers());

            foreach (Farmer online_farmer in Game1.getOnlineFarmers()) {
                offline_farmers.Remove(online_farmer);
            }

            for (int i = 0; i < offline_farmers.Count; i++) {
                Farmer farmer = offline_farmers[i];
                if (Utility.getHomeOfFarmer(farmer) is Cabin home && (farmer.isUnclaimedFarmhand || home.inventoryMutex.IsLocked())) {
                    offline_farmers.RemoveAt(i);
                    i--;
                }
            }

            return offline_farmers;
        }

        private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
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
                getValue: () => Config.CraneGameCost,
                setValue: value => Config.CraneGameCost = value,
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
