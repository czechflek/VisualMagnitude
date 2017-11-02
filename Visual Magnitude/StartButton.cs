using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
namespace Visual_Magnitude {
    internal class StartButton : Button {
        protected override void OnClick() {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Started");

            System.Diagnostics.Debug.WriteLine("************************************************************************");

            GeoMap geoMapMock = GeoMap.CreateMock(20, 20);
            WorkManager workManager = new WorkManager(4);
            workManager.AddWork(new SpatialUtils.ViewpointProps(10, 10));
            workManager.AddWork(new SpatialUtils.ViewpointProps(12, 10));
            workManager.AddWork(new SpatialUtils.ViewpointProps(11, 10));
            workManager.StartWorking(ref geoMapMock);
            System.Diagnostics.Debug.WriteLine("threads started");

        }
    }
}
