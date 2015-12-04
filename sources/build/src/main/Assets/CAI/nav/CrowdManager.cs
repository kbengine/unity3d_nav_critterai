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
using org.critterai.nav.rcn;
using org.critterai.interop;
using System.Collections.Generic;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Provides local steering behaviors for a group of agents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the core class for all crowd features.
    /// </para>
    /// <para>
    /// The standard method for setting up the crowd is as follows:
    /// </para>
    /// <ol>
    /// <li>Construct the crowd object.</li>
    /// <li>Set the avoidance configurations using <see cref="SetAvoidanceConfig"/>.</li>
    /// <li>Add agents using <see cref="AddAgent"/> and make an initial movement request using 
    /// <see cref="CrowdAgent.RequestMoveTarget"/>.</li>
    /// </ol>
    /// <para>
    /// The standard process for managing the crowd is as follows:
    /// </para>
    /// <ol>
    /// <li>Call <see cref="Update"/> to allow the crowd to manage its agents.</li>
    /// <li>Use the <see cref="CrowdAgent"/> objects to retreive state, such as position.</li>
    /// <li>Make movement requests using  <see cref="CrowdAgent.RequestMoveTarget"/> and 
    /// <see cref="CrowdAgent.AdjustMoveTarget"/>.</li>
    /// <li>Repeat every frame.</li>
    /// </ol>
    /// <para>
    /// Some agent configuration settings can be updated using <see cref="CrowdAgent.SetConfig"/>.  
    /// But the crowd owns the agent position and velocity.  If agent position must be fed back 
    /// into the crowd, the agent must be removed and re-added.
    /// </para>
    /// <para>
    /// Notes:
    /// </para>
    /// <ul>
    /// <li>Path related information is available for newly added agents only after an 
    /// <see cref="Update"/> has been performed.</li>
    /// <li>This class is meant to provide 'local' movement. There is a limit of 256 polygons in 
    /// the path corridor. So the crowd can't provide pathfinding services over long distances.</li>
    /// </ul>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    public sealed class CrowdManager
        : ManagedObject
    {
        /*
         * Warning:  The current design is based on not allowing
         * re-initialization of the native class after it is constructed.  
         * Re-evaluate this class if re-initailization is added.
         */

        /// <summary>
        /// The maximum number of avoidance configurations that can be associated with the manager.
        /// </summary>
        public const int MaxAvoidanceParams = 8;

        /// <summary>
        /// An instance of a dtCrowd object.
        /// </summary>
        internal IntPtr root;
        
        private NavmeshQueryFilter mFilter = null;
        private CrowdProximityGrid mGrid = null;
        private NavmeshQuery mQuery = null;

        private float mMaxAgentRadius;
        private Navmesh mNavmesh;
        
        internal CrowdAgent[] mAgents;
        // Needs to be a separate array since it is used as an argument
        // for an interop call.
        internal CrowdAgentCoreState[] agentStates;

        /// <summary>
        /// The maximum number agents that can be managed by the object.
        /// </summary>
        public int MaxAgents { get { return mAgents.Length; } }

        /// <summary>
        /// The maximum allowed agent radius supported by the object.
        /// </summary>
        public float MaxAgentRadius { get { return mMaxAgentRadius; } }

        /// <summary>
        /// The navigation mesh used by the object.
        /// </summary>
        public Navmesh Navmesh { get { return mNavmesh; } }

        /// <summary>
        /// The query filter used by the manager.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Updating the state of this filter will alter the steering behaviors of all agents.
        /// All agents share the same filter.
        /// </para>
        /// </remarks>
        public NavmeshQueryFilter QueryFilter { get { return mFilter; } }

        /// <summary>
        /// The number of agents being managed.
        /// </summary>
        public int AgentCount 
        {
            get 
            {
                int result = 0;
                for (int i = 0; i < mAgents.Length; i++)
                {
                    result += (mAgents[i] == null) ? 0 : 1;
                }
                return result;
            } 
        }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero || mNavmesh.IsDisposed); }
        }

        /// <summary>
        /// The agent from the agent buffer.
        /// </summary>
        /// <param name="index">
        /// The buffer index. [Limit: 0 &lt;= value &lt; <see cref="MaxAgents"/>]
        /// </param>
        /// <returns>The agent from the buffer, or null no agent at the index.</returns>
        public CrowdAgent this[int index]
        {
            get { return (IsDisposed ? null : mAgents[index]); }
        }

        private CrowdManager(IntPtr crowd, Navmesh navmesh, int maxAgents, float maxAgentRadius)
            : base(AllocType.External)
        {
            mMaxAgentRadius = maxAgentRadius;
            mNavmesh = navmesh;

            root = crowd;

            mAgents = new CrowdAgent[maxAgents];
            agentStates = new CrowdAgentCoreState[maxAgents];

            IntPtr ptr = CrowdManagerEx.dtcGetFilter(root);
            mFilter = new NavmeshQueryFilter(ptr, AllocType.ExternallyManaged);

            ptr = CrowdManagerEx.dtcGetGrid(root);
            mGrid = new CrowdProximityGrid(ptr);

            ptr = CrowdManagerEx.dtcGetNavMeshQuery(root);
            mQuery = new NavmeshQuery(ptr, true, AllocType.ExternallyManaged);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~CrowdManager()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Creates a new crowd manager.
        /// </summary>
        /// <param name="maxAgents">
        /// The maximum number of agents that can be added to the manager.
        /// </param>
        /// <param name="maxAgentRadius">The maximum allowed agent radius.</param>
        /// <param name="navmesh">
        /// The navigation mesh to use for path planning and steering related queries.
        /// </param>
        /// <returns>A new crowd manager, or null on error.</returns>
        public static CrowdManager Create(int maxAgents, float maxAgentRadius, Navmesh navmesh)
        {
            if (navmesh == null || navmesh.IsDisposed)
                return null;

            maxAgents = Math.Max(1, maxAgents);
            maxAgentRadius = Math.Max(0, maxAgentRadius);

            IntPtr root =
                CrowdManagerEx.dtcDetourCrowdAlloc(maxAgents, maxAgentRadius, navmesh.root);

            if (root == IntPtr.Zero)
                return null;

            return new CrowdManager(root, navmesh, maxAgents, maxAgentRadius);
        }

        /// <summary>
        /// Immediately frees all unmanaged resources allocated by the object.
        /// </summary>
        public override void RequestDisposal()
        {
            // There are no managed or local allocations.
            if (root != IntPtr.Zero)
            {
                mFilter.RequestDisposal();
                mFilter = null;
                mGrid.Dispose();
                mGrid = null;
                mQuery.RequestDisposal();
                mQuery = null;
                mNavmesh = null;
                mMaxAgentRadius = -1;
                agentStates = null;

                for (int i = 0; i < mAgents.Length; i++)
                {
                    if (mAgents[i] == null)
                        continue;
                    mAgents[i].Dispose();
                    mAgents[i] = null;
                }

                CrowdManagerEx.dtcDetourCrowdFree(root);
                root = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Sets the shared avoidance configuration for a specified index.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Multiple avoidance configurations can be set for the manager with agents assigned to 
        /// different avoidance behaviors via the <see cref="CrowdAgentParams"/> object.
        /// </para>
        /// </remarks>
        /// <param name="index">
        /// The index. [Limits: 0 &lt;= value &lt; <see cref="MaxAvoidanceParams"/>].
        /// </param>
        /// <param name="config">The avoidance configuration.</param>
        /// <returns>True if the configuration is successfully set.</returns>
        public bool SetAvoidanceConfig(int index
            , CrowdAvoidanceParams config)
        {
            if (IsDisposed || index < 0 || index >= MaxAvoidanceParams)
                return false;

            CrowdManagerEx.dtcSetObstacleAvoidanceParams(root, index, config);

            return true;
        }

        /// <summary>
        /// Gets the shared avoidance configuration for a specified index.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Getting a configuration for an index that has not been set will return a configuration 
        /// in an undefined state.
        /// </para>
        /// </remarks>
        /// <param name="index">
        /// The index. [Limits: 0 &lt;= value &lt; <see cref="MaxAvoidanceParams"/>]
        /// </param>
        /// <returns>An avoidance configuration.</returns>
        public CrowdAvoidanceParams GetAvoidanceConfig(int index)
        {
            if (!IsDisposed && index >= 0 && index < MaxAvoidanceParams)
            {
                CrowdAvoidanceParams result = new CrowdAvoidanceParams();
                CrowdManagerEx.dtcGetObstacleAvoidanceParams(root
                    , index
                    , result);

                return result;             
            }
            return null;
        }

        /// <summary>
        /// Adds an agent to the manager.
        /// </summary>
        /// <param name="position">
        /// The current position of the agent within the navigation mesh.
        /// </param>
        /// <param name="agentParams">The agent configuration.</param>
        /// <returns>
        /// A reference to the agent object created by the manager, or null on error.
        /// </returns>
        public CrowdAgent AddAgent(Vector3 position
            , CrowdAgentParams agentParams)
        {
            if (IsDisposed)
                return null;

            IntPtr ptr = IntPtr.Zero;
            CrowdAgentCoreState initialState = new CrowdAgentCoreState();

            int index = CrowdManagerEx.dtcAddAgent(root
                , ref position
                , ref agentParams
                , ref ptr
                , ref initialState);

            if (index == -1)
                return null;

            mAgents[index] = new CrowdAgent(this, ptr, index);
            agentStates[index] = initialState;

            return mAgents[index];
        }

        /// <summary>
        /// Removes an agent from the manager.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="CrowdAgent"/> object will be immediately disposed. Continued use will 
        /// result in undefined behavior.
        /// </para>
        /// </remarks>
        /// <param name="agent">The agent to remove.</param>
        public void RemoveAgent(CrowdAgent agent)
        {
            for (int i = 0; i < mAgents.Length; i++)
            {
                if (mAgents[i] == agent)
                {
                    CrowdManagerEx.dtcRemoveAgent(root, agent.managerIndex);
                    agent.Dispose();
                    mAgents[i] = null;
                    // Don't need to do anything about the core data array.
                }
            }
        }

        /// <summary>
        /// Updates the steering and positions for all agents.
        /// </summary>
        /// <param name="deltaTime">The time in seconds to update the simulation.</param>
        public void Update(float deltaTime)
        {
            if (IsDisposed)
                return;

            CrowdManagerEx.dtcUpdate(root, deltaTime, agentStates);
        }

        /// <summary>
        /// The extents used by the manager when it performs queries against the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All agents and targets should remain within these distances of the navigation mesh 
        /// surface.  For example, if the y-axis extent is 1.0, then the agent should remain 
        /// between 1.0 above and 1.0 below the surface of the mesh.
        /// </para>
        /// <para>
        /// The extents remains constant over the life of the crowd manager.
        /// </para>
        /// </remarks>
        /// <returns>The extents.</returns>
        public Vector3 GetQueryExtents()
        {
            if (IsDisposed)
                return Vector3Util.Zero;
            Vector3 result = Vector3Util.Zero;
            CrowdManagerEx.dtcGetQueryExtents(root, ref result);
            return result;
        }
        /// <summary>
        /// Gets the velocity sample count.
        /// </summary>
        /// <returns>The velocity sample count.</returns>
        public int GetVelocitySampleCount()
        {
            if (IsDisposed)
                return -1;
            return CrowdManagerEx.dtcGetVelocitySampleCount(root);
        }

        /// <summary>
        /// Gets the proximity grid.
        /// </summary>
        public CrowdProximityGrid ProximityGrid { get { return mGrid; } }

        /// <summary>
        /// The navmesh query used by the manager.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The configuration of this query makes it useful only for local pathfinding queries. 
        /// (Its search size is limited.) Also, it is marked as 
        /// <see cref="NavmeshQuery.IsRestricted"> restricted</see>.
        /// </para>
        /// </remarks>
        public NavmeshQuery Query { get { return mQuery; } }
    }
}
