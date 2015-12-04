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
    /// Represents a dynamic polygon corridor used to plan client movement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The corridor is loaded with a path, usually obtained from a 
    /// <see cref="NavmeshQuery">NavmeshQuery</see> <c> FindPath</c> call.  The corridor
    /// is then used to plan local movement, with the corridor automatically updating as needed to 
    /// deal with inaccurate client locomotion.
    /// </para>
    /// <para>
    /// Example of a common use case:</para>
    /// <ol>
    /// <li>Construct the corridor object using the <see cref="NavmeshQuery"/> 
    /// and <see cref="NavmeshQueryFilter"/> objects in use by the navigation client.</li>
    /// <li>Obtain a path from a <see cref="NavmeshQuery"/> object.</li>
    /// <li>Use <see cref="Reset"/> to load the client's current position. 
    /// (At the beginning of the path.)</li>
    /// <li>Use <see cref="SetCorridor"/> to load the path and target.</li>
    /// <li>Use <see cref="Corners"/> to plan movement. (This is the straightend path.)</li>
    /// <li>Use the <see cref="MovePosition"/> to feed client movement back into the corridor. 
    /// (Or <see cref="Move"/> if the target is dynamic.) The corridor will automatically adjust 
    /// as needed.</li>
    /// <li>Repeat the previous 2 steps to continue to move the client.</li>
    /// </ol>
    /// <para>
    /// The corridor position and target are always constrained to  the navigation mesh.
    /// </para>
    /// <para>
    /// One of the difficulties in maintaining a path is that floating point errors, locomotion 
    /// inaccuracies, and/or local steering can result in  the client crossing the boundary of the 
    /// path corridor, temporarily invalidating the path.  This class uses local mesh queries 
    /// to detect and update the corridor as needed to handle these types of issues.
    /// </para>
    /// <para>
    /// The fact that local mesh queries are used to move the position and target locations 
    /// results in two beahviors that need to be considered:
    /// </para>
    /// <para>
    /// Every time a move method is used there is a chance that the path will become non-optimial. 
    /// Basically, the further the target is moved from its original location, and the further the 
    /// position is moved outside the original corridor, the more likely the path will become 
    /// non-optimal. This issue can be addressed by periodically running the 
    /// <see cref="OptimizePathTopology"/> and <see cref="OptimizePathVisibility"/> methods.
    /// </para>
    /// <para>
    /// All local mesh queries have distance limitations. (Review the <see cref="NavmeshQuery"/> 
    /// methods for details.) So the most accurate use case is to move the position and target in 
    /// small increments.  If a large increment is used, then the corridor may not be able to 
    /// accurately find the new location.  Because of this limiation, if a position is moved in a 
    /// large increment, then compare the desired and resulting polygon references. If the two do 
    /// not match, then path replanning may be needed.  E.g. If you move the target, check 
    /// the polygon reference of <see cref="Target"/> to see if it is as expected.
    /// </para>
    /// </remarks>
    public sealed class PathCorridor
        : IManagedObject
    {
        private IntPtr mRoot;
        private int mMaxPathSize;
        private NavmeshQueryFilter mFilter;
        private NavmeshQuery mQuery;

        private NavmeshPoint mPosition;
        private NavmeshPoint mTarget;

        private CornerData mCorners;

        /// <summary>
        /// The corner point for the specified index.
        /// </summary>
        /// <param name="index">The corner index. [Limit: &lt;= <see cref="MaxCorners"/></param>
        /// <returns>The corner point.</returns>
        public NavmeshPoint this[int index] { get { return mCorners[index]; } }

        /// <summary>
        /// The maximum path size that can be handled by the corridor.
        /// </summary>
        public int MaxPathSize { get { return mMaxPathSize; } }

        /// <summary>
        /// The maximum number of corners that the corner buffer can hold.
        /// </summary>
        public int MaxCorners { get { return mCorners.MaxCorners; } }

        /// <summary>
        /// The type of unmanaged resource used by the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mRoot == IntPtr.Zero); } }

        /// <summary>
        /// The query object used by the corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property can't be used to set the query to null.
        /// </para>
        /// </remarks>
        public NavmeshQuery Query
        {
            get { return mQuery; }
            set 
            {
                if (value != null)
                    mQuery = value; 
            }
        }

        /// <summary>
        /// The query filter used by the corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property can't be used to set the filter to null.
        /// </para>
        /// </remarks>
        public NavmeshQueryFilter Filter
        {
            get { return mFilter; }
            set 
            { 
                if (value != null)
                    mFilter = value; 
            }
        }

        /// <summary>
        /// The position within the first polygon of the corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is automatically constrained to the navigation mesh.
        /// </para>
        /// </remarks>
        public NavmeshPoint Position { get { return mPosition; } }

        /// <summary>
        /// The target within the last polygon of the corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is automatically constrained to the navigation mesh.
        /// </para>
        /// </remarks>
        public NavmeshPoint Target { get { return mTarget; } }


        /// <summary>
        /// The straight path corners for the corridor. (The string-pulled path.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Due to internal optimizations, the maximum number of detectable corners returned will 
        /// be <c>(Corners.MaxCorners - 1)</c> For example: If the corner buffers are sized to 
        /// hold 10 corners, there will never be more than 9 corners available.
        /// </para>
        /// <para>
        /// If the target is within range, it will be the last corner and have a polygon reference of zero.
        /// </para>
        /// <para>
        /// This is a reference, not a copy, of the internal data buffer. It should be treated with
        /// care. (I.e. Not mutated.)
        /// </para>
        /// </remarks>
        public CornerData Corners { get { return mCorners; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Important:</b> The <see cref="Reset"/> method must be called before the corridor 
        /// can be used. (That is how the position is set.)
        /// </para>
        /// <para>
        /// Due to internal optimizations, the maximum number of detectable corners will be 
        /// <c>(<paramref name="maxCorners"/> - 1)</c>.
        /// </para>
        /// <para>The query and filter parameters can be set to null.  This supports the ability 
        /// to create pools of re-usable path corridor objects.  But it means that care needs to 
        /// be taken not to use the corridor until query and filter objects have been set.
        /// See <see cref="ReleaseLocals"/> and <see cref="LoadLocals"/> for pool related utility 
        /// functions.
        /// </para>
        /// </remarks>
        /// <param name="maxPathSize">
        /// The maximum path size that can be handled by the object. [Limit: >= 1]
        /// </param>
        /// <param name="maxCorners">
        /// The maximum number of corners the corner buffer can hold. [Limit: >= 2]
        /// </param>
        /// <param name="query">The query to be used by the corridor.</param>
        /// <param name="filter">The query filter to be used by the corridor.</param>
        public PathCorridor(int maxPathSize, int maxCorners
            , NavmeshQuery query, NavmeshQueryFilter filter)
        {
            maxPathSize = Math.Max(1, maxPathSize);

            mRoot = PathCorridorEx.dtpcAlloc(maxPathSize);

            if (mRoot == IntPtr.Zero)
            {
                mMaxPathSize = 0;
                return;
            }

            mQuery = query;
            mFilter = filter;
            mMaxPathSize = maxPathSize;
            mCorners = new CornerData(Math.Max(2, maxCorners));
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~PathCorridor()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Immediately frees all unmanaged resources allocated by the object.
        /// </summary>
        public void RequestDisposal()
        {
            if (!IsDisposed)
            {
                PathCorridorEx.dtpcFree(mRoot);
                mRoot = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Resizes the corner buffers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Due to internal optimizations, the maximum number of detectable corners will be 
        /// <c>(<paramref name="maxCorners"/> - 1)</c>.
        /// </para>
        /// <para>
        /// All possible corner buffer state will be preserved. (Some state may be lost if the 
        /// buffer is reduced in size.)
        /// </para>
        /// </remarks>
        /// <param name="maxCorners">
        /// The maximum number of corners the corner buffer can hold. [Limit: >= 2]
        /// </param>
        public void ResizeCornerBuffer(int maxCorners)
        {
            CornerData nc = new CornerData(Math.Max(2, maxCorners));
            CornerData.Copy(mCorners, nc);
            mCorners = nc;
        }

        /// <summary>
        /// Resets the corridor to the specified position.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method sets the position and target to the specified location, and reduces the 
        /// corridor to the location's polygon. (Path size = 1)
        /// </para>
        /// <para>
        /// This method does not perform any validation of the input data.
        /// </para>
        /// </remarks>
        /// <param name="position">The position of the client.</param>
        public void Reset(NavmeshPoint position)
        {
            PathCorridorEx.dtpcReset(mRoot, position);

            mPosition = position;
            mTarget = position;

            mCorners.cornerCount = 1;
            mCorners.verts[0] = position.point;
            mCorners.flags[0] = WaypointFlag.Start | WaypointFlag.End;
            mCorners.polyRefs[0] = position.polyRef;
        }

        /// <summary>
        /// Finds the corners in the corridor from the position toward the target. 
        /// (The straightened path.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can be used to do corner searches that exceed the capacity of the 
        /// corridor's normal corner buffers.
        /// </para>
        /// <para>
        /// This method performs essentially the same function as 
        /// <see cref="NavmeshQuery.GetStraightPath"/>.
        /// </para>
        /// <para>
        /// Due to internal optimizations, the actual maximum number of corners returned 
        /// will be <c>(buffer.MaxCorners - 1)</c>
        /// </para>
        /// <para>
        /// If the target is within range, it will be the last corner and have a polygon 
        /// reference of zero.
        /// </para>
        /// <para>
        /// Behavior is undefined if the buffer structure is malformed. E.g. The flag and polygon 
        /// buffers are different sizes.
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer to load the results into. [Length: >= 2]</param>
        /// <returns>The number of corners returned in the buffers.</returns>
        public int FindCorners(CornerData buffer)
        {
            buffer.cornerCount = PathCorridorEx.dtpcFindCorners(mRoot
                , buffer.verts, buffer.flags, buffer.polyRefs, buffer.polyRefs.Length
                , mQuery.root, mFilter.root);

            return buffer.cornerCount;
        }

        /// <summary>
        /// Attempts to optimize the path if the specified point is visible from the current 
        /// position.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Improves pathfinding appearance when using meshes that contain non-border.
        /// vertices.  (E.g. Tiled meshes and meshes constructed using multiple areas.)
        /// </para>
        /// <para>
        /// The only time <paramref name="updateCorners"/> should be set to false is if a move 
        /// or other optimization method is to be called next. Otherwise the corner data may 
        /// become invalid.
        /// </para>
        /// <para>
        /// Inaccurate locomotion or dynamic obstacle avoidance can force the agent position 
        /// significantly outside the original corridor. Over time this can result in the 
        /// formation of a non-optimal corridor. A non-optimal corridor can also form near
        /// non-border vertices.  (I.e. At tile corners or area transitions.)
        /// </para>
        /// <para>
        /// This function uses an efficient local visibility search to try to optimize the corridor
        /// between the current position and <paramref name="next"/>.
        /// </para>
        /// <para>
        /// The corridor will change only if <paramref name="next"/> is visible from the 
        /// current position and moving directly toward the point is better than following the 
        /// existing path.
        /// </para>
        /// <para>
        /// The more inaccurate the client movement, the more beneficial this method becomes.  
        /// Simply adjust the frequency of the call to match the needs to the client.
        /// </para>
        /// <para>
        /// This method is not suitable for long distance searches.
        /// </para>
        /// </remarks>
        /// <param name="next">The point to search toward.</param>
        /// <param name="optimizationRange">The maximum range to search. [Limit: > 0]</param>
        /// <param name="updateCorners">True if the corners data should be refreshed.</param>
        public void OptimizePathVisibility(Vector3 next, float optimizationRange
            , bool updateCorners)
        {
            if (updateCorners)
            {
                mCorners.cornerCount = PathCorridorEx.dtpcOptimizePathVisibilityExt(mRoot
                    , ref next, optimizationRange
                    , mCorners.verts, mCorners.flags, mCorners.polyRefs, mCorners.polyRefs.Length
                    , mQuery.root, mFilter.root);
            }
            else
            {
                PathCorridorEx.dtpcOptimizePathVisibility(
                    mRoot, ref next, optimizationRange, mQuery.root, mFilter.root);
            }
        }

        /// <summary>
        /// Attempts to optimize the path using a local area search.
        /// (Partial replanning.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Improves pathfinding appearance in crowded areas and for complex meshes.
        /// </para>
        /// <para>
        /// The only time <paramref name="updateCorners"/> should be set to false is if a move or 
        /// another optimization method is to be called next. Otherwise the corner data may 
        /// become invalid.
        /// </para>
        /// <para>
        /// Inaccurate locomotion or dynamic obstacle avoidance can force the client position 
        /// significantly outside the original corridor. Over time this can result in the 
        /// formation of a non-optimal corridor.  This method will use a local area path search 
        /// to try to re-optimize the corridor.
        /// </para>
        /// <para>
        /// The more inaccurate the client movement, the more beneficial this method becomes.  
        /// Simply adjust the frequency of the call to match the needs to the client.
        /// </para>
        /// <para>
        /// This is a local optimization.  It usually doesn't effect the entire corridor 
        /// through to the goal.  It should normally be called based on a time increment rather 
        /// than movement events. I.e. Call once a second.
        /// </para>
        /// </remarks>
        /// <param name="updateCorners">True if the corners data should be refreshed.</param>
        public void OptimizePathTopology(bool updateCorners)
        {
            if (updateCorners)
            {
                mCorners.cornerCount = PathCorridorEx.dtpcOptimizePathTopologyExt(mRoot
                    , mCorners.verts, mCorners.flags, mCorners.polyRefs, mCorners.polyRefs.Length
                    , mQuery.root, mFilter.root);
            }
            else
                PathCorridorEx.dtpcOptimizePathTopology(mRoot, mQuery.root, mFilter.root);
        }

        /// <summary>
        /// Moves over an off-mesh connection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is minimally tested and documented.
        /// </para>
        /// </remarks>
        /// <param name="connectionRef">The connection polygon reference.</param>
        /// <param name="endpointRefs">Polygon endpoint references. [Length: 2]</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="endPosition">The end position.</param>
        /// <returns>True if the operation succeeded.</returns>
        public bool MoveOverConnection(uint connectionRef, uint[] endpointRefs
            , Vector3 startPosition, Vector3 endPosition)
        {
            return PathCorridorEx.dtpcMoveOverOffmeshConnection(mRoot
                , connectionRef, endpointRefs, ref startPosition, ref endPosition, ref mPosition
                , mQuery.root);
        }

        /// <summary>
        /// Moves the position from its current location to the desired location, adjusting the 
        /// corridor as needed to reflect the new position.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior:
        /// </para>
        /// <ul>
        /// <li>The movement is constrained to the surface of the navigation mesh.</li>
        /// <li>The corridor is automatically adjusted (shorted or lengthened) and 
        /// <see cref="Corners"/> updated in order to remain valid.</li>
        /// <li>The new position will be located in the adjusted corridor's first polygon.</li>
        /// </ul>
        /// <para>
        /// The expected use case: The desired position will be 'near' the corridor. What is 
        /// considered 'near' depends on local polygon density, query search extents, etc.
        /// </para>
        /// <para>
        /// The resulting position will differ from the desired position if the desired position 
        /// is not on the navigation mesh, or it can't be reached using a local search.
        /// </para>
        /// </remarks>
        /// <param name="desiredPosition">The desired position.</param>
        /// <returns>The result of the move.</returns>
        public NavmeshPoint MovePosition(Vector3 desiredPosition)
        {
            mCorners.cornerCount = PathCorridorEx.dtpcMovePosition(mRoot
                , ref desiredPosition, ref mPosition
                , mCorners.verts, mCorners.flags, mCorners.polyRefs, mCorners.polyRefs.Length
                , mQuery.root, mFilter.root);

            return mPosition;
        }

        /// <summary>
        /// Moves the target from its curent location to the desired location, adjusting the 
        /// corridor as needed to reflect the change.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Behavior:
        /// </para>
        /// <ul>
        /// <li>The movement is constrained to the surface of the navigation mesh.</li>
        /// <li>The corridor is automatically adjusted (shorted or lengthened) and 
        /// <see cref="Corners"/> updated in order to remain valid.</li>
        /// <li>The new position will be located in the adjusted corridor's last polygon.</li>
        /// </ul>
        /// <para>
        /// The expected use case: The desired target will be 'near' the corridor. What is 
        /// considered 'near' depends on local polygon density, query search extents, etc.
        /// </para>
        /// <para>
        /// The resulting target will differ from the desired target if the desired target is 
        /// not on the navigation mesh, or it can't be reached using a local search.
        /// </para>
        /// </remarks>
        /// <param name="desiredTarget">The desired target.</param>
        /// <returns>The result of the move.</returns>
        public NavmeshPoint MoveTarget(Vector3 desiredTarget)
        {
            mCorners.cornerCount = PathCorridorEx.dtpcMoveTargetPosition(mRoot
                    , ref desiredTarget, ref mTarget
                    , mCorners.verts, mCorners.flags, mCorners.polyRefs, mCorners.polyRefs.Length
                    , mQuery.root, mFilter.root);

            return mTarget;
        }

        /// <summary>
        /// Moves the position and target from their curent locations to the desired locations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Performs an aggregrate operation in the following order:
        /// </para>
        /// <ol>
        /// <li><see cref="MoveTarget"/></li>
        /// <li><see cref="MovePosition"/></li>
        /// </ol>
        /// <para>
        /// See the documentation of the related functions for details on behavior.
        /// </para>
        /// <para>
        /// This method is more efficient than calling the other methods individually.
        /// </para>
        /// </remarks>
        /// <param name="desiredPosition">The desired position.</param>
        /// <param name="desiredTarget">The desired target.</param>
        public void Move(Vector3 desiredPosition, Vector3 desiredTarget)
        {
            mCorners.cornerCount = PathCorridorEx.dtpcMove(mRoot
                , ref desiredPosition, ref desiredTarget, ref mPosition, ref mTarget
                , mCorners.verts, mCorners.flags, mCorners.polyRefs, mCorners.polyRefs.Length
                , mQuery.root, mFilter.root);
        }

        /// <summary>
        /// Loads a new path and target into the corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The current position is expected to be within the first
        /// polygon in the path.  The target is expected to be in the last 
        /// polygon.
        /// </para>
        /// </remarks>
        /// <param name="target">The target location within the last polygon of the path.</param>
        /// <param name="path">
        /// The path corridor. [(polyRef) * <paramref name="pathCount"/>]
        /// </param>
        /// <param name="pathCount">
        /// The number of polygons in the path. 
        /// [Limits: 0 &lt;= value &lt;= <see cref="MaxPathSize"/>]
        /// </param>
        public void SetCorridor(Vector3 target
            , uint[] path
            , int pathCount)
        {
            mCorners.cornerCount = PathCorridorEx.dtpcSetCorridor(mRoot
                , ref target, path, pathCount, ref mTarget
                , mCorners.verts, mCorners.flags, mCorners.polyRefs, mCorners.polyRefs.Length
                , mQuery.root, mFilter.root);
        }

        /// <summary>
        /// Obtains a copy of the corridor path.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The buffer should be sized to hold the entire path.
        /// (See: <see cref="GetPathCount"/> and <see cref="MaxPathSize"/>.)
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer to load with the result. [(polyRef) * pathCount]</param>
        /// <returns>The number of polygons in the path.</returns>
        public int GetPath(uint[] buffer)
        {
            return PathCorridorEx.dtpcGetPath(mRoot, buffer, buffer.Length);
        }

        /// <summary>
        /// The number of polygons in the corridor path.
        /// </summary>
        /// <returns>The number of polygons in the corridor path.
        /// </returns>
        public int GetPathCount()
        {
            return PathCorridorEx.dtpcGetPathCount(mRoot);
        }

        /// <summary>
        /// Checks the corridor path to see if its polygon references remain valid.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The path can be invalidated if there are structural changes to the underlying 
        /// navigation mesh, or the state of a polygon within the path changes resulting in it 
        /// being filtered out. (E.g. An exclusion or inclusion flag changes.)
        /// </para>
        /// </remarks>
        /// <param name="maxLookAhead">
        /// The number of polygons from the beginning of the corridor to search.
        /// </param>
        /// <returns>True if the seached portion of the path is still valid.</returns>
        public bool IsValid(int maxLookAhead)
        {
            return PathCorridorEx.dtpcIsValid(mRoot, maxLookAhead, mQuery.root, mFilter.root);
        }

        /// <summary>
        /// Loads the corridor data into the provided <see cref="PathCorridorData"/> buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Make sure the buffer is sized to hold the entire result.
        /// (See: <see cref="GetPathCount"/> and <see cref="MaxPathSize"/>.)
        /// </para>
        /// </remarks>
        /// <param name="buffer">
        /// The buffer to load the data into. 
        /// [Length: Maximum Path Size >= <see cref="GetPathCount"/>]
        /// </param>
        /// <returns>False if the operation failed.</returns>
        public bool GetData(PathCorridorData buffer)
        {
            // Only performs a partial parameter validation.
            if (buffer == null|| buffer.path == null|| buffer.path.Length < 1)
                return false;

            if (buffer.path.Length == PathCorridorData.MarshalBufferSize)
                return PathCorridorEx.dtpcGetData(mRoot, buffer);

            buffer.pathCount = GetPath(buffer.path);
            buffer.position = mPosition.point;
            buffer.target = mTarget.point;

            return true;
        }

        /// <summary>
        /// Released the references to the query and filter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Warning:</b> The corridor will not be in a useable state after this operation.  
        /// Using the corridor without successfully calling <see cref="LoadLocals"/> will result in 
        /// undefined behavior.
        /// </para>
        /// <para>
        /// This method is useful when pooling path corridors for use by mulitple clients.
        /// </para>
        /// </remarks>
        /// <param name="corridor">The corridor to update.</param>
        public static void ReleaseLocals(PathCorridor corridor)
        {
            corridor.mQuery = null;
            corridor.mFilter = null;
        }

        /// <summary>
        /// Sets the specified resources and resets the corridor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is useful when pooling path corridors for use by mulitple clients.
        /// </para>
        /// <para>
        /// See <see cref="Reset"/> for information on the effect of the reset.
        /// </para>
        /// <para>
        /// Existing references will be replaced by the new references.
        /// </para>
        /// <para>
        /// This method cannot be used to set references to null. Attempting to do so will result 
        /// in a failure.
        /// </para>
        /// </remarks>
        /// <param name="corridor">The corridor to update.</param>
        /// <param name="position">The position to reset to corridor to.</param>
        /// <param name="query">The query object to use.</param>
        /// <param name="filter">The filter object to use.</param>
        /// <returns>True if successful.</returns>
        public static bool LoadLocals(PathCorridor corridor, NavmeshPoint position
            , NavmeshQuery query, NavmeshQueryFilter filter)
        {
            // Basic checks first.
            if (position.polyRef == 0|| corridor == null|| query == null|| filter == null)
                return false;

            // Validate optional parameters.

            // Assign and reset.

            corridor.mQuery = query;
            corridor.mFilter = filter;
            corridor.Reset(position);

            return true;
        }
    }
}
