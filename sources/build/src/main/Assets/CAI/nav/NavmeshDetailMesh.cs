/*
 * Copyright (c) 2011 Stephen A. Pratt
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

namespace org.critterai.nav
{
    /// <summary>
    /// The header data for a polygon's detail mesh in a <see cref="NavmeshTile"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All indices refer to the vertex and triangle data in the associated 
    /// <see cref="NavmeshTile"/>.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshDetailMesh
    {
        /// <summary>
        /// The index of the base vertex for the detail mesh.
        /// </summary>
	    public uint vertBase;

        /// <summary>
        /// The index of the base triangle for the detail mesh.
        /// </summary>
        public uint triBase;
		
		/// <summary>
		/// The number of vertices in the detail mesh.
		/// </summary>
        public byte vertCount;
		
		/// <summary>
		/// The number of triangles in the detail mesh.
		/// </summary>
        public byte triCount;
    }
}
