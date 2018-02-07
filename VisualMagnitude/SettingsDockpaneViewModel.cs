using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace VisualMagnitude {
    internal class SettingsDockpaneViewModel : DockPane {
        private const string _dockPaneID = "VisualMagnitude_SettingsDockpane";
        private const string _menuID = "VisualMagnitude_SettingsDockpane_Menu";

        protected SettingsDockpaneViewModel() { }

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
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "My DockPane";
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
