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
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// A navigation mesh based on convex polygons.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="1b3cfec9-7cd3-444c-b83d-dfc551454822.htm">
    /// An Introduction to Navigation</a> for information on how to use the navigation mesh.
    /// </para>
    /// <para>
    /// This class is used in conjunction with the <see cref="NavmeshQuery"/> class to
    /// provide path planning features.
    /// </para>
    /// <para>
    /// Most object references returned by this class cannot be compared for equality.  
    /// I.e. <c>mesh.GetTile(0) != mesh.GetTile(0)</c>. The object state may be equal, but the 
    /// references are not.
    /// </para>
    /// <para>
    /// This class does not have any asynchronous methods.  So the return status of all 
    /// methods will always contain either a success or failure flag.
    /// </para>
    /// <para>
    /// <b>Warning:</b> The serializable attribute and interface will be removed in v0.5. 
    /// Use <see cref="GetSerializedMesh"/> instead.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class Navmesh
        : ManagedObject, ISerializable
    {
        // Local version is not needed.  Detour includes its own mesh 
        // versioning.
        // private const long ClassVersion = 1L;

        private const string DataKey = "d";

        /// <summary>
        /// A flag that indicates a link is external.  (Context dependent.)
        /// </summary>
        public const ushort ExternalLink = 0x8000;

        /// <summary>
        /// Indicates that a link is null. (Does not link to anything.)
        /// </summary>
        public const uint NullLink = 0xffffffff;

        /// <summary>
        /// The maximum supported vertices per polygon.
        /// </summary>
        public const int MaxAllowedVertsPerPoly = 6;

        /// <summary>
        /// The maximum allowed area value.
        /// </summary>
        public const byte MaxArea = 63;

        /// <summary>
        /// Represents an unwalkable area.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a data element is given this value it is considered to no longer be 
        /// assigned to a walkable area. (It usually becomes an obstruction.)
        /// </para>
        /// <para>
        /// This is also the minimum value that can be used as an area value.
        /// </para>
        /// </remarks>
        public const byte NullArea = 0;

        /// <summary>
        /// The reference for a null polygon. (Does not exist.)
        /// </summary>
        public const uint NullPoly = 0;

        /// <summary>
        /// The reference for a null tile. (Does not exist.)
        /// </summary>
        public const uint NullTile = 0;

        /// <summary>
        /// Represents an polygon index that does not point to anything.
        /// </summary>
        public const ushort NullIndex = 0xffff;

        /// <summary>
        /// dtNavMesh object.
        /// </summary>
        internal IntPtr root;

        private Navmesh(IntPtr mesh)
            : base(AllocType.External)
        {
            root = mesh;
        }

        private Navmesh(SerializationInfo info, StreamingContext context)
            : base(AllocType.External)
        {
            root = IntPtr.Zero;

            if (info.MemberCount != 1)
                return;
            
            byte[] data = (byte[])info.GetValue(DataKey, typeof(byte[]));
            NavmeshEx.dtnmBuildDTNavMeshFromRaw(data, data.Length, true, ref root);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Navmesh()
        {
            RequestDisposal();
        }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public override bool IsDisposed
        {
            get { return (root == IntPtr.Zero); }
        }

        /// <summary>
        /// Request all resources controlled by the object be immediately freed and the object 
        /// marked as disposed.
        /// </summary>
        public override void RequestDisposal()
        {
            if (root != IntPtr.Zero)
            {
                NavmeshEx.dtnmFreeNavMesh(ref root, false);
                root = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Returns the configuration parameters used to initialize the navigation mesh.
        /// </summary>
        /// <returns>The configuration parameters used to initialize the navigation mesh.</returns>
        public NavmeshParams GetConfig()
        {
            NavmeshParams result = new NavmeshParams();
            NavmeshEx.dtnmGetParams(root, result);
            return result;
        }

        /// <summary>
        /// Adds a tile to the navigation mesh.
        /// </summary>
        /// <param name="tileData">The tile data.</param>
        /// <param name="desiredTileRef">
        /// The desired reference for the tile.
        /// (Or <see cref="NullTile"/> if the reference doesn't matter or is not known.)
        /// </param>
        /// <param name="resultTileRef">The actual reference assigned to the tile.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        public NavStatus AddTile(NavmeshTileData tileData
            , uint desiredTileRef
            , out uint resultTileRef)
        {
            if (tileData == null
                || tileData.IsOwned
                || tileData.Size == 0)
            {
                resultTileRef = 0;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            resultTileRef = 0;

            return NavmeshEx.dtnmAddTile(root
                , tileData
                , desiredTileRef
                , ref resultTileRef);
        }

        /// <summary>
        /// Removes the specified tile from the mesh.
        /// </summary>
        /// <param name="tileRef">The tile reference.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        public NavStatus RemoveTile(uint tileRef)
        {
            int trash = 0;
            IntPtr dump = IntPtr.Zero;
            return NavmeshEx.dtnmRemoveTile(root, tileRef, ref dump, ref trash);
        }

        /// <summary>
        /// Derives the tile grid location based on the provided world space position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="x">The tile's grid x-location.</param>
        /// <param name="z">The tiles's grid z-location.</param>
        public void DeriveTileLocation(Vector3 position
            , out int x
            , out int z)
        {
            x = 0;
            z = 0;
            NavmeshEx.dtnmCalcTileLoc(root, ref position, ref x, ref z);
        }

        /// <summary>
        /// Gets the tile at the specified grid location.
        /// </summary>
        /// <param name="x">The tile grid x-location.</param>
        /// <param name="z">The tile grid z-location.</param>
        /// <param name="layer">The tile layer.</param>
        /// <returns>The tile at the specified grid location. (May be empty.)</returns>
        public NavmeshTile GetTile(int x, int z, int layer)
        {
            IntPtr tile = NavmeshEx.dtnmGetTileAt(root, x, z, layer);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }

        /// <summary>
        /// Gets all tiles at the specified grid location.  (All layers.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Some tiles in the result may be empty. (Zero polygon count.)
        /// </para>
        /// </remarks>
        /// <param name="x">The tile grid x-location.</param>
        /// <param name="z">The tile grid z-location.</param>
        /// <param name="buffer">The result tiles.</param>
        /// <returns>The number of tiles returned in the buffer.</returns>
        public int GetTiles(int x, int z, NavmeshTile[] buffer)
        {
            IntPtr[] tiles = new IntPtr[buffer.Length];

            int tileCount = NavmeshEx.dtnmGetTilesAt(root, x, z, tiles, tiles.Length);

            for (int i = 0; i < tileCount; i++)
            {
                buffer[i] = new NavmeshTile(this, tiles[i]);
            }

            return tileCount;
        }

        /// <summary>
        /// Gets the reference for the tile at the specified grid location.
        /// </summary>
        /// <param name="x">The tile grid x-location.</param>
        /// <param name="z">The tile grid z-location.</param>
        /// <param name="layer">The tiles layer.</param>
        /// <returns>The tile reference, or zero if there is no tile at the location.</returns>
        public uint GetTileRef(int x, int z, int layer)
        {
            return NavmeshEx.dtnmGetTileRefAt(root, x, z, layer);
        }

        /// <summary>
        /// Gets a tile using its reference.
        /// </summary>
        /// <param name="tileRef">The reference of the tile.</param>
        /// <returns>The tile, or null if none was found.</returns>
        public NavmeshTile GetTileByRef(uint tileRef)
        {
            IntPtr tile = NavmeshEx.dtnmGetTileByRef(root, tileRef);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }

        /// <summary>
        /// The maximum number of tiles supported by the navigation mesh.
        /// </summary>
        /// <returns>The maximum number of tiles supported by the navigation mesh.</returns>
        public int GetMaxTiles()
        {
            return NavmeshEx.dtnmGetMaxTiles(root);
        }

        /// <summary>
        /// Gets a tile from the tile buffer.
        /// </summary>
        /// <param name="tileIndex">
        /// The index of the tile. [Limits: 0 &lt;= index &lt; <see cref="GetMaxTiles"/>]
        /// </param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        public NavmeshTile GetTile(int tileIndex)
        {
            IntPtr tile = NavmeshEx.dtnmGetTile(root, tileIndex);
            if (tile == IntPtr.Zero)
                return null;
            return new NavmeshTile(this, tile);
        }

        /// <summary>
        /// Gets a polygon and its tile.
        /// </summary>
        /// <param name="polyRef">The reference of the polygon.</param>
        /// <param name="tile">The tile the polygon belongs to.</param>
        /// <param name="poly">The polygon.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        public NavStatus GetTileAndPoly(uint polyRef
            , out NavmeshTile tile
            , out NavmeshPoly poly)
        {
            IntPtr pTile = IntPtr.Zero;
            IntPtr pPoly = IntPtr.Zero;

            NavStatus status = NavmeshEx.dtnmGetTileAndPolyByRef(root
                , polyRef
                , ref pTile
                , ref pPoly);

            if (NavUtil.Succeeded(status))
            {
                tile = new NavmeshTile(this, pTile);
                poly = (NavmeshPoly)Marshal.PtrToStructure(pPoly
                    , typeof(NavmeshPoly));
            }
            else
            {
                tile = null;
                poly = new NavmeshPoly();
            }

            return status;

        }

        /// <summary>
        /// Indicates whether or not the specified polygon reference is valid.
        /// </summary>
        /// <param name="polyRef">The reference to check.</param>
        /// <returns>True if the provided reference is valid.</returns>
        public bool IsValidPolyRef(uint polyRef)
        {
            return NavmeshEx.dtnmIsValidPolyRef(root, polyRef);
        }

        /// <summary>
        /// Gets the endpoints for an off-mesh connection, ordered by 'direction of travel'.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Off-mesh connections are stored in the navigation mesh as special 2-vertex polygons 
        /// with a single edge.  At least one of the vertices is expected to be inside a normal 
        /// polygon. So an off-mesh connection is "entered" from a normal polygon at one of its 
        /// endpoints. This is the polygon identified by <paramref name="startPolyRef"/>.
        /// </para>
        /// </remarks>
        /// <param name="startPolyRef">
        /// The reference of the polygon that contains the start point.
        /// </param>
        /// <param name="connectionPolyRef">The off-mesh connection's reference.</param>
        /// <param name="startPoint">The start point. (Out)</param>
        /// <param name="endPoint">The end point. (Out)</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        public NavStatus GetConnectionEndpoints(uint startPolyRef
            , uint connectionPolyRef
            , out Vector3 startPoint
            , out Vector3 endPoint)
        {
            startPoint = Vector3Util.Zero;
            endPoint = Vector3Util.Zero;
            return NavmeshEx.dtnmGetConnectionEndPoints(root
                , startPolyRef
                , connectionPolyRef
                , ref startPoint
                , ref endPoint);
        }

        /// <summary>
        /// Gets an off-mesh connection.
        /// </summary>
        /// <param name="polyRef">The reference of the off-mesh connection.</param>
        /// <returns>The off-mesh connection.</returns>
        public NavmeshConnection GetConnectionByRef(uint polyRef)
        {
            IntPtr conn = NavmeshEx.dtnmGetOffMeshConnectionByRef(root, polyRef);

            NavmeshConnection result;
            if (conn == IntPtr.Zero)
                result = new NavmeshConnection();
            else
                result = (NavmeshConnection)Marshal.PtrToStructure(conn
                    , typeof(NavmeshConnection));

            return result;
        }

        /// <summary>
        /// Returns the flags for the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference of the polygon.</param>
        /// <param name="flags">The polygon flags.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus GetPolyFlags(uint polyRef, out ushort flags)
        {
            flags = 0;
            return NavmeshEx.dtnmGetPolyFlags(root, polyRef, ref flags);
        }

        /// <summary>
        /// Sets the flags for the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference of the polygon.</param>
        /// <param name="flags">The polygon flags.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus SetPolyFlags(uint polyRef, ushort flags)
        {
            return NavmeshEx.dtnmSetPolyFlags(root, polyRef, flags);
        }

        /// <summary>
        /// Returns the area of the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference of the polygon.</param>
        /// <param name="area">The area of the polygon.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus GetPolyArea(uint polyRef, out byte area)
        {
            area = 0;
            return NavmeshEx.dtnmGetPolyArea(root, polyRef, ref area);
        }

        /// <summary>
        /// Sets the area of the specified polygon.
        /// </summary>
        /// <param name="polyRef">The reference of the polygon.</param>
        /// <param name="area">The area of the polygon.
        /// [Limit: &lt;= <see cref="Navmesh.MaxArea"/>]</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public NavStatus SetPolyArea(uint polyRef, byte area)
        {
            return NavmeshEx.dtnmSetPolyArea(root, polyRef, area);
        }

        /// <summary>
        /// Gets a serialized version of the mesh.
        /// </summary>
        /// <returns>The serialized mesh.</returns>
        public byte[] GetSerializedMesh()
        {
            if (IsDisposed)
                return null;

            IntPtr data = IntPtr.Zero;
            int dataSize = 0;

            NavmeshEx.dtnmGetNavMeshRawData(root, ref data, ref dataSize);

            if (dataSize == 0)
                return null;

            byte[] resultData = UtilEx.ExtractArrayByte(data, dataSize);

            NavmeshEx.dtnmFreeBytes(ref data);

            return resultData;
        }

        /// <summary>
        /// Gets serialization data for the object.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        [System.Obsolete]
        public void GetObjectData(SerializationInfo info
            , StreamingContext context)
        {
            if (IsDisposed)
                return;

            // info.AddValue(VersionKey, ClassVersion);
            info.AddValue(DataKey, GetSerializedMesh());
        }

        /// <summary>
        /// Creates a single-tile navigation mesh.
        /// </summary>
        /// <param name="buildData">The tile build data.</param>
        /// <param name="resultMesh">The result mesh.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public static NavStatus Create(NavmeshTileBuildData buildData
            , out Navmesh resultMesh)
        {
            IntPtr navMesh = IntPtr.Zero;

            NavStatus status = NavmeshEx.dtnmBuildSingleTileMesh(buildData
                , ref navMesh);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(navMesh);
            else
                resultMesh = null;

            return status;
        }

        /// <summary>
        /// Creates a navigation mesh from data obtained from the <see cref="GetSerializedMesh"/> 
        /// method.
        /// </summary>
        /// <param name="serializedMesh">The serialized mesh.</param>
        /// <param name="resultMesh">The result mesh.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public static NavStatus Create(byte[] serializedMesh
            , out Navmesh resultMesh)
        {
            return UnsafeCreate(serializedMesh, true, out resultMesh);
        }

        private static NavStatus UnsafeCreate(byte[] serializedMesh
            , bool safeStorage
            , out Navmesh resultMesh)
        {
            if (serializedMesh == null || serializedMesh.Length == 0)
            {
                resultMesh = null;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            IntPtr root = IntPtr.Zero;

            NavStatus status = NavmeshEx.dtnmBuildDTNavMeshFromRaw(serializedMesh
                , serializedMesh.Length
                , safeStorage
                , ref root);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(root);
            else
                resultMesh = null;

            return status;
        }

        /// <summary>
        /// Creates an empty navigation mesh ready for tiles to be added.
        /// </summary>
        /// <remarks>
        /// This is the method used when creating new multi-tile meshes.
        /// Tiles are added using the <see cref="AddTile"/> method.
        /// </remarks>
        /// <param name="config">The mesh configuration.</param>
        /// <param name="resultMesh">The result mesh.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.
        /// </returns>
        public static NavStatus Create(NavmeshParams config
            , out Navmesh resultMesh)
        {
            if (config == null || config.maxTiles < 1)
            {
                resultMesh = null;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            IntPtr root = IntPtr.Zero;

            NavStatus status = NavmeshEx.dtnmInitTiledNavMesh(config, ref root);

            if (NavUtil.Succeeded(status))
                resultMesh = new Navmesh(root);
            else
                resultMesh = null;

            return status;
        }

        /// <summary>
        /// Extracts the tile data from a serialized navigation mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Tile data is normally preserved by serializing
        /// the the content of the <see cref="NavmeshTileData"/> objects used to
        /// create the navigation mesh.  That is the most efficient method
        /// and should be used whenever possible.
        /// </para>
        /// <para>
        /// This method can be used to extract the tile data when
        /// the original data is not available.  It should only be used as
        /// a backup to the normal method since this method is not efficient.
        /// </para>
        /// <para>
        /// Always check the header polygon count
        /// of the resulting <see cref="NavmeshTileExtract"/> objects before use since some tiles
        /// in the navigation mesh may be empty.  The <see cref="NavmeshTileExtract.data"/> 
        /// field will be null for empty tiles.
        /// </para>
        /// </remarks>
        /// <param name="serializedMesh">A valid serialized navigation mesh.</param>
        /// <param name="tileData">
        /// The extracted tile data. [Length: <see cref="Navmesh.GetMaxTiles()"/>]
        /// </param>
        /// <param name="config">The navigation mesh's configuration.</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        public static NavStatus ExtractTileData(byte[] serializedMesh
            , out NavmeshTileExtract[] tileData
            , out NavmeshParams config)
        {
            /*
             * Design notes:
             * 
             * Normally, the only way to get tile data out of a navigation mesh
             * is when the tile data is NOT owned by the mesh.  This is not
             * permitted for normal mesh objects, which is why the RemoveTile() method
             * never returns the tile data.
             * 
             * The most efficient way to extract the data is to get it directly
             * from the serialized data.  But that would be a code maintenance issue.
             * (Duplicating the mesh creation process.) So I'm using this rather 
             * convoluted method instead.
             * 
             */

            if (serializedMesh == null)
            {
                tileData = null;
                config = null;
                return NavStatus.Failure | NavStatus.InvalidParam;
            }

            Navmesh mesh;
            NavStatus status = Navmesh.UnsafeCreate(serializedMesh, false, out mesh);

            if ((status & NavStatus.Failure) != 0)
            {
                tileData = null;
                config = null;
                return status;
            }

            config = mesh.GetConfig();

            int count = mesh.GetMaxTiles();

            tileData = new NavmeshTileExtract[count];

            if (count == 0)
                return NavStatus.Sucess;

            for (int i = 0; i < count; i++)
            {
                NavmeshTile tile = mesh.GetTile(i);

                tileData[i].header = tile.GetHeader();

                if (tileData[i].header.polyCount == 0)
                    // Tile not in use.
                    continue;

                tileData[i].tileRef = tile.GetTileRef();

                IntPtr tdata = new IntPtr();
                int tsize = 0;

                NavmeshEx.dtnmRemoveTile(mesh.root, tileData[i].tileRef, ref tdata, ref tsize);

                tileData[i].data = UtilEx.ExtractArrayByte(tdata, tsize);

                NavmeshEx.dtnmFreeBytes(ref tdata);
            }

            return NavStatus.Sucess;
        }
    }
}
