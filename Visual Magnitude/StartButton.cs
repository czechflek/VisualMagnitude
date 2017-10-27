﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Cudafy;
using Cudafy.Translator;
using Cudafy.Host;

namespace Visual_Magnitude {
    internal class StartButton : Button {
        protected override void OnClick() {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Started");

            System.Diagnostics.Debug.WriteLine("************************************************************************");

            GPGPU gpu = CudafyHost.GetDevice(eGPUType.OpenCL);
            CudafyModule km = CudafyTranslator.Cudafy(eArchitecture.OpenCL);
            
            gpu.LoadModule(km);

            gpu.Launch().Kernel();
        }

        [Cudafy]
        public static void Kernel() {
        }
    }
}