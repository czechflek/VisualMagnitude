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

namespace Visual_Magnitude {
    internal class StartButton : Button {
        protected async override void OnClick() {
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Started");

            System.Diagnostics.Debug.WriteLine("************************************************************************");
            IReadOnlyList<Layer> selectedLayerList = MapView.Active.GetSelectedLayers();
            BasicRasterLayer currentRasterLayer = null;
            foreach (Layer currentSelectedLayer in selectedLayerList) {

                if (currentSelectedLayer is BasicRasterLayer) {
                    // If the current selected layer is a raster layer or image service layer, 
                    // both are already basic raster layers.
                    currentRasterLayer = currentSelectedLayer as BasicRasterLayer;
                    //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Raster");
                }
            }
            await QueuedTask.Run(() => {
                Raster raster = currentRasterLayer.GetRaster();
                                
                //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(raster.GetBandCount().ToString());
                PixelBlock currentPixelBlock = raster.CreatePixelBlock(raster.GetWidth(), raster.GetHeight());
                raster.Read(0, 0, currentPixelBlock);
                Array pixels = currentPixelBlock.GetPixelData(0, false);
                System.Diagnostics.Debug.WriteLine(pixels.GetLength(0).ToString());
                System.Diagnostics.Debug.WriteLine(pixels.GetLength(1).ToString());
                GeoMap elevationMap = new GeoMap(raster.GetWidth(), raster.GetHeight());
                Tuple<double, double> cellSize = raster.GetMeanCellSize();
                if (cellSize.Item1 != cellSize.Item2) {
                    System.Diagnostics.Debug.WriteLine("Cells do not have equal size");
                }
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(String.Format("Raster\n Cell size: {0}-{1}", cellSize.Item1.ToString(), cellSize.Item2.ToString()));
                elevationMap.CellSize = cellSize.Item1;
                WorkManager workManager = new WorkManager(4);
                elevationMap.ImportData(pixels);
                workManager.AddWork(new SpatialUtils.ViewpointProps(10, 10));
                workManager.AddWork(new SpatialUtils.ViewpointProps(12, 10));
                workManager.AddWork(new SpatialUtils.ViewpointProps(11, 10));
                workManager.StartWorking(ref elevationMap);

                Raster result = raster.GetRasterDataset().CreateDefaultRaster();
                result.SetSpatialReference(raster.GetSpatialReference());
                result.SetPixelType(RasterPixelType.DOUBLE);
                result.SetExtent(raster.GetExtent());
                result.SetHeight(raster.GetHeight());
                result.SetWidth(raster.GetWidth());
                PixelBlock resultPixelBlock = raster.CreatePixelBlock(raster.GetWidth(), raster.GetHeight());


            });

            //GeoMap geoMapMock = GeoMap.CreateMock(20, 20);
            //WorkManager workManager = new WorkManager(4);
            //workManager.AddWork(new SpatialUtils.ViewpointProps(10, 10));
            //workManager.AddWork(new SpatialUtils.ViewpointProps(12, 10));
            //workManager.AddWork(new SpatialUtils.ViewpointProps(11, 10));
            //workManager.StartWorking(ref geoMapMock);
            //System.Diagnostics.Debug.WriteLine("threads started");

        }
    }
}
