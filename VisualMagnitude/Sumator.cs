using ArcGIS.Core.Data.Raster;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VisualMagnitude {
    class Sumator {
        private ConcurrentQueue<VisualMagnitudeResult> results = new ConcurrentQueue<VisualMagnitudeResult>();
        private GeoMap visualMagnitudeMap;
        private bool alive = false;

        public Sumator(int dimensionY, int dimensionX) {
            VisualMagnitudeMap = new GeoMap(dimensionY, dimensionX);
            VisualMagnitudeMap.Initialize();
        }

        public void Start() {
            alive = true;
            Thread thread = new Thread(Listen);
            thread.Start();
        }

        public void Stop() {
            alive = false;
            System.Diagnostics.Debug.WriteLine("Sumator will stop");
        }

        public void AddResult(VisualMagnitudeResult result) {
            results.Enqueue(result);
        }

        private void Listen() {
            bool incomingResult;
            while((incomingResult = results.TryDequeue(out VisualMagnitudeResult result)) || alive) {
                if(incomingResult)
                    VisualMagnitudeMap[result.Y, result.X] += result.VisualMagnitude;
            }
            System.Diagnostics.Debug.WriteLine("Sumator stopped");
            WorkManager.AutoEvent.Set();
        }


        internal GeoMap VisualMagnitudeMap { get => visualMagnitudeMap; set => visualMagnitudeMap = value; }


        public struct VisualMagnitudeResult {
            private int y;
            private int x;
            double visualMagnitude;

            public VisualMagnitudeResult(int y, int x, double visualMagnitude) : this() {
                Y = y;
                X = x;
                VisualMagnitude = visualMagnitude;
            }

            public int Y { get => y; set => y = value; }
            public int X { get => x; set => x = value; }
            public double VisualMagnitude { get => visualMagnitude; set => visualMagnitude = value; }
        }
    }

}
