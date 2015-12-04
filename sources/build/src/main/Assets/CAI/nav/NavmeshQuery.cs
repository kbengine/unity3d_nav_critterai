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
    /// Provides core pathfinding functionality for navigation meshes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In the context of this class: A wall is a polygon segment that is considered impassable.  
    /// A portal is a passable segment between polygons.
    /// </para>
    /// <para>
    /// If a buffer is too small to hold the entire result, the return status will include 
    /// the <see cref="NavStatus.BufferTooSmall"/> flag. (For methods that support undersized 
    /// buffers.)
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    public sealed class NavmeshQuery
        : ManagedObject
    {
        internal IntPtr root; // dtNavmeshQuery

        private bool mIsRestricted;

        internal NavmeshQuery(IntPtr query, bool isConstant, AllocType type)
            : base(type)
        {
            mIsRestricted = isConstant;
            root = query;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~NavmeshQuery()
        {
            RequestDisposal();
        }

        /// <summary>
        /// If true, certain methods are disabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Certain methods are generally not safe for use by multiple clients.  These 
        /// methods will fail if the object is marked as restricted.
        /// </para>
        /// </remarks>
        public bool IsRestricted { get { return mIsRestricted; } }


        /// <summary>
        /// Marks the object as disposed and immediately frees all unmanaged resources for 
        /// locally owned objects.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is not projected by the <see cref="IsRestricted"/> feature.
        /// </para>
        /// </remarks>
        public override void RequestDisposal()
        {
            if (ResourceType == AllocType.External)
                NavmeshQueryEx.dtnqFree(ref root);

            root = IntPtr.Zero;
        }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Finds the nearest point on the surface of the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the search box does not intersect any polygons the search will return success, 
        /// but the result polygon reference will be zero.  So always check the polygon reference
        /// before using the point data.
        /// </para>
        /// <b>Warning:</b> This function is not suitable for large area searches.  If the 
        /// search extents overlaps more than 128 polygons it may return an invalid result.
        /// <para>
        /// The detail mesh is used to correct the y-value of result.
        /// </para>
        /// </remarks>
        /// <param name="searchPoint">The center of the search box.</param>
        /// <param name="extents">The search distance along each axis.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="result">The nearest point on the polygon.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the query.</returns>
        public NavStatus GetNearestPoint(Vector3 searchPoint, Vector3 extents
            , NavmeshQueryFilter filter
            , out NavmeshPoint result)
        {
            result = NavmeshPoint.Zero;

            return NavmeshQueryEx.dtqFindNearestPoly(root
                , ref searchPoint
                , ref extents
                , filter.root
                , ref result);
        }

        /// <summary>
        /// Returns the wall segments for the specified polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A segment that is normally a portal will be included in the result if the 
        /// filter results in the neighbor polygon being considered impassable.
        /// </para>
        /// <para>
        /// The vertex buffer must be sized for the maximum segments per polygon of the 
        /// source navigation mesh. I.e: 2 * <see cref="Navmesh.MaxAllowedVertsPerPoly"/>
        /// </para>
        /// <para>
        /// The segments can be used for simple 2D collision detection.
        /// </para>
        /// </remarks>
        /// <param name="polyRef">The polygon reference.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultSegments">
        /// The segment vertex buffer for all walls. [(vertA, vertB) * segmentCount]
        /// </param>
        /// <param name="segmentCount">
        /// The number of segments returned in the segments array.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyRef
            , NavmeshQueryFilter filter
            , Vector3[] resultSegments
            , out int segmentCount)
        {
            segmentCount = 0;

            return NavmeshQueryEx.dtqGetPolyWallSegments(root
                , polyRef
                , filter.root
                , resultSegments
                , null
                , ref segmentCount
                , resultSegments.Length / 2);
        }

        /// <summary>
        /// Returns the segments for the specified polygon, optionally excluding portals.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the segmentPolyRefs parameter is provided, then all polygon segments will be 
        /// returned.  If the parameter is null, then only  the wall segments are returned.
        /// </para>
        /// <para>
        /// A segment that is normally a portal will be included in the result as a wall 
        /// if the filter results in the neighbor polygon being considered impassable.
        /// </para>
        /// <para>
        /// The vertex and polyRef buffers must be sized for the maximum  segments per polygon of 
        /// the source navigation mesh. 
        /// I.e. <c>(2 * <see cref="Navmesh.MaxAllowedVertsPerPoly"/>)</c>
        /// </para>
        /// </remarks>
        /// <param name="polyRef">The polygon reference.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultSegments">
        /// The segment vertex buffer for all segments. [(vertA, vertB) * segmentCount]
        /// </param>
        /// <param name="segmentPolyRefs">
        /// Refernce ids of the each segment's neighbor polygon.  Or zero if the segment is 
        /// considered impassable. [(polyRef) * segmentCount] (Optional)</param>
        /// <param name="segmentCount">The number of segments returned.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolySegments(uint polyRef
            , NavmeshQueryFilter filter
            , Vector3[] resultSegments
            , uint[] segmentPolyRefs
            , out int segmentCount)
        {
            segmentCount = 0;

            return NavmeshQueryEx.dtqGetPolyWallSegments(root
                , polyRef
                , filter.root
                , resultSegments
                , segmentPolyRefs
                , ref segmentCount
                , resultSegments.Length / 2);
        }

        /// <summary>
        /// Gets all polygons whose AABB's overlap the search box.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a fast, but inaccurate query since only AABB's are checked.  A strict 
        /// polygon-box overlap check is not performed.
        /// </para>
        /// <para>
        /// If no polygons are found, the method will return success with a result count of zero.
        /// </para>
        /// <para>
        /// If the result buffer is too small to hold the entire result then the buffer 
        /// will be filled to capacity.  The method of  choosing which polygons from the full
        /// result are included in the partial result is undefined.
        /// </para>
        /// </remarks>
        /// <param name="searchPoint">The center of the query box.</param>
        /// <param name="extents">The search distance along each axis.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">
        /// The references of the polygons that overlap the query box. 
        /// [(polyRef) * resultCount] (Out)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolys(Vector3 searchPoint
            , Vector3 extents
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs
            , out int resultCount)
        {
            resultCount = 0;

            return NavmeshQueryEx.dtqQueryPolygons(root
                , ref searchPoint
                , ref extents
                , filter.root
                , resultPolyRefs
                , ref resultCount
                , resultPolyRefs.Length);
        }

        /// <summary>
        /// Finds the polygons within the graph that touch the specified circle.
        /// </summary>
        /// <remarks>
        /// <para>
        /// At least one result buffer must be provided.
        /// </para>
        /// <para>
        /// The order of the result is from least to highest cost to reach the polygon.
        /// </para>
        /// <para>
        /// The primary use case for this method is for performing Dijkstra searches.  
        /// Candidate polygons are found by searching the graph beginning at the start polygon.
        /// </para>
        /// <para>
        /// If a polygon is not found via the graph search, even if it intersects the 
        /// search circle, it will not be included in the result. Example scenario:</para>
        /// <para>
        /// polyA is the start polygon.<br/>
        /// polyB shares an edge with polyA. (Is adjacent.)<br/>
        /// polyC shares an edge with polyB, but not with polyA<br/>
        /// Even if the search circle overlaps polyC, it will not be included in the result 
        /// unless polyB is also in the set.
        /// </para>
        /// <para>
        /// The value of the center point is used as the start point for cost calculations.  
        /// It is not projected onto the surface of the mesh, so its y-value will effect the costs.
        /// </para>
        /// <para>
        /// Intersection tests occur in 2D.  All polygons and the search circle are projected
        /// onto the xz-plane.  So the y-value of the center point does not effect intersection 
        /// tests.
        /// </para>
        /// <para>If the buffers are to small to hold the entire result, they will be filled to 
        /// capacity.
        /// </para>
        /// </remarks>
        /// <param name="start">
        /// The center point to start from which to start the search. (Must be valid.)
        /// </param>
        /// <param name="radius">The radius of the query circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">
        /// The references of the polygons touched by the circle. 
        /// [(polyRef) * resultCount] (Optional)
        /// </param>
        /// <param name="resultParentRefs">
        /// The reference of the parent polygons for each result. Zero if a result polygon has no 
        /// parent. [(parentRef) * resultCount] (Optional)</param>
        /// <param name="resultCosts">
        /// The search cost from the center point to the polygon. [(cost) * resultCount] (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPolys(NavmeshPoint start, float radius
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs, uint[] resultParentRefs, float[] resultCosts
            , out int resultCount)
        {
            resultCount = 0;

            // Set max count to the smallest length.
            int maxCount = (resultPolyRefs == null ? 0 : resultPolyRefs.Length);

            maxCount = (resultParentRefs == null
                ? maxCount 
                : Math.Min(maxCount, resultParentRefs.Length));

            maxCount = (resultCosts == null 
                ? maxCount
                : Math.Min(maxCount, resultCosts.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.dtqFindPolysAroundCircle(root
                , start.polyRef
                , ref start.point
                , radius
                , filter.root
                , resultPolyRefs
                , resultParentRefs
                , resultCosts
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the navigation polygons within the graph that touch the specified convex polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The order of the result is from least to highest cost.
        /// </para>
        /// <para>
        /// At least one result buffer must be provided.
        /// </para>
        /// <para>
        /// The primary use case for this method is for performing Dijkstra searches.  
        /// Candidate polygons are found by searching the graph beginning at the start polygon.
        /// </para>
        /// <para>
        /// The same intersection test restrictions that apply to the circle version of this 
        /// method apply to this method.
        /// </para>
        /// <para>
        /// The 3D centroid of the polygon is used as the start position for cost 
        /// calculations.
        /// </para>
        /// <para>Intersection tests occur in 2D.  All polygons are projected onto the xz-plane,  
        /// so the y-values of the vertices do not effect intersection tests.
        /// </para>
        /// <para>
        /// If the buffers are is too small to hold the entire result, they will be 
        /// filled to capacity.
        /// </para>
        /// </remarks>
        /// <param name="startPolyRef">The reference of the polygon to start the search at.</param>
        /// <param name="vertices">
        /// The vertices of the convex polygon. [Length: vertCount]
        /// </param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">
        /// The references of the polygons touched by the search 
        /// polygon. [(polyRef) * resultCount] (Optional)
        /// </param>
        /// <param name="resultParentRefs">
        /// The references of the parent polygons for each result.
        /// Zero if a result polygon has no parent. [(parentRef) * resultCount] (Optional)
        /// </param>
        /// <param name="resultCosts">
        /// The search cost from the centroid point to the polygon. 
        /// [(cost) * resultCount] (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPolys(uint startPolyRef
            , Vector3[] vertices
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs
            , uint[] resultParentRefs
            , float[] resultCosts
            , out int resultCount)
        {
            resultCount = 0;

            // Set max count to the smallest length.
            int maxCount = (resultPolyRefs == null ? 0 : resultPolyRefs.Length);

            maxCount = (resultParentRefs == null 
                ? maxCount
                : Math.Min(maxCount, resultParentRefs.Length));

            maxCount = (resultCosts == null
                ? maxCount
                : Math.Min(maxCount, resultCosts.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.dtqFindPolysAroundShape(root
                , startPolyRef
                , vertices
                , vertices.Length
                , filter.root
                , resultPolyRefs
                , resultParentRefs
                , resultCosts
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the non-overlapping navigation polygons in the local neighborhood around the 
        /// specified point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is optimized for a small query radius and small number of result polygons.
        /// </para>
        /// <para>
        /// The order of the result is from least to highest cost.</para>
        /// <para>
        /// At least one result buffer must be provided.</para>
        /// <para>
        /// The primary use case for this method is for performing Dijkstra searches.  
        /// Candidate polygons are found by searching the graph beginning at the start polygon.
        /// </para>
        /// <para>
        /// The same intersection test restrictions that apply to the FindPoly methods apply 
        /// to this method.
        /// </para>
        /// <para>
        /// The value of the center point is used as the start point for cost calculations.  
        /// It is not projected onto the surface of the mesh, so its y-value will effect the costs.
        /// </para>
        /// <para>
        /// Intersection tests occur in 2D.  All polygons and the search circle are projected 
        /// onto the xz-plane, so the y-value of the center point does not effect intersection 
        /// tests.
        /// </para>
        /// <para>
        /// If the buffers are is too small to hold the entire result, they will be 
        /// filled to capacity.
        /// </para>
        /// </remarks>
        /// <param name="start">
        /// The center point to start from which to start the search. (Must be valid.)
        /// </param>
        /// <param name="radius">The radius of the search circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPolyRefs">
        /// The references of the polygons touched by the circle. 
        /// [(polyRef) * resultCount] (Optional)
        /// </param>
        /// <param name="resultParentRefs">
        /// The references of the parent polygons for each result.
        /// Zero if a result polygon has no parent. [(parentRef) * resultCount] (Optional)
        /// </param>
        /// <param name="resultCount">The number of polygons found.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolysLocal(NavmeshPoint start, float radius
            , NavmeshQueryFilter filter
            , uint[] resultPolyRefs, uint[] resultParentRefs, out int resultCount)
        {
            resultCount = 0;

            // Set max count to the smallest length.
            int maxCount = (resultPolyRefs == null ? 0 : resultPolyRefs.Length);

            maxCount = (resultParentRefs == null 
                ? maxCount
                : Math.Min(maxCount, resultParentRefs.Length));

            if (maxCount == 0)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.dtqFindLocalNeighbourhood(root
                , start.polyRef
                , ref start.point
                , radius
                , filter.root
                , resultPolyRefs
                , resultParentRefs
                , ref resultCount
                , maxCount);
        }

        /// <summary>
        /// Finds the closest point on the specified polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Uses the height detail to provide the most accurate information.
        /// </para>
        /// <para>
        /// The source point does not have to be within the bounds of the navigation mesh.
        /// </para>
        /// </remarks>
        /// <param name="polyRef">The polygon reference.</param>
        /// <param name="sourcePoint">The position to search from.</param>
        /// <param name="resultPoint">The closest point on the polygon.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPoint(uint polyRef, Vector3 sourcePoint, out Vector3 resultPoint)
        {
            resultPoint = Vector3Util.Zero;

            return NavmeshQueryEx.dtqClosestPointOnPoly(root
                , polyRef
                , ref sourcePoint
                , ref resultPoint);
        }

        /// <summary>
        /// Returns a point on the boundary closest to the source point if the source point is 
        /// outside the polygon's xz-column.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Much faster than the other nearest point methods.
        /// </para>
        /// <para>
        /// If the provided point lies within the polygon's xz-column (above or below), then 
        /// the source and result points will be equal.
        /// </para>
        /// <para>
        /// The boundary point will be the polygon boundary, not the height corrected detail 
        /// boundary.  Use <see cref="GetPolyHeight"/> if needed.
        /// </para>
        /// <para>The source point does not have to be within the bounds of the navigation mesh.
        /// </para>
        /// </remarks>
        /// <param name="polyRef">The polygon reference.</param>
        /// <param name="sourcePoint">The point to check.</param>
        /// <param name="resultPoint">The closest point.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetNearestPointF(uint polyRef
            , Vector3 sourcePoint
            , out Vector3 resultPoint)
        {
            resultPoint = Vector3Util.Zero;

            return NavmeshQueryEx.dtqClosestPointOnPolyBoundary(root
                , polyRef
                , ref sourcePoint
                , ref resultPoint);
        }

        /// <summary>
        /// Gets the height of the polygon at the provided point using the detail mesh. 
        /// (Most accurate.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method will return falure if the provided point is outside the xz-column 
        /// of the polygon.
        /// </para>
        /// </remarks>
        /// <param name="point">The point within the polygon's xz-column.</param>
        /// <param name="height">The height at the surface of the polygon.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetPolyHeight(NavmeshPoint point, out float height)
        {
            height = 0;

            return NavmeshQueryEx.dtqGetPolyHeight(root
                , point
                , ref height);
        }

        /// <summary>
        /// Returns the distance from the specified position to the nearest polygon wall.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The closest point is not height adjusted using the detail data. 
        /// Use <see cref="GetPolyHeight"/> if needed.
        /// </para>
        /// <para>
        /// The distance will equal the search radius if there is no wall within the radius.  
        /// In this case the values of closestPoint and normal are undefined.
        /// </para>
        /// <para>
        /// The normal will become unpredicable if the distance is a very small number.
        /// </para>
        /// </remarks>
        /// <param name="searchPoint">The center of the search circle.</param>
        /// <param name="searchRadius">The radius of the search circle.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="distance">Distance to nearest wall.</param>
        /// <param name="closestPoint">The nearest point on the wall.</param>
        /// <param name="normal">
        /// The normalized ray formed from the wall point to the source point.
        /// </param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindDistanceToWall(NavmeshPoint searchPoint
            , float searchRadius
            , NavmeshQueryFilter filter
            , out float distance
            , out Vector3 closestPoint
            , out Vector3 normal)
        {
            distance = 0;
            closestPoint = Vector3Util.Zero;
            normal = Vector3Util.Zero;

            return NavmeshQueryEx.dtqFindDistanceToWall(root
                , searchPoint
                , searchRadius
                , filter.root
                , ref distance
                , ref closestPoint
                , ref normal);
        }

        /// <summary>
        /// Finds the polygon path from the start to the end polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the end polygon cannot be reached, then the last polygon is the nearest one 
        /// found to the end polygon.
        /// </para>
        /// <para>
        /// If the path buffer is to small to hold the result, it will be filled as far as 
        /// possible from the start polygon toward the end polygon.
        /// </para>
        /// <para>
        /// The start and end points are used to calculate traversal costs. (y-values matter.)
        /// </para>
        /// </remarks>
        /// <param name="start">A point within the start polygon.</param>
        /// <param name="end">A point within the end polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">
        /// An ordered list of polygoon references in the path. (Start to end.) (Out) 
        /// [(polyRef) * pathCount]
        /// </param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPath(NavmeshPoint start, NavmeshPoint end
            , NavmeshQueryFilter filter
            , uint[] resultPath, out int pathCount)
        {
            pathCount = 0;

            return NavmeshQueryEx.dtqFindPath(root
                , start
                , end
                , filter.root
                , resultPath
                , ref pathCount
                , resultPath.Length);
        }

        /// <summary>
        /// Finds the polygon path from the start to the end polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is useful if the polygon reference of either the 
        /// <paramref name="start"/> or <paramref name="end"/> point is not known. If both points 
        /// have a polygon reference of zero, then this method is equivalent to the following:
        /// </para>
        /// <ol>
        /// <li>
        /// Using <see cref="GetNearestPoint(uint, Vector3, out Vector3)"/> with the 
        /// <paramref name="start"/> point to get the start polygon.
        /// </li>
        /// <li>
        /// Using <see cref="GetNearestPoint(uint, Vector3, out Vector3)"/> with the 
        /// <paramref name="end"/> point to get the end polygon.
        /// </li>
        /// <li>Calling the normal find path using the two new start and end points.</li>
        /// </ol>
        /// <para>
        /// <em>A point search will only be performed for points with a polygon reference 
        /// of zero.</em> If a point search is required, the point and its polygon reference 
        /// parameter become output parameters and the point will be snapped to the navigation mesh.
        /// </para>
        /// <para>
        /// This method may return a partial result, even if there is a failure.  If there is 
        /// no failure, it will at least perform the required point searches.  If the point
        /// searches succeed, then the find path operation will be performed.
        /// </para>
        /// <para>
        /// Checking the return results:
        /// </para>
        /// <ul>
        /// <li>If the <paramref name="pathCount"/> is greater than zero, then the path and all 
        /// required point searches succeeded.</li>
        /// <li>If the overall operation failed, but a point with an input polygon reference of 
        /// zero has an output polygon reference that is non-zero, then that point's search 
        /// succeeded.</li>
        /// </ul>
        /// <para>
        /// For the path results:
        /// </para>
        /// <para>
        /// If the end polygon cannot be reached, then the last polygon is the nearest one 
        /// found to the end polygon.
        /// </para>
        /// <para>
        /// If the path buffer is to small to hold the result, it will be filled as far as 
        /// possible from the start polygon toward the end polygon.</para>
        /// <para>
        /// The start and end points are used to calculate traversal costs. 
        /// (y-values matter.)
        /// </para>
        /// </remarks>
        /// <param name="start">
        /// A point within the start polygon. (In) (Out if the polygon reference is zero.)
        /// </param>
        /// <param name="end">
        /// A point within the end polygon. (In) (Out if the polygon reference is zero.)
        /// </param>
        /// <param name="extents">
        /// The search extents to use if the start or end point polygon reference is zero.
        /// </param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPath">
        /// An ordered list of polygon references in the path. (Start to end.) (Out)
        /// [(polyRef) * pathCount]
        /// </param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FindPath(ref NavmeshPoint start, ref NavmeshPoint end
            , Vector3 extents, NavmeshQueryFilter filter
            , uint[] resultPath, out int pathCount)
        {
            pathCount = 0;

            return NavmeshQueryEx.dtqFindPathExt(root
                , ref start
                , ref end
                , ref extents
                , filter.root
                , resultPath
                , ref pathCount
                , resultPath.Length);
        }

        /// <summary>
        /// Returns true if the polygon refernce is in the current closed list.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The closed list is the list of polygons that were fully evaluated during a find 
        /// operation.
        /// </para>
        /// <para>
        /// All methods prefixed with "Find" and all sliced path methods generate a closed 
        /// list.  The content of the list will persist until the next find/sliced method is called.
        /// </para>
        /// </remarks>
        /// <param name="polyRef">The polygon reference.</param>
        /// <returns>True if the polgyon is in the current closed list.</returns>
        public bool IsInClosedList(uint polyRef)
        {
            return NavmeshQueryEx.dtqIsInClosedList(root, polyRef);
        }

        /// <summary>
        /// Returns true if the polygon reference is valid and passes the filter restrictions.
        /// </summary>
        /// <param name="polyRef">The polygon reference.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>
        /// True if the polygon reference is valid and passes the filter restrictions.
        /// </returns>
        public bool IsValidPolyRef(uint polyRef, NavmeshQueryFilter filter)
        {
            return NavmeshQueryEx.dtqIsValidPolyRef(root, polyRef, filter.root);
        }

        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh from the start point 
        /// toward the end point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is meant to be used for quick short distance checks.
        /// </para>
        /// <para>
        /// If the path buffer is too small to hold the result, it will be filled as far as 
        /// possible from the start point toward the end point.</para>
        /// <para>
        /// <b>Using the Hit Paramter</b></para>
        /// <para>
        /// If the hit parameter is a very high value (>1E38), then the ray has hit the 
        /// end point.  In this case the path represents a valid corridor to the end point and 
        /// the value of hitNormal is undefined.
        /// </para>
        /// <para>
        /// If the hit parameter is zero, then the start point is on the border that was hit 
        /// and the value of hitNormal is undefined.
        /// </para>
        /// <para>
        /// If <c>0 &lt; hitParameter &lt; 1.0 </c> then the following applies:</para>
        /// <code>
        /// distanceToHitBorder = distanceToEndPoint * hitParameter
        /// hitPoint = startPoint + (endPoint - startPoint) * hitParameter
        /// </code>
        /// <para>
        /// <b>Use Case Restriction</b>
        /// </para>
        /// <para>
        /// The raycast ignores the y-value of the end point.  (2D check) This places 
        /// significant limits on how it can be used.  Example scenario:</para>
        /// <para>
        /// Consider a scene where there is a main floor with a second floor balcony that 
        /// hangs over the main floor.  So the first floor mesh extends below the balcony mesh.  
        /// The start point is somewhere on the first floor.  The end point is on the balcony.
        /// </para>
        /// <para>
        /// The raycast will search toward the end point along the first floor mesh.  If it 
        /// reaches the end point's xz-coordinates it will indicate 'no hit', meaning it reached 
        /// the end point.
        /// </para>
        /// </remarks>
        /// <param name="start">
        /// A point within the start polygon representing the start of the ray.
        /// </param>
        /// <param name="end">The point to cast the ray toward.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="hitParameter">The hit parameter.  (>1E38 if no hit.)</param>
        /// <param name="hitNormal">The normal of the nearest wall hit.</param>
        /// <param name="path">
        /// The references of the visited polygons. [(polyRef) * pathCount] (Optional)
        /// </param>
        /// <param name="pathCount">The number of visited polygons.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus Raycast(NavmeshPoint start, Vector3 end
            , NavmeshQueryFilter filter
            , out float hitParameter, out Vector3 hitNormal
            , uint[] path, out int pathCount)
        {
            pathCount = 0;
            hitParameter = 0;
            hitNormal = Vector3Util.Zero;

            int maxCount = (path == null ? 0 : path.Length);

            return NavmeshQueryEx.dtqRaycast(root
                , start
                , ref end
                , filter.root
                , ref hitParameter
                , ref hitNormal
                , path
                , ref pathCount
                , maxCount);
        }

        /// <summary>
        /// Returns the staight path from the start to the end point within the polygon corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method peforms what is often called 'string pulling'.
        /// </para>
        /// <para>
        /// If the provided result buffers are too small for the entire result, they will 
        /// be filled as far as possible from the start point toward the end point.
        /// </para>
        /// <para>
        /// The start point is clamped to the first polygon in the path, and the end point 
        /// is clamped to the last. So the start  and end points should be within or very near the 
        /// first and last polygons respectively.  The pathStart and pathCount parameters can be 
        /// adjusted to restrict the usable portion of the the path to  meet this requirement. 
        /// (See the example below.)
        /// </para>
        /// <para>
        /// The returned polygon references represent the polygon that is entered at the 
        /// associated path point.  The reference associated with the end point will always be 
        /// zero.
        /// </para>
        /// <para>
        /// Example use case for adjusting the straight path during locomotion:
        /// </para>
        /// <para>
        /// Senario: The path consists of polygons A, B, C, D, with the start point in A and 
        /// the end point in D.
        /// </para>
        /// <para>
        /// The first call to the method will return straight waypoints for the entire path:
        /// </para>
        /// <code>
        /// query.GetStraightPath(startPoint, endPoint
        ///     , path
        ///     , 0, 4   // pathStart, pathCount
        ///     , straigthPath, null, null
        ///     , out straightCount);
        /// </code>
        /// <para>
        /// If the agent moves into polygon B and needs to recaclulate its straight path for 
        /// some reason, it can call the method as follows using the original path buffer:
        /// </para>
        /// <code>
        /// query.GetStraightPath(startPoint, endPoint
        ///     , path
        ///     , 1, 3   // pathStart, pathCount  &lt;- Note the changes here.
        ///     , straigthPath, null, null
        ///     , out straightCount);
        /// </code>
        /// </remarks>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <param name="path">
        /// The list of polygon references that represent the path corridor.
        /// </param>
        /// <param name="pathStart">
        /// The index within the path buffer of the polygon that contains the start point.
        /// </param>
        /// <param name="pathCount">
        /// The length of the path within the path buffer. (endPolyIndex - startPolyIndex)
        /// </param>
        /// <param name="resultPoints">
        /// Points describing the straight path. [(point) * straightPathCount].</param>
        /// <param name="resultFlags">
        /// Flags describing each point. [(flags) * striaghtPathCount] (Optional)</param>
        /// <param name="resultRefs">
        /// The reference of the polygon that is being entered at the point position.
        /// [(polyRef) * straightPathCount] (Optional)</param>
        /// <param name="resultCount">The number of points in the straight path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetStraightPath(Vector3 start, Vector3 end
            , uint[] path, int pathStart, int pathCount
            , Vector3[] resultPoints, WaypointFlag[] resultFlags, uint[] resultRefs
            , out int resultCount)
        {
            resultCount = 0;

            int maxPath = resultPoints.Length;

            maxPath = (resultFlags == null ? maxPath
                : Math.Min(resultFlags.Length, maxPath));

            maxPath = (resultRefs == null ? maxPath
                : Math.Min(resultRefs.Length, maxPath));

            if (maxPath < 1)
                return (NavStatus.Failure | NavStatus.InvalidParam);

            return NavmeshQueryEx.dtqFindStraightPath(root
                , ref start
                , ref end
                , path
                , pathStart
                , pathCount
                , resultPoints
                , resultFlags
                , resultRefs
                , ref resultCount
                , maxPath);
        }

        /// <summary>
        /// Moves from the start to the end point constrained to the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is optimized for small delta movement and a small number of polygons. 
        /// If used for too great a distance, the result will form an incomplete path.</para>
        /// <para>
        /// The result point will equal the end point if the end is reached. Otherwise the 
        /// closest reachable point will be returned.
        /// </para>
        /// <para>
        /// The result position is not projected to the surface of the navigation mesh.  If 
        /// that is needed, use <see cref="GetPolyHeight"/>.</para>
        /// <para>
        /// This method treats the end point in the same manner as the <see cref="Raycast"/> 
        /// method.  (As a 2D point.) See that method's documentation for details on the impact.
        /// </para>
        /// <para>
        /// If the result buffer is too small to hold the entire result, it will be 
        /// filled as far as possible from the start point toward the end point.
        /// </para>
        /// </remarks>
        /// <param name="start">A position within the start polygon.</param>
        /// <param name="end">The end position.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="resultPoint">The result point from the move.</param>
        /// <param name="visitedPolyRefs">The references of the polygons
        /// visited during the move. [(polyRef) * visitedCount]</param>
        /// <param name="visitedCount">The number of polygons visited during
        /// the move.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus MoveAlongSurface(NavmeshPoint start, Vector3 end
            , NavmeshQueryFilter filter
            , out Vector3 resultPoint
            , uint[] visitedPolyRefs, out int visitedCount)
        {
            visitedCount = 0;
            resultPoint = Vector3Util.Zero;

            return NavmeshQueryEx.dtqMoveAlongSurface(root
                , start
                , ref end
                , filter.root
                , ref resultPoint
                , visitedPolyRefs
                , ref visitedCount
                , visitedPolyRefs.Length);
        }

        /// <summary>
        /// Returns a random point on the navigation mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The search speed is linear to the number of polygons.
        /// </para>
        /// </remarks>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="randomPoint">A random point on the navigation mesh.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetRandomPoint(NavmeshQueryFilter filter, out NavmeshPoint randomPoint)
        {
            randomPoint = new NavmeshPoint();

            return NavmeshQueryEx.dtqFindRandomPoint(root, filter.root, ref randomPoint);
        }

        /// <summary>
        /// Returns a random point within reach of the specified location.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The result point is constrainted to the polygons overlapped by the circle, not 
        /// the circle itself.  The overlap test follows the same rules as the FindPolys method.
        /// </para>
        /// <para>
        /// The search speed is linear to the number of polygons.
        /// </para>
        /// </remarks>
        /// <param name="start">The point to search from.</param>
        /// <param name="radius">The polygon overlap radius.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="randomPoint">A random point within reach of the specified location.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus GetRandomPoint(NavmeshPoint start, float radius
            , NavmeshQueryFilter filter
            , out NavmeshPoint randomPoint)
        {
            randomPoint = new NavmeshPoint();

            return NavmeshQueryEx.dtqFindRandomPointCircle(root, start, radius
                , filter.root
                , ref randomPoint);
        }

        /// <summary>
        /// Initializes a sliced path find query.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will fail if <see cref="IsRestricted"/> is true.
        /// </para>
        /// <para>
        /// <b>Warning:</b> Calling any other query methods besides the other sliced path methods 
        /// before finalizing this query may result in corrupted data.
        /// </para>
        /// <para>
        /// The filter is stored and used for the duration of the query.
        /// </para>
        /// <para>
        /// The standard use case:
        /// </para>
        /// <ol>
        /// <li>Initialize the sliced path query</li>
        /// <li>Call <see cref="UpdateSlicedFindPath"/> until its status returns complete.</li>
        /// <li>Call <see cref="FinalizeSlicedFindPath"/> to get the path.</li>
        /// </ol>
        /// </remarks>
        /// <param name="start">A point within the start polygon.</param>
        /// <param name="end">A point within the end polygon.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus InitSlicedFindPath(NavmeshPoint start, NavmeshPoint end
            , NavmeshQueryFilter filter)
        {
            if (mIsRestricted)
                return NavStatus.Failure;

            return NavmeshQueryEx.dtqInitSlicedFindPath(root
                , start
                , end
                , filter.root);
        }

        /// <summary>
        /// Continues a sliced path find query.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will fail if <see cref="IsRestricted"/> is true.
        /// </para>
        /// </remarks>
        /// <param name="maxIterations">The maximum number of iterations to perform.</param>
        /// <param name="actualIterations">The actual number of iterations performed.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus UpdateSlicedFindPath(int maxIterations
            , out int actualIterations)
        {
            actualIterations = 0;

            if (mIsRestricted)
                return NavStatus.Failure;

            return NavmeshQueryEx.dtqUpdateSlicedFindPath(root
                , maxIterations
                , ref actualIterations);
        }

        /// <summary>
        /// Finalizes and returns the results of the sliced path query.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will fail if <see cref="IsRestricted"/> is true.
        /// </para>
        /// </remarks>
        /// <param name="path">An ordered list of polygons representing the path. 
        /// [(polyRef) * pathCount]</param>
        /// <param name="pathCount">The number of polygons in the path.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the query.</returns>
        public NavStatus FinalizeSlicedFindPath(uint[] path
            , out int pathCount)
        {
            pathCount = 0;

            if (mIsRestricted)
                return NavStatus.Failure;

            return NavmeshQueryEx.dtqFinalizeSlicedFindPath(root
                , path
                , ref pathCount
                , path.Length);
        }

        /// <summary>
        /// Creates a new navigation mesh query based on the provided navigation mesh.
        /// </summary>
        /// <param name="navmesh">A navigation mesh to query against.</param>
        /// <param name="maximumNodes">
        /// The maximum number of nodes allowed when performing A* and Dijkstra searches.
        /// </param>
        /// <param name="resultQuery">A navigation mesh query object.</param>
        /// <returns>The <see cref="NavStatus" /> flags for the build request.</returns>
        public static NavStatus Create(Navmesh navmesh
            , int maximumNodes
            , out NavmeshQuery resultQuery)
        {
            IntPtr query = IntPtr.Zero;

            NavStatus status = NavmeshQueryEx.dtnqBuildDTNavQuery(
                navmesh.root
                , maximumNodes
                , ref query);

            if (NavUtil.Succeeded(status))
                resultQuery = new NavmeshQuery(query, false, AllocType.External);
            else
                resultQuery = null;

            return status;
        }

    }
}
