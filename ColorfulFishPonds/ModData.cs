using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorfulFishPonds {
    public sealed class ModData {
        
        public Dictionary<string, FishPondColorOverride> Changes { get; set; } = new Dictionary<string, FishPondColorOverride>();
        
    }

    public class FishPondColorOverride {

        public string FishID { get; set; }
        public int RequiredPopulation { get; set; } = 1;
        public Dictionary<string, int> PondColor = new Dictionary<string, int>() {
            { "R", 0 },
            { "G", 0 },
            { "B", 0 }
        };
        
    }
}
