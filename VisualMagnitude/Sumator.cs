using System.Collections.Concurrent;
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
                    VisualMagnitudeMap[result.Y, result.X] += result.VisualMagnitude * result.Weight;
            }
            System.Diagnostics.Debug.WriteLine("Sumator stopped");
            WorkManager.AutoEvent.Set();
        }


        internal GeoMap VisualMagnitudeMap { get => visualMagnitudeMap; set => visualMagnitudeMap = value; }

        /// <summary>
        /// Wraper for a single result.
        /// </summary>
        public struct VisualMagnitudeResult {
            public int Y { get; set; }
            public int X { get; set; }
            public double VisualMagnitude { get; set; }
            public double Weight { get; set; }
        }
    }

}
