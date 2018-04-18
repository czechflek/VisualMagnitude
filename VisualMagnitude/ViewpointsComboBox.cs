using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
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
