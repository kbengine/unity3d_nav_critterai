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
    /// Applies <see cref="CompactHeightfield.MarkBoxArea"/> to a <see cref="CompactHeightfield"/>.
    /// </summary>
	public sealed class AreaBoxMarker
        : AreaMarker
	{
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;

        private AreaBoxMarker(string name, int priority, byte area
            , Vector3 boundsMin, Vector3 boundsMax)
            : base(name, priority, area)
        {
            mBoundsMin = boundsMin;
            mBoundsMax = boundsMax;
        }

        /// <summary>
        /// The mimimum world bounds of the area to mark.
        /// </summary>
        public Vector3 BoundsMin { get { return mBoundsMin; } }

        /// <summary>
        /// The maximum world bounds of the area to mark.
        /// </summary>
        public Vector3 BoundsMax { get { return mBoundsMax; } }

        /// <summary>
        /// Always threadsafe. (True)
        /// </summary>
        public override bool IsThreadSafe { get { return true; } }

        /// <summary>
        /// Process the build context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The area will be applied during the <see cref="NMGenState.CompactFieldBuild"/> state.
        /// </para>
        /// </remarks>
        /// <param name="state">The current build state.</param>
        /// <param name="context">The context to process.</param>
        /// <returns>False on error, otherwise true.</returns>
        public override bool ProcessBuild(NMGenContext context, NMGenState state)
        {
            if (state != NMGenState.CompactFieldBuild)
                return true;

            if (context.CompactField.MarkBoxArea(context, mBoundsMin, mBoundsMax, Area))
            {
                context.Log(string.Format("{0} : Marked box area: Area: {1}, Priority: {2}"
                    , Name, Area, Priority)
                    , this);
                return true;
            }

            context.Log(Name + ": Failed to mark box area.", this);
            return false;
        }

        /// <summary>
        /// Creates a new marker.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return null if the bounds are invalid.
        /// </para>
        /// </remarks>
        /// <param name="name">The processor name.</param>
        /// <param name="priority">The processor priority.</param>
        /// <param name="area">The area to apply.</param>
        /// <param name="boundsMin">The mimimum world bounds of the area to mark.</param>
        /// <param name="boundsMax">The maximum world bounds of the area to mark.</param>
        /// <returns>A new marker, or null on error.</returns>
        public static AreaBoxMarker Create(string name, int priority, byte area
            , Vector3 boundsMin, Vector3 boundsMax)
        {
            if (org.critterai.geom.TriangleMesh.IsBoundsValid(boundsMin, boundsMax))
                return new AreaBoxMarker(name, priority, area, boundsMin, boundsMax);

            return null;
        }
    }
}
