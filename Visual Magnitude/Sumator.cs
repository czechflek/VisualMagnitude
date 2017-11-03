using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Visual_Magnitude {
    class Sumator {
        private ConcurrentQueue<VisualMagnitudeResult> results = new ConcurrentQueue<VisualMagnitudeResult>();
        GeoMap visualMagnitudeMap;
        bool alive = false;

        public Sumator(int dimensionY, int dimensionX) {
            visualMagnitudeMap = new GeoMap(dimensionY, dimensionX);
        }

        public void Start() {
            alive = true;
            Thread thread = new Thread(Listen);
            thread.Start();
        }

        public void Stop() {
            alive = false;
        }

        public void AddResult(VisualMagnitudeResult result) {
            results.Enqueue(result);
        }

        private void Listen() {
            while(results.TryDequeue(out VisualMagnitudeResult result) || alive) {
                visualMagnitudeMap[result.Y, result.X] += result.VisualMagnitude;
            }

            System.Diagnostics.Debug.WriteLine("Sumator ended");

        }


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
