using System.Collections.Concurrent;
using System.Threading;

namespace Visual_Magnitude {
    class WorkManager {
        private ConcurrentQueue<SpatialUtils.ViewpointProps> workQueue = new ConcurrentQueue<SpatialUtils.ViewpointProps>();
        private int threadCount;
        private int startingQueueSize;
        private Thread[] threads;  

        public WorkManager(int threadCount) {
            this.threadCount = threadCount;
            threads = new Thread[threadCount];
        }

        public void AddWork(SpatialUtils.ViewpointProps viewpoint) {
            workQueue.Enqueue(viewpoint);
        }

        public void StartWorking(ref GeoMap elevationMap) {
            for (int i = 0; i < threadCount; i++) {
                VisualMagnitudeWorker worker = new VisualMagnitudeWorker(ref workQueue, ref elevationMap, 0);
                Thread thread = new Thread(worker.Start);
                threads[i] = thread;
                thread.Start();
            }
        }
    }
}
