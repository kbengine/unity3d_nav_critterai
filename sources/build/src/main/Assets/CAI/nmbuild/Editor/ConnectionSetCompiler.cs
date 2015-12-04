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
using System.Collections.Generic;
using org.critterai.nmgen;
using org.critterai.nav;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Used to compile a set of off-mesh connections in a dynamic fashion.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used for dynamically building a set of off-mesh connections.
    /// </para>
    /// </remarks>
    /// <seealso cref="ConnectionSet"/>
	public sealed class ConnectionSetCompiler
	{
        private readonly List<Vector3> mVerts;
        private readonly List<float> mRadii;
        private readonly List<byte> mDirs;
        private readonly List<byte> mAreas;
        private readonly List<ushort> mFlags;
        private readonly List<uint> mUserIds;

        /// <summary>
        /// The number of loaded connections.
        /// </summary>
        public int Count { get { return mRadii.Count; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the internal buffers.</param>
        public ConnectionSetCompiler(int initialCapacity)
        {
            mVerts = new List<Vector3>(initialCapacity);
            mRadii = new List<float>(initialCapacity);
            mDirs = new List<byte>(initialCapacity);
            mAreas = new List<byte>(initialCapacity);
            mFlags = new List<ushort>(initialCapacity);
            mUserIds = new List<uint>(initialCapacity);
        }

        /// <summary>
        /// The connections in the set.
        /// </summary>
        /// <param name="index">
        /// The index of the connection. [Limits: 0 &lt;= value &lt; <see cref="Count"/>].
        /// </param>
        /// <returns>The connection.</returns>
        public OffMeshConnection this[int index]
        {
            get
            {
                // Not using parameterized constructor.  Don't need validation cost.
                OffMeshConnection result = new OffMeshConnection();

                result.start = mVerts[index * 2 + 0];
                result.end = mVerts[index * 2 + 1];
                result.radius = mRadii[index];
                result.direction = mDirs[index];
                result.flags = mFlags[index];
                result.area = mAreas[index];
                result.userId = mUserIds[index];

                return result;
            }

            set
            {
                // Must be validated.
                // Let exceptions be thrown.

                mRadii[index] = MathUtil.ClampToPositiveNonZero(value.radius);
                mDirs[index] = (byte)(value.IsBiDirectional ? 1 : 0);
                mAreas[index] = NMGen.ClampArea(value.area);
                mFlags[index] = value.flags;
                mUserIds[index] = value.userId;

                // Keep verts last.
                mVerts[index * 2 + 0] = value.start;
                mVerts[index * 2 + 1] = value.end;
            }
        }

        /// <summary>
        /// Add a connection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All values are auto-clamped to valid values.
        /// </para>
        /// </remarks>
        /// <param name="start">The connection start point.</param>
        /// <param name="end">The connection end point.</param>
        /// <param name="radius">The radius of the connection vertices.</param>
        /// <param name="isBidirectional">True if the connection can be traversed in both
        /// directions. (Start to end, end to start.)</param>
        /// <param name="area">The connection area id.</param>
        /// <param name="flags">The connection flags.</param>
        /// <param name="userId">The connection user id.</param>
        public void Add(Vector3 start, Vector3 end, float radius
            , bool isBidirectional, byte area, ushort flags, uint userId)
        {
            mVerts.Add(start);
            mVerts.Add(end);
            mRadii.Add(System.Math.Max(MathUtil.Epsilon, radius));
            mDirs.Add((byte)(isBidirectional ? 1 : 0));
            mAreas.Add(NMGen.ClampArea(area));
            mFlags.Add(flags);
            mUserIds.Add(userId);
        }

        /// <summary>
        /// Add a connection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The connection fields are auto-clamped to valid values.
        /// </para>
        /// </remarks>
        /// <param name="connection">The connection to add.</param>
        public void Add(OffMeshConnection connection)
        {
            // Must be validated.
            mVerts.Add(connection.start);
            mVerts.Add(connection.end);
            mRadii.Add(MathUtil.ClampToPositiveNonZero(connection.radius));
            mDirs.Add((byte)(connection.IsBiDirectional ? 1 : 0));
            mAreas.Add(NMGen.ClampArea(connection.area));
            mFlags.Add(connection.flags);
            mUserIds.Add(connection.userId);
        }

        /// <summary>
        /// Removes the connection at the specified index.
        /// </summary>
        /// <param name="index">The connection index.</param>
        public void RemoveAt(int index)
        {
            // Let exceptions be thrown.

            mRadii.RemoveAt(index);
            mDirs.RemoveAt(index);
            mAreas.RemoveAt(index);
            mFlags.RemoveAt(index);
            mUserIds.RemoveAt(index);

            // Keep last.
            mVerts.RemoveAt(index * 2 + 1);
            mVerts.RemoveAt(index * 2 + 0);
        }

        /// <summary>
        /// Resets the compiler to zero connections.
        /// </summary>
        public void Reset()
        {
            mVerts.Clear();
            mRadii.Clear();
            mDirs.Clear();
            mAreas.Clear();
            mFlags.Clear();
            mUserIds.Clear();
        }

        /// <summary>
        /// Creates a thread-safe, immutable, fully validated connection set from the 
        /// connections.
        /// </summary>
        /// <returns>A conneciton set created from the connections.</returns>
        public ConnectionSet CreateConnectionSet()
        {
            if (mVerts.Count == 0)
                return ConnectionSet.CreateEmpty();

            Vector3[] lverts = mVerts.ToArray();
            float[] lradii = mRadii.ToArray();
            byte[] ldirs = mDirs.ToArray();
            byte[] lareas = mAreas.ToArray();
            ushort[] lflags = mFlags.ToArray();
            uint[] lids = mUserIds.ToArray();

            if (ConnectionSet.IsValid(lverts, lradii, ldirs, lareas, lflags, lids))
            {
                return ConnectionSet.UnsafeCreate(mVerts.ToArray()
                    , mRadii.ToArray()
                    , mDirs.ToArray()
                    , mAreas.ToArray()
                    , mFlags.ToArray()
                    , mUserIds.ToArray());
            }

            return null;
        }
	}
}
