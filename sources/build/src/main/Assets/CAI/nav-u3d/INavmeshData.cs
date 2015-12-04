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

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// A ScritableObject that represents navigation data that baked at design time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is only valid when implemented by a ScriptableObject.
    /// </para>
    /// </remarks>
    public interface INavmeshData
    {
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
        NavmeshBuildInfo BuildInfo { get; }

        /// <summary>
        /// True if the navigation mesh is available.
        /// </summary>
        /// <returns>True if the navigation mesh is available.</returns>
        bool HasNavmesh { get; }

        /// <summary>
        /// Creates a new <see cref="Navmesh"/> object from the mesh data
        /// </summary>
        /// <returns>
        /// A new <see cref="Navmesh"/> object. Or null if the mesh is not available.
        /// </returns>
        Navmesh GetNavmesh();

        /// <summary>
        /// The version of the data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is used to track changes to the mesh data.  It is incremented every time new data is 
        /// loaded.
        /// </para>
        /// </remarks>
        int Version { get; }

        /// <summary>
        /// Loads a single-tile navigation mesh from the provided data.
        /// </summary>
        /// <param name="buildData">The tile build data.</param>
        /// <param name="buildConfig">The build information. (Optional)</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        NavStatus Load(NavmeshTileBuildData buildData, NavmeshBuildInfo buildConfig);

        /// <summary>
        /// Loads a navigation mesh.
        /// </summary>
        /// <param name="config">The mesh configuration.</param>
        /// <param name="tiles">The tiles to add to the mesh.</param>
        /// <param name="buildConfig">The build information. (Optional)</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        NavStatus Load(NavmeshParams config, NavmeshTileData[] tiles, NavmeshBuildInfo buildConfig);

        /// <summary>
        /// Load a navigation mesh from data created from the <see cref="Navmesh.GetSerializedMesh"/> 
        /// method.
        /// </summary>
        /// <param name="serializedMesh">The serialized mesh.</param>
        /// <param name="buildConfig">The build information. (Optional)</param>
        /// <returns>The <see cref="NavStatus"/> flags for the operation.</returns>
        NavStatus Load(byte[] serializedMesh, NavmeshBuildInfo buildConfig);

        /// <summary>
        /// Generates a human readable report of the mesh data.
        /// </summary>
        /// <returns>A human readable report of the mesh data.</returns>
        string GetMeshReport();
    }
}
