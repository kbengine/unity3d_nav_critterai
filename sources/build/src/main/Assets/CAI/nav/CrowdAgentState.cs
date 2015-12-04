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
    /// The type of navmesh polygon an agent is currently traversing.
    /// </summary>
    public enum CrowdAgentState : byte
    {
        /*
         * Source: DetourCrowd.h CrowdAgentState
         * 
         * The source does not explicitly assign values.
         *
         */

        /// <summary>
        /// The agent is not in a valid state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An agent will enter this state if its navigation mesh position cannot be determined.  
        /// If the cause is a temporary modification to the navigation mesh, then the problem may 
        /// auto-resolve itself. Otherwise, error handling will be needed.
        /// </para>
        /// </remarks>
        Invalid,

        /// <summary>
        /// The agent is traversing a normal navmesh polygon.
        /// </summary>
        OnMesh,

        /// <summary>
        /// The agent is traversing an off-mesh connection.
        /// </summary>
        OffMesh
    }
}
