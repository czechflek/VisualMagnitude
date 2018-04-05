using ArcGIS.Core.Data.Raster;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VisualMagnitude {
    
    /// <summary>
    /// Class which aggregates results from all workers. 
    /// </summary>
    class Sumator {
        private ConcurrentQueue<VisualMagnitudeResult> results = new ConcurrentQueue<VisualMagnitudeResult>();
        private GeoMap visualMagnitudeMap;
        private bool alive = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dimensionY">Y size of the map</param>
        /// <param name="dimensionX">X size of the map</param>
        public Sumator(int dimensionY, int dimensionX) {
            VisualMagnitudeMap = new GeoMap(dimensionY, dimensionX);
            VisualMagnitudeMap.Initialize();
        }

        /// <summary>
        /// Start listening for results.
        /// </summary>
        public void Start() {
            alive = true;
            Thread thread = new Thread(Listen);
            thread.Start();
        }

        /// <summary>
        /// Stop listening for results.
        /// </summary>
        public void Stop() {
            alive = false;
            System.Diagnostics.Debug.WriteLine("Sumator will stop");
        }

        /// <summary>
        /// Add a new result.
        /// </summary>
        /// <param name="result"></param>
        public void AddResult(VisualMagnitudeResult result) {
            results.Enqueue(result);
        }

        /// <summary>
        /// Listen for new results.
        /// </summary>
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

        /// <summary>
        /// Wraper for a single result.
        /// </summary>
        public struct VisualMagnitudeResult {
            private int y;
            private int x;
            double visualMagnitude;

            /// <summary>
            /// Cpnstructor
            /// </summary>
            /// <param name="y">Y coordinate</param>
            /// <param name="x">X coordinate</param>
            /// <param name="visualMagnitude">Visual magnitude value</param>
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
