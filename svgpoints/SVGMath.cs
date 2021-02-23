using System;
using System.Collections.Generic;
using System.Text;

namespace svgpoints
{
    internal static class SVGMath
    {
        /// <summary>
        /// Calculates a single x or y value along the beizer curve for value 't'
        /// </summary>
        /// <param name="xy0">Quadratic Beizer P0 parameter for x or y (i.e. the starting x or y coordinate)</param>
        /// <param name="xy1">Quadratic Beizer P1 parameter for x or y (i.e. the first curve x or y coordinate)</param>
        /// <param name="xy2">Quadratic Beizer P2 parameter for x or y (i.e. the ending x or y coordinate)</param>
        /// <param name="t">How far along the curve to calculate the x or y coordinate. A value between 0 and 1</param>
        /// <returns>The x or y value for the beizer curve at distance 't' along the curve</returns>
        internal static double CalculateQuadraticBeizerSinglePoint(double xy0, double xy1, double xy2, double t)
        {
            return Math.Pow(1 - t, 2) * xy0
                   + 2 * (1 - t) * t * xy1
                   + Math.Pow(t, 2) * xy2;
        }

        /// <summary>
        /// Read here for more info on Beizer curves: https://www.freecodecamp.org/news/nerding-out-with-bezier-curves-6e3c0bc48e2f/
        /// 
        /// Calculates the Quadratic Beizer curve, returning a list of points of size 'numPoints' along the curve.
        /// </summary>
        /// <param name="p0">Quadratic Beizer P0 parameter (i.e. the starting xy coordinate)</param>
        /// <param name="p1">Quadratic Beizer P1 parameter (i.e. the first curve coordinate)</param>
        /// <param name="p3">Quadratic Beizer P2 parameter (i.e. the ending xy coordinate)</param>
        /// <param name="numPoints">Number of points to return along the curve</param>
        /// <returns>List of 'numPoints' long of points along the beizer curve</returns>
        internal static IEnumerable<Point> CalculateQuadraticBeizer(Point p0, Point p1, Point p2, int numPoints)
        {
            for (int i = 1; i <= numPoints; i++)
            {
                double t = (double)i / (double)numPoints;
                yield return new Point(CalculateQuadraticBeizerSinglePoint(p0.X, p1.X, p2.X, t),
                                       CalculateQuadraticBeizerSinglePoint(p0.Y, p1.Y, p2.Y, t));
            }
        }

        /// <summary>
        /// Calculates a single x or y value along the beizer curve for value 't'
        /// </summary>
        /// <param name="xy0">Cubic Beizer P0 parameter for x or y (i.e. the starting x or y coordinate)</param>
        /// <param name="xy1">Cubic Beizer P1 parameter for x or y (i.e. the first curve x or y coordinate)</param>
        /// <param name="xy2">Cubic Beizer P2 parameter for x or y (i.e. the second curve x or y coordinate)</param>
        /// <param name="xy3">Cubic Beizer P3 parameter for x or y (i.e. the ending x or y coordinate)</param>
        /// <param name="t">How far along the curve to calculate the x or y coordinate. A value between 0 and 1</param>
        /// <returns>The x or y value for the beizer curve at distance 't' along the curve</returns>
        internal static double CalculateCubicBeizerSinglePoint(double xy0, double xy1, double xy2, double xy3, double t)
        {
            return Math.Pow(1 - t, 3) * xy0
                   + 3 * Math.Pow(1 - t, 2) * t * xy1
                   + 3 * (1 - t) * Math.Pow(t, 2) * xy2
                   + Math.Pow(t, 3) * xy3;
        }

        /// <summary>
        /// Read here for more info on Beizer curves: https://www.freecodecamp.org/news/nerding-out-with-bezier-curves-6e3c0bc48e2f/
        /// 
        /// Calculates the Cubic Beizer curve, returning a list of points of size 'numPoints' along the curve.
        /// </summary>
        /// <param name="p0">Cubic Beizer P0 parameter (i.e. the starting xy coordinate)</param>
        /// <param name="p1">Cubic Beizer P1 parameter (i.e. the first curve coordinate)</param>
        /// <param name="p2">Cubic Beizer P2 parameter (i.e. the second curve coordinate)</param>
        /// <param name="p3">Cubic Beizer P3 parameter (i.e. the ending xy coordinate)</param>
        /// <param name="numPoints">Number of points to return along the curve</param>
        /// <returns>List of 'numPoints' long of points along the beizer curve</returns>
        internal static IEnumerable<Point> CalculateCubicBeizer(Point p0, Point p1, Point p2, Point p3, int numPoints)
        {
            for (int i = 1; i <= numPoints; i++)
            {
                double t = (double)i / (double)numPoints;
                yield return new Point(CalculateCubicBeizerSinglePoint(p0.X, p1.X, p2.X, p3.X, t),
                                       CalculateCubicBeizerSinglePoint(p0.Y, p1.Y, p2.Y, p3.Y, t));
            }
        }
    }
}
