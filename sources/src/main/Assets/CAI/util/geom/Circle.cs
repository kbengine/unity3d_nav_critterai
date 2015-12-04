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
    /// Provides circle related utility methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Static methods are thread safe.
    /// </para>
    /// </remarks>
    public static class Circle
    {
        /// <summary>
        /// Determines whether or not two circles intersect each other.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Test is inclusive of the circle boundary.
        /// </para>
        /// <para>
        /// Containment of one circle by another is considered intersection.
        /// </para>
        /// </remarks>
        /// <param name="acenter">The center point of circle A.</param>
        /// <param name="aradius">The radius of circle A.</param>
        /// <param name="bcenter">The center point of circle B.</param>
        /// <param name="bradius">The radius of Circle B.</param>
        /// <returns>True if the circles intersect.</returns>
        public static bool Intersects(Vector2 acenter, float aradius
            , Vector2 bcenter, float bradius)
        {
            float dx = acenter.x - bcenter.x;
            float dy = acenter.y - bcenter.y;
            return (dx * dx + dy * dy) <= (aradius + bradius) * (aradius + bradius);
        }

        /// <summary>
        /// Determines whether or not a point is contained within a circle.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The test is inclusive of the circle boundary.
        /// </para>
        /// </remarks>
        /// <param name="point">The point to test.</param>
        /// <param name="circle">The center point of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns>True if the point is contained within the circle.</returns>
        public static bool Contains(Vector2 point, Vector2 circle, float radius)
        {
            float dx = point.x - circle.x;
            float dy = point.y - circle.y;
            return (dx * dx + dy * dy) <= radius * radius;
        }
    }
}
