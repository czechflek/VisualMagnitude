using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualMagnitude {
    class Toolbox {
        public static async Task<IGPResult> AddXY(BasicFeatureLayer featureLayer) {
            return await ExecuteTool("AddXY", featureLayer);
        }

        public static async Task<IGPResult> AddXY(String featureLayer) {
            return await ExecuteTool("AddXY", featureLayer);
        }

        public static async Task<IGPResult> FeatureVerticesToPoints(BasicFeatureLayer featureLayer, String outputFeatureClass) {
            return await ExecuteTool("FeatureVerticesToPoints", featureLayer, outputFeatureClass);
        }

        public static async Task<IGPResult> CreateFileGDB(String outputFolder, String name) {
            return await ExecuteTool("CreateFileGDB", outputFolder, name);
        }

        public static async Task<IGPResult> CreateFeatureDataset(String outputDatabase, String name) {
            return await ExecuteTool("CreateFeatureDataset", outputDatabase, name);
        }

        public static async Task<IGPResult> CreateFeatureClass(String outputFeatureDataset, String name) {
            return await ExecuteTool("CreateFeatureclass", outputFeatureDataset, name);
        }

        private static async Task<IGPResult> ExecuteTool(String toolName, params object[] args) {
            var toolArgs = Geoprocessing.MakeValueArray(args);
            Task<IGPResult> result = Geoprocessing.ExecuteToolAsync(toolName, toolArgs, null, null, null);
            return await result;
        }
    }
}
