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
using System.Runtime.InteropServices;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Provides high level information related to a <see cref="NavmeshTile"/> object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshTileHeader
    {
        /// <summary>
        /// The tile magic number. (Undocumented.)
        /// </summary>
        public int magic;

        /// <summary>
        /// The tile version. (Undocumented.)
        /// </summary>
        public int version;

        /// <summary>
        /// The x-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int tileX;

        /// <summary>
        /// The z-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int tileZ;

        /// <summary>
        /// The layer of the tile.
        /// </summary>
        /// <remarks>
        /// Layering occurs on the y-axis. (Height)
        /// </remarks>
        public int layer;

        /// <summary>
        /// The user-defined id of the tile.
        /// </summary>
        public uint userId;

        /// <summary>
        /// The number of polygons in the tile.
        /// </summary>
        public int polyCount;

        /// <summary>
        /// The number of polygon vertices in the tile. 
        /// </summary>
        public int vertCount;

        /// <summary>
        /// The number of links allocated. 
        /// </summary>
        public int maxLinkCount;

        /// <summary>
        /// The number of sub-meshes in the detail mesh.
        /// </summary>
        public int detailMeshCount;

        /// <summary>
        /// The number of unique vertices in the detail mesh. (In addition to the polygon vertices.)
        /// </summary>
        public int detailVertCount;

        /// <summary>
        /// The number of triangles in the detail mesh.
        /// </summary>
        public int detailTriCount;

        /// <summary>
        /// The number of bounding volume nodes. (Zero if bounding volumes are disabled.)
        /// </summary>
        public int bvNodeCount;

        /// <summary>
        /// The number of off-mesh connections.
        /// </summary>
        public int connCount;

        /// <summary>
        /// The index of the first polygon which is an off-mesh connection.
        /// </summary>
        public int connBase;

        /// <summary>
        /// The minimum floor to 'ceiling' height that will still  \allow the floor area to be 
        /// considered traversable.
        /// </summary>
        public float walkableHeight;

        /// <summary>
        /// The amount the polygon walls have been eroded away from 
        /// obstructions.
        /// </summary>
        public float walkableRadius;

        /// <summary>
        /// The maximum ledge height that is considered to still be
        /// traversable.
        /// </summary>
        public float walkableStep;

        /// <summary>
        /// The minimum bounds of the tile's AABB.
        /// </summary>
        public Vector3 boundsMin;

        /// <summary>
        /// The maximum bounds of the tile's AABB.
        /// </summary>
        public Vector3 boundsMax;

        /// <summary>
        /// The bounding volumn quantization factor. (For converting from world to bounding volumn 
        /// coordinates.)
        /// </summary>
        public float bvQuantFactor;

        /// <summary>
        /// Privides human readable text for the header. (Multi-line.)
        /// </summary>
        /// <returns>Text describing the header.</returns>
        public override string ToString()
        {
            return string.Format("Tile: X: {2}, Z: {3}, Layer: {4}\n"
                + "Version: {1}, UserId: {5}\n"
                + "Polys: {6}, Verts: {7}\n"
                + "Detail: Meshes: {9}, Tris: {11}, Verts: {10}\n"
                + "Conns: {13}, ConnBase: {14}\n"
                + "Walkable: Height: {15}, Radius: {16}, Step: {17}\n"
                + "Bounds: Min: {18}, Max: {19}\n"
                + "MaxLinks: {8}, BVQuantFactor: {20}, BVNodes: {12}\n"
                + "Magic: {0}\n"
                , magic, version
                , tileX, tileZ, layer
                , userId
                , polyCount, vertCount
                , maxLinkCount
                , detailMeshCount
                , detailVertCount
                , detailTriCount
                , bvNodeCount
                , connCount, connBase
                , walkableHeight
                , walkableRadius
                , walkableStep
                , Vector3Util.ToString(boundsMin)
                , Vector3Util.ToString(boundsMax)
                , bvQuantFactor);
        }
    }
}
