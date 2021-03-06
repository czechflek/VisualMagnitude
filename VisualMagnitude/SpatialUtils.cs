﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace VisualMagnitude {
    /// <summary>
    /// Class which handles calculations needed to calculate the visual magnitude.
    /// </summary>
    class SpatialUtils {
        private enum Orientation { N, NNE, NE, ENE, E, ESE, SE, SSE, S, SSW, SW, WSW, W, WNW, NW, NNW }

        private static Dictionary<Orientation, LosCells> losCellsDict;
        private const double radInDeg = 57.2957795;
        private const int earthDiameter = 12740000;
        private const float lightRefraction = 0.13F;
        private Vector3 verticalVector = new Vector3(0, 0, 1);

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

        /// <summary>
        /// Constructor. Initialize the class with an elevation map.
        /// </summary>
        /// <param name="elevationMap">Elevtion map</param>
        public SpatialUtils(ref GeoMap elevationMap) {
            cellResolution = elevationMap.CellSize;
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

            if (viewingLos > cellLos) {
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
        /// <returns>Visual magnitude value</returns>
        public double GetVisualMagnitude(int cellY, int cellX) { //TODO: typo!
            double distance = GetDistance(cellY, cellX);

            //if the viewpoints are wind turbines, we can ignore the cell slope
            if (SettingsManager.Instance.CurrentSettings.WindTurbines) {
                return Math.Pow(cellResolution, 2) / Math.Pow(distance, 2);
            } else {
                double viewingSlope = GetViewingSlope(cellY, cellX);
                double viewingAspect = GetViewingAspect(cellY, cellX);
                Vector3 viewVector = MakeNormalizedVector(viewingAspect, viewingSlope);

                double cellSlope = GetCellSlope(cellY, cellX);
                double cellAspect = GetCellAspect(cellY, cellX);
                Vector3 cellNormal = MakeNormalizedVector(cellAspect, cellSlope);

                double vectorAngle = GetVectorAngle(viewVector, cellNormal);

                if (vectorAngle < Math.PI / 2) {
                    return 0D;
                }

                return (Math.Pow(cellResolution, 2) / Math.Pow(distance, 2)) * Math.Abs(Math.Cos(vectorAngle));
            }

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

            double cellAspect = GetViewingAspect(cellY, cellX);
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
            return (-Math.Atan2(Viewpoint.X - cellX, Viewpoint.Y - cellY) + 2 * Math.PI) % (2 * Math.PI); //prohozeno
        }

        /// <summary>
        /// Calculate the angle from origin to the specified point. The angle is compensated for Earth's curvature and light 
        /// diffraction. The values range from 0 to Pi, where 0 is looking directly up, and Pi is looking directly down from the viewpoint.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>Vertical angle from origin to the specified point in radians relative to zenith.</returns>
        private double GetViewingSlope(int cellY, int cellX) {
            GetYXZDistances(cellY, cellX, ElevationMap[cellY, cellX], out double distY, out double distX, out double distZ);
            double directDistance = Math.Sqrt(Math.Pow(distY, 2) + Math.Pow(distX, 2));

            return Math.Atan2(directDistance, distZ);
        }

        /// <summary>
        /// Calculate cell slope.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>cell slope in radians</returns>
        /*private double GetCellSlope(int cellY, int cellX) {
            GetCellSlopeComponents(cellY, cellX, out double westeast, out double northsouth);

            return Math.Sqrt(Math.Pow(westeast, 2) + Math.Pow(northsouth, 2)) + (Math.PI / 2);
        }*/

        /// <summary>
        /// Calculate cell aspect. 0 = North, Pi/2 = East, Pi = South, 3Pi/2 = West
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>cell acpect in radians</returns>
        private double GetCellAspect(int cellY, int cellX) {
            GetCellSlopeComponents(cellY, cellX, out double westeast, out double northsouth);

            return (Math.Atan2(westeast, -1 * northsouth) + 2 * Math.PI) % (2 * Math.PI);
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
            distZ = cellElevation - curvature + lightRefraction * curvature - Viewpoint.Elevation;
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
        /// <param name="southnorth">output North-South component</param>
        private void GetCellSlopeComponents(int cellY, int cellX, out double eastwest, out double northsouth) {
            double cn = Math.Sqrt(2);

            if (cellX <= 0 || cellY <= 0 || cellX >= ElevationMap.GetLength(1) - 1 || cellY >= ElevationMap.GetLength(0) - 1) {
                northsouth = eastwest = 0;
                return;
            }

            try {
                northsouth = (cn * ElevationMap[cellY - 1, cellX - 1] + ElevationMap[cellY - 1, cellX] + cn * ElevationMap[cellY - 1, cellX + 1]) / 4
                           - (cn * ElevationMap[cellY + 1, cellX - 1] + ElevationMap[cellY + 1, cellX] + cn * ElevationMap[cellY + 1, cellX + 1]) / 4;

                eastwest = (cn * ElevationMap[cellY - 1, cellX - 1] + ElevationMap[cellY, cellX - 1] + cn * ElevationMap[cellY + 1, cellX - 1]) / 4
                         - (cn * ElevationMap[cellY - 1, cellX + 1] + ElevationMap[cellY, cellX + 1] + cn * ElevationMap[cellY + 1, cellX + 1]) / 4;
            } catch (IndexOutOfRangeException) {
                System.Diagnostics.Debug.WriteLine("{0}, {1}", cellX, cellY);
                northsouth = eastwest = 0;
            }
        }

        /// <summary>
        /// Get the cell slope in radians based on its circular neighborhood. The results is the angle between vertical axis and normal of the cell.
        /// E.g. 0 = cell is completely flat, pi/2 = the cell is a vertical wall. 
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>Cell slope in radians</returns>
        private double GetCellSlope(int cellY, int cellX) {
            double cn = Math.Sqrt(2);
            double northsouth;
            double eastwest;

            if (cellX <= 0 || cellY <= 0 || cellX >= ElevationMap.GetLength(1) - 1 || cellY >= ElevationMap.GetLength(0) - 1) {
                return 0;
            }

            try {
                northsouth = (cn * ElevationMap[cellY - 1, cellX - 1] + ElevationMap[cellY - 1, cellX] + cn * ElevationMap[cellY - 1, cellX + 1]) / 4
                           - (cn * ElevationMap[cellY + 1, cellX - 1] + ElevationMap[cellY + 1, cellX] + cn * ElevationMap[cellY + 1, cellX + 1]) / 4;

                eastwest = (cn * ElevationMap[cellY - 1, cellX - 1] + ElevationMap[cellY, cellX - 1] + cn * ElevationMap[cellY + 1, cellX - 1]) / 4
                         - (cn * ElevationMap[cellY - 1, cellX + 1] + ElevationMap[cellY, cellX + 1] + cn * ElevationMap[cellY + 1, cellX + 1]) / 4;
            } catch (IndexOutOfRangeException) {
                System.Diagnostics.Debug.WriteLine("{0}, {1}", cellX, cellY);
                return 0;
            }

            Vector3 northVector = Vector3.Normalize(new Vector3(0, 2 * (float)cellResolution, (float)northsouth));
            Vector3 eastVector = Vector3.Normalize(new Vector3(2 * (float)cellResolution, 0, (float)eastwest));

            Vector3 slopeVector = Vector3.Cross(eastVector, northVector);

            return GetVectorAngle(slopeVector, verticalVector);
        }

        /// <summary>
        /// Determine in which cardinal direction the cell is relative to the viewpoint.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>Cell's cardinal direction</returns>
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

        /// <summary>
        /// Calculate segment code in which the cell is relative to the viewpoint.
        /// </summary>
        /// <param name="cellY">Y coordinate of the cell</param>
        /// <param name="cellX">X coordinate of the cell</param>
        /// <returns>Segment code</returns>
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
            float x = (float)(Math.Cos(azimuth) * Math.Sin(slope));
            float y = (float)(Math.Sin(azimuth) * Math.Sin(slope));
            float z = (float)(Math.Cos(slope));

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

        /// <summary>
        /// Structure to store two cells.
        /// </summary>
        private struct LosCells {
            public LosCells(int yCell1, int xCell1, int yCell2, int xCell2) : this() {
                YCell1 = yCell1;
                XCell1 = xCell1;
                YCell2 = yCell2;
                XCell2 = xCell2;
            }

            public int YCell1 { get; set; }

            public int XCell1 { get; set; }

            public int YCell2 { get; set; }

            public int XCell2 { get; set; }
        }

        /// <summary>
        /// Structure to store the viewpoint properties.
        /// </summary>
        public class ViewpointProps {
            public ViewpointProps() {
                Y = -1;
                X = -1;
                Elevation = 0;
                ElevationOffset = 0;
                Weight = 1;
            }
            public int Y { get; set; }
            public int X { get; set; }
            public double Elevation { get; set; }
            public double ElevationOffset { get; set; }
            public double Weight { get; set; }

        }

    }
}
