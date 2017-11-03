using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Visual_Magnitude {
    class SpatialUtils {
        private enum Orientation { N, NNE, NE, ENE, E, ESE, SE, SSE, S, SSW, SW, WSW, W, WNW, NW, NNW }

        private static Dictionary<Orientation, LosCells> losCellsDict;
        private const double radInDeg = 57.2957795;
        private const int earthDiameter = 12740000;
        private const float lightRefraction = 0.13F;

        private double cellResolution;
        private ViewpointProps viewpoint;

        private GeoMap elevationMap;



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
                { Orientation.SE, new LosCells(-1, -1, -1, -1) },
                { Orientation.SSE, new LosCells(-1, 0, -1, -1) },
                { Orientation.S, new LosCells(-1, 0, -1, 0) },
                { Orientation.SSW, new LosCells(-1, 0, -1, 1) },
                { Orientation.SW, new LosCells(-1, 1, -1, 1) },
                { Orientation.WSW, new LosCells(0, 1, -1, 1) }
            };
        }

        public SpatialUtils(ref GeoMap elevationMap, ViewpointProps viewpoint, double cellResolution) {
            Viewpoint = viewpoint;
            this.cellResolution = cellResolution;
            this.elevationMap = elevationMap;
        }

        public SpatialUtils(ref GeoMap elevationMap, double cellResolution) {
            this.cellResolution = cellResolution;
            ElevationMap = elevationMap;
        }

        /// <summary>
        /// Determine if the cell is visible from the viewpoint. All cells in LOS cell-viewpoint have to be previously calculated and present in LOS map.
        /// </summary>
        /// <param name="losMap">Map which contains LOS values of the cells</param>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns></returns>
        public bool IsCellVisible(GeoMap losMap, int cellY, int cellX) {
            Orientation cellOrientation = GetCellOrientation(cellY, cellX);
            GetNeighborCells(cellY, cellX, cellOrientation, out int adjacentY, out int adjacentX, out int offsetY, out int offsetX);

            double adjacentWeight = InterpolateWeight(cellY, cellX, cellOrientation);

            double viewingLos = GetViewingSlope(cellY, cellX);
            double cellLos = losMap[adjacentY, adjacentX] * adjacentWeight + losMap[offsetY, offsetX] * (1 - adjacentWeight);

            if (viewingLos < cellLos) {
                losMap[cellY, cellX] = cellLos;
                return false;
            } else {
                losMap[cellY, cellX] = viewingLos;
                return true;
            }
        }

        /// <summary>
        /// Calculate visual magnitude of the cell relative to the origin.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>visual magnitude value</returns>
        public double GetVisualMagnutude(int cellY, int cellX) {
            double distance = GetDistance(cellY, cellX);
            
            double viewingSlope = GetViewingSlope(cellY, cellX);            
            double viewingAspect = GetViewingAspect(cellY, cellX);
            Vector3 viewVector = MakeNormalizedVector(viewingAspect, viewingSlope);

            double cellSlope = GetCellSlope(cellY, cellX);
            double cellAspect = GetCellAspect(cellY, cellX);
            Vector3 cellNormal = MakeNormalizedVector(cellAspect, cellSlope);

            double vectorAngle = GetVectorAngle(viewVector, cellNormal);

            if(vectorAngle < Math.PI / 2) {
                return 0D;
            }

            return (Math.Pow(cellResolution, 2) / Math.Pow(distance, 2)) * Math.Abs(Math.Cos(vectorAngle));
        }

        /// <summary>
        /// Calculate the weight of the cell adjacent to the current one.
        /// </summary>
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
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>Angle (radians)</returns>
        private double GetViewingAspect(int cellY, int cellX) {
            return (Math.Atan2((cellY - Viewpoint.Y) * cellResolution, (cellX - Viewpoint.X) * cellResolution) + 2.5 * Math.PI) % (2 * Math.PI);
        }

        /// <summary>
        /// Calculate the angle from origin to the specified point. The angle is compensated for Earth's curvature and light 
        /// diffraction.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>Vertical angle from origin to the specified point in radians relative to horizon.</returns>
        private double GetViewingSlope(int cellY, int cellX) {
            GetYXZDistances(cellY, cellX, ElevationMap[cellY, cellX], out double distY, out double distX, out double distZ);
            double directDistance = Math.Sqrt(Math.Pow(distY, 2) + Math.Pow(distX, 2));

            return Math.Atan2(distZ, directDistance);
        }

        /// <summary>
        /// Calculate cell slope.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>cell slope in radians</returns>
        private double GetCellSlope(int cellY, int cellX) {
            GetCellSlopeComponents(cellY, cellX, out double westeast, out double northsouth);

            return Math.Sqrt(Math.Pow(westeast, 2) + Math.Pow(northsouth, 2)) + (Math.PI / 2);
        }

        /// <summary>
        /// Calculate cell aspect. 0 = North, Pi/2 = East, Pi = South, 3Pi/2 = West
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>cell acpect in radians</returns>
        private double GetCellAspect(int cellY, int cellX) {
            GetCellSlopeComponents(cellY, cellX, out double westeast, out double northsouth);

            return (int) Math.Floor((Math.Atan2(-1 * northsouth, -1 * westeast) + 3.5 * Math.PI)) % (int) (2 * Math.PI); 
        }

        /// <summary>
        /// Calculate distance from the viewpoint to the cell.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns></returns>
        private double GetDistance(int cellY, int cellX) {
            GetYXZDistances(cellY, cellX, ElevationMap[cellY, cellX], out double distY, out double distX, out double distZ);
            return Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2) + Math.Pow(distZ, 2));
        }

        /// <summary>
        /// Calculate difference between origin and cell for y, x coordinates and elevation adjusted for Earth's curvature.
        /// </summary>
        /// <param name = "cellY" > Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <param name="cellElevation">Elevation of the cell</param>
        /// <param name="distY">distance on Y axis</param>
        /// <param name="distX">distance on X axis</param>
        /// <param name="distZ">distance on Z axis</param>
        private void GetYXZDistances(int cellY, int cellX, double cellElevation, out double distY, out double distX, out double distZ) {
            distX = Math.Abs(Viewpoint.X - cellX) * cellResolution;
            distY = Math.Abs(Viewpoint.Y - cellY) * cellResolution;

            double curvature = (Math.Pow(distX, 2) + Math.Pow(distY, 2)) / earthDiameter;
            distZ = (float)(cellElevation - curvature + lightRefraction * curvature - Viewpoint.Elevation);
        }

        /// <summary>
        /// Get cells through which line of sight passes.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <param name="cellOrientation">position of the cell relative to the viewpoint</param>
        /// <param name="adjacentY">Y coordinate of the adjacent cell</param>
        /// <param name="adjacentX">X coordinate of the adjacent cell</param>
        /// <param name="offsetY">Y coordinate of the offset cell</param>
        /// <param name="offsetX">X coordinate of the offset cell</param>
        private void GetNeighborCells(int cellY, int cellX, Orientation cellOrientation, out int adjacentY, out int adjacentX, out int offsetY, out int offsetX) {
            losCellsDict.TryGetValue(cellOrientation, out LosCells losCells);
            adjacentX = cellX + losCells.XCell1;
            adjacentY = cellY + losCells.YCell1;
            offsetX = cellX + losCells.XCell2;
            offsetY = cellY + losCells.YCell2;
        }

        /// <summary>
        ///  Calculate West-East and North-South components of the cell slope.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <param name="westeast">output West-East component</param>
        /// <param name="northsouth">output North-South component</param>
        private void GetCellSlopeComponents(int cellY, int cellX, out double westeast, out double northsouth) {
            double cn = Math.Sqrt(2);
            try {
                westeast = ((cn * ElevationMap[cellY - 1, cellX - 1] + ElevationMap[cellY, cellX - 1] + cn * ElevationMap[cellY + 1, cellX - 1])
                           - (cn * ElevationMap[cellY - 1, cellX + 1] + ElevationMap[cellY, cellX + 1] + cn * ElevationMap[cellY + 1, cellX + 1]))
                           / (8 * cellResolution);
            } catch (IndexOutOfRangeException) {
                westeast = 0;

            }

            try {
                northsouth = ((cn * ElevationMap[cellY - 1, cellX - 1] + ElevationMap[cellY - 1, cellX] + cn * ElevationMap[cellY - 1, cellX + 1])
                           - (cn * ElevationMap[cellY + 1, cellX - 1] + ElevationMap[cellY + 1, cellX] + cn * ElevationMap[cellY + 1, cellX + 1]))
                           / (8 * cellResolution);
            } catch (IndexOutOfRangeException) {
                northsouth = 0;
            }
        }

        private Orientation GetCellOrientation(int cellY, int cellX) {
            int orientationCode = GetCellOrientationCode(cellY, cellX);
            switch (orientationCode) {
                case 0x0020:
                    return Orientation.W;
                case 0x0100:
                    return Orientation.WNW;
                case 0x1200:
                    return Orientation.NW;
                case 0x0000:
                    return Orientation.NNW;
                case 0x0002:
                    return Orientation.N;
                case 0x0001:
                    return Orientation.NNE;
                case 0x2101:
                    return Orientation.NE;
                case 0x1001:
                    return Orientation.ENE;
                case 0x0021:
                    return Orientation.E;
                case 0x1011:
                    return Orientation.ESE;
                case 0x1211:
                    return Orientation.SE;
                case 0x1111:
                    return Orientation.SSE;
                case 0x0012:
                    return Orientation.S;
                case 0x1110:
                    return Orientation.SSW;
                case 0x2110:
                    return Orientation.SW;
                case 0x0110:
                    return Orientation.WSW;
                default:
                    throw new ArgumentException();
            }
        }

        private int GetCellOrientationCode(int cellY, int cellX) {
            int relativeX = cellX - viewpoint.X;
            int relativeY = cellY - viewpoint.Y;
            int orientation = 0;

            if (relativeX > 0) {
                orientation += 0x0001;
            } else if (relativeX == 0) {
                orientation += 0x0002;
            }

            if (relativeY > 0) {
                orientation += 0x0010;
            } else if (relativeY == 0) {
                orientation += 0x0020;
            }

            if (relativeX == relativeY) {
                orientation += 0x1200;
            } else if (relativeX == -relativeY) {
                orientation += 0x2100;
            } else if (!(relativeX == 0 || relativeY == 0)) {
                if (relativeY > relativeX) {
                    orientation += 0x0100;
                }
                if (relativeY > -relativeX) {
                    orientation += 0x1000;
                }
            }

            return orientation;
        }

        /// <summary>
        /// Create a normalized vector representation of two angles - azimuth and slope.
        /// </summary>
        /// <param name="azimuth">azimuth (horizontal angle, radians)</param>
        /// <param name="slope">slope (vertical angle, radians)</param>
        /// <returns>normalized 3D vector</returns>
        private static Vector3 MakeNormalizedVector(double azimuth, double slope) {
            float x = (float)(Math.Sin(azimuth) * Math.Cos(slope));
            float y = (float)(Math.Cos(azimuth) * Math.Cos(slope));
            float z = (float)(Math.Sin(slope));

            return Vector3.Normalize(new Vector3(x, y, z));
        }

        /// <summary>
        /// Calculate the angle between two vectors.
        /// </summary>
        /// <param name="viewpointToCell">vector from the viewpoint to the cell</param>
        /// <param name="cellNormal">normal vector of the cell</param>
        /// <returns>angle between two vectors in radians</returns>
        private double GetVectorAngle(Vector3 viewpointToCell, Vector3 cellNormal) {
            return Math.Acos(Vector3.Dot(viewpointToCell, cellNormal));
        }


        public ViewpointProps Viewpoint { get => viewpoint; set => viewpoint = value; }
        public GeoMap ElevationMap { get => elevationMap; set => elevationMap = value; }

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
            private int y;
            private int x;
            private double elevation;

            public ViewpointProps(int y, int x, double elevation) {
                this.Y = y;
                this.X = x;
                this.Elevation = elevation;
            }

            public ViewpointProps(int y, int x) {
                this.Y = y;
                this.X = x;
                System.Diagnostics.Debug.WriteLine("Construct VP: [{0},{1}]", Y, X);
            }

            public int Y { get => y; set => y = value; }
            public int X { get => x; set => x = value; }
            public double Elevation { get => elevation; set => elevation = value; }

        }

    }
}
