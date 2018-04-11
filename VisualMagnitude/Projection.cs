﻿using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Must be called on MCT
/// </summary>
namespace VisualMagnitude {
    
    /// <summary>
    /// Class handling conversion of vector viewpoints to raster coordinates. It stores the converted viewpoints.
    /// </summary>
    class Projection : IEnumerable {
        Raster inputRaster;
        FeatureType viewpointType;
        String tempFolderPath;
        double stepLength = SettingsManager.Instance.CurrentSettings.LineInterval;
        HashSet<Tuple<int, int>> viewpoints = new HashSet<Tuple<int, int>>();

        private const string tempGdbName = "tempGDB";
        private const string tempFeatureDatasetName = "tempFeatureDataset";
        private const string tempFeatureClassName = "tempFeatureClass";

        public enum FeatureType {
            POINTS,
            LINES
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputRaster">Raster map</param>
        /// <param name="viewpointType">Type of the vector</param>
        /// <param name="tempFolderPath">Path for temporary files</param>
        public Projection(Raster inputRaster, FeatureType viewpointType, String tempFolderPath) {
            this.inputRaster = inputRaster;
            this.viewpointType = viewpointType;
            this.tempFolderPath = tempFolderPath;
        }

        /// <summary>
        /// Convert vector viewpoints to raster coordinates.
        /// </summary>
        /// <param name="viewpointLayer">Layer which contains the viewpoints.</param>
        /// <returns>True on success</returns>
        public async Task<bool> CalculateViewpoints(BasicFeatureLayer viewpointLayer) {
            //TODO: check for failures
            if (viewpointType == FeatureType.LINES) {
                // Sequence of tools which converts vertices of polylines to points
                var fileGDBResult = await Toolbox.CreateFileGDB(tempFolderPath, tempGdbName);
                System.Diagnostics.Debug.WriteLine(fileGDBResult.ReturnValue);
                System.Diagnostics.Debug.WriteLine(Path.Combine(tempFolderPath, tempGdbName + ".gdb"));
                GarbageHelper.Instance.AddGarbage(Path.Combine(tempFolderPath, tempGdbName + ".gdb"));

                var featureDatasetResult = await Toolbox.CreateFeatureDataset(fileGDBResult.ReturnValue, tempFeatureDatasetName);
                System.Diagnostics.Debug.WriteLine(featureDatasetResult.ReturnValue);

                var featureClassResult = await Toolbox.CreateFeatureClass(featureDatasetResult.ReturnValue, tempFeatureClassName);
                System.Diagnostics.Debug.WriteLine(featureClassResult.ReturnValue);

                var verticesResult = await Toolbox.FeatureVerticesToPoints(viewpointLayer, featureClassResult.ReturnValue);
                System.Diagnostics.Debug.WriteLine(verticesResult.ReturnValue);

                var xyResult = await Toolbox.AddXY(verticesResult.ReturnValue);
                System.Diagnostics.Debug.WriteLine(xyResult.ReturnValue);
                viewpoints = GetLine(fileGDBResult.ReturnValue, tempFeatureClassName);
            } else {
                var xyResult = await Toolbox.AddXY(viewpointLayer);
                System.Diagnostics.Debug.WriteLine(xyResult.ReturnValue);
                viewpoints = GetPoints(viewpointLayer);
            }
            System.Diagnostics.Debug.WriteLine("Total viewpoints: " + viewpoints.Count);
            return true;
        }

        /// <summary>
        /// Return enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator() {
            //TODO: check if the points are inbounds (MapToPixel works for OB points)
            return viewpoints.GetEnumerator();

        }

        /// <summary>
        /// Calculate viewpoints along a vector line, e.g. road, path.
        /// </summary>
        /// <param name="gdbPath">Path to GDB</param>
        /// <param name="featureClassName">Feature class</param>
        /// <returns>Viewpoints</returns>
        private HashSet<Tuple<int, int>> GetLine(String gdbPath, String featureClassName) {
            HashSet<Tuple<int, int>> result = new HashSet<Tuple<int, int>>(); //using hash set to prevent duplicates, possible speed up with array

            using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath))))
            using (FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(featureClassName)) {

                QueryFilter queryFilter = new QueryFilter {
                    SubFields = "POINT_X, POINT_Y, ORIG_FID",
                    PostfixClause = "ORDER BY ORIG_FID"
                };

                using (RowCursor rowCursor = featureClass.Search(queryFilter, false)) {
                    double previousX = Double.NaN;
                    double previousY = Double.NaN;
                    int previousFid = Int32.MinValue;

                    while (rowCursor.MoveNext()) {
                        using (Row row = rowCursor.Current) {
                            double pointX = Convert.ToDouble(row["POINT_X"]);
                            double pointY = Convert.ToDouble(row["POINT_Y"]);
                            int fid = Convert.ToInt32(row["ORIG_FID"]);

                            if (fid == previousFid) {
                                double length = Math.Sqrt(Math.Pow(pointX - previousX, 2) + Math.Pow(pointY - previousY, 2));
                                int steps = (int)Math.Floor(length / stepLength);
                                double xStep = (pointX - previousX) / steps;
                                double yStep = (pointY - previousY) / steps;
                                for (int i = 0; i <= steps; i++) {
                                    Tuple<int, int> point = inputRaster.MapToPixel(previousX + xStep * i, previousY + yStep * i);
                                    result.Add(point);
                                }
                            } else if (previousFid != Int32.MinValue) { //endpoint
                                Tuple<int, int> point = inputRaster.MapToPixel(previousX, previousY);
                                result.Add(point);
                            }

                            previousX = pointX;
                            previousY = pointY;
                            previousFid = fid;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate viewpoint coordinates from vector points.
        /// </summary>
        /// <param name="viewpointLayer">Layer which contains the viewpoints.</param>
        /// <returns></returns>
        private HashSet<Tuple<int, int>> GetPoints(BasicFeatureLayer viewpointLayer) {
            HashSet<Tuple<int, int>> result = new HashSet<Tuple<int, int>>(); //using hash set to prevent duplicates, possible speed up with array
            Table table = viewpointLayer.GetTable();

            QueryFilter queryFilter = new QueryFilter {
                SubFields = "POINT_X, POINT_Y"
            };

            using (RowCursor rowCursor = table.Search(queryFilter, false)) {
                while (rowCursor.MoveNext()) {
                    using (Row row = rowCursor.Current) {
                        double pointX = Convert.ToDouble(row["POINT_X"]);
                        double pointY = Convert.ToDouble(row["POINT_Y"]);
                        Tuple<int, int> point = inputRaster.MapToPixel(pointX, pointY);
                        result.Add(point);
                    }
                }
            }

            return result;
        }
    }
}
