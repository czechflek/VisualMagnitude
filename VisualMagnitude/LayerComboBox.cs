﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class LayerComboBox<T> : ComboBox where T : Layer {
        private readonly ObservableCollection<T> listofLayers = new ObservableCollection<T>();

        /// <summary>
        /// Combo Box constructor
        /// </summary>
        public LayerComboBox() {
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

        protected override void OnDropDownOpened() {
            UpdateCombo();
        }

        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override void OnSelectionChange(ComboBoxItem item) {
            T layer = FindLayerByName<T>(item.Text);
            SaveSelection(layer);
        }

        virtual protected void SaveSelection(T layer) {
            /* 
             * To be overriden. Should never be called.
             */
            throw new System.NotImplementedException();
        }

        private T FindLayerByName<T>(string name) where T : Layer {
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
