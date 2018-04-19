using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    /// <summary>
    /// ComboBox listing all Feature layers. The selected layer will be used as viewpoint layer.
    /// </summary>
    internal class ViewpointsComboBox : LayerComboBox<FeatureLayer> {
        /// <summary>
        /// Save the selected layer to settings manager as viewpoint layer.
        /// </summary>
        /// <param name="layer">Viewpoint layer</param>
        protected override void SaveSelection(FeatureLayer layer) {
            SettingsManager.Instance.SelectedViewpointLayer = layer;
        }
    }
}
