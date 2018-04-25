using ArcGIS.Desktop.Mapping;
using System;
using System.Xml;
using System.Xml.Linq;

namespace VisualMagnitude {
    /// <summary>
    /// Simpleton which manages settings persistence. 
    /// All non-defaut settings are saved inside a project and not shared with others.
    /// </summary>
    class SettingsManager {
        private static SettingsManager instance;

        public RasterLayer SelectedDemLayer { get; set; }
        public FeatureLayer SelectedViewpointLayer { get; set; }

        /// <summary>
        /// Private constructor. Initial settings are loaded.
        /// </summary>
        private SettingsManager() {
            LoadSettings();
        }

        /// <summary>
        /// Get an instance of this simpleton.
        /// </summary>
        public static SettingsManager Instance {
            get {
                if (instance == null) {
                    instance = new SettingsManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// Load setting from a file. If the file is not found, the default settings are used.
        /// </summary>
        /// <returns>Settings container</returns>
        public Settings LoadSettings() {
            Settings settings = new Settings();

            try {
                XDocument xmlDocument = XDocument.Load("VisualMagnitudeConfig.xml");
                XElement xmlSettings = xmlDocument.Element("VisualMagnitude");
                settings.OffsetGlobal = bool.Parse(xmlSettings.Element("OffsetGlobal").Value);
                settings.AltOffset = Settings.StringToDouble(xmlSettings.Element("AltOffset").Value);
                settings.LineInterval = Settings.StringToDouble(xmlSettings.Element("LineInterval").Value);
                settings.OmittedRings = int.Parse(xmlSettings.Element("OmittedRings").Value);
                settings.OutputFilename = xmlSettings.Element("OutputFilename").Value;
                settings.WorkerThreads = int.Parse(xmlSettings.Element("WorkerThreads").Value);
                settings.WindTurbines = bool.Parse(xmlSettings.Element("WindTurbines").Value);
                settings.WeightedViewpoints = bool.Parse(xmlSettings.Element("WeightedViewpoints").Value);
            } catch (Exception) {
                CreateDefaultSettings();
            }
            CurrentSettings = settings;
            return settings;
        }

        /// <summary>
        /// Save the settings to a file.
        /// </summary>
        /// <param name="settings">Settings to be saved</param>
        public void SaveSettings(Settings settings) {
            using (XmlWriter writer = XmlWriter.Create("VisualMagnitudeConfig.xml")) {
                writer.WriteStartDocument();
                writer.WriteStartElement("VisualMagnitude");
                writer.WriteElementString("OffsetGlobal", settings.OffsetGlobal.ToString());
                writer.WriteElementString("AltOffset", settings.AltOffset.ToString());
                writer.WriteElementString("LineInterval", settings.LineInterval.ToString());
                writer.WriteElementString("OmittedRings", settings.OmittedRings.ToString());
                writer.WriteElementString("OutputFilename", settings.OutputFilename);
                writer.WriteElementString("WorkerThreads", settings.WorkerThreads.ToString());
                writer.WriteElementString("WindTurbines", settings.WindTurbines.ToString());
                writer.WriteElementString("WeightedViewpoints", settings.WeightedViewpoints.ToString());
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            CurrentSettings = settings;
        }

        /// <summary>
        /// Creates and saves the default settings.
        /// </summary>
        private void CreateDefaultSettings() {
            SaveSettings(new Settings());
        }

        /// <summary>
        /// Getter/setter for CurrentSettings.
        /// </summary>
        public Settings CurrentSettings { get; private set; }

        /// <summary>
        /// Settings container.
        /// </summary>
        public class Settings {
            public Settings() {
                OffsetGlobal = true;
                AltOffset = 0;
                LineInterval = 10;
                OmittedRings = 0;
                WorkerThreads = 4;
                OutputFilename = "VisualMagnitude.tiff";
                WindTurbines = false;
                WeightedViewpoints = false;
            }

            public bool OffsetGlobal { get; set; }
            public double AltOffset { get; set; }
            public double LineInterval { get; set; }
            public int OmittedRings { get; set; }
            public string OutputFilename { get; set; }
            public int WorkerThreads { get; set; }
            public bool WindTurbines { get; set; }
            public bool WeightedViewpoints { get; set; }

            /// <summary>
            /// Convert string to double.
            /// </summary>
            /// <param name="number">String to be converted</param>
            /// <returns>Converted value</returns>
            public static double StringToDouble(string number) {
                Double.TryParse(number, out double result);
                return result;
            }

        }

    }
}
