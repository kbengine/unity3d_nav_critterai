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
    /// Provides core data for agents managed by a <see cref="CrowdManager"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This structure is useful for marshalling information from the <see cref="CrowdManager"/> 
    /// back to the actual agent implementation.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CrowdAgentCoreState
    {
        /*
         * Design notes:
         * 
         * This is a structure because it must be passed
         * in an array during interop.
         * 
         * Duplicate of: rcnCrowdAgentCoreData
         */

        /// <summary>
        /// The state of the agent.
        /// </summary>
	    public CrowdAgentState state;

        /// <summary>
        /// The reference of the polygon that contains the position.
        /// </summary>
        public uint positionPoly;

        /// <summary>
        /// The reference of the polygon that contains the target.
        /// </summary>
        public uint targetPoly;

        /// <summary>
        /// The reference of the polygon the contains the next corner.
        /// (Or zero if the next corner is the target.)
        /// </summary>
        public uint nextCornerPoly;

        /// <summary>
        /// The number of neighbors.
        /// </summary>
        public int neighborCount;

        /// <summary>
        /// The desired speed of the agent.
        /// </summary>
        public float desiredSpeed;

        /// <summary>
        /// The position of the agent.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// disp-value. (Not documented.)
        /// </summary>
        private Vector3 disp;

        /// <summary>
        /// The desired velocity of the agent.
        /// </summary>
        public Vector3 desiredVelocity;

        /// <summary>
        /// nvel-value. (Not documented.)
        /// </summary>
        private Vector3 nvel;

        /// <summary>
        /// The velocity of the agent.
        /// </summary>
        public Vector3 velocity;

        /// <summary>
        /// The target of the agent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the same as the corridor target.
        /// </para>
        /// </remarks>
        public Vector3 target;

        /// <summary>
        /// The next corner in the path corridor.
        /// </summary>
        public Vector3 nextCorner;
    }
}
