using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.IO;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Dialogs;

namespace Visual_Magnitude {
    internal class StartButton : Button {
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
                string rasterName = "name2.tiff";
                string rasterFormat = "TIFF";
                string tempFolder = Path.Combine(Project.Current.HomeFolderPath, outputFolderName);
                Directory.CreateDirectory(tempFolder);

                Raster raster = currentRasterLayer.GetRaster();
                //Tuple<int, int> test = raster.MapToPixel(2, 10);
                Projection projection = new Projection(raster, Projection.FeatureType.LINES, tempFolder);
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
                WorkManager workManager = new WorkManager(4);
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
                RasterDataset resultRasterDataset = raster.SaveAs(rasterName, outputDataStore, rasterFormat);
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
                resultRaster.Refresh(); LayerFactory.Instance.CreateLayer(new Uri(Path.Combine(outputFolder, rasterName)),
                                      MapView.Active.Map);
                resultRaster.SaveAs("finishedRaster2.tiff", outputDataStore, rasterFormat);
                LayerFactory.Instance.CreateLayer(new Uri(Path.Combine(outputFolder, "finishedRaster2.tiff")),
                                      MapView.Active.Map);
            });
        }













        #region old


        //    await QueuedTask.Run(() =>
        //    {
        //        //#region Temporary test
        //        //string outputFolderName = "VisualMagnitudeOutput";
        //        //string rasterName = "name.tiff";
        //        //string rasterFormat = "TIFF";

        //        //Raster raster = currentRasterLayer.GetRaster();

        //        ////create output directory and a new datastore \
        //        //string outputFolder = Path.Combine(Project.Current.HomeFolderPath, outputFolderName);
        //        //Directory.CreateDirectory(outputFolder);
        //        //FileSystemConnectionPath outputConnectionPath = new FileSystemConnectionPath(
        //        //    new System.Uri(outputFolder), FileSystemDatastoreType.Raster);
        //        //FileSystemDatastore outputDataStore = new FileSystemDatastore(outputConnectionPath);

        //        ////create a copy of the opened raster 
        //        //raster.SetNoDataValue(0);
        //        //raster.SetPixelType(RasterPixelType.DOUBLE);
        //        //RasterDataset resultRasterDataset = raster.SaveAs(rasterName, outputDataStore, rasterFormat);
        //        //Raster resultRaster = resultRasterDataset.CreateRaster(new int[1] { 0 });

        //        //resultRaster.Refresh();

        //        //if (!resultRaster.CanEdit())
        //        //{
        //        //    MessageBox.Show("Cannot edit raster");
        //        //    return;
        //        //}
        //        //MessageBox.Show(resultRaster.GetPixelType().ToString());
        //        //PixelBlock pixelBlock = resultRaster.CreatePixelBlock(resultRaster.GetWidth(), resultRaster.GetHeight());
        //        //resultRaster.Read(0, 0, pixelBlock);
        //        //pixelBlock.Clear(0);
        //        //Array pixel = pixelBlock.GetPixelData(0, false);
        //        //for (int x = 200; x < 300; x++)
        //        //{
        //        //    for (int y = 200; y < 300; y++)
        //        //    {
        //        //        pixel.SetValue(10.25D, x, y);
        //        //    }
        //        //}
        //        //pixelBlock.SetPixelData(0, pixel);

        //        //resultRaster.Write(0, 0, pixelBlock);
        //        //resultRaster.Refresh();
        //        //resultRaster.SaveAs("finishedRaster.tiff", outputDataStore, rasterFormat);

        //        //#endregion



        //        Raster raster = currentRasterLayer.GetRaster();

        //        string rasterName = "result2.tiff";
        //        string rasterFormat = "TIFF";
        //        string ouputFolderName = "VisualMagnitudeOutput";


        //        string outputFolder = Path.Combine(Project.Current.HomeFolderPath, ouputFolderName);
        //        Directory.CreateDirectory(outputFolder);
        //        FileSystemConnectionPath outputConnectionPath = new FileSystemConnectionPath(
        //                        new System.Uri(outputFolder), FileSystemDatastoreType.Raster);
        //        FileSystemDatastore outputDataStore = new FileSystemDatastore(outputConnectionPath);

        //        PixelBlock currentPixelBlock = raster.CreatePixelBlock(raster.GetWidth(), raster.GetHeight());
        //        raster.Read(0, 0, currentPixelBlock);

        //        Array pixels = currentPixelBlock.GetPixelData(0, false);

        //        //pixels = GeoMap.CreateMock(raster.GetWidth(), raster.GetHeight()).geoMap;

        //        GeoMap elevationMap = new GeoMap(raster.GetHeight(), raster.GetWidth());
        //        Tuple<double, double> cellSize = raster.GetMeanCellSize();
        //        if (cellSize.Item1 != cellSize.Item2)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Cells do not have equal size");
        //        }
        //        elevationMap.CellSize = cellSize.Item1;
        //        WorkManager workManager = new WorkManager(4);
        //        elevationMap.ImportData(pixels);
        //        workManager.AddWork(new SpatialUtils.ViewpointProps((int)(raster.GetHeight() / 2), (int)(raster.GetWidth() / 2)));
        //        //workManager.AddWork(new SpatialUtils.ViewpointProps(150, 30));
        //        // workManager.AddWork(new SpatialUtils.ViewpointProps(51, 200));
        //        //workManager.AddWork(new SpatialUtils.ViewpointProps(20, 30));
        //        //workManager.AddWork(new SpatialUtils.ViewpointProps(65, 20));

        //        workManager.StartWorking(ref elevationMap);

        //        WorkManager.AutoEvent.WaitOne();

        //        GeoMap result = workManager.GetResult();





        //        //System.Diagnostics.Debug.WriteLine("PB: {0}x{1}", raster.GetWidth(), raster.GetHeight());
        //        //System.Diagnostics.Debug.WriteLine("Res: {0}x{1}", result.geoMap.GetLength(0), result.geoMap.GetLength(1));
        //        //result.WriteDataToRaster(raster);
        //        File.WriteAllLines(@"D:/Data/Temp/ArcGIS/data.csv",
        //        ToCsv(result.geoMap));


        //        RasterStorageDef rasterStorageDef = new RasterStorageDef();
        //        rasterStorageDef.SetPyramidLevel(0);
        //        raster.SetNoDataValue(0);
        //        raster.SetPixelType(RasterPixelType.DOUBLE);
        //        RasterDataset finalRaster = raster.SaveAs(rasterName, outputDataStore, rasterFormat, rasterStorageDef);

        //        Raster resultRaster = finalRaster.CreateRaster(new int[1] { 0 });

        //        if (resultRaster.CanEdit())
        //        {
        //            MessageBox.Show("Cannot edit raster :(");
        //            return;
        //        }

        //        currentPixelBlock.Clear(0);
        //        resultRaster.Write(0, 0, currentPixelBlock);

        //        PixelBlock newPixelBlock = resultRaster.CreatePixelBlock(resultRaster.GetWidth(), resultRaster.GetHeight());


        //        double[,] pixel = (double[,])newPixelBlock.GetPixelData(0, false);
        //        double[,] res = (double[,])result.Transpose();
        //        for (int x = 200; x < 300; x++)
        //        {
        //            for (int y = 200; y < 300; y++)
        //            {
        //                pixel[x, y] = 10.5;
        //            }
        //        }

        //        newPixelBlock.SetPixelData(0, res);

        //        resultRaster.Write(0, 0, newPixelBlock);
        //        resultRaster.Refresh();

        //        resultRaster.SaveAs("DalsiRaster.tiff", outputDataStore, rasterFormat, rasterStorageDef);




        //        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Raster written: " + rasterName);

        //        LayerFactory.Instance.CreateLayer(new Uri(Path.Combine(outputFolder, rasterName)),
        //                      MapView.Active.Map);
        //        LayerFactory.Instance.CreateLayer(new Uri(Path.Combine(outputFolder, "DalsiRaster.tiff")),
        //                      MapView.Active.Map);



        //    });

        //    //GeoMap geoMapMock = GeoMap.CreateMock(20, 20);
        //    //WorkManager workManager = new WorkManager(4);
        //    //workManager.AddWork(new SpatialUtils.ViewpointProps(10, 10));
        //    //workManager.AddWork(new SpatialUtils.ViewpointProps(12, 10));
        //    //workManager.AddWork(new SpatialUtils.ViewpointProps(11, 10));
        //    //workManager.StartWorking(ref geoMapMock);
        //    //System.Diagnostics.Debug.WriteLine("threads started");
        //}
        #endregion
        private static IEnumerable<String> ToCsv<T>(T[,] data, string separator = ";") {
            for (int i = 0; i < data.GetLength(0); ++i)
                yield return string.Join(separator, Enumerable
                  .Range(0, data.GetLength(1))
                  .Select(j => data[i, j])); // simplest, we don't expect ',' and '"' in the items
        }
    }
}
