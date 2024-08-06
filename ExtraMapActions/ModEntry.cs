using StardewModdingAPI;
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

                switch (Game1.currentLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Action", "Buildings", true)) {
                    case "EMA_CraneGame":
                        Game1.currentLocation.createQuestionDialogue(
                            Config.CraneGameCost > 0
                            ? $"{Config.CraneGameCost} {Helper.Translation.Get("start-play-cost.text")}"
                            : $"{Helper.Translation.Get("start-play-free.text")}",
                        Game1.currentLocation.createYesNoResponses(),
                        TryToStartCraneGame);
                        break;
                    case "EMA_LostAndFound":
                        if (Game1.player.team.returnedDonations.Count > 0 && !Game1.player.team.returnedDonationsMutex.IsLocked()) {
                            Game1.currentLocation.createQuestionDialogue(
                                "Open lost and found?",
                                Game1.currentLocation.createYesNoResponses(),
                                OpenLostAndFound);
                        } else {
                            string prompt = 
                                SpecialOrder.IsSpecialOrdersBoardUnlocked() 
                                ? Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check_OrdersUnlocked") 
                                : Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check");
                            Game1.drawObjectDialogue(prompt);
                        }
                        break;
                    case "EMA_OfflineFarmhandInventory":
                        List<Response> choices = new List<Response>();

                        foreach (Farmer retrievableFarmer in GetRetrievableFarmers()) {
                            string key = retrievableFarmer.UniqueMultiplayerID.ToString() ?? "";
                            string name = retrievableFarmer.Name;

                            if (retrievableFarmer.Name == "") {
                                name = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
                            }

                            choices.Add(new Response(key, name));
                        }

                        choices.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));

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
