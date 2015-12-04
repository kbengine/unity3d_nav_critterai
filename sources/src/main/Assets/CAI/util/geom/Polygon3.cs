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
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.geom
{
    /// <summary>
    /// Provides various 3D polygon utility methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unless otherwise noted, methods expect all polygon vertices to be co-planar.
    /// </para>
    /// <para>
    /// Static methods are thread safe.
    /// </para>
    /// </remarks>
    public static class Polygon3
    {
        /// <summary>
        /// Determines whether a polygon is convex.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if vertices are not coplanar.
        /// </para>
        /// <para>
        /// This method may improperly return false if the area of the triangle formed 
        /// by the first three vertices of the polygon is too small to detect on both the (x, z) 
        /// and (x, y) planes.
        /// </para>
        /// </remarks>
        /// <param name="vertices">
        /// An array of vertices that contains a representation of polygons with an  arbitrary 
        /// number of sides. Wrap direction does not matter.
        /// </param>
        /// <param name="startVert">The index of the first vertex in the polygon.</param>
        /// <param name="vertCount">The number of vertices in the polygon.</param>
        /// <returns>True if the polygon is convex.</returns>
        public static bool IsConvex(Vector3[] vertices, int startVert, int vertCount)
        {  
            if (vertCount < 3)
                return false;
            if (vertCount == 3)
                // It is a triangle, so it must be convex.
                return true;  
            
            // At this point we know that the polygon has at least 4 sides.
            
            /*
             *  Process will be to step through the sides, 3 vertices at a time.
             *  As long the signed area for the triangles formed by each set of
             *  vertices is the same (negative or positive), then the polygon 
             *  is convex.
             *  
             *  Using a shortcut by projecting onto the (x, z) or (x, y) plane 
             *  for all calculations. For a proper polygon, if the 2D 
             *  projection is convex, the 3D polygon is convex.
             *  
             *  There is one special case: A polygon that is vertical.  
             *  I.e. 2D on the (x, z) plane.
             *  This is detected during the first test.
             */

            bool offset = true;  // Start by projecting to the (x, z) plane.
            
            float initDirection = Triangle2.GetSignedAreaX2(
                vertices[startVert + 0].x
                , vertices[startVert + 0].z
                , vertices[startVert + 1].x
                , vertices[startVert + 1].z
                , vertices[startVert + 2].x
                , vertices[startVert + 2].z);
            
            if (initDirection > -2 * MathUtil.Epsilon 
                    && initDirection < 2 * MathUtil.Epsilon)
            {
                // The polygon is on or very close to the vertical plane.  
                // Switch to projecting on the (x, y) plane.
                offset = false;

                initDirection = Triangle2.GetSignedAreaX2(
                    vertices[startVert + 0].x
                    , vertices[startVert + 0].y
                    , vertices[startVert + 1].x
                    , vertices[startVert + 1].y
                    , vertices[startVert + 2].x
                    , vertices[startVert + 2].y);

                // Design note: This is meant to be a strict zero test.
                if (initDirection == 0)
                    // Some sort of problem.  Should very rarely ever get here.
                    return false;  
            }
            
            int length = (startVert + vertCount);
            for (int iVertA = startVert + 1; iVertA < length; iVertA++)
            {
                int iVertB = iVertA + 1;

                if (iVertB >= length) 
                    // Wrap it back to the start.
                    iVertB = startVert;  

                int iVertC = iVertB + 1;

                if (iVertC >= length)
                    // Wrap it back to the start.
                    iVertC = startVert;

                float direction = Triangle2.GetSignedAreaX2(
                        vertices[iVertA].x
                        , offset ? vertices[iVertA].z : vertices[iVertA].y
                        , vertices[iVertB].x
                        , offset ? vertices[iVertB].z : vertices[iVertB].y
                        , vertices[iVertC].x
                        , offset ? vertices[iVertC].z : vertices[iVertC].y);

                if (!(initDirection < 0 && direction < 0) && !(initDirection > 0  && direction > 0))
                    // The sign of the current direction is not the same as the sign of the 
                    // initial direction.  Can't be convex.
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Returns the <a href="http://en.wikipedia.org/wiki/Centroid" target="_blank">
        /// centroid</a> of a convex polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if the polygon is not convex.
        /// </para>
        /// <para>
        /// Behavior is undefined if the vector being overwritten in the out array is a vertex 
        /// in the polygon.  (Can only happen if the vertices and result arrays are the same object.)
        /// </para>
        /// </remarks>
        /// <param name="vertices">
        /// An array of vertices that contains a representation of a polygon with an  arbitrary 
        /// number of sides.  Wrap direction does not matter.
        /// </param>
        /// <param name="startVert">The index of the first vertex in the polygon.</param>
        /// <param name="vertCount">The number of vertices in the polygon.</param>
        /// <param name="result">The array to store the result in.</param>
        /// <param name="resultVert">The index in the result array to store the result.</param>
        /// <returns>A reference to the result argument.</returns>
        public static Vector3[] GetCentroid(Vector3[] vertices
                , int startVert
                , int vertCount
                , Vector3[] result
                , int resultVert)
        {
            // Reference: 
            // http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
            
            result[resultVert] = new Vector3(0, 0, 0);
            int length = (startVert+vertCount);

            for (int i = startVert; i < length; i++)
            {
                result[resultVert] += vertices[i];
            }

            result[resultVert].x /= vertCount;
            result[resultVert].y /= vertCount;
            result[resultVert].z /= vertCount;
            
            return result;
        }
        
        /// <summary>
        /// Returns the <a href="http://en.wikipedia.org/wiki/Centroid" target="_blank">
        /// centroid</a> of a convex polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if the polygon is not convex.
        /// </para>
        /// </remarks>
        /// <param name="vertices">
        /// An array of vertices that contains a representation of a polygon with an arbitrary 
        /// number of sides. Wrap direction does not matter.
        /// </param>
        /// <param name="startVert">The index of the first vertex in the polygon.</param>
        /// <param name="vertCount">The number of vertices in the polygon.</param>
        /// <returns>The centroid of the polygon.</returns>
        public static Vector3 GetCentroid(Vector3[] vertices, int startVert, int vertCount)
        {
            // Reference:
            // http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
            

            Vector3 result = new Vector3();
            int vertLength = (startVert + vertCount);

            for (int i = startVert; i < vertLength; i++)
            {
                result += vertices[i];
            }

            result.x /= vertCount;
            result.y /= vertCount;
            result.z /= vertCount;
            
            return result;
        }
        
        /// <summary>
        /// Returns the  <a href="http://en.wikipedia.org/wiki/Centroid" target="_blank">
        /// centroid</a> of a convex polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior is undefined if the polygon is not convex.
        /// </para>
        /// </remarks>
        /// <param name="vertices">
        /// An list of vertices that represent a  polygon with an arbitrary number of sides. Wrap 
        /// direction does not matter.
        /// </param>
        /// <returns>The centroid of the polygon.</returns>
        public static Vector3 GetCentroid(params Vector3[] vertices)
        {
            // Reference: 
            // http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
            
            Vector3 result = new Vector3();
            
            int vertCount = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                result += vertices[i];
                vertCount++;
            }

            result.x /= vertCount;
            result.y /= vertCount;
            result.z /= vertCount;

            return result;
        }
    }
}
