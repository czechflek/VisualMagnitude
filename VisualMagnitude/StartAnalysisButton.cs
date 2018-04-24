using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;

namespace VisualMagnitude {

    /// <summary>
    /// Clss reporesenting the Start analysis button.
    /// </summary>
    internal class StartAnalysisButton : Button {

        /// <summary>
        /// Event triggered when user clicks the button.
        /// </summary>
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
