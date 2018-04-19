using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace VisualMagnitude {
    
    /// <summary>
    /// Class which manages all worker threads.
    /// </summary>
    class WorkManager {
        private ConcurrentQueue<SpatialUtils.ViewpointProps> workQueue = new ConcurrentQueue<SpatialUtils.ViewpointProps>();
        private Sumator sumator;
        private int threadCount;
        private int startingQueueSize;
        private Thread[] threads;
        private int runningThreads;
        static AutoResetEvent autoEvent = new AutoResetEvent(false);        

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="threadCount">Number of workers</param>
        public WorkManager(int threadCount) {
            this.threadCount = threadCount;
            threads = new Thread[threadCount];
            
        }

        /// <summary>
        /// Add a new viewpoint to be calculated.
        /// </summary>
        /// <param name="viewpoint">Viewpoint</param>
        public void AddWork(SpatialUtils.ViewpointProps viewpoint) {
            workQueue.Enqueue(viewpoint);
        }

        /// <summary>
        /// Start calculationg the visual magnitude.
        /// </summary>
        /// <param name="elevationMap">Elevation map</param>
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

        /// <summary>
        /// Listens for workers who finished their calculations and ensures the sumator won"t stop until everything is done.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ThreadFinished() {
            runningThreads--;
            System.Diagnostics.Debug.WriteLine("Running Threads: " + runningThreads);
            if (runningThreads <= 0) {
                sumator.Stop();
            }
        }

        /// <summary>
        /// Retrieve the results.
        /// </summary>
        /// <returns>Results</returns>
        public GeoMap GetResult() {
            return sumator.VisualMagnitudeMap;
        }

        public static AutoResetEvent AutoEvent { get => autoEvent; set => autoEvent = value; }
    }
}
