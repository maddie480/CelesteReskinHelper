using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ReskinHelper {
    class ReskinHelperModule : EverestModule {
        public override void Load() {
            Logger.Log("ReskinHelper", "This mod exists!");
        }

        public override void Unload() {
            // nothing
        }
    }
}
