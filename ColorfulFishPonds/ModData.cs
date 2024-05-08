using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorfulFishPonds
{
    public sealed class ModData
    {

        public Dictionary<string, SingleFishColorOverride> SingleOverrides { get; set; } = new Dictionary<string, SingleFishColorOverride>();
        public Dictionary<string, FishGroupColorOverride> GroupOverrides { get; set; } = new Dictionary<string, FishGroupColorOverride>();

    }

    public class SingleFishColorOverride
    {
        public string? FishID { get; set; }
        public Dictionary<string, int> PondColor = new Dictionary<string, int>() {
            { "R", 0 },
            { "G", 0 },
            { "B", 0 }
        };

    }

    public class FishGroupColorOverride
    {
        public string? GroupTag { get; set; }
        public Dictionary<string, int> PondColor = new Dictionary<string, int>() {
            { "R", 0 },
            { "G", 0 },
            { "B", 0 }
        };
    }
}
