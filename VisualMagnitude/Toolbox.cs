using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualMagnitude {

    /// <summary>
    /// Helper class which provides an interface between ArcGIS's python tools and C#. 
    /// </summary>
    class Toolbox {

        /// <summary>
        /// AddXY fuction. Adds X and Y columns.
        /// </summary>
        /// <param name="featureLayer">Vector layer</param>
        /// <returns>Result</returns>
        public static async Task<IGPResult> AddXY(BasicFeatureLayer featureLayer) {
            return await ExecuteTool("AddXY", featureLayer);
        }

        /// <summary>
        /// AddXY fuction. Adds X and Y columns.
        /// </summary>
        /// <param name="featureLayer">Name of the vector layer</param>
        /// <returns>Result</returns>
        public static async Task<IGPResult> AddXY(String featureLayer) {
            return await ExecuteTool("AddXY", featureLayer);
        }

        /// <summary>
        /// FeatureVerticesToPoints function. Converts a vector line to several vertices.
        /// </summary>
        /// <param name="featureLayer">Vector layer</param>
        /// <param name="outputFeatureClass">Output feature class</param>
        /// <returns>Result</returns>
        public static async Task<IGPResult> FeatureVerticesToPoints(BasicFeatureLayer featureLayer, String outputFeatureClass) {
            return await ExecuteTool("FeatureVerticesToPoints", featureLayer, outputFeatureClass);
        }

        /// <summary>
        /// CreateFileGDB function. Creates a new file GDB.
        /// </summary>
        /// <param name="outputFolder">Output folder name</param>
        /// <param name="name">Name of GDB</param>
        /// <returns>Result</returns>
        public static async Task<IGPResult> CreateFileGDB(String outputFolder, String name) {
            return await ExecuteTool("CreateFileGDB", outputFolder, name);
        }

        /// <summary>
        /// CreateFeatureDataset function. Creates a new Feature dataset.
        /// </summary>
        /// <param name="outputDatabase">GDB name</param>
        /// <param name="name">Name of the new Feature dataset</param>
        /// <returns>Result</returns>
        public static async Task<IGPResult> CreateFeatureDataset(String outputDatabase, String name) {
            return await ExecuteTool("CreateFeatureDataset", outputDatabase, name);
        }

        /// <summary>
        /// CreateFeatureclass function. Creates a new Feature class.
        /// </summary>
        /// <param name="outputFeatureDataset">Name of feature dataset</param>
        /// <param name="name">name of the new feature class</param>
        /// <returns>Result</returns>
        public static async Task<IGPResult> CreateFeatureClass(String outputFeatureDataset, String name) {
            return await ExecuteTool("CreateFeatureclass", outputFeatureDataset, name);
        }

        /// <summary>
        /// Delete function. Deletes a file from the catalog and filesystem.
        /// </summary>
        /// <param name="file">file to be deleted</param>
        /// <returns>Result</returns>
        public static async Task<IGPResult> Delete(String file) {
            return await ExecuteTool("Delete", file);
        }

        /// <summary>
        /// Execute a tool in A
        /// </summary>
        /// <param name="toolName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static async Task<IGPResult> ExecuteTool(String toolName, params object[] args) {
            var toolArgs = Geoprocessing.MakeValueArray(args);
            Task<IGPResult> result = Geoprocessing.ExecuteToolAsync(toolName, toolArgs, null, null, null);
            return await result;
        }
    }
}
