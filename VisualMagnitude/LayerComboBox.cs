using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    /// <summary>
    /// Parent class for all combo boxes in the menu.
    /// </summary>
    internal class LayerComboBox<T> : ComboBox where T : Layer {
        private readonly ObservableCollection<T> listofLayers = new ObservableCollection<T>();

        /// <summary>
        /// Combo Box constructor
        /// </summary>
        protected LayerComboBox() {
            UpdateCombo();
        }

        /// <summary>
        /// Updates the combo box with all the items.
        /// </summary>
        private async void UpdateCombo() {
            int lastLayerCount = listofLayers.Count;
            await RetrieveLayers();

            if (lastLayerCount == listofLayers.Count)
                return; //no need to update

            Clear();
            foreach (T layer in listofLayers) {
                Add(new ComboBoxItem(layer.Name));
            }
            Enabled = true;
            SelectedItem = ItemCollection.FirstOrDefault();
        }

        /// <summary>
        /// Event triggered when the drop down is opened.
        /// </summary>
        protected override void OnDropDownOpened() {
            UpdateCombo();
        }

        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override void OnSelectionChange(ComboBoxItem item) {
            T layer = FindLayerByName(item.Text);
            SaveSelection(layer);
        }

        /// <summary>
        /// Save the selected layer. This method has to be overriden in children classes.
        /// </summary>
        /// <param name="layer">Selected layer</param>
        virtual protected void SaveSelection(T layer) {
            /* 
             * To be overriden. Should never be called.
             */
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Find a layer by the name.
        /// </summary>
        /// <param name="name">Name of the layer</param>
        /// <returns>Actual layer</returns>
        private T FindLayerByName(string name) {
            foreach (T layer in listofLayers as ObservableCollection<T>) {
                if (layer.Name.Equals(name)) {
                    return layer;
                }
            }
            return null;
        }

        /// <summary>
        /// Method for retrieving map items in the project.
        /// </summary>
        private async Task<bool> RetrieveLayers() {
            listofLayers.Clear();
            if (Project.Current != null) {
                await QueuedTask.Run(() => {
                    MapView mapView = MapView.Active;
                    if (mapView == null)
                        return;

                    foreach (T layer in mapView.Map.Layers.OfType<T>().ToList()) {
                        listofLayers.Add(layer);
                    }
                });
            }
            return await Task.FromResult(false);
        }
    }
}
