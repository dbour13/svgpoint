using svgpoints.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace svgpoints
{
    public static class SvgPoints
    {
        /// <summary>
        /// Reads an SVG Stream (XML Format) and extracts the SVG Path information.
        /// 
        /// It then returns a list of list of Points, one list of points for each SVG Path
        /// found in the stream.
        /// </summary>
        /// <param name="svgStream">SVG XML Stream</param>
        /// <returns>List of List of Points, one list of points for each SVG Path</returns>
        public static IEnumerable<IEnumerable<Point>> GetPointsFromSVG(Stream svgStream)
        {
            List<List<Point>> pointsListResults = new List<List<Point>>();

            // Load the XML
            var doc = new XmlDocument();
            doc.Load(svgStream);

            // Extract the SVG paths from the XML
            var pathTags = doc.GetElementsByTagName("path");
            for (int i = 0; i < pathTags.Count; i++)
            {
                string s = pathTags[i].Attributes.GetNamedItem("d").Value;
                pointsListResults.Add(svgpoints.SvgPoints.GetPointsFromSVGPathsString(s).ToList());
            }

            return pointsListResults;
        }

        /// <summary>
        /// Reads an SVG File (XML Format) and extracts the SVG Path information.
        /// 
        /// It then returns a list of list of Points, one list of points for each SVG Path
        /// found in the File.
        /// </summary>
        /// <param name="svgStream">SVG XML File</param>
        /// <returns>List of List of Points, one list of points for each SVG Path</returns>
        public static IEnumerable<IEnumerable<Point>> GetPointsFromSVG(string svgFilePath)
        {
            using (var stream = File.OpenRead(svgFilePath))
            {
                return GetPointsFromSVG(stream);
            }
        }

        /// <summary>
        /// Returns a list of XY coordinates for an SVG path string.  
        /// 
        /// Drawing lines between these coordinates will allow you to draw the SVG image (although you lose fill/colour/line thickness
        /// information).
        /// </summary>
        /// <param name="svgPathsString">SVG Path string</param>
        /// <param name="pointsPerBeizerCommand">Number of points to use for Beizer commands</param>
        /// <returns>Enumerable list of Points</returns>
        public static IEnumerable<Point> GetPointsFromSVGPathsString(string svgPathsString, int pointsPerBeizerCommand = 100)
        {
            List<Point> points = new List<Point>();
            var svgPaths = GetSVGCommandsFromSVGPathsString(svgPathsString);
            Point currentPoint = new Point(0, 0);

            foreach (var command in svgPaths)
            {
                switch (command.SVGCommandType)
                {
                    case SVGCommandType.M:
                        currentPoint = new Point(command.XYParams.FirstOrDefault(),
                                                 command.XYParams.Skip(1).FirstOrDefault());
                        points.Add(currentPoint);
                        break;
                    case SVGCommandType.c:
                    case SVGCommandType.C:
                        double dX = (command.SVGCommandType == SVGCommandType.c) ? currentPoint.X : 0;
                        double dY = (command.SVGCommandType == SVGCommandType.c) ? currentPoint.Y : 0;

                        Point p0 = currentPoint;
                        Point p1 = new Point(dX + command.XYParams.FirstOrDefault(),
                                             dY + command.XYParams.Skip(1).FirstOrDefault());
                        Point p2 = new Point(dX + command.XYParams.Skip(2).FirstOrDefault(),
                                             dY + command.XYParams.Skip(3).FirstOrDefault());
                        Point p3 = new Point(dX + command.XYParams.Skip(4).FirstOrDefault(),
                                             dY + command.XYParams.Skip(5).FirstOrDefault());

                        currentPoint = p3;
                        points.AddRange(SVGMath.CalculateCubicBeizer(p0, p1, p2, p3, pointsPerBeizerCommand));
                        break;
                }
            }

            return points;
        }

        /// <summary>
        /// Extract the SVG Path commands from an SVG Path string
        /// </summary>
        /// <param name="svgPathsString">SVG Path string</param>
        /// <returns>An enumerable list of commands and their parameters (e.g. SVGCommandType 'M', XY Parameters 100, 100)</returns>
        private static IEnumerable<SVGCommand> GetSVGCommandsFromSVGPathsString(string svgPathsString)
        {
            return Regex.Match(svgPathsString, "([a-zA-Z])([0-9|\\.|\\,|\\-]+)")
                        .GetMatches()
                        .Select(m => new SVGCommand()
                        {
                            SVGCommandType = (SVGCommandType)Enum.Parse(typeof(SVGCommandType), m.Groups[1].Value),
                            XYParams = Regex.Match(m.Groups[2].Value, "\\-?[0-9\\.]+")
                                            .GetMatches()
                                            .Select(p => Double.Parse(p.Value))
                        });
        }       
    }
}
