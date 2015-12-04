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
using System;
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Applies <see cref="CompactHeightfield.MarkConvexPolyArea"/> to a 
    /// <see cref="CompactHeightfield"/>.
    /// </summary>
    public sealed class AreaConvexMarker
        : AreaMarker
    {
        private readonly Vector3[] verts;
        private readonly float ymin;
        private readonly float ymax;

        private AreaConvexMarker(string name
            , int priority
            , byte area
            , Vector3[] verts
            , float ymin
            , float ymax)
            : base(name, priority, area)
        {
            this.verts = verts;
            this.ymin = ymin;
            this.ymax = ymax;
        }

        /// <summary>
        /// Always threadsafe. (True)
        /// </summary>
        public override bool IsThreadSafe { get { return true; } }

        /// <summary>
        /// Process the build context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The area will be applied during the <see cref="NMGenState.CompactFieldBuild"/>
        /// state.
        /// </para>
        /// </remarks>
        /// <param name="state">The current build state.</param>
        /// <param name="context">The context to process.</param>
        /// <returns>False on error, otherwise true.</returns>
        public override bool ProcessBuild(NMGenContext context, NMGenState state)
        {
            if (state != NMGenState.CompactFieldBuild)
                return true;

            if (context.CompactField.MarkConvexPolyArea(context, verts, ymin, ymax, Area))
            {
                context.Log(string.Format(
                    "{0}: Marked convex polygon area: Area: {1}, Priority: {2}"
                    , Name, Area, Priority)
                    , this);

                return true;
            }

            context.Log(Name + ": Failed to mark convex polygon area.", this);
            return false;
        }

        /// <summary>
        /// Creates a new marker.
        /// </summary>
        /// <remarks>
        /// <para>Will return null on an invalid vertices array or invalid min/max values.</para>
        /// </remarks>
        /// <param name="name">The processor name.</param>
        /// <param name="priority">The processor priority.</param>
        /// <param name="area">The area to apply.</param>
        /// <param name="verts">A list of vertices that form a convex polygon.</param>
        /// <param name="ymin">The minimum y-axis world position.</param>
        /// <param name="ymax">The maximum y-axis world position.</param>
        /// <returns>A new marker, or null on error.</returns>
        public static AreaConvexMarker Create(string name, int priority, byte area
            , Vector3[] verts, float ymin, float ymax)
        {
            if (verts == null || verts.Length == 0 || ymin > ymax)
                return null;

            return new AreaConvexMarker(name, priority, area, verts, ymin, ymax);
        }
    }
}
