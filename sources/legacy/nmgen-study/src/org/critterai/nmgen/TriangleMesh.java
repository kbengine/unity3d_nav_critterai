/*
 * Copyright (c) 2010 Stephen A. Pratt
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
package org.critterai.nmgen;

/**
 * Represents the a triangle mesh created by the navigation mesh generation
 * process.
 * <p>WARNING: The core data within this class is unprotected.</p>
 * <p>
 * <a href=
 * "http://www.critterai.org/projects/nmgen/images/stage_detail_mesh.png"
 * target="_parent">
 * <img class="insert" height="465" src=
 * "http://www.critterai.org/projects/nmgen/images/stage_detail_mesh.jpg"
 * width="620" />
 * </a></p>
 */
public final class TriangleMesh
{

    /*
     * Recast Reference: rcPolyMeshDetail in Recast.h
     */
    
    /**
     * Vertices for the triangle mesh in the forma (x, y, z)
     */
    public float[] vertices = null;
    
    /**
     * Triangles in the mesh in the forma (vertAIndex, vertBIndex, vertCIndex)
     * where the vertices are wrapped clockwise.
     */
    public int[] indices = null;
    
    /**
     * The region to which each triangle belongs.
     * <p>Index corresponds to the indices array index.<p>
     */
    public int[] triangleRegions = null;
    
    /**
     * Gets the region ID associated with a triangle
     * @param index The index of the triangle.
     * @return The region ID of the triangle.  Or -1 if the index is invalid.
     */
    public int getTriangleRegion(int index)
    {
        if (index < 0 || index >= triangleRegions.length)
            return -1;
        return triangleRegions[index];
    }
    
    /**
     * Gets the vertices for a particular triangle in the form
     * (vertAx, vertAy, vertAz, vertBx, vertBy, vertBz, vertCx, vertCy, vertCz)
     * @param index The index of the triangle to retrieve.
     * @return The vertices in the specified triangle. Or null if the index is
     * invalid.
     */
    public float[] getTriangleVerts(int index)
    {
        
        int pTriangle = index*3;
        if (index < 0 || pTriangle >= indices.length)
            return null;
        
        float[] result = new float[9];
        
        for (int i = 0; i < 3; i++)
        {
            int pVert = indices[pTriangle+i]*3;
            result[i*3] = vertices[pVert];
            result[i*3+1] = vertices[pVert+1];
            result[i*3+2] = vertices[pVert+2];
        }
        
        return result;
    }
    
    /**
     * The number of triangles in the mesh.
     * @return The number of triangles in the mesh.
     */
    public int triangleCount()
    {
        return (triangleRegions == null ? 0 : triangleRegions.length);
    }
    
    /**
     * The number of vertices in the mesh.
     * @return The number of vertices in the mesh.
     */
    public int vertCount()
    {
        return (vertices == null ? 0 : vertices.length / 3);
    }

}
