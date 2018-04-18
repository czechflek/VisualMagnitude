using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    /// <summary>
    /// ComboBox listing all raster layers. The selected layer will be used as DEM.
    /// </summary>
    internal class DemComboBox : LayerComboBox<RasterLayer> {
        protected override void SaveSelection(RasterLayer layer) {
            SettingsManager.Instance.SelectedDemLayer = layer;
        }
    }
}
