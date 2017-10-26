﻿using Cudafy;
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
        public static void IsCellVisible(int threadId, ViewpointProps viewpoint, double[][] losMap, int cellY, int cellX, Orientation cellOrientation, bool[] visibility) {


        }

        [Cudafy]
        private static void InterpolateWeight() {

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

        private struct LosCells {
            int yCell1;
            int xCell1;
            int yCell2;
            int xCell2;

            public LosCells(int yCell1, int xCell1, int yCell2, int xCell2) {
                this.yCell1 = yCell1;
                this.xCell1 = xCell1;
                this.yCell2 = yCell2;
                this.xCell2 = xCell2;
            }
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
