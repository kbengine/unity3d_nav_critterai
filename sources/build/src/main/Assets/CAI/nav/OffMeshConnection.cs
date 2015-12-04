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
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// An off-mesh connection not associated with a <see cref="Navmesh"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a convenience element for use during the build process.
    /// </para>
    /// </remarks>
    /// <seealso cref="NavmeshConnection"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct OffMeshConnection
    {
        /// <summary>
        /// The start point of the connection.
        /// </summary>
        public Vector3 start;

        /// <summary>
        /// The end point of the connection.
        /// </summary>
        public Vector3 end;

        /// <summary>
        /// The radius of the start and end points. [Limit: >0]
        /// </summary>
        public float radius;


        /// <summary>
        /// The direction of the connection. [Limit: 0 or 1]
        /// </summary>
        /// <remarks>
        /// A vlaue of <see cref="NavmeshConnection.BiDirectionalFlag"/> indicates bi-directional.
        /// </remarks>
        public byte direction;

        /// <summary>
        /// The area of the connection. [Limit: &lt;= <see cref="Navmesh.MaxArea"/>].
        /// </summary>
        public byte area;

        /// <summary>
        /// The connection flags.
        /// </summary>
        public ushort flags;

        /// <summary>
        /// The id of the off-mesh connection. (User defined.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value has no meaning to the core navigation system.  Its purpose is entirely user 
        /// defined.
        /// </para>
        /// </remarks>
        public uint userId;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <param name="radius">The radius of the start and end points. [Limit: >0]</param>
        /// <param name="isBidDrectional">True if the connection is bi-directional.</param>
        /// <param name="area">The area. [Limit: &lt;= <see cref="Navmesh.MaxArea"/>].</param>
        /// <param name="flags">The connection flags.</param>
        /// <param name="userId">The id of the off-mesh connection. (User defined.)</param>
        public OffMeshConnection(Vector3 start, Vector3 end
            ,float radius, bool isBidDrectional, byte area, ushort flags, uint userId)
        {
            this.start = start;
            this.end = end;
            this.radius = MathUtil.ClampToPositiveNonZero(radius);
            direction = (byte)(isBidDrectional ? 1 : 0);
            this.area = NavUtil.ClampArea(area);
            this.flags = flags;
            this.userId = userId;
        }

        /// <summary>
        /// True if the conneciton is bi-directional.
        /// </summary>
        public bool IsBiDirectional
        {
            get { return (direction == 1); }
            set { direction = (byte)(value ? 1 : 0); }
        }
    }
}
