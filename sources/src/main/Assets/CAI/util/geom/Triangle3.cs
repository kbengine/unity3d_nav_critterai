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
using System;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.geom
{
    /// <summary>
    /// Provides various 3D triangle utility methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Static methods are thread safe.
    /// </para>
    /// </remarks>
    public static class Triangle3 
    {
        /// <summary>
        /// Returns the area of the triangle ABC. (Costly method!)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use <see cref="GetAreaComp">GetAreaComp</see> if the value is only needed 
        /// for comparison with other triangles.
        /// </para>
        /// </remarks>
        /// <param name="a">Vertex A of triangle ABC.</param>
        /// <param name="b">Vertex B of triangle ABC.</param>
        /// <param name="c">Vertex C of triangle ABC.</param>
        /// <returns>The area of the triangle ABC.</returns>
        public static float GetArea(Vector3 a, Vector3 b, Vector3 c)
        {
            return (float)(Math.Sqrt(GetAreaComp(a, b, c)) / 2);
        }
        
        /// <summary>
        /// Returns a value suitable for comparing the relative area of two triangles. (E.g. 
        /// Is triangle A larger than triangle B.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value returned by this method can be converted to an area as follows: 
        /// <c>Area = Math.sqrt(value) / 2</c>
        /// </para>
        /// <para>
        /// Useful for cheaply comparing the size of triangles.
        /// </para>
        /// </remarks>
        /// <param name="a">Vertex A of triangle ABC.</param>
        /// <param name="b">Vertex B of triangle ABC.</param>
        /// <param name="c">Vertex C of triangle ABC.</param>
        /// <returns>A value suitable for comparing the relative area of two triangles.</returns>
        public static float GetAreaComp(Vector3 a, Vector3 b, Vector3 c)
        {
            // References:
            // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm#Modern%20Triangles
            
            // Get directional vectors.
            
            Vector3 u = b - a;  // A -> B
            Vector3 v = c - a;  // A -> C

            // Cross product.
            Vector3 n = new Vector3(u.y * v.z - u.z * v.y
                , -u.x * v.z + u.z * v.x
                , u.x * v.y - u.y * v.x);
            
            return Vector3Util.GetLengthSq(n);
        }
        
        /// <summary>
        /// Returns the normal for the  triangle. (Costly method!)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The normal of a triangle is the vector perpendicular to the triangle's plane 
        /// with the direction determined by the  
        /// <a href="http://en.wikipedia.org/wiki/Right-hand_rule" target="_blank">
        /// right-handed rule</a>.
        /// </para>
        /// </remarks>
        /// <param name="a">Vertex A of triangle ABC.</param>
        /// <param name="b">Vertex B of triangle ABC.</param>
        /// <param name="c">Vertex C of triangle ABC.</param>
        /// <returns>The normal of the  triangle.</returns>
        public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            // Reference: 
            // http://en.wikipedia.org/wiki/Surface_normal#Calculating_a_surface_normal
            // N = (B - A) x (C - A) with the final result normalized.
             
            return Vector3Util.Normalize(Vector3Util.Cross(b - a, c - a));
        }

        /// <summary>
        /// Returns the normal for the  triangle. (Costly method!)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The normal of a triangle is the vector perpendicular to the triangle's plane with
        /// the direction determined by the  <a href="http://en.wikipedia.org/wiki/Right-hand_rule"
        /// target="_blank">right-handed rule</a>.
        /// </para>
        /// </remarks>
        /// <param name="vertices">
        /// An array of vertices which contains a representation of triangles. The wrap direction 
        /// is  expected to be clockwise.
        /// </param>
        /// <param name="triangle">The index of the first vertex in the triangle.</param>
        /// <returns>The normal of the triangle.</returns>
        public static Vector3 GetNormal(Vector3[] vertices, int triangle)
        {
            return GetNormal(vertices[triangle], vertices[triangle + 1], vertices[triangle + 2]);
        }
    }
}
