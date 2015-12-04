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
    /// Represents the solid (wall) polygon segments in the vicinity of a reference point.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instances of this class are required by <see cref="CrowdAgent.GetBoundary"/>.
    /// </para>
    /// <para>
    /// This class is used as an interop buffer.  Behavior is undefined if the size of the array 
    /// fields are changed after construction.
    /// </para>
    /// </remarks>
    /// <seealso cref="CrowdAgent.GetBoundary"/>
    /// <seealso cref="CrowdAgent"/>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class LocalBoundaryData
    {
        /*
         * Design note:
         * 
         * Implemented as a class to permit use as a buffer.
         * 
         */

        /// <summary>
        /// The maximum allowed segments.
        /// </summary>
        /// <remarks>Used to size the <see cref="segments"/> buffer.</remarks>
        public const int MaxSegments = 8;

        /// <summary>
        /// The reference point for which the boundary data was compiled. 
        /// </summary>
        public Vector3 center;

        /// <summary>
        /// The solid navigation mesh polygon segments in the vicinity of <see cref="center"/>.
        /// [(vertA, vertB) * <see cref="segmentCount"/>]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSegments * 2)]
        public Vector3[] segments = new Vector3[MaxSegments * 2];

        /// <summary>
        /// The number of segments in the <see cref="segments"/> field.
        /// </summary>
        public int segmentCount = 0;

        /// <summary>
        /// Creates an instance with properly sized buffers.
        /// </summary>
        public LocalBoundaryData() { }
    }
}
