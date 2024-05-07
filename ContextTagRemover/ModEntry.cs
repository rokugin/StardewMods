using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using System.Diagnostics;

namespace ContextTagRemover {
    internal class ModEntry : Mod {

        public override void Entry(IModHelper helper) {
            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e) {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects")) {
                e.Edit(asset => {
                    var data = asset.AsDictionary<string, ObjectData>().Data;

                    foreach ((string itemID, ObjectData itemData) in data) {
                        if (itemID == "74") {
                            itemData.ContextTags.Remove("crystalarium_banned");
                        }
                    }
                });
            }
        }

    }
}
