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
using System.Collections.Generic;
using org.critterai.nmgen;

namespace org.critterai.nmbuild.u3d.editor
{
    internal static class MeshUtil
    {
        public static void CombineMeshes(Queue<CombineInstance> items
            , byte area
            , InputGeometryCompiler compiler)
        {
            const int MaxTris = 65000;

            List<CombineInstance> combineInstancesPart = new List<CombineInstance>();
            byte[] areas = NMGen.CreateAreaBuffer(MaxTris, area);

            while (items.Count != 0)
            {
                int vertCount = 0;

                while (items.Count > 0
                    && (vertCount + items.Peek().mesh.vertexCount < MaxTris))
                {
                    vertCount += items.Peek().mesh.vertexCount;
                    combineInstancesPart.Add(items.Dequeue());
                }

                Mesh meshPart = new Mesh();

                meshPart.CombineMeshes(combineInstancesPart.ToArray(), true, true);

                compiler.AddTriangles(meshPart.vertices, meshPart.vertexCount
                    , meshPart.triangles, areas, meshPart.triangles.Length / 3);

                Object.DestroyImmediate(meshPart);

                combineInstancesPart.Clear();
            }
        }
    }
}
