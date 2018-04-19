using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    /// <summary>
    /// ComboBox listing all raster layers. The selected layer will be used as DEM.
    /// </summary>
    internal class DemComboBox : LayerComboBox<RasterLayer> {

        /// <summary>
        /// Save the selected layer to settings manager as DEM layer.
        /// </summary>
        /// <param name="layer">DEM layer</param>
        protected override void SaveSelection(RasterLayer layer) {
            SettingsManager.Instance.SelectedDemLayer = layer;
        }
    }
}
