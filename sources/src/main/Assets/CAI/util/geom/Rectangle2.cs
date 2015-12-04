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
    /// Provides various 2D rectangle utility methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Static methods are thread safe.
    /// </para>
    /// </remarks>
    public static class Rectangle2
    {
        /* 
         * Design note:
         * 
         * This class does not use Vector2's since a common use case is to test
         * on the xz-plane using Vector3's.
         * 
         */
        
        /// <summary>
        /// Indicates whether or not a point is contained within an axis-aligned rectangle. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The test is inclusive of the edges of the rectangle.
        /// </para>
        /// </remarks>
        /// <param name="minX">The minimum x-axis bounds of the rectangle.</param>
        /// <param name="minY">The minimum y-axis bounds of the rectangle.</param>
        /// <param name="maxX">The maximum x-axis bounds of the rectangle.</param>
        /// <param name="maxY">The maximum y-axis bounds of the rectangle.</param>
        /// <param name="x">x-value of the point (x, y) to test.</param>
        /// <param name="y">y-value of the point (x, y) to test.</param>
        /// <returns>True if the point lies within the rectangle.</returns>
        public static bool Contains(float minX, float minY, float maxX, float maxY
            , float x, float y)
        {
            return !(x < minX || y < minY || x > maxX || y > maxY);
        }

        /// <summary>
        /// Indicates whether or not an axis-alighed rectangle (B) is fully contained within 
        /// another axis-aligned rectangle (A).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The test is inclusive of the edges of the rectangles.
        /// </para>
        /// </remarks>
        /// <param name="minXA">The minimum x-axis bounds of rectangle A.</param>
        /// <param name="minYA">The minimum y-axis bounds of rectangle A.</param>
        /// <param name="maxXA">The maximum x-axis bounds of rectangle A.</param>
        /// <param name="maxYA">The maximum y-axis bounds of rectangle A.</param>
        /// <param name="minXB">The minimum x-axis bounds of rectangle B.</param>
        /// <param name="minYB">The minimum y-axis bounds of rectangle B.</param>
        /// <param name="maxXB">The maximum x-axis bounds of rectangle B.</param>
        /// <param name="maxYB">The maximum y-axis bounds of rectangle B.</param>
        /// <returns>True if rectangle B is fully contained by rectangle A.  
        /// </returns>
        public static bool Contains(float minXA, float minYA, float maxXA, float maxYA
            , float minXB, float minYB, float maxXB, float maxYB)
        {
            return (minXB >= minXA && minYB >= minYA && maxXB <= maxXA && maxYB <= maxYA);
        }

        /// <summary>
        /// Indicates whether or not two axis-aligned rectangles intersect. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The test is inclusive of the edges of the rectangles.
        /// </para>
        /// </remarks>
        /// <param name="minAX">The minimum x-axis bounds of rectangle A.</param>
        /// <param name="minAY">The minimum y-axis bounds of rectangle A.</param>
        /// <param name="maxAX">The maximum x-axis bounds of rectangle A.</param>
        /// <param name="maxAY">The maximum y-axis bounds of rectangle A.</param>
        /// <param name="minBX">The minimum x-axis bounds of rectangle B.</param>
        /// <param name="minBY">The minimum y-axis bounds of rectangle B.</param>
        /// <param name="maxBX">The maximum x-axis bounds of rectangle B.</param>
        /// <param name="maxBY">The maximum y-axis bounds of rectangle B.</param>
        /// <returns>True if the two rectangles intersect in any manner.</returns>
        public static bool IntersectsAABB(float minAX, float minAY, float maxAX, float maxAY
            , float minBX, float minBY, float maxBX, float maxBY)
        {
            return !(maxBX < minAX || maxAX < minBX || maxBY < minAY || maxAY < minBY);
        }
    }
}
