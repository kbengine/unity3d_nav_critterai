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
using org.critterai.nav.rcn;
using org.critterai.interop;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Represents an agent managed by a <see cref="CrowdManager"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Objects of this type can only be obtained from a <see cref="CrowdManager"/> object.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.  Disposal is controled by the 
    /// <see cref="CrowdManager"/> that owns the agent.
    /// </para>
    /// </remarks>
    /// <seealso cref="CrowdManager"/>
    public sealed class CrowdAgent
    {
        /*
         * Design notes:
         * 
         * The difficulty in the design of the agent is that the
         * structure on the native side contains embedded classes.
         * (Rather than pointers to instances.)  And interop can't seem to 
         * handle that. So I'm forced into a less efficient design.
         * 
         */

        private IntPtr root;  // dtCrowdAgent
        private CrowdManager mManager;
        internal int managerIndex;

        /// <summary>
        /// The <see cref="CrowdManager"/> the agent belongs to.
        /// </summary>
        public CrowdManager Manager
        {
            get { return mManager; }
        }

        /// <summary>
        /// The type of mesh polygon the agent is traversing.
        /// </summary>
        public CrowdAgentState State 
        { 
            get { return mManager.agentStates[managerIndex].state; } 
        }

        /// <summary>
        /// The number of neighbor agents. (In vicinity of agent.)
        /// </summary>
        public int NeighborCount 
        { 
            get { return mManager.agentStates[managerIndex].neighborCount; } 
        }

        /// <summary>
        /// The desired agent speed. (Derived)
        /// </summary>
        public float DesiredSpeed 
        { 
            get { return mManager.agentStates[managerIndex].desiredSpeed; } 
        }

        /// <summary>
        /// The position of the agent.
        /// </summary>
        public Vector3 Position 
        { 
            get { return mManager.agentStates[managerIndex].position; } 
        }

        /// <summary>
        /// The reference of the polygon containing the agent position.
        /// </summary>
        public uint PositionPoly
        {
            get { return mManager.agentStates[managerIndex].positionPoly; }
        }

        /// <summary>
        /// The agent's target. (Location the agent is moving toward.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The same as the path corridor target.
        /// </para>
        /// </remarks>
        public Vector3 Target
        {
            get { return mManager.agentStates[managerIndex].target; }
        }

        /// <summary>
        /// The reference of the polygon containing the target.
        /// </summary>
        public uint TargetPoly
        {
            get { return mManager.agentStates[managerIndex].targetPoly; }
        }

        /// <summary>
        /// The next corner in the path corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The same as the next corner in the agent's path corridor.
        /// </para>
        /// </remarks>
        public Vector3 NextCorner
        {
            get { return mManager.agentStates[managerIndex].nextCorner; }
        }

        /// <summary>
        /// The reference of the polygon containing the next corner.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the polygon being entered at the corner, or zero if the next corner is the 
        /// target.
        /// </para>
        /// </remarks>
        public uint NextCornerPoly
        {
            get { return mManager.agentStates[managerIndex].nextCornerPoly; }
        }

        /// <summary>
        /// The desired agent velocity. (Derived)
        /// </summary>
        public Vector3 DesiredVelocity 
        { 
            get { return mManager.agentStates[managerIndex].desiredVelocity; } 
        }

        /// <summary>
        /// The actual agent velocity.
        /// </summary>
        public Vector3 Velocity 
        { 
            get { return mManager.agentStates[managerIndex].velocity; } 
        }

        /// <summary>
        /// Indicates whether the object's resources have been disposed.
        /// </summary>
        public bool IsDisposed { get { return (root == IntPtr.Zero); } }

        internal CrowdAgent(CrowdManager manager
            , IntPtr agent
            , int mangerIndex)
        {
            mManager = manager;
            root = agent;
            managerIndex = mangerIndex;
        }

        internal void Dispose()
        {
            root = IntPtr.Zero;
            mManager = null;
            managerIndex = -1;
        }

        /// <summary>
        /// Gets the agent configuration.
        /// </summary>
        /// <returns>The agent configuration.</returns>
        public CrowdAgentParams GetConfig()
        {
            CrowdAgentParams config = new CrowdAgentParams();
            if (!IsDisposed)
                CrowdAgentEx.dtcaGetAgentParams(root, ref config);
            return config;
        }

        /// <summary>
        /// Updates the configuration for an agent.
        /// </summary>
        /// <param name="config">The new agent configuration.</param>
        public void SetConfig(CrowdAgentParams config)
        {
            if (!IsDisposed)
                CrowdManagerEx.dtcUpdateAgentParameters(mManager.root
                    , managerIndex
                    , ref config);
        }

        /// <summary>
        /// Submits a new move request for the agent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used when a new target is set.  Use <see cref="AdjustMoveTarget"/> when 
        /// only small local adjustments are needed. (Such as happens when following a moving 
        /// target.)
        /// </para>
        /// <para>
        /// The position will be constrained to the surface of the navigation mesh.
        /// </para>
        /// <para>
        /// The request will be processed during the next <see cref="CrowdManager.Update"/>.
        /// </para>
        /// </remarks>
        /// <param name="target">The target position.</param>
        /// <returns>True if the target was successfully set.</returns>
        public bool RequestMoveTarget(NavmeshPoint target)
        {
            if (IsDisposed)
                return false;

            return CrowdManagerEx.dtcRequestMoveTarget(mManager.root
                , managerIndex
                , target);
        }

        /// <summary>
        /// Adjusts the position of an agent's target.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is used to make small local adjustments to the target. (Such as happens 
        /// when following a moving target.)  Use <see cref="RequestMoveTarget"/> when a new 
        /// target is needed.
        /// </para>
        /// </remarks>
        /// <param name="position">The new target position.</param>
        /// <returns>True if the adjustment was successful.</returns>
        public bool AdjustMoveTarget(NavmeshPoint position)
        {
            if (IsDisposed)
                return false;

            return CrowdManagerEx.dtcAdjustMoveTarget(mManager.root
                , managerIndex
                , position);
        }

        /// <summary>
        /// Gets the corridor data related to the agent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available after after a <see cref="CrowdManager"/> update.
        /// </para>
        /// <para>
        /// <b>Warning:</b> The buffer object must be sized to a maximum path size equal to 
        /// <see cref="PathCorridorData.MarshalBufferSize"/>!
        /// </para>
        /// </remarks>
        /// <param name="buffer">
        /// A buffer to load with the corridor data. (Out)
        /// [Length: Maximum Path Size = 
        /// <see cref="PathCorridorData.MarshalBufferSize"/>]
        /// </param>
        /// <returns>True if the data was sucessfully retrieved.</returns>
        public bool GetCorridor(PathCorridorData buffer)
        {
            // Only a partial validation.
            if (IsDisposed
                || buffer == null
                || buffer.path.Length != PathCorridorData.MarshalBufferSize)
            {
                return false;
            }
            CrowdAgentEx.dtcaGetPathCorridorData(root, buffer);
            return true;
        }

        /// <summary>
        /// Gets the local boundary data for the agent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available after after a <see cref="CrowdManager"/> update.
        /// </para>
        /// <para>
        /// This data is not updated every frame.  So the boundary center will not always 
        /// equal the position of the agent.
        /// </para>
        /// </remarks>
        /// <param name="buffer">A buffer to load with the boundary data. (Out)</param>
        /// <seealso cref="LocalBoundaryData"/>
        public void GetBoundary(LocalBoundaryData buffer)
        {
            if (!IsDisposed)
                CrowdAgentEx.dtcaGetLocalBoundary(root, buffer);
        }

        /// <summary>
        /// Gets data related to the neighbors in the vicinity of the agent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available after after a <see cref="CrowdManager"/> update.
        /// </para>
        /// </remarks>
        /// <param name="buffer">
        /// A buffer to load with the neighbor data. 
        /// [Length: >= <see cref="CrowdNeighbor.MaxNeighbors"/>]
        /// </param>
        /// <returns>The number of neighbors in the buffer, or -1 on error.</returns>
        public int GetNeighbors(CrowdNeighbor[] buffer)
        {
            if (IsDisposed
                || buffer == null
                || buffer.Length < CrowdNeighbor.MaxNeighbors)
            {
                return -1;
            }

            int count = CrowdAgentEx.dtcaGetAgentNeighbors(root
                , buffer
                , buffer.Length);

            return count;
        }

        /// <summary>
        /// Gets the agent associated with the specified neighbor.
        /// </summary>
        /// <param name="neighbor">The agent neighbor data.</param>
        /// <returns>The agent associated with the neighbor data, or null on error.</returns>
        public CrowdAgent GetNeighbor(CrowdNeighbor neighbor)
        {
            if (IsDisposed || neighbor.index >= mManager.MaxAgents)
                return null;
            return mManager.mAgents[neighbor.index];
        }

        /// <summary>
        /// Gets the local corner data for the agent.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available after after a <see cref="CrowdManager"/> update.
        /// </para>
        /// <para>
        /// <b>Warning:</b> The buffer object must be sized to <see cref="CornerData.MarshalBufferSize"/>!
        /// </para>
        /// </remarks>
        /// <param name="buffer">
        /// The buffer to load with corner data. (Out) [Required:
        /// <see cref="CornerData.MaxCorners"/> = <see cref="CornerData.MarshalBufferSize"/>]
        /// </param>
        /// <returns>True if the data was sucessfully retrieved.</returns>
        public bool GetCornerData(CornerData buffer)
        {
            // This is only a partial argument validation.
            if (IsDisposed
                || buffer == null
                || buffer.polyRefs.Length != CornerData.MarshalBufferSize)
            {
                return false;
            }
            CrowdAgentEx.dtcaGetAgentCorners(root, buffer);
            return true;
        }
    }
}
