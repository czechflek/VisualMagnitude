using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visual_Magnitude {
    class VisualMagnitudeWorker {
        private SpatialUtils spatialUtils;
        private int omittedRings = 0;
        private GeoMap elevationMap;
        private double elevationOffset;
        private ConcurrentQueue<SpatialUtils.ViewpointProps> workQueue;

        public VisualMagnitudeWorker(ref ConcurrentQueue<SpatialUtils.ViewpointProps> workQueue, ref GeoMap elevationMap, double cellResolution) {
            spatialUtils = new SpatialUtils(ref elevationMap, cellResolution);
            this.workQueue = workQueue;
            this.elevationMap = elevationMap;
        }

        public void Start() {
            GeoMap losMap = new GeoMap(elevationMap.GetLength(0), elevationMap.GetLength(1));

            while (workQueue.TryDequeue(out SpatialUtils.ViewpointProps viewpoint)) {                
                CalculateVisualMagnitude(viewpoint, losMap);
                System.Diagnostics.Debug.WriteLine("[{0},{1}]", viewpoint.Y, viewpoint.X); //ALL VP VALUES ARE 0
            }
        }

        private void CalculateVisualMagnitude(SpatialUtils.ViewpointProps viewpoint, GeoMap losMap) {
            spatialUtils.Viewpoint = viewpoint;
            viewpoint.Elevation = elevationMap[viewpoint.Y, viewpoint.X] + elevationOffset;

            losMap[viewpoint.Y, viewpoint.X] = GeoMap.UndefinedValue; //initialize LOS of the viewpoint

            //find out the maximum distance to the edge of map
            int maxDistance = GetMaximumDistance(viewpoint);
            for (int i = 1; i < maxDistance; i++) {
                GeoMap.Ring ring = elevationMap.GetRing(viewpoint.Y, viewpoint.X, i);
                foreach (int[] item in ring) {
                    if (spatialUtils.IsCellVisible(losMap, item[0], item[1])) {
                        //System.Diagnostics.Debug.WriteLine("[{0},{1}] = visible", viewpoint.Y, viewpoint.X);
                    }
                    //System.Diagnostics.Debug.WriteLine("[{0},{1}] = done", viewpoint.Y, viewpoint.X);

                }
            }

            System.Diagnostics.Debug.WriteLine("ThreadDone");
        }

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
