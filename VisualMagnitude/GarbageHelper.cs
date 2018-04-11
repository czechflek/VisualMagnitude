using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualMagnitude {
    
    /// <summary>
    /// Helper class which collects files to be deleted.
    /// </summary>
    class GarbageHelper {
        private List<String> garbage = new List<string>();
        private static GarbageHelper instance;

        /// <summary>
        /// Private constructor. Initial settings are loaded.
        /// </summary>
        private GarbageHelper() {
        }

        public static GarbageHelper Instance {
            get {
                if (instance == null) {
                    instance = new GarbageHelper();
                }
                return instance;
            }
        }


        /// <summary>
        /// Add a file to be deleted.
        /// </summary>
        /// <param name="filepath">File path</param>
        public void AddGarbage(string filepath) {
            garbage.Add(filepath);
        }

        /// <summary>
        /// Delete all garbage files from catalog and filesystem.
        /// </summary>
        public async void CleanUp() {
            await QueuedTask.Run(async () => {
                foreach (string file in garbage) {
                    await Toolbox.Delete(file);
                }
            });
        }
    }
}
