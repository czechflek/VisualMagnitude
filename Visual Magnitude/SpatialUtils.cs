using Cudafy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visual_Magnitude
{
    class SpatialUtils
    {
        public enum Position { N, NNE, NE, ENE, E, ESE, ES, SSE, S, SSW, SW, WSW, W, WNW, NW, NNW }

        private int originY;
        private int originX;
        private double originElevation;
        private double cellResolution;
        private static Dictionary<Position, LosCells> losCellsDict;

        static SpatialUtils() {
            losCellsDict = new Dictionary<Position, LosCells>();
            losCellsDict.Add(Position.W, new LosCells(0, 1, 0, 1));
            losCellsDict.Add(Position.WNW, new LosCells(0, 1, 1, 1));
            losCellsDict.Add(Position.NW, new LosCells(1, 1, 1, 1));
            losCellsDict.Add(Position.NNW, new LosCells(1, 0, 1, 1));
            losCellsDict.Add(Position.N, new LosCells(1, 0, 1, 0));
            losCellsDict.Add(Position.NNE, new LosCells(1, 0, 1, -1));
            losCellsDict.Add(Position.NE, new LosCells(1, -1, 1, -1));
            losCellsDict.Add(Position.ENE, new LosCells(0, -1, 1, -1));
            losCellsDict.Add(Position.E, new LosCells(0, -1, 0, -1));
            losCellsDict.Add(Position.ESE, new LosCells(0, -1, -1, -1));
            losCellsDict.Add(Position.ES, new LosCells(-1, -1, -1, -1));
            losCellsDict.Add(Position.SSE, new LosCells(-1, 0, -1, -1));
            losCellsDict.Add(Position.S, new LosCells(-1, 0, -1, 0));
            losCellsDict.Add(Position.SSW, new LosCells(-1, 0, -1, 1));
            losCellsDict.Add(Position.SW, new LosCells(-1, 1, -1, 1));
            losCellsDict.Add(Position.WSW, new LosCells(0, 1, -1, 1));
        }

        public SpatialUtils(int originY, int originX, double originElevation, double cellResolution) {
            this.originY = originY;
            this.originX = originX;
            this.originElevation = originElevation;
            this.cellResolution = cellResolution;
        }

        [Cudafy]
        public static void IsCellVisible(int threadId, double[][] losMap, int cellY, int cellX, int viewpointY, int viewpointX) {


        }

        private struct LosCells
        {
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
    }
}
