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
 * Represents a set of related convex polygons within a bounded field.
 * <p>The polygons are usually connected (share edges), but are expected
 * to not intersect.</p>
 * <p>The data within this class is unprotected.  So care must be taken.</p>
 * <p><a href=
 * "http://www.critterai.org/projects/nmgen/images/stage_polygon_mesh.png"
 * target="_parent">
 * <img class="insert" height="465" src=
 * "http://www.critterai.org/projects/nmgen/images/stage_polygon_mesh.jpg"
 * width="620" />
 * </a></p>
 * @see PolyMeshFieldBuilder
 */
public final class PolyMeshField
    extends BoundedField
{
    
    /*
     * Recast Reference: rcPolyMesh in Recast.h
     */
    
    /**
     * Represents an index value for a null (non-existent) vertex.
     * <p>This is required since polygons held within {@link #polys} are
     * variable in size. So all entries for polygons within {@link #polys}
     * that have fewer vertices than {@link #maxVertsPerPoly()} are padded
     * with this value.  See {@link #polys} for more information.</p>
     */
    public static final int NULL_INDEX = -1;
    
    /**
     * The maximum vertices per polygon held within the {@link #polys} array.
     * ({@link #maxVertsPerPoly()} * 2) represents the stride of the array.
     * <p>This value is informational only.  There is no enforcement.
     */
    private final int mMaxVertsPerPoly;
    
    /**
     * The vertices that make up the field in the form (x, y, z). Value
     * defaults to null.
     */
    public int[] verts = null;
    
    /**
     * Holds flattened polygon index and neighbor information.  Value
     * defaults to null.
     * <p>Where mvpp = {@link #maxVertsPerPoly()}:</p>
     * <ul>
     * <li>Each polygon entry is 2*mvpp.</li>
     * <li>The first mvpp of each entry contains the indices of the polygon.
     * The first instance  of {@link #NULL_INDEX} means the end of polygon
     * indices for the entry.</li>
     * <li>The second mvpp of each entry contains indices to neighbor
     * polygons. A value of NULL_INDEX indicates no connection for that
     * particular edge.</li>
     * </ul>
     * <p>Example:</p>
     * <p>If mvpp = 6, the polygon has 4 vertices and 2 neighbor
     * connections.<br/>
     * Then for (1, 3, 4, 8, NULL_INDEX, NULL_INDEX, 18, NULL_INDEX, 21,
     * NULL_INDEX, NULL_INDEX, NULL_INDEX)<br/>
     * (1, 3, 4, 8) defines the polygon.<br/>
     * Polygon 18 shares edge 1->3.<br/>
     * Polygon 21 shares edge 4->8.<br/>
     * Edges 3->4 and 8->1 are border edges. (Not shared with any other
     * polygon.)<br/></p>
     */
    public int[] polys = null;
    
    /**
     * Holds membership region data for each polygon in the form (regionID).
     * Value defaults to null.
     */
    public int[] polyRegions = null;
    
    /**
     * Consructor
     * @param gridBoundsMin The minimum bounds of the field in the form
     * (minX, minY, minZ).
     * @param gridBoundsMax The maximum bounds of the field in the form
     * (maxX, maxY, maxZ).
     * @param cellSize The size of the cells.  (The grid that forms the base
     * of the field.)
     * @param cellHeight The height increment of the field.
     * @param maxVertsPerPoly The maximum vertices per polygon.  Value will
     * be auto-clamped to >=3.
     * @throws IllegalArgumentException If the bounds are null or the
     * wrong size.
     */
    public PolyMeshField(float[] gridBoundsMin
            , float[] gridBoundsMax
            , float cellSize
            , float cellHeight
            , int maxVertsPerPoly)
        throws IllegalArgumentException
    {
        super(gridBoundsMin, gridBoundsMax, cellSize, cellHeight);
        mMaxVertsPerPoly = Math.max(maxVertsPerPoly, 3);
    }
    
    /**
     * Gets the region ID of the polygon.
     * @param polyIndex The index of the polygon.
     * @return The region ID of the polygon, or -1 if the polygon index
     * is invalid.
     */
    public int getPolyRegion(int polyIndex)
    {
        if (polyIndex < 0 || polyIndex >= polyRegions.length)
            return -1;
        return polyRegions[polyIndex];
    }
    
    /**
     * Gets an array containing the vertices of the polygon.
     * <p>This is a costly convenience operation.</p>
     * @param polyIndex The index of the polygon.
     * @return An array containing the vertices of the polygon.
     */
    public int[] getPolyVerts(int polyIndex)
    {
        
        int pPoly = polyIndex*mMaxVertsPerPoly*2;
        if (polyIndex < 0 || pPoly >= polys.length)
            return null;
        
        // Determine the vertex count for this polygon.
        int polyVertCount = getPolyVertCount(pPoly, polys, mMaxVertsPerPoly);
        int[] result = new int[polyVertCount*3];
        
        // Get the vertices.
        for (int i = 0; i < polyVertCount; i++)
        {
            int pVert = polys[pPoly+i]*3;
            result[i*3] = verts[pVert];
            result[i*3+1] = verts[pVert+1];
            result[i*3+2] = verts[pVert+2];
        }
        
        return result;
    }
    
    /**
     * The maximum vertices per polygon held within the {@link #polys} array.
     * ({@link #maxVertsPerPoly()} * 2) represents the stride of the array.
     * <p>This value is informational only.  There is no enforcement.
     * @return The maximum vertices per polygon held within the
     * {@link #polys} array.
     */
    public int maxVertsPerPoly() { return mMaxVertsPerPoly; }
    
    /**
     * The number of polygons in the {@link #polys} array.
     * @return The number of vertices in the {@link #verts} array.
     */
    public int polyCount()
    {
        if (polys == null)
            return 0;
        return polys.length / (2 * mMaxVertsPerPoly);
    }
    
    /**
     * The number of vertices in the {@link #verts} array.
     * @return The number of vertices in the {@link #verts} array.
     */
    public int vertCount()
    {
        if (verts == null)
            return 0;
        return verts.length / 3;
    }
    
    /**
     * Returns the vertex count for the specified polygon in the polygon array.
     * <p>The array is assumed to to be well formed (correct size and layout).
     * Otherwise behavior is undefined.</p>
     * <p>Basically, this operation starts counting array entries starting at
     * the pointer. It continues to count until it runs into a NULL_INDEX
     * value or reaches maxVertsPerPoly, whichever occurs first.</p>
     * @param polyPointer  A pointer of the start of the polygon.
     * @param polys  An array of polygons in the standard format.
     * (E.g. NULL_INDEX indicates end of the polygon and each entry is
     * maxVertsPerPoly long.)
     * @param maxVertsPerPoly The maximum number of vertices for a polygon
     * in the array.
     * @return The number of vertices for the specified polygon.
     */
    static int getPolyVertCount(int polyPointer
                    , int[] polys
                    , int maxVertsPerPoly)
    {
        for (int i = 0; i < maxVertsPerPoly; i++)
            if (polys[polyPointer+i] == NULL_INDEX)
                // Ran into a null index. No more vertices for this polygon.
                return i;
        return maxVertsPerPoly;
    }

}
