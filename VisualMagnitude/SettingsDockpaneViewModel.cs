using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace VisualMagnitude {
    internal class SettingsDockpaneViewModel : DockPane {
        private const string _dockPaneID = "VisualMagnitude_SettingsDockpane";
        private const string _menuID = "VisualMagnitude_SettingsDockpane_Menu";
        private ICommand _saveCommand;

        #region dataFields
        private string altOffset = "0";
        private string outputFilename = "VisualMagnitude.tiff";
        private string omittedRings = "0";
        private string lineInterval = "0";
        #endregion

        public ICommand SaveCommand => _saveCommand;

        public SettingsDockpaneViewModel() {

            _saveCommand = new RelayCommand(() => SaveSettings(), () => true);
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show() {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        private void SaveSettings() {
            /*ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(AltOffset);
            double.TryParse(AltOffset.Replace(',', '.'), out double offset); //workaround for decimal ','
            double.TryParse(LineInterval.Replace(',', '.'), out double interval);
            int.TryParse(OmittedRings, out int omitted);
            */

            using (XmlWriter writer = XmlWriter.Create("VisualMagnitudeConfig.xml")) {
                writer.WriteStartDocument();
                writer.WriteStartElement("VisualMagnitude");
                writer.WriteElementString("AltOffset", AltOffset.Replace(',', '.'));
                writer.WriteElementString("LineInterval", LineInterval.Replace(',', '.'));
                writer.WriteElementString("OmittedRings", OmittedRings);
                writer.WriteElementString("OutputFilename", OutputFilename);
                writer.WriteEndElement();
                writer.WriteEndDocument();
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Settings were saved successfuly.", "Success!");
            }

            // ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(offset.ToString() + "\n" + interval.ToString() + "\n" + omitted.ToString() + "\n" + OutputFilename);
        }

        public string AltOffset { get => altOffset; set => altOffset = value; }
        public string OutputFilename { get => outputFilename; set => outputFilename = value; }
        public string OmittedRings { get => omittedRings; set => omittedRings = value; }
        public string LineInterval { get => lineInterval; set => lineInterval = value; }


        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Visual Magnitude Settings";
        public string Heading {
            get { return _heading; }
            set {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        #region Burger Button

        /// <summary>
        /// Tooltip shown when hovering over the burger button.
        /// </summary>
        public string BurgerButtonTooltip {
            get { return "Options"; }
        }

        /// <summary>
        /// Menu shown when burger button is clicked.
        /// </summary>
        public System.Windows.Controls.ContextMenu BurgerButtonMenu {
            get { return FrameworkApplication.CreateContextMenu(_menuID); }
        }
        #endregion

        
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SettingsDockpane_ShowButton : Button {
        protected override void OnClick() {
            SettingsDockpaneViewModel.Show();
        }
    }

    /// <summary>
    /// Button implementation for the button on the menu of the burger button.
    /// </summary>
    internal class SettingsDockpane_MenuButton : Button {
        protected override void OnClick() {
        }
    }
}
