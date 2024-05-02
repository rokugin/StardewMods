using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorfulFishPonds {
    public sealed class ModData {
        // "Entry": "FishID RequiredPopulation R G B"
        public Dictionary<string, string> FishPondColorOverrides { get; set; } = new Dictionary<string, string>();
        
    }
}
