using StardewModdingAPI;
using StardewValley.Tools;
using StardewValley;
using HarmonyLib;
using StardewValley.Objects;
using StardewValley.Triggers;
using StardewValley.Delegates;
using System.Reflection.Emit;

namespace StrongerTools {
    public class ModEntry : Mod {

        ModConfig Config = new();
        static IMonitor? SMonitor;

        bool hardwareCursor = false;

        public override void Entry(IModHelper helper) {
            Config = helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;

            helper.ConsoleCommands.Add("rokugin.set_health", "Sets player health to specified amount.\n\nUsage: rokugin.set_health <amount>\n- amount: integer amount", SetHealth);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Trinket), nameof(Trinket.canBeShipped)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Trinket_CanBeShipped_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "drawHUD"),
                transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Game1_DrawHUD_Transpiler))
            );
        }

        private void OnReturnedToTitle(object? sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e) {
            hardwareCursor = false;
        }

        private void OnUpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e) {
            if (hardwareCursor) return;
            EnableHardwareCursor();
        }

        static void Trinket_CanBeShipped_Postfix(ref bool __result) {
            __result = true;
        }

        static bool GetHealthBarVisibilitySetting() {
            return true;
        }

        static IEnumerable<CodeInstruction> Game1_DrawHUD_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            var method = AccessTools.Method(typeof(ModEntry), nameof(GetHealthBarVisibilitySetting));

            var matcher = new CodeMatcher(instructions, generator);

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Stsfld),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Stsfld),
                new CodeMatch(OpCodes.Ldc_I4)
            ).ThrowIfNotMatch("Match not found for show health label");

            matcher.CreateLabel(out Label showHealthLabel);

            matcher.MatchStartBackwards(
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Bge)
            ).ThrowIfNotMatch("Match not found for health visibility setting check insertion point.");

            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, method),
                new CodeInstruction(OpCodes.Brtrue_S, showHealthLabel)
            );

            return matcher.InstructionEnumeration();
        }

        private void OnDayStarted(object? sender, StardewModdingAPI.Events.DayStartedEventArgs e) {
            Game1.netWorldState.Value.canDriveYourselfToday.Value = true;
        }

        private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
            SetupGMCM();
            EnableHardwareCursor();

            TriggerActionManager.RegisterAction("rokugin.SetPlayerHealth", SetPlayerHealth);
        }

        private void OnButtonsChanged(object? sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e) {
            if (!Context.IsPlayerFree) return;

            if (e.Pressed.Any(button => button.IsUseToolButton())
                && Game1.player.CurrentTool is Item item) {
                if (item is Pickaxe pickaxe) {
                    switch (pickaxe.ItemId) {
                        case "GoldPickaxe":
                            if (pickaxe.additionalPower.Value < 3) pickaxe.additionalPower.Value = 3;
                            break;
                        case "IridiumPickaxe":
                            if (pickaxe.additionalPower.Value < 10) pickaxe.additionalPower.Value = 10;
                            break;
                    }
                    if (Config.DebugLogging) Monitor.Log($"{pickaxe.Name} current additional power: +{pickaxe.additionalPower.Value}", LogLevel.Info);
                }
            }
        }

        public static bool SetPlayerHealth(string[] args, TriggerActionContext context, out string error) {
            if (ArgUtility.TryGet(args, 1, out string amount, out error, allowBlank: false)) {
                if (amount == "full") {
                    Game1.player.health = Game1.player.maxHealth;
                    return true;
                }

                if (!int.TryParse(amount, out int healthAmount)) {
                    return false;
                }

                if (healthAmount <= 0) healthAmount = 1;

                Game1.player.health = healthAmount;
                return true;
            }

            return false;
        }

        void SetHealth(string command, string[] args) {
            if (!Context.IsWorldReady) {
                Monitor.Log("Load a save first.", LogLevel.Error);
                return;
            }
            if (!int.TryParse(args[0], out int healthAmount)) {
                Monitor.Log("Could not parse amount to set player health.", LogLevel.Error);
                return;
            }

            if (healthAmount <= 0) healthAmount = 1;
            Game1.player.health = healthAmount;
            Monitor.Log($"Set player health to {healthAmount}.", LogLevel.Info);
        }

        void EnableHardwareCursor() {
            Game1.options.hardwareCursor = true;
            hardwareCursor = true;
        }

        void SetupGMCM() {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Debug"
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.DebugLogging,
                setValue: value => Config.DebugLogging = value,
                name: () => "Enabled"
            );
        }

    }
}
