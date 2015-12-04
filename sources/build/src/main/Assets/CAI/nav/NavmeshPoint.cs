/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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
using System.Runtime.InteropServices;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// A point within a navigation mesh.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshPoint
    {
        /// <summary>
        /// The reference of the polygon the contains the point. (Or zero if not known.)
        /// </summary>
        public uint polyRef;

        /// <summary>
        /// The location of the point.
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="polyRef">
        /// The reference of the polygon that contains the point. (Or zero if not known.)
        /// </param>
        /// <param name="point">The location of the point.</param>
        public NavmeshPoint(uint polyRef, Vector3 point)
        {
            this.polyRef = polyRef;
            this.point = point;
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="v">A navmesh point.</param>
        /// <param name="u">A navmesh point.</param>
        /// <returns>True if the points are equal.</returns>
        public static bool operator ==(NavmeshPoint v, NavmeshPoint u)
        {
            return (v.point == u.point && v.polyRef == u.polyRef);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="v">A navmesh point.</param>
        /// <param name="u">A navmesh point.</param>
        /// <returns>True if the points are not equal.</returns>
        public static bool operator !=(NavmeshPoint v, NavmeshPoint u)
        {
            return !(v.point == u.point && v.polyRef == u.polyRef);
        }

        /// <summary>
        /// The navmesh point hash code.
        /// </summary>
        /// <returns>The navmesh point hash code.</returns>
        public override int GetHashCode()
        {
            int result = 17;
            result = 31 * result + point.x.GetHashCode();
            result = 31 * result + point.y.GetHashCode();
            result = 31 * result + point.z.GetHashCode();
            result = 31 * result + polyRef.GetHashCode();
            return result;
        }

        /// <summary>
        /// Tests the navmesh point for equality.
        /// </summary>
        /// <param name="obj">The point to compare.</param>
        /// <returns>True if each element of the point is equal to this point.</returns>
        public override bool Equals(object obj)
        {
            if (obj is NavmeshPoint)
            {
                NavmeshPoint u = (NavmeshPoint)obj;
                return (point == u.point && polyRef == u.polyRef);
            }
            return false;
        }

        /// <summary>
        /// Returns a standard string representation of point.
        /// </summary>
        /// <returns>A standard string representation of point.</returns>
        public override string ToString()
        {
            return string.Format("[{0:F3}, {1:F3}, {2:F3}] (Ref: {3})"
                , point.x, point.y, point.z, polyRef);
        }

        /// <summary>
        /// The zero point.
        /// </summary>
        public static NavmeshPoint Zero
        {
            get { return new NavmeshPoint(0, Vector3Util.Zero); }
        }

        /// <summary>
        /// Creates an array of vectors from the provided navmesh points.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A new array will be created if the <paramref name="target"/> array is null.
        /// </para>
        /// </remarks>
        /// <param name="source">The source array.</param>
        /// <param name="sourceIndex">The start of the copy in the source.</param>
        /// <param name="target">The target of the copy. (Optional)</param>
        /// <param name="targetIndex">The start copy location within the target.</param>
        /// <param name="count">The number of vectors to copy.</param>
        /// <returns>
        /// An array containing the copied vectors. (A reference to <paramref name="target"/>
        /// if it was non-null.)
        /// </returns>
        public static Vector3[] GetPoints(NavmeshPoint[] source, int sourceIndex
            , Vector3[] target, int targetIndex
            , int count)
        {
            if (target == null)
                target = new Vector3[source.Length + targetIndex];

            for (int i = 0; i < count; i++)
            {
                target[targetIndex + i] = source[sourceIndex + i].point;
            }

            return target;
        }
    }
}
