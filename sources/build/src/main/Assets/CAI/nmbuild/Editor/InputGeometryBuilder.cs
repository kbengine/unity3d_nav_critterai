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
using org.critterai.geom;
using System.Collections.Generic;
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Builds an <see cref="InputGeometry"/> object.
    /// </summary>
    public sealed class InputGeometryBuilder
    {
        private ChunkyTriMeshBuilder mBuilder;
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;
        private readonly bool mIsThreadSafe;
        private InputGeometry mGeom;

        private InputGeometryBuilder(ChunkyTriMeshBuilder builder
            , Vector3 boundsMin
            , Vector3 boundsMax
            , bool isThreadSafe)
        {
            mBuilder = builder;
            mBoundsMin = boundsMin;
            mBoundsMax = boundsMax;
            mIsThreadSafe = isThreadSafe;
        }

        /// <summary>
        /// True if the builder is safe to run on its own thread.
        /// </summary>
        public bool IsThreadSafe { get { return mIsThreadSafe; } }

        /// <summary>
        /// The input geometry created by the builder. (Null until finished.)
        /// </summary>
        public InputGeometry Result { get { return mGeom; } }

        /// <summary>
        /// True if the builder is finished and the input geometry is avaiable.
        /// </summary>
        public bool IsFinished { get { return mGeom != null; } }

        /// <summary>
        /// Performs a single build step.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method must be called repeatedly until it resturns false in order
        /// to complete the build. (Useful in GUI environments.)
        /// </para>
        /// </remarks>
        /// <returns>
        /// True if the build is still underway and another call is required. False
        /// if the build is finished.
        /// </returns>
        public bool Build()
        {
            if (mBuilder == null)
                return false;

            if (mBuilder.Build())
                return true;

            mGeom = new InputGeometry(mBuilder.Result, mBoundsMin, mBoundsMax);
            mBuilder = null;

            return false;
        }

        /// <summary>
        /// Performs the build in a single step.
        /// </summary>
        public void BuildAll()
        {
            if (mBuilder == null)
                return;

            mBuilder.BuildAll();

            mGeom = new InputGeometry(mBuilder.Result, mBoundsMin, mBoundsMax);
            mBuilder = null;
        }

        /// <summary>
        /// Creates a thread-safe, fully validated builder.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The input mesh and area parameters are fully validated.
        /// </para>
        /// <para>
        /// Will return null if there are zero triangles.
        /// </para>
        /// <para>
        /// All triangleswill default to <see cref="NMGen.MaxArea"/> if the 
        /// <paramref name="areas"/> parameter is null.
        /// </para>
        /// <para>
        /// If walkable slope if greather than zero then the builder will apply 
        /// <see cref="NMGen.ClearUnwalkableTriangles"/> to the areas.
        /// </para>
        /// </remarks>
        /// <param name="mesh">The triangle mesh to use for the build.</param>
        /// <param name="areas">The triangle areas. (Null permitted.)</param>
        /// <param name="walkableSlope">The walkable slope. 
        /// (See <see cref="NMGenParams.WalkableSlope"/>)</param>
        /// <returns>A thread-safe, fully validated builder. Or null on error.</returns>
        public static InputGeometryBuilder Create(TriangleMesh mesh
            , byte[] areas
            , float walkableSlope)
        {
            if (!IsValid(mesh, areas))
                return null;

            TriangleMesh lmesh = new TriangleMesh(mesh.vertCount, mesh.triCount);
            lmesh.triCount = mesh.triCount;
            lmesh.vertCount = mesh.vertCount;

            System.Array.Copy(mesh.verts, 0, lmesh.verts, 0, lmesh.verts.Length);
            System.Array.Copy(mesh.tris, 0, lmesh.tris, 0, lmesh.tris.Length);

            byte[] lareas;
            if (areas == null)
                lareas = NMGen.CreateDefaultAreaBuffer(mesh.triCount);
            else
            {
                lareas = new byte[mesh.triCount];
                System.Array.Copy(areas, 0, lareas, 0, lareas.Length);
            }

            return UnsafeCreate(lmesh, lareas, walkableSlope, true);
        }

        /// <summary>
        /// Creates a builder.
        /// </summary>
        /// <remarks>
        /// <para>
        /// No validation is performed and the builder will use the parameters directly
        /// during the build.
        /// </para>
        /// <para>
        /// Builders created using this method are not guarenteed to produce a usable result.
        /// </para>
        /// <para>
        /// It is the responsibility of the caller to ensure thread safely if 
        /// <paramref name="isThreadSafe"/> is set to true.
        /// </para>
        /// <para>
        /// <b>Warning:</b> If walkable slope if greather than zero then the builder will
        /// apply <see cref="NMGen.ClearUnwalkableTriangles"/> directly to the areas parameter.
        /// </para>
        /// </remarks>
        /// <param name="mesh">The triangle mesh to use for the build.</param>
        /// <param name="areas">The triangle areas. (Null not permitted.)</param>
        /// <param name="walkableSlope">The walkable slope. 
        /// (See <see cref="NMGenParams.WalkableSlope"/>)</param>
        /// <param name="isThreadSafe">True if the builder can run safely on its own thread.</param>
        /// <returns>A builder, or null on error.</returns>
        public static InputGeometryBuilder UnsafeCreate(TriangleMesh mesh
            , byte[] areas
            , float walkableSlope
            , bool isThreadSafe)
        {
            if (mesh == null || areas == null || mesh.triCount < 0)
                return null;

            walkableSlope = System.Math.Min(NMGen.MaxAllowedSlope, walkableSlope);

            if (walkableSlope > 0)
            {
                BuildContext context = new BuildContext();
                if (!NMGen.ClearUnwalkableTriangles(context, mesh, walkableSlope, areas))
                    return null;
            }

            ChunkyTriMeshBuilder builder = ChunkyTriMeshBuilder.Create(mesh, areas, 32768);

            if (builder == null)
                return null;

            Vector3 bmin;
            Vector3 bmax;
            mesh.GetBounds(out bmin, out bmax);

            return new InputGeometryBuilder(builder, bmin, bmax, isThreadSafe);
        }

        /// <summary>
        /// Validates the mesh and areas.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="areas"/> parameter is validated only if it is non-null.
        /// </para>
        /// </remarks>
        /// <param name="mesh">The mesh to validate.</param>
        /// <param name="areas">The area to validate. (If non-null.)</param>
        /// <returns>True if the structure and content of the parameters are considered valid.
        /// </returns>
        public static bool IsValid(TriangleMesh mesh, byte[] areas)
        {
            if (mesh == null || mesh.triCount == 0 || !TriangleMesh.IsValid(mesh, true))
            {
                return false;
            }

            if (areas != null && (areas.Length != mesh.triCount
                    || !NMGen.IsValidAreaBuffer(areas, mesh.triCount)))
            {
                return false;
            }

            return true;
        }

    }
}
