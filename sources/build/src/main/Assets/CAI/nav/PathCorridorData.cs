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
using System;
using System.Runtime.InteropServices;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Path corridor data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used as a marshalling buffer for corridor data.
    /// </para>
    /// <para>
    /// Certain methods that take objects of this type require a fixed buffer size equal to 
    /// <see cref="MarshalBufferSize"/>.  So be careful when initializing and using objects of 
    /// this type.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class PathCorridorData
    {
        /// <summary>
        /// The required maximum path size required to use with interop
        /// method calls.
        /// </summary>
        public const int MarshalBufferSize = 256;

        /// <summary>
        /// The current position within the path corridor.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The target position within the path corridor.
        /// </summary>
        public Vector3 target;

        /// <summary>
        /// An ordered list of polygon references representing the corridor.
        /// [(polyRef) * <see cref="pathCount"/>]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarshalBufferSize)]
        public uint[] path;

        /// <summary>
        /// The number of polygons in the path.
        /// </summary>
        public int pathCount;

        /// <summary>
        /// Creates an object with buffers sized for use with interop method calls. 
        /// (Maximum Path Size = <see cref="MarshalBufferSize"/>)
        /// </summary>
        public PathCorridorData() 
        {
            path = new uint[MarshalBufferSize];
        }

        /// <summary>
        /// Creates an object with a non-standard buffer size.
        /// </summary>
        /// <param name="maxPathSize">The maximum path size the buffer can hold.</param>
        public PathCorridorData(int maxPathSize)
        {
            path = new uint[maxPathSize];
        }
    }
}
