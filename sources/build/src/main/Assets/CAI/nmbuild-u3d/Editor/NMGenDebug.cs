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
using UnityEngine;
using org.critterai.nmgen;
using org.critterai.u3d;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// Provides debug utilities related to navigation mesh generation. (Editor Only)
    /// </summary>
    public static class NMGenDebug
    {
        /// <summary>
        /// Draws a debug view of a <see cref="PolyMeshData"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Meant to be called during the MonoBehavior.OnRenderObject() method.
        /// </para>
        /// </remarks>
        /// <param name="polyData">The polygon mesh to draw.</param>
        public static void Draw(PolyMeshData polyData)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            Color walkableColor = new Color(0, 0.75f, 1.0f, 0.25f);
            Color nullRegionColor = new Color(0, 0, 0, 0.25f);

            int[] pTargetVert = new int[3];

            GL.Begin(GL.TRIANGLES);
            for (int iPoly = 0; iPoly < polyData.polyCount; iPoly++)
            {
                int pPoly = iPoly * polyData.maxVertsPerPoly * 2;

                if (polyData.areas[iPoly] == NMGen.MaxArea)
                    GL.Color(walkableColor);
                else if (polyData.areas[iPoly] == NMGen.NullRegion)
                    GL.Color(nullRegionColor);
                else
                    GL.Color(ColorUtil.IntToColor(polyData.areas[iPoly], 1.0f));

                pTargetVert[0] = polyData.polys[pPoly + 0] * 3;
                for (int iPolyVert = 2
                    ; iPolyVert < polyData.maxVertsPerPoly
                    ; iPolyVert++)
                {

                    if (polyData.polys[pPoly + iPolyVert]
                            == PolyMesh.NullIndex)
                        break;

                    pTargetVert[1] =
                        polyData.polys[pPoly + iPolyVert] * 3;
                    pTargetVert[2] =
                        polyData.polys[pPoly + iPolyVert - 1] * 3;

                    for (int i = 0; i < 3; i++)
                    {
                        int p = pTargetVert[i];
                        int x = polyData.verts[p + 0];
                        // Offset y a little to ensure it clears the 
                        // source geometry.
                        int y = polyData.verts[p + 1] + 1;
                        int z = polyData.verts[p + 2];
                        GL.Vertex3(polyData.boundsMin[0] + x * polyData.xzCellSize
                            , polyData.boundsMin[1] + y * polyData.yCellSize
                            , polyData.boundsMin[2] + z * polyData.xzCellSize);
                    }
                }
            }
            GL.End();

            Color internalEdgeColor = new Color(0, 0.2f, 0.25f, 0.25f);
            Color boundaryEdgeColor = new Color(0.65f, 0.2f, 0, 0.9f);

            GL.Begin(GL.LINES);
            for (int iPoly = 0; iPoly < polyData.polyCount; iPoly++)
            {
                int pPoly = iPoly * polyData.maxVertsPerPoly * 2;

                for (int iPolyVert = 0; iPolyVert < polyData.maxVertsPerPoly; iPolyVert++)
                {
                    int iv = polyData.polys[pPoly + iPolyVert];

                    if (iv == PolyMesh.NullIndex)
                        break;

                    if (polyData.polys[pPoly + polyData.maxVertsPerPoly + iPolyVert]
                                == PolyMesh.NullIndex)
                    {
                        GL.Color(boundaryEdgeColor);
                    }
                    else
                        GL.Color(internalEdgeColor);

                    // Note: Using only first two indexes.
                    pTargetVert[0] = iv * 3;

                    if (iPolyVert + 1 >= polyData.maxVertsPerPoly)
                    {
                        // Reached hard end of polygon.  Loop back.
                        iv = polyData.polys[pPoly + 0];
                    }
                    else
                    {
                        iv = polyData.polys[pPoly + iPolyVert + 1];

                        if (iv == PolyMesh.NullIndex)
                            // Reached soft end of polygon.  Loop back.
                            iv = polyData.polys[pPoly + 0];
                    }

                    pTargetVert[1] = iv * 3;

                    for (int i = 0; i < 2; i++)
                    {
                        int p = pTargetVert[i];
                        int x = polyData.verts[p + 0];
                        // Offset y a little to ensure it clears the 
                        // source geometry.
                        int y = polyData.verts[p + 1] + 1;
                        int z = polyData.verts[p + 2];

                        GL.Vertex3(polyData.boundsMin[0] + x * polyData.xzCellSize
                            , polyData.boundsMin[1] + y * polyData.yCellSize
                            , polyData.boundsMin[2] + z * polyData.xzCellSize);
                    }
                }
            }
            GL.End();
        }

        /// <summary>
        /// Draws a debug view of a <see cref="PolyMeshDetailData"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Meant to be called during the MonoBehavior.OnRenderObject() method.
        /// </para>
        /// </remarks>
        /// <param name="detailData">The detail mesh to draw.</param>
        public static void Draw(PolyMeshDetailData detailData)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.TRIANGLES);
            for (int iMesh = 0; iMesh < detailData.meshCount; iMesh++)
            {
                GL.Color(ColorUtil.IntToColor(iMesh, 0.75f));

                int pMesh = iMesh * 4;
                int pVertBase = (int)detailData.meshes[pMesh + 0];
                int pTriBase = (int)detailData.meshes[pMesh + 2] * 4;
                int tCount = (int)detailData.meshes[pMesh + 3];

                for (int iTri = 0; iTri < tCount; iTri++)
                {
                    for (int iVert = 0; iVert < 3; iVert++)
                    {
                        int pVert = pVertBase
                            + (detailData.tris[pTriBase + (iTri * 4 + iVert)]);

                        GL.Vertex(detailData.verts[pVert]);
                    }
                }
            }
            GL.End();

            // Draw the triangle lines.

            GL.Begin(GL.LINES);
            Color portalColor = new Color(0, 0, 0, 0.25f);
            for (int iMesh = 0; iMesh < detailData.meshCount; iMesh++)
            {
                Color meshColor = ColorUtil.IntToColor(iMesh, 1.0f);

                int pMesh = iMesh * 4;
                int pVertBase = (int)detailData.meshes[pMesh + 0];
                int pTriBase = (int)detailData.meshes[pMesh + 2] * 4;
                int tCount = (int)detailData.meshes[pMesh + 3];

                for (int iTri = 0; iTri < tCount; iTri++)
                {
                    byte flags = detailData.tris[pTriBase + (iTri * 4 + 3)];
                    for (int iVert = 0, iPrevVert = 2
                        ; iVert < 3
                        ; iPrevVert = iVert++)
                    {
                        if (((flags >> (iPrevVert * 2)) & 0x3) == 0)
                            GL.Color(meshColor);
                        else
                            GL.Color(portalColor);

                        int pVert = pVertBase
                            + (detailData.tris[pTriBase + (iTri * 4 + iVert)]);
                        int pPrevVert = pVertBase
                            + (detailData.tris[pTriBase + (iTri * 4 + iPrevVert)]);

                        GL.Vertex(detailData.verts[pVert]);
                        GL.Vertex(detailData.verts[pPrevVert]);
                    }
                }
            }
            GL.End();
        }
    }
}
