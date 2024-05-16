using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ShopsAnywhere {
    internal static class AssetManager {

        internal const string ShopsAnywherePrefix = "Mods/rokugin.shopsanywhere/";

        static Lazy<Texture2D> advShopTex = new(() => Game1.content.Load<Texture2D>(advShopTextureName!.BaseName));
        internal static Texture2D advShopTexture => advShopTex.Value;
        internal static IAssetName advShopTextureName { get; set; } = null!;

        internal static void Initialize(IGameContentHelper parser) {
            advShopTextureName = parser.ParseAssetName($"{ShopsAnywherePrefix}advShopButtonTexture");
        }

        internal static void OnAssetRequested(AssetRequestedEventArgs e) {
            if (!e.NameWithoutLocale.StartsWith(ShopsAnywherePrefix, false, false)) {
                return;
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo(advShopTextureName)) {
                e.LoadFromModFile<Texture2D>("assets/textures/advShopButton.png", AssetLoadPriority.Exclusive);
            }
        }

        internal static void Reset(IReadOnlySet<IAssetName>? assets = null) {
            if ((assets is null || assets.Contains(advShopTextureName)) && advShopTex.IsValueCreated) {
                advShopTex = new(() => Game1.content.Load<Texture2D>(advShopTextureName.BaseName));
            }
        }

    }
}
