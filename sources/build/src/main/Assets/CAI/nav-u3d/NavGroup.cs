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
using UnityEngine;

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Provides a convenient structure for grouping together related navigation resources.
    /// </summary>
    public struct NavGroup
    {
        /// <summary>
        /// The navigation mesh used by <see cref="query"/>.
        /// </summary>
        public Navmesh mesh;

        /// <summary>
        /// A navigation mesh query.
        /// </summary>
        public NavmeshQuery query;

        /// <summary>
        /// The filter to use with <see cref="query"/>.
        /// </summary>
        public NavmeshQueryFilter filter;

        /// <summary>
        /// A crowd.
        /// </summary>
        public CrowdManager crowd;

        /// <summary>
        /// The extents to use with <see cref="query"/>.
        /// </summary>
        public Vector3 extents;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mesh">The navigation mesh used by the query.</param>
        /// <param name="query">A navigation mesh query.</param>
        /// <param name="crowd">A crowd.</param>
        /// <param name="filter">The filter to use with the query.</param>
        /// <param name="extents">The extents to use with the query.</param>
        /// <param name="cloneFilter">
        /// If true, the filter will be cloned rather than referenced.
        /// </param>
        public NavGroup(Navmesh mesh, NavmeshQuery query, CrowdManager crowd
            , NavmeshQueryFilter filter, Vector3 extents
            , bool cloneFilter)
        {
            this.mesh = mesh;
            this.query = query;
            this.crowd = crowd;
            this.filter = (cloneFilter ? filter.Clone() : filter);
            this.extents = extents;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="copy">The group to copy.</param>
        /// <param name="cloneFilter">
        /// If true, the filter will be cloned. Otherwise it will be referenced.
        /// </param>
        public NavGroup(NavGroup copy, bool cloneFilter)
        {
            this.mesh = copy.mesh;
            this.query = copy.query;
            this.crowd = copy.crowd;
            this.filter = (cloneFilter ? copy.filter.Clone() : copy.filter);
            this.extents = copy.extents;
        }
    }
}
