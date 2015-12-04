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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using org.critterai.interop;
using org.critterai.nmgen.rcn;

namespace org.critterai.nmgen
{
    /// <summary>
    /// Contains triangle meshes that represent detailed height data 
    /// associated with the polygons in its associated 
    /// <see cref="PolyMesh"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Warning:</b> The serializable attribute and interface will be removed in v0.5. Use 
    /// <see cref="GetSerializedData"/> instead.
    /// </para>
    /// <para>
    /// The detail mesh is made up of triangle sub-meshes which provide extra height detail for 
    /// each polygon in its assoicated polygon mesh.
    /// </para>
    /// <para>
    /// This class is moslty opaque.  The <see cref="PolyMeshDetailData"/> class provides the 
    /// ability to inspect and update the content.
    /// </para>
    /// <para>
    /// This class is not compatible with Unity serialization. The <see cref="GetSerializedData"/> 
    /// method can be used for manual serialization within Unity.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public sealed class PolyMeshDetail
        : IManagedObject, ISerializable
    {
        /*
         * Design Notes:
         * 
         * The internal structure of this class will have to be transformed to
         * match the pattern used by PolyMesh in order to add the merge 
         * mesh functionality.  (Can't pass arrays of pointers across the
         * interop boundary.) Not changing the structure until then.
         * 
         */

        private const string DataKey = "d";

        private IntPtr mMeshes;
        private IntPtr mVerts;
        private IntPtr mTris;
        private int mMeshCount;
        private int mVertCount;
        private int mTriCount;

        private int mMaxMeshes;
        private int mMaxVerts;
        private int mMaxTris;
        private readonly AllocType mResourceType;

        /// <summary>
        /// The number of sub-meshes in the detail mesh.
        /// </summary>
        public int MeshCount { get { return mMeshCount; } }

        /// <summary>
        /// The total number of vertices in the detail mesh.
        /// </summary>
        public int VertCount { get { return mVertCount; } }

        /// <summary>
        /// The total number of triangles in the detail mesh.
        /// </summary>
        public int TriCount { get { return mTriCount; } }

        /// <summary>
        /// The maximum number of sub-meshes the mesh buffers can hold.
        /// </summary>
        public int MaxMeshes { get { return mMaxMeshes; } }

        /// <summary>
        /// The maximum number of vertices the vertex buffer can hold.
        /// </summary>
        public int MaxVerts { get { return mMaxVerts; } }

        /// <summary>
        /// The maximum number of triangls the triangle buffers can hold.
        /// </summary>
        public int MaxTris { get { return mMaxTris; } }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mMeshes == IntPtr.Zero); } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return mResourceType; } }

        /// <summary>
        /// Constructs an object with all buffers allocated and ready to load with data. 
        /// (See: <see cref="Load"/>)
        /// </summary>
        /// <param name="maxVerts">
        /// The maximum vertices the vertex buffer will hold. [Limit: >= 3]
        /// </param>
        /// <param name="maxTris">
        /// The maximum triangles the triangle buffer  will hold. [Limit: > 0]
        /// </param>
        /// <param name="maxMeshes">
        /// The maximum sub-meshes the mesh buffer will hold. [Limit: > 0]
        /// </param>
        private PolyMeshDetail(int maxVerts
            , int maxTris
            , int maxMeshes)
        {
            if (maxVerts < 3 || maxTris < 1 || maxMeshes < 1)
                return;

            mResourceType = AllocType.Local;

            mMaxVerts = maxVerts;
            mMaxTris = maxTris;
            mMaxMeshes = maxMeshes;

            int size = sizeof(float) * mMaxVerts * 3;
            mVerts = UtilEx.GetBuffer(size, true);

            size = sizeof(byte) * mMaxTris * 4;
            mTris = UtilEx.GetBuffer(size, true);

            size = sizeof(uint) * mMaxMeshes * 4;
            mMeshes = UtilEx.GetBuffer(size, true);
        }

        private PolyMeshDetail(AllocType resourceType)
        {
            mResourceType = resourceType;
        }

        private PolyMeshDetail(SerializationInfo info, StreamingContext context)
        {
            // Note: Version compatability is handled by the interop call.
            if (info.MemberCount != 1)
                return;

            byte[] rawData = (byte[])info.GetValue(DataKey, typeof(byte[]));
            PolyMeshDetailEx.rcpdBuildFromMeshData(rawData, rawData.Length, this);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~PolyMeshDetail()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Loads the data into the mesh buffers, overwriting existing content.
        /// </summary>
        /// <param name="data">The data to load.</param>
        /// <returns>True if the load was successful.</returns>
        public bool Load(PolyMeshDetailData data)
        {
            if (IsDisposed
                || data == null
                || data.meshes == null
                || data.tris == null
                || data.verts == null
                || data.meshCount < 0 || data.meshCount > mMaxMeshes
                || data.triCount < 0 || data.triCount > mMaxTris
                || data.vertCount < 0 || data.vertCount > mMaxVerts
                || data.meshes.Length < data.meshCount * 4
                || data.tris.Length < data.triCount * 4
                || data.verts.Length < data.vertCount)
            {
                return false;
            }

            mMeshCount = data.meshCount;
            mTriCount = data.triCount;
            mVertCount = data.vertCount;

            UtilEx.Copy(data.meshes, 0, mMeshes, mMeshCount * 4);
            Marshal.Copy(data.tris, 0, mTris, mTriCount * 4);
            float[] fverts = Vector3Util.Flatten(data.verts, mVertCount);
            Marshal.Copy(fverts, 0, mVerts, mVertCount * 3);

            return true;
        }

        /// <summary>
        /// Gets the data from the mesh buffers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is useful for extracting mesh data so it can be inspected or altered and 
        /// reloaded.
        /// </para>
        /// </remarks>
        /// <param name="includeBuffer">
        /// If true, includes the unused buffer space.  Otherwise only the used buffer data is 
        /// returned.
        /// </param>
        /// <returns>The data from the mesh buffers.</returns>
        public PolyMeshDetailData GetData(bool includeBuffer)
        {
            if (IsDisposed)
                return null;

            PolyMeshDetailData result = new PolyMeshDetailData(
                (includeBuffer ? mMaxVerts : mVertCount)
                , (includeBuffer ? mMaxTris : mTriCount)
                , (includeBuffer ? mMaxMeshes : mMeshCount));

            FillData(result);

            return result;
        }

        /// <summary>
        /// Loads the data from the mesh buffers into the data object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the buffer argument is null, a new buffer will be returned. If the buffer is too 
        /// small it will be resized.
        /// </para>
        /// <para>
        /// Only the used portions of the mesh buffers are copied.
        /// </para>
        /// </remarks>
        /// <param name="buffer">A buffer to load the data into.</param>
        /// <returns>A reference to the mesh data.</returns>
        public PolyMeshDetailData GetData(PolyMeshDetailData buffer)
        {
            if (IsDisposed)
                return null;
            if (buffer == null)
                return GetData(true);

            if (!buffer.CanFit(mVertCount, mTriCount, mMeshCount))
                buffer.Reset(mMaxVerts, mMaxTris, mMaxMeshes);

            FillData(buffer);

            return buffer;

        }

        private void FillData(PolyMeshDetailData buffer)
        {
            buffer.vertCount = mVertCount;
            buffer.triCount = mTriCount;
            buffer.meshCount = mMeshCount;

            float[] fverts = new float[mVertCount * 3];
            Marshal.Copy(mVerts, fverts, 0, mVertCount * 3);
            buffer.verts = Vector3Util.GetVectors(fverts, 0, buffer.verts, 0, mVertCount);
            Marshal.Copy(mTris, buffer.tris, 0, mTriCount * 4);
            UtilEx.Copy(mMeshes, buffer.meshes, mMeshCount * 4);
        }

        /// <summary>
        /// Frees all resources and marks the object as disposed.
        /// </summary>
        public void RequestDisposal()
        {
            if (!IsDisposed)
            {
                if (ResourceType == AllocType.Local)
                {
                    Marshal.FreeHGlobal(mMeshes);
                    Marshal.FreeHGlobal(mTris);
                    Marshal.FreeHGlobal(mVerts);
                }
                else if (ResourceType == AllocType.External)
                    PolyMeshDetailEx.rcpdFreeMeshData(this);

                mMeshes = IntPtr.Zero;
                mTris = IntPtr.Zero;
                mVerts = IntPtr.Zero;
                mMeshCount = 0;
                mVertCount = 0;
                mTriCount = 0;
                mMaxMeshes = 0;
                mMaxTris = 0;
                mMaxVerts = 0;
            }
        }

        /// <summary>
        /// Gets a serialized version of the mesh that can be used to recreate it later.
        /// </summary>
        /// <param name="includeBuffer">
        /// True if serialized data should include the full buffer size.  Otherwise the buffers will
        /// be stripped and the smallest possible serialized data returned.
        /// </param>
        /// <returns>A serialized version of the mesh.</returns>
        public byte[] GetSerializedData(bool includeBuffer)
        {
            if (IsDisposed)
                return null;

            // Design note:  This is implemented using interop calls
            // bacause it is so much easier and faster to serialize in C++
            // than in C#.

            IntPtr ptr = IntPtr.Zero;
            int dataSize = 0;

            if (!PolyMeshDetailEx.rcpdGetSerializedData(this
                , includeBuffer
                , ref ptr
                , ref dataSize))
            {
                return null;
            }

            byte[] result = UtilEx.ExtractArrayByte(ptr, dataSize);

            NMGenEx.nmgFreeSerializationData(ref ptr);

            return result;
        }

        /// <summary>
        /// Gets serialization data for the object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will always include the unused buffer space. (No compression.)
        /// </para>
        /// </remarks>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            /*
             * Design Notes:
             * 
             * Default serialization security is OK.
             * Validation and versioning is handled by GetSerializedData().
             */

            byte[] rawData = GetSerializedData(true);

            if (rawData == null)
                return;

            info.AddValue(DataKey, rawData);
        }

        /// <summary>
        /// Builds a detail mesh from the provided polygon mesh.
        /// </summary>
        /// <param name="context">The context to use for the operation.</param>
        /// <param name="polyMesh">The source polygon mesh.</param>
        /// <param name="field">The compact heightfield used to build the polygon mesh.</param>
        /// <param name="detailSampleDistance">
        /// The sample distance to use when sampling the surface height of the polygon mesh.
        /// </param>
        /// <param name="detailMaxDeviation">
        /// The maximum the surface of the detail mesh should deviate from the heightfield data.
        /// </param>
        /// <returns>A new detail mesh, or null on error.</returns>
        public static PolyMeshDetail Build(BuildContext context
            , PolyMesh polyMesh, CompactHeightfield field
            , float detailSampleDistance, float detailMaxDeviation)
        {
            if (context == null || polyMesh == null || polyMesh.IsDisposed
                || field == null || field.IsDisposed
                || detailSampleDistance < 0
                || detailMaxDeviation < 0)
            {
                return null;
            }

            PolyMeshDetail result = new PolyMeshDetail(AllocType.External);

            if (PolyMeshDetailEx.rcpdBuildPolyMeshDetail(context.root
                , ref polyMesh.root
                , field
                , detailSampleDistance
                , detailMaxDeviation
                , result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Constructs an object with all buffers allocated and ready to load with data. 
        /// (See: <see cref="Load"/>)
        /// </summary>
        /// <param name="maxVerts">
        /// The maximum vertices the vertex buffer will hold. [Limit: >= 3]
        /// </param>
        /// <param name="maxTris">
        /// The maximum triangles the triangle buffer will hold. [Limit: > 0]
        /// </param>
        /// <param name="maxMeshes">
        /// The maximum sub-meshes the mesh buffer will hold. [Limit: > 0]
        /// </param>
        /// <returns>The new detail mesh, or null on error.</returns>
        public static PolyMeshDetail Create(int maxVerts
            , int maxTris
            , int maxMeshes)
        {
            if (maxVerts < 3 || maxTris < 1 || maxMeshes < 1)
                return null;

            return new PolyMeshDetail(maxVerts, maxTris, maxMeshes);
        }

        /// <summary>
        /// Constructs a detail mesh from the data generated by the <see cref="GetSerializedData"/> 
        /// method.
        /// </summary>
        /// <param name="serializedMesh">The source data.</param>
        /// <returns>The new detail mesh, or null on error.</returns>
        public static PolyMeshDetail Create(byte[] serializedMesh)
        {
            PolyMeshDetail result = new PolyMeshDetail(AllocType.External);

            if (PolyMeshDetailEx.rcpdBuildFromMeshData(serializedMesh
                , serializedMesh.Length
                , result))
            {
                return result;
            }

            return null;
        }
    }
}
