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
using org.critterai.nav.rcn;
using org.critterai.interop;

namespace org.critterai.nav
{
    /// <summary>
    /// Defines area traversal cost and flag restrictions for navigation querys.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The way filtering works, a navigation mesh polygon must have at least one flag set to ever 
    /// be considered by a query.  So setting the include flags to 0 will always result in all 
    /// polygons being excluded.
    /// </para>
    /// <para>
    /// The array form is used to access area cost. E.g. <c>myFilter[5] = 2.5f</c>
    /// </para>
    /// <para>
    /// <b>Warning:</b> Behavior is undefined if an area index is out of range.  The error may 
    /// result in a runtime error, or it may operate as if there is no problem whatsoever.  
    /// E.g. Setting and getting <c>myFilter[myFilter.AreaCount]</c> may get and set the value 
    /// normally.  Do not write code that depends on this behavior since it may change in future 
    /// releases.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    public sealed class NavmeshQueryFilter
        : ManagedObject
    {
        /// <summary>
        /// The default include flags.
        /// </summary>
        public const ushort DefaultIncludeFlags = 0xffff;

        /// <summary>
        /// The default exclude flags.
        /// </summary>
        public const ushort DefaultExcludeFlags = 0;

        /// <summary>
        /// The default area cost.
        /// </summary>
        public const float DefaultAreaCost = 1.0f;

        /// <summary>
        ///dtQueryFilter object.
        /// </summary>
        internal IntPtr root = IntPtr.Zero;

        private int mAreaCount;

        /// <summary>
        /// The number of in use areas.
        /// </summary>
        public int AreaCount
        {
            get { return mAreaCount; }
            set { mAreaCount = Math.Min(Navmesh.MaxArea + 1, value); }
        }

        /// <summary>
        /// The traversal cost for each area, indexed by area.
        /// [Default: <see cref="DefaultAreaCost"/>]
        /// </summary>
        /// <param name="index">The area.</param>
        /// <returns>The traversal cost of the area.</returns>
        public float this[int index]
        {
            get { return NavmeshQueryFilterEx.dtqfGetAreaCost(root, index); }
            set { NavmeshQueryFilterEx.dtqfSetAreaCost(root, index, value); }
        }

        /// <summary>
        /// The flags for polygons that should be included in the query.
        /// [Default: <see cref="DefaultIncludeFlags"/>]
        /// </summary>
        /// <remarks>
        /// <para>
        /// A navigation mesh polygon must have at least one of these flags set in order to be 
        /// included in a query. All polygons will be excluded if this value is set to zero.
        /// </para>
        /// </remarks>
        public ushort IncludeFlags
        {
            get { return NavmeshQueryFilterEx.dtqfGetIncludeFlags(root); }
            set { NavmeshQueryFilterEx.dtqfSetIncludeFlags(root, value); }
        }

        /// <summary>
        /// The flags for polygons that should be excluded from the query.
        /// [Default: <see cref="DefaultExcludeFlags"/>]
        /// </summary>
        /// <remarks>
        /// <para>
        /// If a polygon has any of these flags set it will be excluded by a query.
        /// </para>
        /// </remarks>
        public ushort ExcludeFlags
        {
            get { return NavmeshQueryFilterEx.dtqfGetExcludeFlags(root); }
            set { NavmeshQueryFilterEx.dtqfSetExcludeFlags(root, value); }
        }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Constructor for a filter with the maximum number of areas. 
        /// (<see cref="Navmesh.MaxArea"/> + 1)
        /// </summary>
        public NavmeshQueryFilter()
            : base(AllocType.Local)
        {
            root = NavmeshQueryFilterEx.dtqfAlloc();
            mAreaCount = Navmesh.MaxArea + 1;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="areaCount">The number of used areas.</param>
        public NavmeshQueryFilter(int areaCount)
            : base(AllocType.Local) 
        {
            root = NavmeshQueryFilterEx.dtqfAlloc();
            mAreaCount = Math.Min(Navmesh.MaxArea + 1, mAreaCount);
        }

        internal NavmeshQueryFilter(IntPtr filter, AllocType type)
                : base(type)
        {
            root = filter;
            mAreaCount = Navmesh.MaxArea + 1;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~NavmeshQueryFilter()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Request all unmanaged resources controlled by the object be 
        /// immediately freed and the object marked as disposed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the object was created using a public constructor the resources will be freed 
        /// immediately.
        /// </para>
        /// </remarks>
        public override void RequestDisposal()
        {
            // Note: There is no external unmanaged allocation.
            if (root != IntPtr.Zero && ResourceType == AllocType.Local)
                NavmeshQueryFilterEx.dtqfFree(root);
            root = IntPtr.Zero;
        }

        /// <summary>
        /// Creates a new filter with the same state as the current filter.
        /// </summary>
        /// <returns>A clone of the current filter.</returns>
        public NavmeshQueryFilter Clone()
        {
            if (IsDisposed)
                return null;

            int count = this.AreaCount;

            NavmeshQueryFilter result = new NavmeshQueryFilter(count);
            result.ExcludeFlags = this.ExcludeFlags;
            result.IncludeFlags = this.IncludeFlags;

            for (int i = 0; i < count; i++)
            {
                result[i] = this[i];
            }

            return result;
        }
    }
}
