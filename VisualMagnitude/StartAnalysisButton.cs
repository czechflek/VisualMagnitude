using System;
using System.Collections.Generic;
using System.IO;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace VisualMagnitude {
    internal class StartAnalysisButton : Button {

        protected override void OnClick() {
            try {
                Analysis analysis = new Analysis();
                analysis.StartAnalysis();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Unexpected Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
