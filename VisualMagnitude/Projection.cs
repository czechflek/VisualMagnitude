using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Must be called on MCT
/// </summary>
namespace VisualMagnitude {
    class Projection : IEnumerable {
        Raster inputRaster;
        FeatureType viewpointType;
        String tempFolderPath;
        double stepLength = 2;
        HashSet<Tuple<int, int>> viewpoints = new HashSet<Tuple<int, int>>();

        private const string tempGdbName = "tempGDB";
        private const string tempFeatureDatasetName = "tempFeatureDataset";
        private const string tempFeatureClassName = "tempFeatureClass";

        public enum FeatureType {
            POINTS,
            LINES
        }

        public Projection(Raster inputRaster, FeatureType viewpointType, String tempFolderPath) {
            this.inputRaster = inputRaster;
            this.viewpointType = viewpointType;
            this.tempFolderPath = tempFolderPath;
        }

        public async Task<bool> CalculateViewpoints(BasicFeatureLayer viewpointLayer) {
            //TODO: check for failures
            if (viewpointType == FeatureType.LINES) {
                // Sequence of tools which converts vertices of polylines to points
                var fileGDBResult = await Toolbox.CreateFileGDB(tempFolderPath, tempGdbName);
                System.Diagnostics.Debug.WriteLine(fileGDBResult.ReturnValue);

                var featureDatasetResult = await Toolbox.CreateFeatureDataset(fileGDBResult.ReturnValue, tempFeatureDatasetName);
                System.Diagnostics.Debug.WriteLine(featureDatasetResult.ReturnValue);

                var featureClassResult = await Toolbox.CreateFeatureClass(featureDatasetResult.ReturnValue, tempFeatureClassName);
                System.Diagnostics.Debug.WriteLine(featureClassResult.ReturnValue);

                var verticesResult = await Toolbox.FeatureVerticesToPoints(viewpointLayer, featureClassResult.ReturnValue);
                System.Diagnostics.Debug.WriteLine(verticesResult.ReturnValue);

                var xyResult = await Toolbox.AddXY(verticesResult.ReturnValue);
                System.Diagnostics.Debug.WriteLine(xyResult.ReturnValue);
                viewpoints = GetLine(fileGDBResult.ReturnValue, tempFeatureClassName, viewpointLayer);
            } else {
                var xyResult = await Toolbox.AddXY(viewpointLayer);
                System.Diagnostics.Debug.WriteLine(xyResult.ReturnValue);
                viewpoints = GetPoints(viewpointLayer);
            }
            System.Diagnostics.Debug.WriteLine("Total viewpoints: " + viewpoints.Count);
            return true;
        }


        public IEnumerator GetEnumerator() {
            //TODO: check if the points are inbounds (MapToPixel works for OB points)
            return viewpoints.GetEnumerator();

        }

        private HashSet<Tuple<int, int>> GetLine(String gdbPath, String featureClassName, BasicFeatureLayer viewpointLayer) {
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
