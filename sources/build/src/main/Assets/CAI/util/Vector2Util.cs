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
using Vector2 = org.critterai.Vector2;
#else
using Vector2 = UnityEngine.Vector2;
#endif

namespace org.critterai
{
    /// <summary>
    /// Provides various 2D vector utility methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Static methods are thread safe.
    /// </para>
    /// </remarks>
    public static class Vector2Util
    {
        private const float Epsilon = MathUtil.Epsilon;

        /// <summary>
        /// The zero vector. (0, 0)
        /// </summary>
        public static Vector2 Zero { get { return new Vector2(0, 0); } }

        /// <summary>
        /// Returns the <a href="http://en.wikipedia.org/wiki/Dot_product" target="_blank">
        /// dot product</a> of the specified vectors. (u . v)
        /// </summary>
        /// <param name="u">Vector u</param>
        /// <param name="v">Vector v</param>
        /// <returns>The dot product of the specified vectors.</returns>
        public static float Dot(Vector2 u, Vector2 v)
        {
            return (u.x * v.x) + (u.y * v.y);
        }

        /// <summary>
        /// Derives the normalized direction vector from point A to point B. (Costly method!)
        /// </summary>
        /// <param name="a">The starting point A.</param>
        /// <param name="b">The end point B.</param>
        /// <returns>
        /// The normalized direction vector for the vector pointing from point A to B.
        /// </returns>
        public static Vector2 GetDirectionAB(Vector2 a, Vector2 b)
        {
            // Subtract.
            Vector2 result = new Vector2(b.x - a.x, b.y - a.y);
            
            // Normalize.
            float length = (float)Math.Sqrt((result.x * result.x) + (result.y * result.y));

            if (length <= Epsilon) 
                length = 1;
            
            result.x /= length;
            result.y /= length;
            
            if (Math.Abs(result.x) < Epsilon) 
                result.x = 0;
            if (Math.Abs(result.y) < Epsilon) 
                result.y = 0;    
            
            return result;
        }

        /// <summary>
        /// Determines whether or not the specified vectors are equal within the specified tolerance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Change in beahvior:  Prior to version 0.4, the area of equality  for this method was 
        /// an axis-aligned bounding box at the tip of the vector. As of version 0.4 the area of 
        /// equality is a sphere. This change improves performance.
        /// </para>
        /// </remarks>
        /// <param name="u">Vector u.</param>
        /// <param name="v">Vector v</param>
        /// <param name="tolerance">The allowed tolerance. [Limit: >= 0]
        /// </param>
        /// <returns>
        /// True if the specified vectors are similar enough to be considered equal.
        /// </returns>
        public static bool SloppyEquals(Vector2 u, Vector2 v, float tolerance)
        {
            // Duplicating code for performance reasons.
            float dx = u.x - v.x;
            float dy = u.y - v.y;
            return (dx * dx + dy * dy) <= tolerance * tolerance;
        }

        /// <summary>
        /// Returns the square of the distance between two points.
        /// </summary>
        /// <param name="a">Point A.</param>
        /// <param name="b">Point B.</param>
        /// <returns>The square of the distance between the points.</returns>
        public static float GetDistanceSq(Vector2 a, Vector2 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return (dx * dx + dy * dy);
        }

        /// <summary>
        /// Normalizes the specified vector such that its length is equal to one. (Costly method!)
        /// </summary>
        /// <param name="v">A vector.</param>
        /// <returns>The normalized vector.</returns>
        public static Vector2 Normalize(Vector2 v) 
        {
            float length = (float)Math.Sqrt(v.x * v.x + v.y * v.y);

            if (length <= Epsilon) 
                length = 1;

            v.x /= length;
            v.y /= length;

            if (Math.Abs(v.x) < Epsilon)
                v.x = 0;
            if (Math.Abs(v.y) < Epsilon)
                v.y = 0;

            return v;
        }

        /// <summary>
        /// Scales the vector to the specified length. (Costly method!)
        /// </summary>
        /// <param name="v">A vector.</param>
        /// <param name="length">The length to scale the vector to.</param>
        /// <returns>A vector scaled to the specified length.</returns>
        public static Vector2 ScaleTo(Vector2 v, float length) 
        {
            if (length == 0 || (v.x == 0 && v.y == 0))
                return new Vector2(0, 0);

            float factor = (length / (float)(Math.Sqrt(v.x * v.x + v.y * v.y)));

            v.x *= factor;
            v.y *= factor;

            return v;
        }

        /// <summary>
        /// Truncates the length of the vector to the specified value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the vector's length is longer than the specified value the length of the vector is 
        /// scaled back to the specified length.
        /// </para>
        /// <para>
        /// If the vector's length is shorter than the specified value, it is not changed.
        /// </para>
        /// <para>
        /// This is a potentially costly method.
        /// </para>
        /// </remarks>
        /// <param name="v">The vector to truncate.</param>
        /// <param name="maxLength">The maximum allowed length of the resulting vector.</param>
        /// <returns>A vector with a length at or below the maximum length.</returns>
        public static Vector2 TruncateLength(Vector2 v, float maxLength) 
        {
            if (maxLength == 0 || (v.x < float.Epsilon && v.y < float.Epsilon))
                return new Vector2(0, 0);

            float csq = v.x * v.x + v.y * v.y;

            if (csq <= maxLength * maxLength)
                return v;

            float factor = (float)(maxLength / Math.Sqrt(csq));

            v.x *= factor;
            v.y *= factor;

            return v;
        }
    }
}
