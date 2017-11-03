using System.Collections.Concurrent;
using System.Threading;

namespace Visual_Magnitude {
    class WorkManager {
        private ConcurrentQueue<SpatialUtils.ViewpointProps> workQueue = new ConcurrentQueue<SpatialUtils.ViewpointProps>();
        private Sumator sumator;
        private int threadCount;
        private int startingQueueSize;
        private Thread[] threads;  

        public WorkManager(int threadCount) {
            this.threadCount = threadCount;
            threads = new Thread[threadCount];
            
        }

        public void AddWork(SpatialUtils.ViewpointProps viewpoint) {
            workQueue.Enqueue(viewpoint);
            System.Diagnostics.Debug.WriteLine("New VP: [{0},{1}]", viewpoint.Y, viewpoint.X);
        }

        public void StartWorking(ref GeoMap elevationMap) {
            sumator = new Sumator(elevationMap.GetLength(0), elevationMap.GetLength(1));
            sumator.Start();
            startingQueueSize = workQueue.Count;
            for (int i = 0; i < threadCount; i++) {
                VisualMagnitudeWorker worker = new VisualMagnitudeWorker(ref workQueue, ref elevationMap, ref sumator, 1);
                Thread thread = new Thread(worker.Start);
                threads[i] = thread;
                thread.Start();
            }
        }
    }
}
