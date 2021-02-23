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
        public static List<SVGPathData> GetPointsFromSVG(Stream svgStream)
        {
            List<SVGPathData> pointsListResults = new List<SVGPathData>();

            // Load the XML
            var doc = new XmlDocument();
            doc.Load(svgStream);

            // Extract the SVG paths from the XML
            var pathTags = doc.GetElementsByTagName("path");
            for (int i = 0; i < pathTags.Count; i++)
            {
                string s = pathTags[i].Attributes.GetNamedItem("d").Value;
                pointsListResults.Add(svgpoints.SvgPoints.GetPointsFromSVGPathsString(s));
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
        public static List<SVGPathData> GetPointsFromSVG(string svgFilePath)
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
        public static SVGPathData GetPointsFromSVGPathsString(string svgPathsString, int pointsPerBeizerCommand = 100)
        {
            SVGPathData pathData = new SVGPathData();
            List<Point> points = new List<Point>();
            var svgPaths = GetSVGCommandsFromSVGPathsString(svgPathsString);
            Point currentPoint = new Point(0, 0);
            Point? p0 = null;
            Point? p1 = null;
            Point? p2 = null;
            Point? p3 = null;
            Point? q0 = null;
            Point? q1 = null;
            Point? q2 = null;

            foreach (var command in svgPaths)
            {
                // if lower case use delta values instead of absolute
                double dX = (char.IsLower(command.SVGCommandType.ToString().FirstOrDefault())) ? currentPoint.X : 0;
                double dY = (char.IsLower(command.SVGCommandType.ToString().FirstOrDefault())) ? currentPoint.Y : 0;
                bool keepQValues = false;
                bool keepPValues = false;

                // Handle commands
                switch (command.SVGCommandType)
                {
                    // Fix move, this needs to return an array of SVGPathData if multiple move commands
                    case SVGCommandType.m:
                    case SVGCommandType.M:
                    case SVGCommandType.l:
                    case SVGCommandType.L:
                        for (int i = 0; i < command.XYParams.Count(); i+=2)
                        {
                            currentPoint = new Point(dX + command.XYParams[i],
                                                     dY + command.XYParams[i+1]);

                            points.Add(currentPoint);
                        }
                        break;
                    case SVGCommandType.h:
                    case SVGCommandType.H:
                        for (int i = 0; i < command.XYParams.Count(); i++)
                        {
                            currentPoint = new Point(dX + command.XYParams[i],
                                                     currentPoint.Y);

                            points.Add(currentPoint);
                        }
                        break;
                    case SVGCommandType.v:
                    case SVGCommandType.V:
                        for (int i = 0; i < command.XYParams.Count(); i++)
                        {
                            currentPoint = new Point(currentPoint.X,
                                                     dX + command.XYParams[i]);

                            points.Add(currentPoint);
                        }
                        break;
                    case SVGCommandType.c:
                    case SVGCommandType.C:
                        keepPValues = true;

                        for (int i = 0; i < command.XYParams.Count(); i+=6)
                        {
                            p0 = currentPoint;
                            p1 = new Point(dX + command.XYParams[i+0],
                                           dY + command.XYParams[i+1]);
                            p2 = new Point(dX + command.XYParams[i+2],
                                           dY + command.XYParams[i+3]);
                            p3 = new Point(dX + command.XYParams[i+4],
                                           dY + command.XYParams[i+5]);

                            currentPoint = p3.Value;
                            points.AddRange(SVGMath.CalculateCubicBeizer(p0.Value, p1.Value, p2.Value, p3.Value, pointsPerBeizerCommand));
                        }
                        break;
                    case SVGCommandType.s:
                    case SVGCommandType.S:
                        keepPValues = true;

                        for (int i = 0; i < command.XYParams.Count(); i += 4)
                        {
                            p0 = currentPoint;
                            // P1 is calculatd based off of the parameters for the previous curve
                            p1 = !p2.HasValue ? currentPoint
                                              : new Point((p3.Value.X*2)-p2.Value.X,
                                                          (p3.Value.Y*2)-p2.Value.Y);
                            p2 = new Point(dX + command.XYParams[i + 0],
                                           dY + command.XYParams[i + 1]);
                            p3 = new Point(dX + command.XYParams[i + 2],
                                           dY + command.XYParams[i + 3]);

                            currentPoint = p3.Value;
                            points.AddRange(SVGMath.CalculateCubicBeizer(p0.Value, p1.Value, p2.Value, p3.Value, pointsPerBeizerCommand));
                        }
                        break;
                    case SVGCommandType.q:
                    case SVGCommandType.Q:
                        keepQValues = true;

                        for (int i = 0; i < command.XYParams.Count(); i += 4)
                        {
                            q0 = currentPoint;
                            q1 = new Point(dX + command.XYParams[i + 0],
                                           dY + command.XYParams[i + 1]);
                            q2 = new Point(dX + command.XYParams[i + 2],
                                           dY + command.XYParams[i + 3]);

                            currentPoint = q2.Value;
                            points.AddRange(SVGMath.CalculateQuadraticBeizer(q0.Value, q1.Value, q2.Value, pointsPerBeizerCommand));
                        }
                        break;
                    case SVGCommandType.t:
                    case SVGCommandType.T:
                        keepQValues = true;

                        for (int i = 0; i < command.XYParams.Count(); i += 2)
                        {
                            q0 = currentPoint;
                            q1 = !q2.HasValue ? currentPoint
                                              : new Point((q2.Value.X * 2) - q1.Value.X,
                                                          (q2.Value.Y * 2) - q1.Value.Y);
                            q2 = new Point(dX + command.XYParams[i + 0],
                                           dY + command.XYParams[i + 1]);

                            currentPoint = q2.Value;
                            points.AddRange(SVGMath.CalculateQuadraticBeizer(q0.Value, q1.Value, q2.Value, pointsPerBeizerCommand));
                        }
                        break;
                    case SVGCommandType.z:
                    case SVGCommandType.Z:
                        pathData.IsClosed = true;
                        break;
                    case SVGCommandType.invalid:
                        throw new InvalidOperationException($"Invalid SVG Command {command.SVGCommandString}");
                }

                // Get rid of Q values if there is a non quadratic beizer command run in between quadratic beizer commands
                if (!keepQValues)
                {
                    q0 = null;
                    q1 = null;
                    q2 = null;
                }

                // Get rid of P values if there is a non cubic beizer command run in between cubic beizer commands
                if (!keepPValues)
                {
                    p0 = null;
                    p1 = null;
                    p2 = null;
                    p3 = null;
                }
            }

            pathData.Points = points;

            return pathData;
        }

        /// <summary>
        /// Extract the SVG Path commands from an SVG Path string
        /// </summary>
        /// <param name="svgPathsString">SVG Path string</param>
        /// <returns>An enumerable list of commands and their parameters (e.g. SVGCommandType 'M', XY Parameters 100, 100)</returns>
        private static IEnumerable<SVGCommand> GetSVGCommandsFromSVGPathsString(string svgPathsString)
        {
            return Regex.Match(svgPathsString, "([a-zA-Z])([0-9|\\.|\\,|\\- ]+)")
                        .GetMatches()
                        .Select(m => new SVGCommand()
                        {
                            SVGCommandString = m.Groups[1].Value,
                            SVGCommandType = EnumExtensions.NullableEnumTryParseOrDefault<SVGCommandType>(m.Groups[1].Value) ?? SVGCommandType.invalid,
                            XYParams = Regex.Match(m.Groups[2].Value, "\\-?[0-9\\.]+")
                                            .GetMatches()
                                            .Select(p => Double.Parse(p.Value))
                                            .ToList()
                        });
        }       
    }
}
