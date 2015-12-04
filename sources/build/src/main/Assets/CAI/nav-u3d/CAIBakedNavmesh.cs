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
using UnityEngine;
using org.critterai.nav;
using org.critterai.nav.u3d;

/// <summary>
/// Navigation mesh data that is baked at design time.
/// </summary>
[System.Serializable]
public sealed class CAIBakedNavmesh
    : ScriptableObject, INavmeshData
{
    [SerializeField]
    private byte[] mDataPack = null;

    [SerializeField]
    private NavmeshBuildInfo mBuildInfo;

    [SerializeField]
    private int mVersion;

    /// <summary>
    /// Information related to the build of the mesh data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This information is used by the build system to perform partial re-builds and provide
    /// helpful information to the user.
    /// </para>
    /// <para>
    /// Partial re-build features will not be available if this property is null.
    /// </para>
    /// </remarks>
    public NavmeshBuildInfo BuildInfo { get { return mBuildInfo.Clone(); } }

    /// <summary>
    /// True if the navigation mesh is available.
    /// </summary>
    /// <returns>True if the navigation mesh is available.</returns>
    public bool HasNavmesh
    {
        get { return (mDataPack != null && mDataPack.Length > 0); }
    }

    /// <summary>
    /// The version of the data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used to track changes to the mesh data.  It is incremented every time new data is 
    /// loaded.
    /// </para>
    /// </remarks>
    public int Version { get { return mVersion; } }

    /// <summary>
    /// Creates a new <see cref="Navmesh"/> object from the mesh data
    /// </summary>
    /// <returns>A new <see cref="Navmesh"/> object. Or null if the mesh is not available.</returns>
    public Navmesh GetNavmesh()
    {
        if (!HasNavmesh)
            return null;

        Navmesh result;
        if (NavUtil.Failed(Navmesh.Create(mDataPack, out result)))
            return null;

        return result;
    }

    /// <summary>
    /// Generates a human readable report of the mesh data.
    /// </summary>
    /// <returns>A human readable report of the mesh data.</returns>
    public string GetMeshReport()
    {
        if (!HasNavmesh)
            return "No mesh.";

        Navmesh nm = GetNavmesh();

        NavmeshParams nmconfig = nm.GetConfig();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("Navigation mesh report for " + name);

        if (mBuildInfo != null)
            sb.AppendLine("Built from scene: " + mBuildInfo.inputScene);

        sb.AppendLine(string.Format("Tiles: {0}, Tile Size: {1:F3}x{2:F3}, Max Polys Per Tile: {3}"
            , nmconfig.maxTiles, nmconfig.tileWidth, nmconfig.tileDepth, nmconfig.maxPolysPerTile));

        int polyCount = 0;
        int vertCount = 0;
        int connCount = 0;

        for (int i = 0; i < nmconfig.maxTiles; i++)
        {
            NavmeshTileHeader header = nm.GetTile(i).GetHeader();

            if (header.polyCount == 0)
                continue;

            sb.AppendLine(string.Format(
                "Tile ({0},{1}): Polygons: {2}, Vertices: {3}, Off-mesh Connections: {4}"
                , header.tileX, header.tileZ
                , header.polyCount, header.vertCount
                , header.connCount));

            polyCount += header.polyCount;
            vertCount += header.vertCount;
            connCount += header.connCount;
        }

        sb.AppendLine(string.Format(
            "Totals: Polygons: {0}, Vertices: {1}, Off-mesh Connections: {2}"
            , polyCount, vertCount, connCount));

        return sb.ToString();
    }

    /// <summary>
    /// Loads a single-tile navigation mesh from the provided data.
    /// </summary>
    /// <param name="buildData">The tile build data.</param>
    /// <param name="buildConfig">The build information. (Optional)</param>
    /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
    public NavStatus Load(NavmeshTileBuildData buildData, NavmeshBuildInfo buildConfig)
    {
        if (buildData == null || buildData.IsDisposed)
            return NavStatus.Failure | NavStatus.InvalidParam;

        Navmesh navmesh;
        NavStatus status = Navmesh.Create(buildData, out navmesh);

        if ((status & NavStatus.Sucess) == 0)
            return status;

        mDataPack = navmesh.GetSerializedMesh();

        if (mDataPack == null)
            return NavStatus.Failure;

        mBuildInfo = buildConfig;

        mVersion++;

        return NavStatus.Sucess;
    }

    /// <summary>
    /// Loads a navigation mesh.
    /// </summary>
    /// <param name="config">The mesh configuration.</param>
    /// <param name="tiles">The tiles to add to the mesh.</param>
    /// <param name="buildConfig">The build information. (Optional)</param>
    /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
    public NavStatus Load(NavmeshParams config
        , NavmeshTileData[] tiles
        , NavmeshBuildInfo buildConfig)
    {
        if (config == null || tiles == null || tiles.Length > config.maxTiles)
            return NavStatus.Failure | NavStatus.InvalidParam;

        Navmesh navmesh;
        NavStatus status = Navmesh.Create(config, out navmesh);

        if ((status & NavStatus.Sucess) == 0)
            return status;

        foreach (NavmeshTileData tile in tiles)
        {
            if (tile == null)
                continue;

            uint trash;
            status = navmesh.AddTile(tile, Navmesh.NullTile, out trash);

            if ((status & NavStatus.Sucess) == 0)
                return status | NavStatus.InvalidParam;
        }

        mDataPack = navmesh.GetSerializedMesh();

        if (mDataPack == null)
            return NavStatus.Failure;

        mBuildInfo = buildConfig;

        mVersion++;

        return NavStatus.Sucess;
    }

    /// <summary>
    /// Load a navigation mesh from data created from the <see cref="Navmesh.GetSerializedMesh"/> 
    /// method.
    /// </summary>
    /// <param name="serializedMesh">The serialized mesh.</param>
    /// <param name="buildConfig">The build information. (Optional)</param>
    /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
    public NavStatus Load(byte[] serializedMesh, NavmeshBuildInfo buildConfig)
    {
        if (serializedMesh == null)
            return NavStatus.Failure | NavStatus.InvalidParam;

        // This roundabout method is used for validation.

        Navmesh navmesh;
        NavStatus status = Navmesh.Create(serializedMesh, out navmesh);

        if ((status & NavStatus.Sucess) == 0)
            return status;

        mDataPack = navmesh.GetSerializedMesh();

        if (mDataPack == null)
            return NavStatus.Failure;

        mBuildInfo = buildConfig;

        mVersion++;

        return NavStatus.Sucess;
    }
}
