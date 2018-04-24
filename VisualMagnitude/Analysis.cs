using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Core;
using System;
using System.IO;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    /// <summary>
    /// Class which manages the whole calculation process from start to finish.
    /// </summary>
    class Analysis {
        //TODO: make this configurable
        private readonly string outputFolderName = "VisualMagnitudeOutput";
        private readonly string tmpRasterName = "tmp.tiff";
        private readonly string rasterFormat = "TIFF";

        private string outputFolder;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Analysis() {
            /* empty */
        }

        /// <summary>
        /// Asynchrnously start the visual magnitude analysis.
        /// </summary>
        public async void StartAnalysis() {
            if (!ValidateInputLayers()) {
                return;
            }
            outputFolder = CreateOutputDirectory(outputFolderName);
            if (File.Exists(outputFolder + "/" + SettingsManager.Instance.CurrentSettings.OutputFilename)) {
                System.Windows.MessageBoxResult messageResult = MessageBox.Show("The output file already exists and will be overwritten. Continue?", "File exists!", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning);
                if (messageResult == System.Windows.MessageBoxResult.OK) {
                    GarbageHelper.Instance.AddGarbage(outputFolder + "/" + SettingsManager.Instance.CurrentSettings.OutputFilename);
                    GarbageHelper.Instance.CleanUp();
                } else {
                    return;
                }
            }

            ///most tasks have to run on MCT thread
            await QueuedTask.Run(async () => {
                outputFolder = CreateOutputDirectory(outputFolderName);
                FileSystemDatastore outputDataStore = CreateNewDatastore();

                //get viewpoints
                Raster raster = SettingsManager.Instance.SelectedDemLayer.GetRaster();
                Projection projection = new Projection(raster, outputFolder); //make the detection automatic
                try {
                    if (await projection.CalculateViewpoints(SettingsManager.Instance.SelectedViewpointLayer) == false) {
                        MessageBox.Show("Invalid viewpoint layer type.\nOnly points, lines and polylines are supported.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                } catch (Exception) {
                    if (SettingsManager.Instance.CurrentSettings.OffsetGlobal) {
                        MessageBox.Show("Invalid viewpoint data.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    } else {
                        MessageBox.Show("Invalid viewpoint data. Do the viewpoints have OFFSET column specified?", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                    return;
                }

                GeoMap elevationMap = CreateElevationMap(raster);

                //initialize work manager
                WorkManager workManager = new WorkManager(SettingsManager.Instance.CurrentSettings.WorkerThreads);
                int invalidViewpointsCount = 0;
                foreach (Projection.Viewpoint viewpoint in projection) {
                    if (viewpoint.PointY < 0 || viewpoint.PointX < 0) {
                        invalidViewpointsCount++;
                    } else {
                        workManager.AddWork(new SpatialUtils.ViewpointProps() {
                            X = viewpoint.PointX,
                            Y = viewpoint.PointY,
                            ElevationOffset = SettingsManager.Instance.CurrentSettings.OffsetGlobal
                                                ? SettingsManager.Instance.CurrentSettings.AltOffset
                                                : viewpoint.ElevationOffset
                        });
                    }
                }
                if (invalidViewpointsCount > 0) {
                    string message = invalidViewpointsCount.ToString()
                        + (invalidViewpointsCount == 1
                            ? " viewpoint was invalid or failed to process."
                            : " viewpoints were invalid or failed to process.");
                    MessageBox.Show(message, "Ignored viewpoints", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }

                //wait for the calculation of the visual magnitude for all viewpoints to finish
                var watch = System.Diagnostics.Stopwatch.StartNew();
                workManager.StartWorking(ref elevationMap);
                WorkManager.AutoEvent.WaitOne();
                GeoMap result = workManager.GetResult();
                System.Diagnostics.Debug.WriteLine("Computation finished\n------------\nTime: {0} seconds\nViewpoints: {1}", watch.ElapsedMilliseconds / 1000, projection.GetViewpointsCount());

                //save and display the result
                WriteToRaster(raster, outputDataStore, result);
                LayerFactory.Instance.CreateLayer(new Uri(Path.Combine(outputFolder, SettingsManager.Instance.CurrentSettings.OutputFilename)),
                                          MapView.Active.Map);

                //clean up temporary files
                GarbageHelper.Instance.CleanUp();
            });
        }

        /// <summary>
        /// Check if user celected the input layers correctly.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateInputLayers() {
            if (SettingsManager.Instance.SelectedDemLayer == null) {
                MessageBox.Show("Invalid DEM layer", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
            if (SettingsManager.Instance.SelectedViewpointLayer == null) {
                MessageBox.Show("Invalid viewpoint layer", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create a new folder in the project.
        /// </summary>
        /// <param name="outputFolderName">Folder name</param>
        /// <returns>Path to the folder</returns>
        private string CreateOutputDirectory(string outputFolderName) {
            string folder = Path.Combine(Project.Current.HomeFolderPath, outputFolderName);
            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// Create a new filesystem datastore.
        /// </summary>
        /// <returns></returns>
        private FileSystemDatastore CreateNewDatastore() {
            FileSystemConnectionPath outputConnectionPath = new FileSystemConnectionPath(
                                    new Uri(outputFolder), FileSystemDatastoreType.Raster);
            FileSystemDatastore outputDataStore = new FileSystemDatastore(outputConnectionPath);
            return outputDataStore;
        }

        /// <summary>
        /// Convert the DEM raster to internal elevation map.
        /// </summary>
        /// <param name="raster">DEM raster</param>
        /// <returns>Elevation map</returns>
        private GeoMap CreateElevationMap(Raster raster) {
            PixelBlock currentPixelBlock = raster.CreatePixelBlock(raster.GetWidth(), raster.GetHeight());
            raster.Read(0, 0, currentPixelBlock);

            Array pixels = currentPixelBlock.GetPixelData(0, false);

            GeoMap elevationMap = new GeoMap(raster.GetHeight(), raster.GetWidth());
            Tuple<double, double> cellSize = raster.GetMeanCellSize();
            /*if (Math.Abs(cellSize.Item1-cellSize.Item2) < cellSize.Item1*0.05) {
                MessageBox.Show("Cells are not squares. Using X size of cells.", "Rectuangular cells", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }*/
            elevationMap.CellSize = cellSize.Item1;
            elevationMap.ImportData(pixels);
            return elevationMap;
        }

        /// <summary>
        /// Write results to a rester.
        /// </summary>
        /// <param name="raster">DEM raster</param>
        /// <param name="outputDataStore">Output datastore</param>
        /// <param name="result">Elevation map with resuluts</param>
        private void WriteToRaster(Raster raster, FileSystemDatastore outputDataStore, GeoMap result) {
            raster.SetNoDataValue(0);
            raster.SetPixelType(RasterPixelType.DOUBLE);
            RasterDataset resultRasterDataset = raster.SaveAs(tmpRasterName, outputDataStore, rasterFormat);
            GarbageHelper.Instance.AddGarbage(Path.Combine(outputFolder, tmpRasterName));
            Raster resultRaster = resultRasterDataset.CreateRaster(new int[1] { 0 });

            resultRaster.Refresh();

            if (!resultRaster.CanEdit()) {
                MessageBox.Show("Cannot write to raster", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            PixelBlock pixelBlock = resultRaster.CreatePixelBlock(resultRaster.GetWidth(), resultRaster.GetHeight());
            resultRaster.Read(0, 0, pixelBlock);
            pixelBlock.Clear(0);
            Array pixel = new double[resultRaster.GetWidth(), resultRaster.GetHeight()];

            pixelBlock.SetPixelData(0, result.Transpose());

            resultRaster.Write(0, 0, pixelBlock);
            resultRaster.Refresh();
            resultRaster.SaveAs(SettingsManager.Instance.CurrentSettings.OutputFilename, outputDataStore, rasterFormat);
        }
    }
}
