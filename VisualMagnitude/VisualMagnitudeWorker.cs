using System.Collections.Concurrent;

namespace VisualMagnitude {

    /// <summary>
    /// Worker which calculates a visual magnite from a viewpoint.
    /// </summary>
    class VisualMagnitudeWorker {
        private SpatialUtils spatialUtils;
        private int omittedRings = SettingsManager.Instance.CurrentSettings.OmittedRings;
        private GeoMap elevationMap;
        private double elevationOffset = SettingsManager.Instance.CurrentSettings.AltOffset;
        private ConcurrentQueue<SpatialUtils.ViewpointProps> workQueue;
        private Sumator sumator;
        private WorkManager parent;

        /// <summary>
        /// Contructor to initialize the worker.
        /// </summary>
        /// <param name="workQueue">Work queue share amon workers.</param>
        /// <param name="elevationMap">Elevation map</param>
        /// <param name="sumator">Sumator instance</param>
        /// <param name="parent">Work manager</param>
        public VisualMagnitudeWorker(ref ConcurrentQueue<SpatialUtils.ViewpointProps> workQueue, ref GeoMap elevationMap, ref Sumator sumator, WorkManager parent) {
            spatialUtils = new SpatialUtils(ref elevationMap);
            this.workQueue = workQueue;
            this.elevationMap = elevationMap;
            this.sumator = sumator;
            this.parent = parent;
        }

        /// <summary>
        /// Start processing the work queue.
        /// </summary>
        public void Start() {
            GeoMap losMap = new GeoMap(elevationMap.GetLength(0), elevationMap.GetLength(1));

            while (workQueue.TryDequeue(out SpatialUtils.ViewpointProps viewpoint)) {
                System.Diagnostics.Debug.WriteLine(workQueue.Count + " left");
                CalculateVisualMagnitude(viewpoint, losMap);
            }
            System.Diagnostics.Debug.WriteLine("ThreadDone");
            parent.ThreadFinished();
        }

        /// <summary>
        /// Calculate the visual magintude from a viewpoint.
        /// </summary>
        /// <param name="viewpoint">Viewpoint</param>
        /// <param name="losMap">LOS map</param>
        private void CalculateVisualMagnitude(SpatialUtils.ViewpointProps viewpoint, GeoMap losMap) {
            double visualMagnitude;

            spatialUtils.Viewpoint = viewpoint;
            viewpoint.Elevation = elevationMap[viewpoint.Y, viewpoint.X] + elevationOffset;

            losMap[viewpoint.Y, viewpoint.X] = GeoMap.UndefinedValue; //initialize LOS of the viewpoint            

            //find out the maximum distance to the edge of map
            int maxDistance = GetMaximumDistance(viewpoint);

            for (int i = 1 + omittedRings; i < maxDistance; i++) {
                GeoMap.Ring ring = elevationMap.GetRing(viewpoint.Y, viewpoint.X, i);
                foreach (int[] item in ring) {
                    if (spatialUtils.IsCellVisible(losMap, item[0], item[1])) {
                        visualMagnitude = spatialUtils.GetVisualMagnutude(item[0], item[1]);
                        if (visualMagnitude > 0)
                            sumator.AddResult(new Sumator.VisualMagnitudeResult(item[0], item[1], visualMagnitude));
                    }

                }
            }            
        }

        /// <summary>
        /// Get the maximum distance from the viewpoint to the edge of the map.
        /// </summary>
        /// <param name="viewpoint">Viewpoint</param>
        /// <returns>Maximum distance from viewpoint to the edge</returns>
        private int GetMaximumDistance(SpatialUtils.ViewpointProps viewpoint) {
            int maxDistance = viewpoint.X;

            if (maxDistance < viewpoint.Y) {
                maxDistance = viewpoint.Y;
            }

            if (maxDistance < (elevationMap.GetLength(1) - viewpoint.X)) {
                maxDistance = elevationMap.GetLength(1) - viewpoint.X;
            }

            if (maxDistance < (elevationMap.GetLength(0) - viewpoint.Y)) {
                maxDistance = elevationMap.GetLength(0) - viewpoint.Y;
            }

            return maxDistance;

        }
    }
}
