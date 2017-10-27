using Cudafy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visual_Magnitude {
    class SpatialUtils {
        public enum Orientation { N, NNE, NE, ENE, E, ESE, ES, SSE, S, SSW, SW, WSW, W, WNW, NW, NNW }

        private static Dictionary<Orientation, LosCells> losCellsDict;
        private const double radInDeg = 57.2957795;
        private const int earthDiameter = 12740000;
        private const float lightRefraction = 0.13F;

        /// <summary>
        /// Definitions of positions of neighbor cells in each of 16 sectors.
        /// </summary>
        static SpatialUtils() {
            losCellsDict = new Dictionary<Orientation, LosCells>();
            losCellsDict.Add(Orientation.W, new LosCells(0, 1, 0, 1));
            losCellsDict.Add(Orientation.WNW, new LosCells(0, 1, 1, 1));
            losCellsDict.Add(Orientation.NW, new LosCells(1, 1, 1, 1));
            losCellsDict.Add(Orientation.NNW, new LosCells(1, 0, 1, 1));
            losCellsDict.Add(Orientation.N, new LosCells(1, 0, 1, 0));
            losCellsDict.Add(Orientation.NNE, new LosCells(1, 0, 1, -1));
            losCellsDict.Add(Orientation.NE, new LosCells(1, -1, 1, -1));
            losCellsDict.Add(Orientation.ENE, new LosCells(0, -1, 1, -1));
            losCellsDict.Add(Orientation.E, new LosCells(0, -1, 0, -1));
            losCellsDict.Add(Orientation.ESE, new LosCells(0, -1, -1, -1));
            losCellsDict.Add(Orientation.ES, new LosCells(-1, -1, -1, -1));
            losCellsDict.Add(Orientation.SSE, new LosCells(-1, 0, -1, -1));
            losCellsDict.Add(Orientation.S, new LosCells(-1, 0, -1, 0));
            losCellsDict.Add(Orientation.SSW, new LosCells(-1, 0, -1, 1));
            losCellsDict.Add(Orientation.SW, new LosCells(-1, 1, -1, 1));
            losCellsDict.Add(Orientation.WSW, new LosCells(0, 1, -1, 1));
        }


        [Cudafy]
        public static void IsCellVisible(int threadId, double[][] elevationMap, double[][] losMap, ViewpointProps viewpoint, int cellY, int cellX, Orientation cellOrientation, bool[] visible) {
            GetNeighborCells(cellY, cellX, cellOrientation, out int adjacentY, out int adjacentX, out int offsetY, out int offsetX);

            double adjacentWeight = InterpolateWeight(viewpoint, cellY, cellX, cellOrientation);

            double viewingLos = GetViewingSlope(elevationMap, viewpoint, cellY, cellX);
            double cellLos = losMap[adjacentY][adjacentX] * adjacentWeight + losMap[offsetY][offsetX] * (1 - adjacentWeight);

            if (viewingLos < cellLos) {
                losMap[cellY][cellX] = cellLos;
                visible[threadId] = false;
            } else {
                losMap[cellY][cellX] = viewingLos;
                visible[threadId] = true;
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
        [Cudafy]
        private static double InterpolateWeight(ViewpointProps viewpoint, int cellY, int cellX, Orientation cellOrientation) {
            GetNeighborCells(cellY, cellX, cellOrientation, out int adjacentY, out int adjacentX, out int offsetY, out int offsetX);

            float cellAspect = (float)GetViewingAspect(viewpoint, viewpoint.Y, viewpoint.X);
            float adjacentAspect = (float)GetViewingAspect(viewpoint, adjacentY, adjacentX);
            float offsetAspect = (float)GetViewingAspect(viewpoint, offsetY, offsetX);

            double total = GMath.Abs(adjacentAspect - cellAspect) + GMath.Abs(offsetAspect - cellAspect);

            if (total == 0)
                return 1.0;
            else
                return GMath.Abs(offsetAspect - cellAspect) / total;
        }

        /// <summary>
        /// Calculate viewing angle from the viewpoint to the cell in radians.
        /// </summary>
        /// <param name="viewpoint">Viewpoint properties</param>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>Angle (radians)</returns>
        [Cudafy]
        private static double GetViewingAspect(ViewpointProps viewpoint, int cellY, int cellX) {
            return GMath.Atan2((cellY - viewpoint.Y) * viewpoint.CellResoulution, (cellX - viewpoint.X) * viewpoint.CellResoulution);
        }

        [Cudafy]
        private static double GetViewingSlope(double[][] elevationMap, ViewpointProps viewpoint, int cellY, int cellX) {
            float[] yxzDistances = GetYXZDistances(viewpoint, cellY, cellX, elevationMap[cellY][cellX]);
            float directDistance = GMath.Sqrt(GMath.Pow(yxzDistances[1], 2) + GMath.Pow(yxzDistances[0], 2));

            return GMath.Atan2(yxzDistances[2], directDistance);
        }

        /// <summary>
        /// Calculate difference between origin and cell for y, x coordinates and elevation adjusted for Earth's curvature.
        /// </summary>
        /// <param name="viewpoint">Viewpoint properties</param>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <param name="cellElevation">Elevation of the cell</param>
        /// <returns></returns>
        [Cudafy]
        private static float[] GetYXZDistances(ViewpointProps viewpoint, int cellY, int cellX, double cellElevation) {
            float distX = GMath.Abs(viewpoint.X - cellX) * viewpoint.CellResoulution;
            float distY = GMath.Abs(viewpoint.Y - cellY) * viewpoint.CellResoulution;

            double curvature = (GMath.Pow(distX, 2) + GMath.Pow(distY, 2)) / earthDiameter;
            float distZ = (float)(cellElevation - curvature + lightRefraction * curvature - viewpoint.Elevation);

            return new float[3] { distY, distX, distZ };
        }

        [Cudafy]
        private static void GetNeighborCells(int cellY, int cellX, Orientation cellOrientation, out int adjacentY, out int adjacentX, out int offsetY, out int offsetX) {
            losCellsDict.TryGetValue(cellOrientation, out LosCells losCells);
            adjacentX = cellX + losCells.XCell1;
            adjacentY = cellY + losCells.YCell1;
            offsetX = cellX + losCells.XCell2;
            offsetY = cellY + losCells.YCell2;
        }

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
                this.cellResoulution = cellResoulution;
                this.elevation = elevation;
            }

            public int Y { get; set; }

            public int X { get; set; }

            public int CellResoulution { get; set; }

            public int Elevation { get; set; }
        }
    }
}
