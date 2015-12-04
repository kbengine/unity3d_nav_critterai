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
using System;
using org.critterai.interop;
using org.critterai.nmgen.rcn;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents a group of related <see cref="Contour">contours</see>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A contour set is usually generated from a single <see cref="CompactHeightfield"/>.  All 
    /// contours share the minimum bounds and cell sizes of the set.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    public sealed class ContourSet
        : IManagedObject
    {

        internal ContourSetEx root;
        private Contour[] mContours = null;

        /// <summary>
        /// The width of the set. (Along the x-axis in cell units.)
        /// </summary>
        public int Width { get { return root.width; } }

        /// <summary>
        /// The depth of the set. (Along the z-axis in cell units.)
        /// </summary>
        public int Depth { get { return root.depth; } }

        /// <summary>
        /// The width/depth increment of each cell. (On the xz-plane.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see cref="Contour"/> for information on how the bounds and cell sizes are applied 
        /// to member contours.
        /// </para>
        /// </remarks>
        public float XZCellSize { get { return root.xzCellSize; } }

        /// <summary>
        /// The height increment of each cell. (On the y-axis.)
        /// </summary> 
        /// <remarks>
        /// <para>
        /// See <see cref="Contour"/> for information on how the bounds and cell sizes are 
        /// applied to member contours.
        /// </para>
        /// </remarks>
        public float YCellSize { get { return root.yCellSize; } }

        /// <summary>
        /// The AABB border size used to generate the source data from which the contours were 
        /// derived.
        /// </summary>
        public int BorderSize { get { return root.borderSize; } }

        /// <summary>
        /// The number of contours in the set.
        /// </summary>
        public int Count { get { return root.contourCount; } }

        /// <summary>
        /// The minimum bounds of the set in world space.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see cref="Contour"/> for information on how the bounds and cell sizes are applied 
        /// to member contours.
        /// </para>
        /// </remarks>
        /// <returns>The minimum bounds of the set.</returns>
        public Vector3 BoundsMin { get { return root.boundsMin; } }

        /// <summary>
        /// The maximum bounds of the set in world space.
        /// </summary>
        /// <returns>The maximum bounds of the set.</returns>
        public Vector3 BoundsMax { get { return root.boundsMax; } }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed 
        {
            get { return (root.contours == IntPtr.Zero); } 
        }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        private ContourSet(ContourSetEx root)
        {
            this.root = root;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ContourSet()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Frees and marks as disposed all resources. (Including member <see cref="Contour"/> objects.)
        /// </summary>
        public void RequestDisposal()
        {
            if (IsDisposed)
                return;

            ContourSetEx.nmcsFreeSetData(root);

            if (mContours == null)
                return;

            for (int i = 0; i < mContours.Length; i++)
            {
                if (mContours[i] != null)
                    mContours[i].Reset();
            }
        }

        /// <summary>
        /// Gets the contour for the specified index.
        /// </summary>
        /// <param name="index">
        /// The contour index. [Limits: 0 &lt; value &lt; <see cref="Count"/>]
        /// </param>
        /// <returns>The contour, or null on failure.</returns>
        public Contour GetContour(int index)
        {
            if (IsDisposed || index < 0 || index >= root.contourCount)
                return null;

            if (mContours == null)
                mContours = new Contour[root.contourCount];

            if (mContours[index] != null)
                return mContours[index];

            Contour result = new Contour();

            ContourSetEx.nmcsGetContour(root, index, result);
            mContours[index] = result;

            return result;
        }

        /// <summary>
        /// Builds a contour set from the region outlines in the provided <see cref="CompactHeightfield"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The raw contours will match the region outlines exactly.  The edgeMaxDeviation 
        /// and maxEdgeLength parameters control how closely the simplified contours will match 
        /// the raw contours.
        /// </para>
        /// <para>
        /// Simplified contours are generated such that the vertices for portals between areas 
        /// match up.  (They are considered mandatory vertices.)
        /// </para>
        /// <para>
        /// Setting maxEdgeLength to zero will disabled the feature.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use for the build.</param>
        /// <param name="field">The field to use for the build.(Must have region data.)</param>
        /// <param name="edgeMaxDeviation">
        /// The maximum distance a simplified edge may deviate from the raw contour's vertices.
        /// [Limit: >= 0]
        /// </param>
        /// <param name="maxEdgeLength">
        /// The maximum allowed length of a simplified edge. [Limit: >= 0]
        /// </param>
        /// <param name="flags">The build flags.</param>
        /// <returns>The contour set, or null on failure.</returns>
        public static ContourSet Build(BuildContext context, CompactHeightfield field
            , float edgeMaxDeviation, int maxEdgeLength, ContourBuildFlags flags)
        {
            if (context == null || field == null || field.IsDisposed
                || edgeMaxDeviation < 0
                || maxEdgeLength < 0)
            {
                return null;
            }

            ContourSetEx root = new ContourSetEx();

            if (ContourSetEx.nmcsBuildSet(context.root, field
                , edgeMaxDeviation, maxEdgeLength
                , root
                , flags))
            {
                return new ContourSet(root);
            }

            return null;
        }
    }
}
