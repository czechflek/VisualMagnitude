using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Visual_Magnitude {
    class SpatialUtils {
        public enum Orientation { N, NNE, NE, ENE, E, ESE, ES, SSE, S, SSW, SW, WSW, W, WNW, NW, NNW }

        private static Dictionary<Orientation, LosCells> losCellsDict;
        private const double radInDeg = 57.2957795;
        private const int earthDiameter = 12740000;
        private const float lightRefraction = 0.13F;

        private double cellResolution;
        private ViewpointProps viewpoint;

        private static double[,] elevationMap;



        /// <summary>
        /// Definitions of positions of neighbor cells in each of 16 sectors.
        /// </summary>
        static SpatialUtils() {
            losCellsDict = new Dictionary<Orientation, LosCells>
            {
                { Orientation.W, new LosCells(0, 1, 0, 1) },
                { Orientation.WNW, new LosCells(0, 1, 1, 1) },
                { Orientation.NW, new LosCells(1, 1, 1, 1) },
                { Orientation.NNW, new LosCells(1, 0, 1, 1) },
                { Orientation.N, new LosCells(1, 0, 1, 0) },
                { Orientation.NNE, new LosCells(1, 0, 1, -1) },
                { Orientation.NE, new LosCells(1, -1, 1, -1) },
                { Orientation.ENE, new LosCells(0, -1, 1, -1) },
                { Orientation.E, new LosCells(0, -1, 0, -1) },
                { Orientation.ESE, new LosCells(0, -1, -1, -1) },
                { Orientation.ES, new LosCells(-1, -1, -1, -1) },
                { Orientation.SSE, new LosCells(-1, 0, -1, -1) },
                { Orientation.S, new LosCells(-1, 0, -1, 0) },
                { Orientation.SSW, new LosCells(-1, 0, -1, 1) },
                { Orientation.SW, new LosCells(-1, 1, -1, 1) },
                { Orientation.WSW, new LosCells(0, 1, -1, 1) }
            };
        }

        public SpatialUtils(ViewpointProps viewpoint, double cellResolution) {
            this.Viewpoint = viewpoint;
            this.cellResolution = cellResolution;
        }

        public void IsCellVisible(double[,] losMap, int cellY, int cellX, Orientation cellOrientation) {
            GetNeighborCells(cellY, cellX, cellOrientation, out int adjacentY, out int adjacentX, out int offsetY, out int offsetX);

            double adjacentWeight = InterpolateWeight(cellY, cellX, cellOrientation);

            double viewingLos = GetViewingSlope(cellY, cellX);
            double cellLos = losMap[adjacentY, adjacentX] * adjacentWeight + losMap[offsetY, offsetX] * (1 - adjacentWeight);

            if (viewingLos < cellLos) {
                losMap[cellY, cellX] = cellLos;
            } else {
                losMap[cellY, cellX] = viewingLos;
            }
        }

        /// <summary>
        /// Calculate the weight of the cell adjacent to the current one.
        /// </summary>
        /// <param name="viewpoint">Viewpoint properties</param>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <param name="cellOrientation">Orientation of the cell</param>
        /// <returns>Weight of the adjacent cell</returns>
        private double InterpolateWeight(int cellY, int cellX, Orientation cellOrientation) {
            GetNeighborCells(cellY, cellX, cellOrientation, out int adjacentY, out int adjacentX, out int offsetY, out int offsetX);

            double cellAspect = GetViewingAspect(Viewpoint.Y, Viewpoint.X);
            double adjacentAspect = GetViewingAspect(adjacentY, adjacentX);
            double offsetAspect = GetViewingAspect(offsetY, offsetX);

            double total = Math.Abs(adjacentAspect - cellAspect) + Math.Abs(offsetAspect - cellAspect);

            if (total == 0)
                return 1.0;
            else
                return Math.Abs(offsetAspect - cellAspect) / total;
        }

        /// <summary>
        /// Calculate viewing angle from the viewpoint to the cell in radians.
        /// </summary>
        /// <param name="viewpoint">Viewpoint properties</param>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>Angle (radians)</returns>
        private double GetViewingAspect(int cellY, int cellX) {
            return Math.Atan2((cellY - Viewpoint.Y) * Viewpoint.CellResoulution, (cellX - Viewpoint.X) * Viewpoint.CellResoulution);
        }

        private double GetViewingSlope(int cellY, int cellX) {
            GetYXZDistances(cellY, cellX, ElevationMap[cellY, cellX], out double distY, out double distX, out double distZ);
            double directDistance = Math.Sqrt(Math.Pow(distY, 2) + Math.Pow(distX, 2));

            return Math.Atan2(distZ, directDistance);
        }

        /// <summary>
        /// Calculate difference between origin and cell for y, x coordinates and elevation adjusted for Earth's curvature.
        /// </summary>
        /// <param name="viewpoint">Viewpoint properties</param>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <param name="cellElevation">Elevation of the cell</param>
        /// <returns></returns>
        private void GetYXZDistances(int cellY, int cellX, double cellElevation, out double distY, out double distX, out double distZ) {
            distX = Math.Abs(Viewpoint.X - cellX) * Viewpoint.CellResoulution;
            distY = Math.Abs(Viewpoint.Y - cellY) * Viewpoint.CellResoulution;

            double curvature = (Math.Pow(distX, 2) + Math.Pow(distY, 2)) / earthDiameter;
            distZ = (float)(cellElevation - curvature + lightRefraction * curvature - Viewpoint.Elevation);
        }

        private void GetNeighborCells(int cellY, int cellX, Orientation cellOrientation, out int adjacentY, out int adjacentX, out int offsetY, out int offsetX) {
            losCellsDict.TryGetValue(cellOrientation, out LosCells losCells);
            adjacentX = cellX + losCells.XCell1;
            adjacentY = cellY + losCells.YCell1;
            offsetX = cellX + losCells.XCell2;
            offsetY = cellY + losCells.YCell2;
        }

        private static Vector3 MakeNormalizedVector(double azimuth, double slope) {
            float x = (float)(Math.Sin(azimuth) * Math.Cos(slope));
            float y = (float)(Math.Cos(azimuth) * Math.Cos(slope));
            float z = (float)(Math.Sin(slope));

            return Vector3.Normalize(new Vector3(x, y, z));
        }

        private double GetVectorAngle(Vector3 normal, Vector3 direction) {
            return Math.Acos(Vector3.Dot(normal, direction));
        }


        public ViewpointProps Viewpoint { get => viewpoint; set => viewpoint = value; }
        public static double[,] ElevationMap { get => elevationMap; set => elevationMap = value; }

        private struct LosCells {
            int yCell1;
            int xCell1;
            int yCell2;
            int xCell2;

            public LosCells(int yCell1, int xCell1, int yCell2, int xCell2) : this() {
                this.yCell1 = yCell1;
                this.xCell1 = xCell1;
                this.yCell2 = yCell2;
                this.xCell2 = xCell2;
            }

            public int YCell1 { get; set; }

            public int XCell1 { get; set; }

            public int YCell2 { get; set; }

            public int XCell2 { get; set; }
        }

        public class ViewpointProps {
            int y;
            int x;
            double cellResoulution;
            double elevation;

            public ViewpointProps(int y, int x, double cellResoulution, double elevation) {
                this.y = y;
                this.x = x;
                this.elevation = elevation;
            }

            public int Y { get; set; }

            public int X { get; set; }

            public int CellResoulution { get; set; }

            public int Elevation { get; set; }
        }
    }

}
