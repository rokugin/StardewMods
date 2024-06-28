using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContextTagRemover {
    internal record ModData {

        public Dictionary<string, string[]> ObjectTags { get; }

        public ModData(Dictionary<string, string[]> objectTags) {
            ObjectTags = objectTags ?? new();
        }

    }
}
