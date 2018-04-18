using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    internal class StartAnalysisButton : Button {
        protected async override void OnClick() {
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Started");

            System.Diagnostics.Debug.WriteLine("************************************************************************");
            IReadOnlyList<Layer> selectedLayerList = MapView.Active.GetSelectedLayers();
            
            BasicRasterLayer currentRasterLayer = null;
            BasicFeatureLayer currentFeatureLayer = null;
            foreach (Layer currentSelectedLayer in selectedLayerList) {

                if (currentSelectedLayer is BasicRasterLayer) {
                    // If the current selected layer is a raster layer or image service layer, 
                    // both are already basic raster layers.
                    currentRasterLayer = currentSelectedLayer as BasicRasterLayer;
                } else if (currentSelectedLayer is BasicFeatureLayer) {
                    currentFeatureLayer = currentSelectedLayer as BasicFeatureLayer;
                }
            }

            await QueuedTask.Run(async () => {

                string outputFolderName = "VisualMagnitudeOutput";
                string tmpRasterName = "tmp.tiff";
                string rasterFormat = "TIFF";
                string tempFolder = Path.Combine(Project.Current.HomeFolderPath, outputFolderName);
                Directory.CreateDirectory(tempFolder);

                Raster raster = currentRasterLayer.GetRaster();
                Projection projection = new Projection(raster, Projection.FeatureType.POINTS, tempFolder); //make the detection automatic
                await projection.CalculateViewpoints(currentFeatureLayer);


                PixelBlock currentPixelBlock = raster.CreatePixelBlock(raster.GetWidth(), raster.GetHeight());
                raster.Read(0, 0, currentPixelBlock);

                Array pixels = currentPixelBlock.GetPixelData(0, false);

                GeoMap elevationMap = new GeoMap(raster.GetHeight(), raster.GetWidth());
                Tuple<double, double> cellSize = raster.GetMeanCellSize();
                if (cellSize.Item1 != cellSize.Item2) {
                    System.Diagnostics.Debug.WriteLine("Cells do not have equal size");
                }
                elevationMap.CellSize = cellSize.Item1;
                WorkManager workManager = new WorkManager(SettingsManager.Instance.CurrentSettings.WorkerThreads);
                elevationMap.ImportData(pixels);
                foreach (Tuple<int, int> point in projection) {
                    workManager.AddWork(new SpatialUtils.ViewpointProps(point.Item2, point.Item1));
                }


                workManager.StartWorking(ref elevationMap);

                WorkManager.AutoEvent.WaitOne();

                GeoMap result = workManager.GetResult();

                //create output directory and a new datastore \
                string outputFolder = Path.Combine(Project.Current.HomeFolderPath, outputFolderName);
                Directory.CreateDirectory(outputFolder);
                FileSystemConnectionPath outputConnectionPath = new FileSystemConnectionPath(
                    new System.Uri(outputFolder), FileSystemDatastoreType.Raster);
                FileSystemDatastore outputDataStore = new FileSystemDatastore(outputConnectionPath);

                //create a copy of the opened raster 
                raster.SetNoDataValue(0);
                raster.SetPixelType(RasterPixelType.DOUBLE);
                RasterDataset resultRasterDataset = raster.SaveAs(tmpRasterName, outputDataStore, rasterFormat);
                GarbageHelper.Instance.AddGarbage(Path.Combine(outputFolder, tmpRasterName));
                Raster resultRaster = resultRasterDataset.CreateRaster(new int[1] { 0 });

                resultRaster.Refresh();

                if (!resultRaster.CanEdit()) {
                    MessageBox.Show("Cannot edit raster");
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


                GarbageHelper.Instance.CleanUp();

                LayerFactory.Instance.CreateLayer(new Uri(Path.Combine(outputFolder, SettingsManager.Instance.CurrentSettings.OutputFilename)),
                                      MapView.Active.Map);
            });
        }
    }
}
