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
using UnityEngine;
using org.critterai.u3d;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Provides debug visualizations for <see cref="CrowdAgent"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All draw methods in this class use GL, so they should generally be called from within the 
    /// OnRenderObject() method.
    /// </para>
    /// <para>
    /// The design of this class minimizes impact on garbage collection.
    /// </para>
    /// <para>
    /// Instances of this class are not thread-safe.
    /// </para>
    /// </remarks>
    public sealed class CrowdAgentDebug
    {
        /// <summary>
        /// The color to use when drawing neighbor visualizations.
        /// </summary>
        public static Color neighborColor = 
            new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.66f);

        /// <summary>
        /// The base color to use when drawing visualizations.
        /// </summary>
        public static Color baseColor = 
            new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.66f);

        /// <summary>
        /// The color to use when drawing the agent velocity.
        /// </summary>
        public static Color velocityColor = 
            new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.66f);

        /// <summary>
        /// The color to use when drawing the agent desired velocity.
        /// </summary>
        public static Color desiredVelocityColor = 
            new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f);

        /// <summary>
        /// The color to use when drawing corridor boundary visualizations.
        /// </summary>
        public static Color boundaryColor = 
            new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.66f);

        private Navmesh navmesh;

        // Various buffers.  (Reduces GC impact.)
        private CrowdNeighbor[] neighbors = new CrowdNeighbor[CrowdNeighbor.MaxNeighbors];
        private LocalBoundaryData boundary = new LocalBoundaryData();
        private CornerData corners = new CornerData();
        private PathCorridorData corridor = new PathCorridorData();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="navmesh">The navigation mesh used by the agents.
        /// </param>
        public CrowdAgentDebug(Navmesh navmesh) 
        {
            this.navmesh = navmesh;
        }

        /// <summary>
        /// Draws all agent debug information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawAll(CrowdAgent agent)
        {
            agent.GetCornerData(corners);
            agent.GetCorridor(corridor);

            // Order matters.
            NavDebug.Draw(navmesh, corridor);
            DrawNeighbors(agent);
            DrawLocalBoundary(agent);
            NavDebug.Draw(corners);
            DrawBase(agent);
        }

        /// <summary>
        /// Draws the basic agent debug information.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This does not include the duplicate agent information such as target and corner 
        /// positions.
        /// </para>
        /// </remarks>
        /// <param name="agent">The agent to draw.</param>
        public void DrawBase(CrowdAgent agent)
        {
            Vector3 pos = agent.Position;
            CrowdAgentParams config = agent.GetConfig();
            
            DebugDraw.Circle(pos, config.radius, neighborColor);

            DebugDraw.Arrow(pos + Vector3.up * config.height
                , pos + agent.DesiredVelocity + Vector3.up * config.height
                , 0, 0.05f, desiredVelocityColor);

            DebugDraw.Arrow(pos + Vector3.up * config.height
                , pos + agent.Velocity + Vector3.up * config.height
                , 0, 0.05f, velocityColor);
        }

        /// <summary>
        /// Draws agent neighbor information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawNeighbors(CrowdAgent agent)
        {
            int neighborCount = agent.NeighborCount;

            if (neighborCount == 0)
                return;

            agent.GetNeighbors(neighbors);

            for (int i = 0; i < neighborCount; i++)
            {
                CrowdAgent n = agent.GetNeighbor(neighbors[i]);
                if (n == null)
                    // Not sure why this happens.  Bug in CrowdAgent?
                    continue;
                DebugDraw.Arrow(agent.Position, n.Position, 0, 0.05f, neighborColor);
                DebugDraw.Circle(n.Position, agent.GetConfig().radius, neighborColor);
            }
        }

        /// <summary>
        /// Draws agent local boundary information.
        /// </summary>
        /// <param name="agent">The agent to draw.</param>
        public void DrawLocalBoundary(CrowdAgent agent)
        {
            agent.GetBoundary(boundary);

            if (boundary.segmentCount == 0)
                return;

            DebugDraw.XMarker(boundary.center
                , 0.1f, boundaryColor);

            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(boundaryColor);

            for (int i = 0; i < boundary.segmentCount; i++)
            {
                int p = i * 2;
                GL.Vertex(boundary.segments[p]);
                GL.Vertex(boundary.segments[p + 1]);
            }

            GL.End();
        }
    }
}

