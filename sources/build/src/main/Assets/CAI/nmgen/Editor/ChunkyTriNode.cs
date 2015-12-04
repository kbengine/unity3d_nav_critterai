/*
 * Copyright (c) 2012 Stephen A. Pratt
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

namespace org.critterai.nmgen
{
    /// <summary>
    /// A node in a <see cref="ChunkyTriMesh"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The bounds of the node represents the AABB of the triangles contained by the node.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct ChunkyTriMeshNode
    {
        /// <summary>
        /// The minimum x-bounds.
        /// </summary>
        public float xmin;

        /// <summary>
        /// The minimum z-bounds.
        /// </summary>
        public float zmin;

        /// <summary>
        /// The maximum x-bounds.
        /// </summary>
        public float xmax;

        /// <summary>
        /// The maximum z-bounds.
        /// </summary>
        public float zmax;

        /// <summary>
        /// The start index of the triangles in the node.
        /// </summary>
        public int i;

        /// <summary>
        /// The number of triangles in the node.
        /// </summary>
        public int count;

        internal bool Overlaps(float xmin, float zmin, float xmax, float zmax)
        {
            bool result = true;
            result = (xmin > this.xmax || xmax < this.xmin) ? false : result;
            result = (zmin > this.zmax || zmax < this.zmin) ? false : result;
            return result;
        }
    }
}
