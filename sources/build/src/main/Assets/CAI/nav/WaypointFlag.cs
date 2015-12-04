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

namespace org.critterai.nav
{
    /// <summary>
    /// Flags for path related waypoints.
    /// </summary>
    [System.Flags]
    public enum WaypointFlag : byte
    {
        /// <summary>
        /// The point is the start point in the path.
        /// </summary>
        Start = 0x01,

        /// <summary>
        /// The point is the end point in the path.
        /// </summary>
        End = 0x02,

        /// <summary>
        /// The point is the start of an off-mesh connection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flag is useful in detecting when special locomotion handling needs to occur.
        /// </para>
        /// </remarks>
        OffMesh = 0x04
    }
}
