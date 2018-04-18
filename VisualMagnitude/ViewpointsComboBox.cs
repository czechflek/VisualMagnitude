using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    /// <summary>
    /// ComboBox listing all raster layers. The selected layer will be used as DEM.
    /// </summary>
    internal class ViewpointsComboBox : LayerComboBox<FeatureLayer> {
        protected override void SaveSelection(FeatureLayer layer) {
            SettingsManager.Instance.SelectedViewpointLayer = layer;
        }
    }
}
