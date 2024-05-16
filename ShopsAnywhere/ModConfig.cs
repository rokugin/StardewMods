using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShopsAnywhere {
    internal class ModConfig {

        public ModConfigKeys Controls { get; set; } = new();
        public int AdvShopButtonOffsetX { get; set; } = 0;
        public int AdvShopButtonOffsetY { get; set; } = 0;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context) {
            Controls ??= new ModConfigKeys();
        }

    }
}
