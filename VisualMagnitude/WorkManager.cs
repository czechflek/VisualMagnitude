using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace VisualMagnitude {
    class WorkManager {
        private ConcurrentQueue<SpatialUtils.ViewpointProps> workQueue = new ConcurrentQueue<SpatialUtils.ViewpointProps>();
        private Sumator sumator;
        private int threadCount;
        private int startingQueueSize;
        private Thread[] threads;
        private int runningThreads;
        static AutoResetEvent autoEvent = new AutoResetEvent(false);        

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
            runningThreads = threadCount;
            for (int i = 0; i < threadCount; i++) {
                VisualMagnitudeWorker worker = new VisualMagnitudeWorker(ref workQueue, ref elevationMap, ref sumator, this);
                Thread thread = new Thread(worker.Start);
                threads[i] = thread;
                thread.Start();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ThreadFinished() {
            runningThreads--;
            System.Diagnostics.Debug.WriteLine("Running Threads: " + runningThreads);
            if (runningThreads <= 0) {
                sumator.Stop();
            }
        }

        public GeoMap GetResult() {
            return sumator.VisualMagnitudeMap;
        }

        public static AutoResetEvent AutoEvent { get => autoEvent; set => autoEvent = value; }
    }
}
