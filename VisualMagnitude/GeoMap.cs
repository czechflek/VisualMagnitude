using ArcGIS.Core.Data.Raster;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualMagnitude {
    /// <summary>
    /// Container for elevation map.
    /// </summary>
    class GeoMap {
        public double[,] geoMap;
        public const double UndefinedValue = 100D;
        private double cellSize = 1;

        public double this[int y, int x] {
            get { return geoMap[y, x]; }
            set { geoMap[y, x] = value; }
        }

        /// <summary>
        /// Constructor which initializes the map size.
        /// </summary>
        /// <param name="dimensionY">Y length</param>
        /// <param name="dimensionX">X length</param>
        public GeoMap(int dimensionY, int dimensionX) {
            geoMap = new double[dimensionY, dimensionX];
        }

        /// <summary>
        /// Create a mock map for testing purposes.
        /// </summary>
        /// <param name="dimensionY">Y length</param>
        /// <param name="dimensionX">X length</param>
        /// <returns>Mocked GeoMap</returns>
        public static GeoMap CreateMock(int dimensionY, int dimensionX) {
            GeoMap map = new GeoMap(dimensionY, dimensionX);
            Random rnd = new Random();
            for (int y = 0; y < dimensionY; y++) {
                for (int x = 0; x < dimensionX; x++) {
                    map[y, x] = y;
                }
            }
            return map;
        }

        /// <summary>
        /// Import the elevation data.
        /// </summary>
        /// <param name="data">Elevation data</param>
        public void ImportData(Array data) {
            for (int x = 0; x < data.GetLength(0); x++) {
                for (int y = 0; y < data.GetLength(1); y++) {
                    geoMap[y, x] = Convert.ToDouble(data.GetValue(x, y));
                }
            }
        }

        /// <summary>
        /// Initalize the entire map to 0.
        /// </summary>
        public void Initialize() {
            ClearMap();
        }

        /*
        public Raster WriteDataToRaster(Raster raster) {
            PixelBlock pixelBlock = raster.CreatePixelBlock(raster.GetWidth(), raster.GetHeight());
            //raster.Read(0, 0, pixelBlock);
            pixelBlock.Clear(0);
            System.Diagnostics.Debug.WriteLine(pixelBlock.GetPlaneCount());
            System.Diagnostics.Debug.WriteLine(raster.GetWidth() + "x" + raster.GetHeight());
            pixelBlock.SetPixelData(0, geoMap);
            raster.Write(0, 0, pixelBlock);
            //raster.Refresh();
            return raster;

        }*/

        /// <summary>
        /// Initalize the ommited rings around a viewpont.
        /// </summary>
        /// <param name="viewpointY">Y coordinate of a viewpoint</param>
        /// <param name="viewpointX">X coordinate of a viewpoint</param>
        /// <param name="omittedDistance">Number of omitted rings</param>
        public void InitializeOmittedRings(int viewpointY, int viewpointX, int omittedDistance) {
            for (int i = 0; i < omittedDistance; i++) {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Transpose the map.
        /// </summary>
        /// <returns>Transposed map</returns>
        public double[,] Transpose() {
            double[,] transposed = new double[geoMap.GetLength(1), geoMap.GetLength(0)];
            for (int x = 0; x < geoMap.GetLength(1); x++) {
                for (int y = 0; y < geoMap.GetLength(0); y++) {
                    transposed[x, y] = geoMap[y, x];
                }
            }
            return transposed;

        }

        /// <summary>
        /// Clear the map.
        /// </summary>
        public void ClearMap() {
            Array.Clear(geoMap, 0, geoMap.Length);
        }

        /// <summary>
        /// Return length of the selected dimension of the map
        /// </summary>
        /// <param name="dimension">Dimension</param>
        /// <returns>Length of the dimension</returns>
        public int GetLength(int dimension) {
            return geoMap.GetLength(dimension);
        }

        /// <summary>
        /// Calculate the ring in the specified distance around a viewpoint 
        /// </summary>
        /// <param name="viewpointY">Y coordinate of a viewpoint</param>
        /// <param name="viewpointX">X coordinate of a viewpoint</param>
        /// <param name="distance"></param>
        /// <returns>Ring around viewpoint</returns>
        public Ring GetRing(int viewpointY, int viewpointX, int distance) {
            bool[] inbounds = new bool[4] { true, true, true, true };

            //top-left corner
            int[] topleft = new int[2] { viewpointY - distance, viewpointX - distance };
            if (topleft[0] < 0) {
                topleft[0] = 0;
                inbounds[0] = false;
            }
            if (topleft[1] < 0) {
                topleft[1] = 0;
            }

            //top-right corner
            int[] topright = new int[2] { viewpointY - distance, viewpointX + distance };
            if (topright[0] < 0) {
                topright[0] = 0;
            }
            if (topright[1] >= geoMap.GetLength(1)) {
                topright[1] = geoMap.GetLength(1) - 1;
                inbounds[1] = false;
            }

            //bottom-right corner
            int[] bottomright = new int[2] { viewpointY + distance, viewpointX + distance };
            if (bottomright[0] >= geoMap.GetLength(0)) {
                bottomright[0] = geoMap.GetLength(0) - 1;
                inbounds[2] = false;
            }
            if (bottomright[1] >= geoMap.GetLength(1)) {
                bottomright[1] = geoMap.GetLength(1) - 1;
            }

            //bottom-left corner
            int[] bottomleft = new int[2] { viewpointY + distance, viewpointX - distance };
            if (bottomleft[0] >= geoMap.GetLength(0)) {
                bottomleft[0] = geoMap.GetLength(0) - 1;

            }
            if (bottomleft[1] < 0) {
                bottomleft[1] = 0;
                inbounds[3] = false;
            }

            return new Ring(topleft, topright, bottomright, bottomleft, inbounds);
        }

        public double CellSize { get => cellSize; set => cellSize = value; }

        /// <summary>
        /// Container for ring properties.
        /// </summary>
        public class Ring : IEnumerable {
            int[] topleft;
            int[] topright;
            int[] bottomright;
            int[] bottomleft;
            bool[] inbounds;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="topleft">Top-left corner of the ring</param>
            /// <param name="topright">Top-right corner of the ring</param>
            /// <param name="bottomright">Bottom-right corner of the ring</param>
            /// <param name="bottomleft">Bottom-left corner of the ring</param>
            /// <param name="inbounds">Inbound state of corners</param>
            public Ring(int[] topleft, int[] topright, int[] bottomright, int[] bottomleft, bool[] inbounds) {
                this.topleft = topleft;
                this.topright = topright;
                this.bottomright = bottomright;
                this.bottomleft = bottomleft;
                this.inbounds = inbounds;
            }

            /// <summary>
            /// Enumerator which yealds cells in the ring.
            /// </summary>
            /// <returns>Ring enumerator</returns>
            public IEnumerator GetEnumerator() {
                //top row
                if (inbounds[0]) {
                    for (int x = topleft[1]; x < topright[1]; x++) {
                        yield return new int[2] { topleft[0], x };
                    }
                }

                //right column
                if (inbounds[1]) {
                    for (int y = topright[0]; y < bottomright[0]; y++) {
                        yield return new int[2] { y, topright[1] };
                    }
                }

                //bottom row
                if (inbounds[2]) {
                    for (int x = bottomright[1]; x > bottomleft[1]; x--) {
                        yield return new int[2] { bottomright[0], x };
                    }
                }

                //left column
                if (inbounds[3]) {
                    for (int y = bottomleft[0]; y > topleft[0]; y--) {
                        yield return new int[2] { y, bottomleft[1] };
                    }
                }
            }
        }
    }
}
