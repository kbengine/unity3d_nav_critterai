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
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// A navigation mesh off-mesh connection.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshConnection
    {
        /// <summary>
        /// The flag that indicates the connection is bi-directional.
        /// </summary>
        public const uint BiDirectionalFlag = 0x01;

        /// <summary>
        /// The endpoints of the connection. [(start, end)].
        /// </summary>
        /// <remarks>
        /// <para>
        /// For a properly built navigation mesh, the start vertex will always be within the 
        /// bounds of a navigation mesh polygon.
        /// </para>
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	    public Vector3[] endpoints;

        /// <summary>
        /// The radius of the endpoints. [Limit: >=0]
        /// </summary>
        public float radius;

        /// <summary>
        /// The polygon reference of the connection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All connections are stored as 2-vertex polygons within the navigation mesh.
        /// </para>
        /// </remarks>
        public ushort polyRef;
        
        /// <summary>
        /// Link flags.
        /// </summary>
        /// <remarks>
        /// <para>
        /// These are not the user flags.  Those are assigned to the connection's polygon.  These 
        /// are link flags used for internal purposes.
        /// </para>
        /// </remarks>
        public byte flags;

        /// <summary>
        /// Side.
        /// </summary>
        public byte side;

        /// <summary>
        /// The id of the off-mesh connection. (User assigned when the navmesh is built.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value has no meaning to the core navigation system.  It's purpose is entirely 
        /// user defined.
        /// </para>
        /// </remarks>
        public uint userId;

        /// <summary>
        /// True if the traversal of the connection can start from either endpoint.  False if the 
        /// connection can only be travered from the start to the end point.
        /// </summary>
        public bool IsBiDirectional
        {
            get { return (flags & BiDirectionalFlag) != 0; }
        }
    }
}
