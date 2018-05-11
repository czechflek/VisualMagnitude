using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace VisualMagnitude {

    /// <summary>
    /// Model for dockpane.
    /// </summary>
    internal class SettingsDockpaneViewModel : DockPane, INotifyPropertyChanged {
        private const string _dockPaneID = "VisualMagnitude_SettingsDockpane";
        private const string _menuID = "VisualMagnitude_SettingsDockpane_Menu";
        private ICommand _saveCommand;
        new public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SaveCommand => _saveCommand;

        private string altOffset;
        private string outputFilename;
        private string omittedRings;
        private string lineInterval;
        private string workerThreads;
        private bool windTurbines;
        private bool offsetGlobal;
        private bool offsetPerVP;
        private bool weightedViewpoints;
        private string heading = "Settings";


        /// <summary>
        /// Constructor.
        /// </summary>
        public SettingsDockpaneViewModel() {
            NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;
            _saveCommand = new RelayCommand(() => SaveSettings(), () => true);

            //Settings will be loaded either when the project is opened (the settings pane is already visible) or when the pane is shown
            ProjectOpenedEvent.Subscribe((ProjectEventArgs e) => { LoadSettings(); });
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show() {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
            //Settings will be loaded either when the project is opened (the settings pane is already visible) or when the pane is shown
            ((SettingsDockpaneViewModel)pane).LoadSettings();
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
            OffsetGlobal = settingsManager.CurrentSettings.OffsetGlobal;
            WeightedViewpoints = settingsManager.CurrentSettings.WeightedViewpoints;
            if (!OffsetGlobal) {
                OffsetPerVP = true;
            }

        }

        /// <summary>
        /// Save current settings.
        /// </summary>
        private void SaveSettings() {
            SettingsManager.Settings settings = new SettingsManager.Settings {
                AltOffset = SettingsManager.Settings.StringToDouble(altOffset),
                LineInterval = SettingsManager.Settings.StringToDouble(LineInterval),
                OmittedRings = int.Parse(OmittedRings),
                OutputFilename = outputFilename,
                WorkerThreads = int.Parse(WorkerThreads),
                WindTurbines = WindTurbines,
                OffsetGlobal = OffsetGlobal,
                WeightedViewpoints = WeightedViewpoints
            };

            SettingsManager.Instance.SaveSettings(settings);
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Settings were saved successfuly.", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        #region Properties
        public string AltOffset {
            get {
                return altOffset;
            }
            set {
                altOffset = value;
                OnPropertyChanged("AltOffset");
            }
        }

        public string OutputFilename {
            get {
                return outputFilename;
            }
            set {
                outputFilename = value;
                OnPropertyChanged("OutputFilename");
            }
        }

        public string OmittedRings {
            get {
                return omittedRings;
            }
            set {
                omittedRings = value;
                OnPropertyChanged("OmittedRings");
            }
        }

        public string LineInterval {
            get {
                return lineInterval;
            }
            set {
                lineInterval = value;
                OnPropertyChanged("LineInterval");
            }
        }

        public string WorkerThreads {
            get {
                return workerThreads;
            }
            set {
                workerThreads = value;
                OnPropertyChanged("WorkerThreads");
            }
        }

        public bool WindTurbines {
            get {
                return windTurbines;
            }
            set {
                windTurbines = value;
                OnPropertyChanged("WindTurbines");
            }
        }

        public bool OffsetPerVP {
            get {
                return offsetPerVP;
            }
            set {
                offsetPerVP = value;
                OnPropertyChanged("OffsetPerVP");
            }
        }

        public bool OffsetGlobal {
            get {
                return offsetGlobal;
            }
            set {
                offsetGlobal = value;
                OnPropertyChanged("OffsetGlobal");
            }
        }

        public bool WeightedViewpoints {
            get {
                return weightedViewpoints;
            }
            set {
                weightedViewpoints = value;
                OnPropertyChanged("WeightedViewpoints");
            }
        }

        public string Heading {
            get { return heading; }
            set {
                SetProperty(ref heading, value, () => Heading);
            }
        }
        #endregion

        protected void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
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
