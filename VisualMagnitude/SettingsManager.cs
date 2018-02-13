using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualMagnitude
{
    class SettingsManager
    {
        private static SettingsManager instance;

        private SettingsManager() { }

        public static SettingsManager Instance {
            get {
                if(instance == null) {
                    instance = new SettingsManager();
                }
                return instance;
            }
        }
    }
}
