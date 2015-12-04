/*
 * Copyright (c) 2010-2012 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#if NUNITY
using Vector2 = org.critterai.Vector2;
#else
using Vector2 = UnityEngine.Vector2;
#endif

namespace org.critterai.geom
{
    /// <summary>
    /// Provides various 2D triangle utility methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Static methods are thread safe.
    /// </para>
    /// </remarks>
    public static class Triangle2
    {
        /// <summary>
        /// Returns true if the point is contained by the triangle.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The test is inclusive of the triangle edges.
        /// </para>
        /// </remarks>
        /// <param name="p">The point to test.</param>
        /// <param name="a">Vertex A of triangle ABC.</param>
        /// <param name="b">Vertex B of triangle ABC</param>
        /// <param name="c">Vertex C of triangle ABC</param>
        /// <returns>True if the point is contained by the triangle ABC.</returns>
        public static bool Contains(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 dirAB = b - a;
            Vector2 dirAC = c - a;
            Vector2 dirAP = p - a;

            float dotABAB = Vector2Util.Dot(dirAB, dirAB);
            float dotACAB = Vector2Util.Dot(dirAC, dirAB);
            float dotACAC = Vector2Util.Dot(dirAC, dirAC);
            float dotACAP = Vector2Util.Dot(dirAC, dirAP);
            float dotABAP = Vector2Util.Dot(dirAB, dirAP);

            float invDenom = 1 / (dotACAC * dotABAB - dotACAB * dotACAB);
            float u = (dotABAB * dotACAP - dotACAB * dotABAP) * invDenom;
            float v = (dotACAC * dotABAP - dotACAB * dotACAP) * invDenom;

            // Altered this slightly from the reference so that points on the 
            // vertices and edges are considered to be inside the triangle.
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        /// <summary>
        /// The absolute value of the returned value is two times the area of the triangle ABC.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A positive return value indicates:
        /// </para>
        /// <ul>
        /// <li>Counterclockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the right of line AC, looking from A toward C.</li>
        /// </ul>
        /// <para>A negative value indicates:</para>
        /// <ul>
        /// <li>Clockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the left of line AC, looking from A toward C.</li>
        /// </ul>
        /// <para>
        /// A value of zero indicates that all points are collinear or represent the same point.
        /// </para>
        /// <para>
        /// This is a low cost method.
        /// </para>
        /// </remarks>
        /// <param name="a">Vertex A of triangle ABC.</param>
        /// <param name="b">Vertex B of triangle ABC</param>
        /// <param name="c">Vertex C of triangle ABC</param>
        /// <returns>
        /// The absolute value of the returned value is two times the area of the triangle ABC.
        /// </returns>
        public static float GetSignedAreaX2(Vector2 a, Vector2 b, Vector2 c)
        {
            // References:
            // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm#Modern%20Triangles
            // http://mathworld.wolfram.com/TriangleArea.html 
            // (Search for "signed".)
            return (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
        }

        /// <summary>
        /// The absolute value of the returned value is two times the area of the triangle ABC.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A positive return value indicates:
        /// </para>
        /// <ul>
        /// <li>Counterclockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the right of line AC, looking from A toward C.</li>
        /// </ul>
        /// <para>A negative value indicates:</para>
        /// <ul>
        /// <li>Clockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the left of line AC, looking from A toward C.</li>
        /// </ul>
        /// <para>
        /// A value of zero indicates that all points are collinear or represent the same point.
        /// </para>
        /// <para>
        /// This is a low cost method.
        /// </para>
        /// </remarks>
        /// <param name="ax">The x-value for vertex A of triangle ABC</param>
        /// <param name="ay">The y-value for vertex A of triangle ABC</param>
        /// <param name="bx">The x-value for vertex B of triangle ABC</param>
        /// <param name="by">The y-value for vertex B of triangle ABC</param>
        /// <param name="cx">The x-value for vertex C of triangle ABC</param>
        /// <param name="cy">The y-value for vertex C of triangle ABC</param>
        /// <returns>The absolute value of the returned value is two times the
        /// area of the triangle ABC.</returns>
        public static float GetSignedAreaX2(float ax, float ay
            , float bx, float by
            , float cx, float cy)
        {
            // Note: Keep this around for use by Vector3.
            return (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
        }

        /// <summary>
        /// The absolute value of the returned value is two times the area of the triangle ABC.
        /// (Integer version.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// A positive return value indicates:
        /// </para>
        /// <ul>
        /// <li>Counterclockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the right of line AC, looking from A toward C.</li>
        /// </ul>
        /// <para>
        /// A negative value indicates:
        /// </para>
        /// <ul>
        /// <li>Clockwise wrapping of the vertices.</li>
        /// <li>Vertex B lies to the left of line AC, looking from A toward C.</li>
        /// </ul>
        /// <para>
        /// A value of zero indicates that all points are collinear orrepresent the same point.
        /// </para>
        /// <para>
        /// This is a low cost method.
        /// </para>
        /// </remarks>
        /// <param name="ax">The x-value for vertex A of triangle ABC</param>
        /// <param name="ay">The y-value for vertex A of triangle ABC</param>
        /// <param name="bx">The x-value for vertex B of triangle ABC</param>
        /// <param name="by">The y-value for vertex B of triangle ABC</param>
        /// <param name="cx">The x-value for vertex C of triangle ABC</param>
        /// <param name="cy">The y-value for vertex C of triangle ABC</param>
        /// <returns>
        /// The absolute value of the returned value is two times the area of the triangle ABC.
        /// </returns>
        public static int GetSignedAreaX2(int ax, int ay
            , int bx, int by
            , int cx, int cy)
        {
            return (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
        }
    }
}
