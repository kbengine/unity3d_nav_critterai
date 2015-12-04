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
    /// Provides neighbor data for agents managed by a crowd manager. 
    /// (See: <see cref="CrowdAgent.GetNeighbors"/>)
    /// </summary>
    /// <seealso cref="CrowdManager"/>
    /// <see cref="CrowdAgent"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct CrowdNeighbor
    {
        /// <summary>
        /// The maximum number of agent neighbors. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used to size buffers of this structure.
        /// </para>
        /// </remarks>
        public const int MaxNeighbors = 6;

        /// <summary>
        /// The index of the neighbor. (In the <see cref="CrowdManager"/> agent buffer.)
        /// </summary>
        public int index;

        /// <summary>
        /// The distance to the neighbor.
        /// </summary>
        public float distance;
    }
}
