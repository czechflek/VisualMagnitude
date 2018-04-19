using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace VisualMagnitude {

    /// <summary>
    /// Model for dockpane.
    /// </summary>
    internal class SettingsDockpaneViewModel : DockPane {
        private const string _dockPaneID = "VisualMagnitude_SettingsDockpane";
        private const string _menuID = "VisualMagnitude_SettingsDockpane_Menu";
        private ICommand _saveCommand;

        public ICommand SaveCommand => _saveCommand;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SettingsDockpaneViewModel() {
            NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;
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

        /// <summary>
        /// Asynchronously initialize values in the dockpane.
        /// </summary>
        /// <returns>Task</returns>
        protected override Task InitializeAsync() {
            Task task = base.InitializeAsync();
            LoadSettings();
            return task;
        }

        /// <summary>
        /// Load current settings to the dockpane.
        /// </summary>
        public void LoadSettings() {
            SettingsManager settingsManager = SettingsManager.Instance;
            AltOffset = settingsManager.CurrentSettings.AltOffset.ToString();
            OmittedRings = settingsManager.CurrentSettings.OmittedRings.ToString();
            LineInterval = settingsManager.CurrentSettings.LineInterval.ToString();
            OutputFilename = settingsManager.CurrentSettings.OutputFilename;
            WorkerThreads = settingsManager.CurrentSettings.WorkerThreads.ToString();
            WindTurbines = settingsManager.CurrentSettings.WindTurbines;
        }

        /// <summary>
        /// Save current settings.
        /// </summary>
        private void SaveSettings() {
            SettingsManager.Settings settings = new SettingsManager.Settings {
                AltOffset = SettingsManager.Settings.StringToDouble(AltOffset),
                LineInterval = SettingsManager.Settings.StringToDouble(LineInterval),
                OmittedRings = int.Parse(OmittedRings),
                OutputFilename = OutputFilename,
                WorkerThreads = int.Parse(WorkerThreads),
                WindTurbines = WindTurbines
            };

            SettingsManager.Instance.SaveSettings(settings);
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Settings were saved successfuly.", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public string AltOffset { get; set; }
        public string OutputFilename { get; set; }
        public string OmittedRings { get; set; }
        public string LineInterval { get; set; }
        public string WorkerThreads { get; set; }
        public bool WindTurbines { get; set; }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Settings";
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
}
