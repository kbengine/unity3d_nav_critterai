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
using org.critterai.geom;
using System.Collections.Generic;
using org.critterai.nmgen;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// Provides utility methods related to the UnityEngine.Terrain class.
    /// </summary>
    internal static class TerrainUtil
    {
        public static TriangleMesh TriangulateSurface(Terrain terrain, float resolution)
        {
            if (terrain == null || terrain.terrainData == null || resolution <= 0)
                return null;

            Vector3 origin = terrain.transform.position;

            int xCount;
            int zCount;
            Vector3 scale = DeriveScale(terrain, resolution, out xCount, out zCount);

            TriangleMesh m = new TriangleMesh(xCount * zCount, (xCount - 1) * (zCount - 1) * 2);

            TriangulateSurface(terrain, origin, scale, xCount, zCount, 0, m);

            return m;
        }

        public static TriangleMesh TriangulateSurface(Terrain terrain
            , float xmin, float zmin, float xmax, float zmax
            , float resolution
            , float yOffset)
        {
            if (terrain == null || terrain.terrainData == null 
                || resolution <= 0
                || xmin > xmax || zmin > zmax)
            {
                return null;
            }

            int xCount;
            int zCount;
            Vector3 scale = DeriveScale(terrain, resolution, out xCount, out zCount);

            Vector3 origin = terrain.transform.position;

            /*
             * We are generating part of a larger mesh.  The vertices must match that of the larger
             * mesh.
             * 
             * Convert input values to local grid space.
             * Clamp to the heightfield bounds.
             * Convert back to worldspace.
             */
            xmin = origin.x + Mathf.Max(0, Mathf.Floor((xmin - origin.x) / scale.x)) * scale.x;
            zmin = origin.z + Mathf.Max(0, Mathf.Floor((zmin - origin.z) / scale.z)) * scale.z;
            xmax = origin.x + Mathf.Min(xCount, Mathf.Ceil((xmax - origin.x) / scale.x)) * scale.x;
            zmax = origin.z + Mathf.Min(xCount, Mathf.Ceil((zmax - origin.z) / scale.z)) * scale.z;

            if (xmin + scale.x > xmax || zmin + scale.z > zmax)
                // Triangulation zone is too small.
                return null;

            // Everyting is already snapped to the grid.  But there may be floating point errors.
            // So round it.
            xCount = Mathf.RoundToInt((xmax - xmin) / scale.x);
            zCount = Mathf.RoundToInt((zmax - zmin) / scale.z);

            TriangleMesh m = new TriangleMesh(xCount * zCount, (xCount - 1) * (zCount - 1) * 2);

            TriangulateSurface(terrain
                , new Vector3(xmin, origin.y, zmin)
                , scale
                , xCount, zCount
                , yOffset
                , m);

            return m;
        }

        private static void TriangulateSurface(Terrain terrain
            , Vector3 origin
            , Vector3 scale
            , int xCount
            , int zCount
            , float yOffset
            , TriangleMesh buffer)
        {

            // Create the vertices by sampling the terrain.
            for (int ix = 0; ix < xCount; ix++)
            {
                float x = origin.x + ix * scale.x;
                for (int iz = 0; iz < zCount; iz++)
                {
                    float z = origin.z + iz * scale.z;
                    Vector3 pos = new Vector3(x, origin.y, z);
                    pos.y += terrain.SampleHeight(pos) + yOffset;
                    buffer.verts[buffer.vertCount] = pos;
                    buffer.vertCount++;
                }
            }

            // Triangulate surface sample points.
            for (int ix = 0; ix < xCount - 1; ix++)
            {
                for (int iz = 0; iz < zCount - 1; iz++)
                {
                    int i = iz + (ix * zCount);
                    int irow = i + zCount;

                    buffer.tris[buffer.triCount * 3 + 0] = i;
                    buffer.tris[buffer.triCount * 3 + 1] = irow + 1;
                    buffer.tris[buffer.triCount * 3 + 2] = irow;
                    buffer.triCount++;

                    buffer.tris[buffer.triCount * 3 + 0] = i;
                    buffer.tris[buffer.triCount * 3 + 1] = i + 1;
                    buffer.tris[buffer.triCount * 3 + 2] = irow + 1;
                    buffer.triCount++;
                }
            }
        }

        public static void TriangluateTrees(Terrain terrain
            , byte area
            , InputGeometryCompiler compiler)
        {
            if (terrain == null || terrain.terrainData == null || compiler == null)
                return;

            TerrainData data = terrain.terrainData;

            // Note: This array may be loaded with nulls.
            // This is required to keep indices in sync.
            Mesh[] protoMeshes = new Mesh[data.treePrototypes.Length];

            for (int i = 0; i < protoMeshes.Length; i++)
            {
                TreePrototype prototype = data.treePrototypes[i];
                MeshFilter filter = prototype.prefab.GetComponent<MeshFilter>();

                if (filter == null || filter.sharedMesh == null)
                {
                    Debug.LogWarning(string.Format(
                        "{0} : There is no mesh attached the {1} tree prototype."
                            + "Trees based on this prototype will be ignored."
                        , terrain.name
                        , prototype.prefab.name));
                    protoMeshes[i] = null;
                }
                else
                    protoMeshes[i] = filter.sharedMesh;
            }

            Vector3 terrainPos = terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            Queue<CombineInstance> combineInstances =
                new Queue<CombineInstance>(data.treeInstances.Length);

            foreach (TreeInstance tree in data.treeInstances)
            {
                if (protoMeshes[tree.prototypeIndex] == null)
                    // Prototype for this tree doesn't have a mesh.
                    continue;

                Vector3 pos = tree.position;
                pos.x *= terrainSize.x;
                pos.y *= terrainSize.y;
                pos.z *= terrainSize.z;
                pos += terrainPos;

                Vector3 scale =
                    new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);

                CombineInstance ci = new CombineInstance();
                ci.mesh = protoMeshes[tree.prototypeIndex];
                ci.transform = Matrix4x4.TRS(pos, Quaternion.identity, scale);

                combineInstances.Enqueue(ci);
            }

            protoMeshes = null;

            if (combineInstances.Count == 0)
                return;

            MeshUtil.CombineMeshes(combineInstances, area, compiler);
        }

        public static Vector3 DeriveScale(Terrain terrain, float resolution
            , out int widthCount, out int depthCount)
        {
            widthCount = 0;
            depthCount = 0;

            if (terrain == null || terrain.terrainData == null || resolution <= 0)
                return Vector3.zero;

            Vector3 scale = terrain.terrainData.heightmapScale;
            widthCount = terrain.terrainData.heightmapWidth;
            depthCount = terrain.terrainData.heightmapHeight;

            if (resolution > 0 && resolution < 1)
            {
                Vector3 size = terrain.terrainData.size;
                widthCount = Mathf.FloorToInt(size.x / scale.x * resolution);
                depthCount = Mathf.FloorToInt(size.z / scale.z * resolution);

                scale.x = size.x / (float)widthCount;
                scale.z = size.z / (float)depthCount;

                // For the vertices along the maximum bounds...
                widthCount++;
                depthCount++;
            }

            return scale;
        }
    }
}
