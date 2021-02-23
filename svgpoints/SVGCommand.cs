using System;
using System.Collections.Generic;
using System.Text;

namespace svgpoints
{
    /// <summary>
    /// All SVG command types in a SVG Path string
    /// </summary>
    internal enum SVGCommandType { M, m, L, l, H, h, V, v, C, c, S, s, Q, q, T, t, A, a, Z, z, invalid };
    /// <summary>
    /// Represents an SVG Command with its parameters
    /// </summary>
    internal class SVGCommand
    {
        internal string SVGCommandString { get; set; }
        /// <summary>
        /// The SVG Command (e.g. M, L, C, Q, Z, etc)
        /// </summary>
        internal SVGCommandType SVGCommandType { get; set; }
        /// <summary>
        /// The parameters passed to the SVG Command (e.g. 100, 100).  Number
        /// of parameters is variable depending on the SVG command type.
        /// </summary>
        internal List<double> XYParams { get; set; }
    }
}
