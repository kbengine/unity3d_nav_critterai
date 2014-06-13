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

import java.util.ArrayList;
import java.util.Hashtable;
import java.util.logging.Logger;

/**
 * Builds an convex polygon mesh consisting of variable sized polygons.
 * The mesh is generated from contour data contained by a {@link ContourSet}
 * object.
 * <p><a href=
 * "http://www.critterai.org/projects/nmgen/images/stage_polygon_mesh.png"
 * target="_parent"> <img class="insert" height="465" src=
 * "http://www.critterai.org/projects/nmgen/images/stage_polygon_mesh.jpg"
 * width="620" />
 * </a></p>
 * @see <a href="http://www.critterai.org/nmgen_polygen"
 * target="_parent">Convex Polygon Generation</a>
 * @see PolyMeshField
 */
public final class PolyMeshFieldBuilder
{
    
    /*
     * Design notes:
     * 
     * Recast Reference: rcBuildPolyMesh in RecastMesh.cpp
     * 
     * Never add setters.  Configuration should remain immutable to keep
     * the class thread friendly.
     */
    
    private static final Logger logger =
        Logger.getLogger(PolyMeshFieldBuilder.class.getName());
    
    /*
     * Flag and associated deflag.  Used during triangulation.
     */
    private static final int FLAG =   0x80000000;
    private static final int DEFLAG = 0x0fffffff;
    
    /**
     * IMPORTANT: Only use this value during creation of the
     * {@link PolyMeshField} objects. After that, use the value from the
     * object since the object may alter the value.
     */
    private final int mMaxVertsPerPoly;
    
    /**
     * Constructor.
     * @param maxVertsPerPoly The maximum vertices per polygon.  The builder
     * will not create polygons with more than this number of vertices.
     */
    public PolyMeshFieldBuilder(int maxVertsPerPoly)
    {
        mMaxVertsPerPoly = maxVertsPerPoly;
    }
    
    /**
     * Builds a convex polygon mesh from the provided contour set.
     * <p>This build algorithm will fail and return null if the
     * {@link ContourSet} contains any single contour with more than
     * 0x0fffffff vertices.
     * @param contours A properly populated contour set.
     * @return The result of the build operation.
     */
    public PolyMeshField build(ContourSet contours)
    {
        // Initialize
        if (contours == null || contours.size() == 0)
            return null;
        
        // Construct the result object.
        PolyMeshField result = new PolyMeshField(contours.boundsMin()
                , contours.boundsMax()
                , contours.cellSize()
                , contours.cellHeight()
                , mMaxVertsPerPoly);
        
        // Number of vertices found in the source.
        int sourceVertCount = 0;
        
        // The maximum possible number of polygons assuming that all will
        // be triangles.
        int maxPossiblePolygons = 0;
        
        // The maximum vertices found in a single contour.
        int maxVertsPerContour = 0;
        
        // Loop through all contours.  Determine the values for the
        // variables above.
        for (int contourIndex = 0
                ; contourIndex < contours.size()
                ; contourIndex++)
        {
            int count = contours.get(contourIndex).vertCount;
            sourceVertCount += count;
            maxPossiblePolygons += count - 2;
            maxVertsPerContour = Math.max(maxVertsPerContour, count);
        }
        
        if (sourceVertCount - 1 > DEFLAG)
        {
            // Too man vertices to be able to process.  Will run into the
            // the flag value.
            logger.severe("Polygon mesh generation failed: One or more" +
                    " input contours contain more than the maximum" +
                    " allowed vertices. (" + DEFLAG + ")");
            return null;
        }
        
        /*
         * Holds the unique vertices found during triangulation.
         * This array is sized to hold the maximum possible vertices.
         * The actual number of vertices will be smaller due to duplication
         * in the source contours.
         */
        final int[] globalVerts = new int[sourceVertCount * 3];
        int globalVertCount = 0;
        
        /*
         * Holds polygon indices.
         * 
         * The array is sized to hold the maximum possible polygons.
         * The actual number will be significantly smaller once polygons
         * are merged.
         * 
         * Where mvpp = maximum vertices per polygon:
         * 
         * Each polygon entry is mvpp. The first instance of NULL_INDEX means
         * the end of poly indices.
         * 
         * Example: If nvp = 6 and the the polygon has 4 vertices ->
         * (1, 3, 4, 8, NULL_INDEX, NULL_INDEX)
         * then (1, 3, 4, 8) defines the polygon.
         */
        final int[] globalPolys =
            new int[maxPossiblePolygons * mMaxVertsPerPoly];
        
        // Fill with null index.
        for (int i = 0; i < globalPolys.length; i++)
            globalPolys[i] = PolyMeshField.NULL_INDEX;
        
        final int[] globalRegions = new int[maxPossiblePolygons];
        int globalPolyCount = 0;

        /*
         * Holds information that allows mapping of contour vertex indices to
         * shared vertex indices.  (i.e. Index of vertex in contour.verts[]
         * to index of vertex in within this operation.)
         * 
         * index (key): The original vertex index of the contour.
         * value in array: The vertex index in the shared vertex array.
         * 
         * This is a working variable whose content is meaningless between
         * iterations. It will contain cross-iteration trash.  But that is
         * OK because of the way the array is used.  (I.e. Trash data left
         * over from a previous iteration  will never be accessed in the
         * current iteration.)
         */
        final int[] contourToGlobalIndicesMap = new int[maxVertsPerContour];
        
        /*
         * Key = Hash representing a unique vertex location.
         * Value = The index of the vertex in the global vertices array.
         * When a new vertex is found, it is added to the vertices array and
         * its global index stored in this hash table.  If a duplicate is
         * found, the value from this table is used.
         * There will always be duplicate vertices since different contours
         * are connected by these duplicate vertices.
         */
        final Hashtable<Integer, Integer> vertIndices =
            new Hashtable<Integer, Integer>();
        
        // Each list is initialized to a size that will minimize resizing.
        final ArrayList<Integer> workingIndices =
            new ArrayList<Integer>(maxVertsPerContour);
        final ArrayList<Integer> workingTriangles =
            new ArrayList<Integer>(maxVertsPerContour);
        
        // Various working variables.
        // (Values are meaningless outside of the iteration.)
        final int[] workingPolys =
            new int[(maxVertsPerContour + 1) * mMaxVertsPerPoly];
        int workingPolyCount = 0;
        final int[] mergeInfo = new int[3];
        final int[] mergedPoly = new int[mMaxVertsPerPoly];
        
        // Process all contours.
        for (int contourIndex = 0
                ; contourIndex < contours.size()
                ; contourIndex++)
        {
            Contour contour = contours.get(contourIndex);
            if (contour.verts.length < 3 * 4)
            {
                // This indicates a problem with contour creation
                // since the contour builder should detect for this.
                logger.severe("Polygon generation failure: Contour has " +
                        "too few vertices. Bad input data. Region "  +
                        contour.regionID);
                continue;
            }
            
            // Create working indices for the contour vertices.
            workingIndices.clear();
            for (int i = 0; i < contour.vertCount; i++)
                workingIndices.add(i);
            
            // Triangulate the contour.
            int triangleCount = triangulate(contour.verts
                    , workingIndices
                    , workingTriangles);
            
            if (triangleCount <= 0)
            {
                /*
                 * Failure of the triangulation.
                 * This is known to occur if the source polygon is
                 * self-intersecting or the source region contains internal
                 * holes.  In both cases, the problem is likely due to bad
                 * region formation.
                 */
                logger.severe("Polygon generation failure: Could not" +
                        " triangulate contour. Region "  + contour.regionID);
                continue;
            }
            
            /*
             * Loop through the vertices in this contour.
             * For new vertices (not seen in previous contours) get a new
             * index and add it to the global vertices array.
             */
            for (int iContourVert = 0
                    ; iContourVert < contour.vertCount
                    ; iContourVert++)
            {
                int pContourVert = iContourVert*4;
                int vertHash = getHashCode(contour.verts[pContourVert]
                                               , contour.verts[pContourVert+1]
                                               , contour.verts[pContourVert+2]);
                Integer iGlobalVert = vertIndices.get(vertHash);
                if (iGlobalVert == null)
                {
                    // This is the first time this vertex has been seen.
                    // Assign it an index and add it to the vertex array.
                    iGlobalVert = globalVertCount;
                    globalVertCount++;
                    vertIndices.put(vertHash, iGlobalVert);
                    globalVerts[iGlobalVert*3] = contour.verts[pContourVert];
                    globalVerts[iGlobalVert*3+1] =
                        contour.verts[pContourVert+1];
                    globalVerts[iGlobalVert*3+2] =
                        contour.verts[pContourVert+2];
                }
                // Creat the map entry.  Contour vertex index -> global
                // vertex index.
                contourToGlobalIndicesMap[iContourVert] = iGlobalVert;
            }
            

            
            // Initialize the working polygon array.
            for (int i = 0; i < workingPolys.length; i++)
                workingPolys[i] = PolyMeshField.NULL_INDEX;
            
            // Load the triangles into to the working polygon array, updating
            // indices in the process.
            workingPolyCount = 0;
            for (int i = 0; i < triangleCount; i++)
            {
                /*
                 * The working triangles list contains vertex index data
                 * from the contour. The working polygon array needs the
                 * global vertex index. So the indices mapping array created
                 * above is used to do  the conversion.
                 */
                workingPolys[workingPolyCount*mMaxVertsPerPoly]
                             = contourToGlobalIndicesMap[
                                                 workingTriangles.get(i*3)];
                workingPolys[workingPolyCount*mMaxVertsPerPoly+1]
                             = contourToGlobalIndicesMap[
                                                 workingTriangles.get(i*3+1)];
                workingPolys[workingPolyCount*mMaxVertsPerPoly+2]
                             = contourToGlobalIndicesMap[
                                                 workingTriangles.get(i*3+2)];
                workingPolyCount++;
            }
            
            if (mMaxVertsPerPoly > 3)
            {
                // Merging of triangles into larger polygons is permitted.
                // Continue until no polygons can be found to merge.
                // http://www.critterai.org/nmgen_polygen#mergepolys
                while(true)
                {
                    
                    int longestMergeEdge = -1;
                    int pBestPolyA = -1;
                    int iPolyAVert = -1;  // Start of the shared edge.
                    int pBestPolyB = -1;
                    int iPolyBVert = -1;  // Start of the shared edge.
                    
                    // Loop through all but the last polygon looking for the
                    // best polygons to merge in this iteration.
                    for (int iPolyA = 0
                            ; iPolyA < workingPolyCount - 1
                            ; iPolyA++)
                    {
                        for (int iPolyB = iPolyA + 1
                                ; iPolyB < workingPolyCount
                                ; iPolyB++)
                        {
                            // Can polyB merge with polyA?
                            getPolyMergeInfo(iPolyA*mMaxVertsPerPoly
                                    , iPolyB*mMaxVertsPerPoly
                                    , workingPolys
                                    , globalVerts
                                    , result.maxVertsPerPoly()
                                    , mergeInfo);
                            if (mergeInfo[0] > longestMergeEdge)
                            {
                                // polyB has the longest shared edge with
                                // polyA found so far. Save the merge
                                // information.
                                longestMergeEdge = mergeInfo[0];
                                pBestPolyA = iPolyA * mMaxVertsPerPoly;
                                iPolyAVert = mergeInfo[1];
                                pBestPolyB = iPolyB * mMaxVertsPerPoly;
                                iPolyBVert = mergeInfo[2];
                            }
                        }
                    }
                    
                    if (longestMergeEdge <= 0)
                        // No valid merges found during this iteration.
                        break;
                    
                    // Found polygons to merge.  Perform the merge.
                    
                    // Prepare the merged polygon array.
                    for (int i = 0; i < mergedPoly.length; i++)
                        mergedPoly[i] = PolyMeshField.NULL_INDEX;
                    
                    // Get the size of each polygon.
                    final int vertCountA
                        = PolyMeshField.getPolyVertCount(pBestPolyA
                                , workingPolys
                                , result.maxVertsPerPoly());
                    final int vertCountB
                        = PolyMeshField.getPolyVertCount(pBestPolyB
                                , workingPolys
                                , result.maxVertsPerPoly());
                    int position = 0;
                    
                    /*
                     * Fill the mergedPoly array.
                     * Start the vertex at the end of polygon A's shared edge.
                     * Add all vertices until looping back to the vertex just
                     * before the start of the shared edge. Repeat for
                     * polygon B.
                     * 
                     * Duplicate vertices are avoided, while ensuring we get
                     * all vertices, since each loop  drops the vertex that
                     * starts its polygon's shared edge and:
                     * 
                     * PolyAStartVert == PolyBEndVert and
                     * PolyAEndVert == PolyBStartVert.
                     */
                    for (int i = 0; i < vertCountA - 1; i++)
                        mergedPoly[position++] = workingPolys[pBestPolyA
                                         + ((iPolyAVert+1+i) % vertCountA)];
                    for (int i = 0; i < vertCountB - 1; i++)
                        mergedPoly[position++] = workingPolys[pBestPolyB
                                         + ((iPolyBVert+1+i) % vertCountB)];
                    
                    // Copy the merged polygon over the top of polygon A.
                    System.arraycopy(mergedPoly
                            , 0
                            , workingPolys
                            , pBestPolyA
                            , mMaxVertsPerPoly);
                    // Remove polygon B by shifting all information to the
                    // left by one polygon,  starting at polygon B.
                    System.arraycopy(workingPolys
                            , pBestPolyB + mMaxVertsPerPoly
                            , workingPolys, pBestPolyB
                            , workingPolys.length
                                    - pBestPolyB - mMaxVertsPerPoly);
                    workingPolyCount--;
                }
                
            }
            
            // Polygon creation for this contour is complete.
            // Add polygons to the global polygon array and store region
            // information.
            
            for (int i = 0; i < workingPolyCount; i++)
            {
                // Copy the polygon from the working array to the
                // correct position in the global array.
                System.arraycopy(workingPolys, i * mMaxVertsPerPoly
                        , globalPolys, globalPolyCount * mMaxVertsPerPoly
                        , mMaxVertsPerPoly);
                globalRegions[globalPolyCount] = contour.regionID;
                globalPolyCount++;
            }
            
        }
        
        /*
         * Transfer global array information into instance fields.
         * Could have loaded data directly into instance fields and saved this
         * processing cost.  But this method has memory benefits since it is
         * not necessary to oversize the instance arrays.
         */
        
        // Transfer vertex data.
        result.verts = new int[globalVertCount * 3];
        System.arraycopy(globalVerts, 0, result.verts, 0, globalVertCount * 3);
        
        /*
         * Transfer polygon indices data.
         * 
         * The global polygon array is half the size of the instance polygon
         * array since the instance polygon array also contains edge adjacency
         * information. So array copy can't be used.
         * 
         * Instead, copy the global polygon array over to instance polygon
         * array in blocks and initialize the instance polygon array's
         * adjacency information.
         */
        result.polys = new int[globalPolyCount * mMaxVertsPerPoly * 2];
        for (int iPoly = 0; iPoly < globalPolyCount; iPoly++)
        {
            int pPoly = iPoly*mMaxVertsPerPoly;
            for (int offset = 0; offset < mMaxVertsPerPoly; offset++)
            {
                // Transfer index information.
                result.polys[pPoly*2+offset] = globalPolys[pPoly+offset];
                // Initialize edge's adjacency field.
                result.polys[pPoly*2+mMaxVertsPerPoly+offset] =
                    PolyMeshField.NULL_INDEX;
            }
        }
        
        // Transfer region data.
        result.polyRegions = new int[globalPolyCount];
        System.arraycopy(globalRegions
                , 0
                , result.polyRegions
                , 0
                , globalPolyCount);

        // Build polygon adjacency information.
        buildAdjacencyData(result);
        
        return result;
        
    }
    
    /**
     * The maximum vertices per polygon.  The builder will not create
     * polygons with more than this number of vertices.
     * @return The maximum vertices per polygon.
     */
    public int maxVertsPerPoly() { return mMaxVertsPerPoly; }
    
    /**
     * Searches all polygons and adds adjacency data to the
     * {@link PolyMeshField#polys} array.
     * <p>All other data initialization must have been completed before
     * calling this operation. It is expected that all adjacency fields
     * within the {@link PolyMeshField#polys} array have been initialized
     * to NULL_INDEX before calling this operation.
     * @param mesh The mesh to use.
     */
    private static void buildAdjacencyData(PolyMeshField mesh)
    {
        
        int vertCount = mesh.verts.length / 3;
        // Purposely using the region count to avoid the division.
        int polyCount = mesh.polyRegions.length;
        
        int maxEdgeCount = polyCount * mesh.maxVertsPerPoly();
        
        /*
         * Holds edge information
         * 
         * IMPORTANT: This array does not catalog all edges.  It is only
         * guaranteed to catalog all shared edges.  It will contain only a
         * sub-set of border edges, which are edges only connected to a
         * single polygon.
         * 
         * Format:
         * 0: Index of primary vertex connected to the edge. This index's
         *    value will always be less  than the value of the secondary index.
         * 1: Index of secondary vertex connected to the edge.
         * 2: Index of polygon A connected to this edge.
         * 3: Polygon A vertex offset.
         * 4: Index of polygon B connected to this edge.
         *    (Or NULL_INDEX if this is a border edge.)
         * 5: Polygon B vertex offset.
         *    (Only meaningful if 4 != NULL_INDEX.)
         */
        int[] edges = new int[maxEdgeCount * 6];
        int edgeCount = 0;
        
        /*
         * An array used in edge searches based on an edge's primary index.
         * 
         * Index: Vertex index
         * Value: An index to an edge in the edges array that has the vertex
         * as its primary vertex.
         * 
         * Example of use:
         * 
         * Vertex index = 10;
         * startEdge[10] = 8  -> This vertex is the primary vertex for edge 8.
         * edges[8 * 6] <- Edge definition.
         * nextEdge[8] = 12 -> This vertex is also the primary vertex for
         *                     edge 12.
         * nextEdge[12] = 15 -> This vertex is also the primary vertex for
         *                      edge 15.
         * nextEdge[15] = NULL_INDEX -> This edge is not the primary index
         *                              for any further edges.
         * 
         * If the value for a vertex is NULL_INDEX then the vertex is not a
         * primary vertex for any known edge.  (This can occur because not
         * all border edges are cataloged by this algorithm.)
         */
        int[] startEdge = new int[vertCount];
        for (int i = 0; i < startEdge.length; i++)
            startEdge[i] = PolyMeshField.NULL_INDEX;
        
        /*
         * An array used in edge searches.
         * 
         * Use the startEdge array to get an index to a value in this array
         * in order to start an edge search.  See doc for startEdge for details.
         * 
         * Index: Edge Index  Same index used for the edges array and as
         *        values in the startEdge array.
         * 
         * Value: Edge index of next edge attacked to the same vertex.
         *        (Or NULL_INDEX if there are no more connected edges.)
         */
        int[] nextEdge = new int[maxEdgeCount];
        
        /*
         * Loop through all polygons.
         * Find all shared edges.  Populate all data arrays.
         * At the end of this loop, all data will be gathered except for
         * fields 4 and 5 in the edge array entries.
         */
        for (int iPoly = 0; iPoly < polyCount; iPoly++)
        {
            int pPoly = iPoly * mesh.maxVertsPerPoly() * 2;
            // Loop through each polygon vertex index.
            for (int vertOffset = 0
                    ; vertOffset < mesh.maxVertsPerPoly()
                    ; vertOffset++)
            {
                int iVert = mesh.polys[pPoly+vertOffset];
                if (iVert == PolyMeshField.NULL_INDEX)
                    // Reached the end of this polygon.
                    break;
                
                int iNextVert;
                if (vertOffset + 1 >= mesh.maxVertsPerPoly()
                        || mesh.polys[pPoly+vertOffset+1]
                                      == PolyMeshField.NULL_INDEX)
                    // Need to wrap to the beginning.  This will only happen
                    // once per iteration since the loop will be forced to
                    // end during the next iteration.
                    iNextVert = mesh.polys[pPoly];
                else
                    // The next vertex in the array is a valid vertex in
                    // the polygon.
                    iNextVert = mesh.polys[pPoly+vertOffset+1];
                
                /*
                 * This next check does several useful things:
                 * - It ensures that a particular edge is never selected
                 *   twice since, for shared edges, this condition will
                 *   only exist for one of the polygons.
                 * - Some border edges will be skipped entirely, saving
                 *   some processing time.
                 */
                if (iVert < iNextVert)
                {
                    // This is an edge's primary vertex.
                    // Set the vertices connected to this edge.
                    edges[edgeCount*6] = iVert;
                    edges[edgeCount*6+1] = iNextVert;
                    // Set the polygons associated with this edge.
                    edges[edgeCount*6+2] = iPoly;
                    edges[edgeCount*6+3] = vertOffset;
                    // Default to unconnected. (Border edge.)
                    edges[edgeCount*6+4] = PolyMeshField.NULL_INDEX;
                    edges[edgeCount*6+5] = PolyMeshField.NULL_INDEX;
                    
                    /*
                     *  Update the search arrays.
                     *  The first time a vertex is assigned as an edge's
                     *  primary vertex, the NULL_INDEX will be copied into
                     *  nextEdge, indicating the end of a vertex chain.
                     * 
                     *  The 2nd time a vertex is assign as an edge's primary
                     *  vertex, the original edge pointer from  startEdge
                     *  is added to nextEdge, and startEdge is assigned
                     *  the new starting edge.  This results in a stack-like
                     *  storage mechanism.
                     * 
                     *  The process is repeated every time the vertex is
                     *  assigned as a primary vertex, creating a chain.
                     */
                    nextEdge[edgeCount]
                             = startEdge[iVert];
                    startEdge[iVert] = edgeCount;
                    
                    edgeCount++;
                }

            }
        }
        
        /*
         * Loop through all polygons.
         * Find the the 2nd polygon's information for all shared edges.
         * (Fields 4 and 5 of the edge array entries.)
         */
        for (int iPoly = 0; iPoly < polyCount; iPoly++)
        {
            int pPoly = iPoly * mesh.maxVertsPerPoly() * 2;
            // Loop through each polygon vertex index.
            for (int vertOffset = 0
                    ; vertOffset < mesh.maxVertsPerPoly()
                    ; ++vertOffset)
            {
                int iVert = mesh.polys[pPoly+vertOffset];
                if (iVert == PolyMeshField.NULL_INDEX)
                    // Reached the end of this polygon.
                    break;
                
                int iNextVert;
                if (vertOffset + 1 >= mesh.maxVertsPerPoly()
                        || mesh.polys[pPoly+vertOffset+1]
                                      == PolyMeshField.NULL_INDEX)
                    // Need to wrap to the beginning.  This will only happen
                    // once per iteration since the loop will be forced to
                    // end during the next iteration.
                    iNextVert = mesh.polys[pPoly];
                else
                    // The next vertex in the array is a valid vertex in
                    // the polygon.
                    iNextVert = mesh.polys[pPoly+vertOffset+1];
                
                // Note that this next conditional is reversed from that
                // used in the previous loop.
                if (iVert > iNextVert)
                {
                    /*
                     * iVert is NOT a primary vertex in this case.  We are
                     * looking for the "other" polygon that shares this edge.
                     * If there is another polygon sharing this edge, its
                     * primary vertex will be iNextVert.
                     * 
                     * Climb the edge chain for iNextVert, looking for an
                     * edge that has iVert as its secondary vertex.
                     */
                    // Loop halts at the end of the chain.
                    for (int edgeIndex = startEdge[iNextVert]
                            ; edgeIndex != PolyMeshField.NULL_INDEX
                            ; edgeIndex = nextEdge[edgeIndex])
                    {
                        if (edges[edgeIndex*6+1] == iVert)
                        {
                            // Found a shared edge.  Assign this polygon
                            // as the secondary connected polygon.
                            edges[edgeIndex*6+4] = iPoly;
                            edges[edgeIndex*6+5] = vertOffset;
                            break;
                        }
                    }
                }
            }
        }
        
        // All necessary data has been gathered.  Any edge in the edge array
        // that has both polygons assigned is a shared edge.
        
        // Store adjacency information.
        // Loop through all edges.
        for (int pEdge = 0; pEdge < edgeCount; pEdge += 6)
        {
            if (edges[pEdge+4] != PolyMeshField.NULL_INDEX)
            {
                // The second polygon in this edge is set.
                // So this is a shared edge.
                int pPolyA = edges[pEdge+2] * mesh.maxVertsPerPoly() * 2;
                int pPolyB = edges[pEdge+4] * mesh.maxVertsPerPoly() * 2;
                // In the second section of the polygon definition, where
                // connection information is stored, put the polygon index
                // into the same position as the edge's primary vertex.
                mesh.polys[pPolyA + mesh.maxVertsPerPoly() + edges[pEdge+3]]
                           = edges[pEdge+4];
                mesh.polys[pPolyB + mesh.maxVertsPerPoly() + edges[pEdge+5]]
                           = edges[pEdge+2];
            }
        }
        
    }
    
    /**
     * Provides a hash value unique to the combination of values.
     * @param x  The vertices x-value. (x, y, z)
     * @param y  The vertices y-value. (x, y, z)
     * @param z  The vertices z-value. (x, y, z)
     * @return A hash that is unique to the vertex.
     */
    private static int getHashCode(int x, int y, int z)
    {
        /*
         * Note: Tried the standard eclipse hash generation method.  But
         * it resulted in non-unique hash values during testing.  Switched
         * to this method.
         * Hex values are arbitrary prime numbers.
         */
        return 0x8da6b343 * x + 0xd8163841 * y + 0xcb1ab31f * z;
    }
    
    /**
     * Returns the index incremented by one, or if the increment causes
     * an out of range high the minimum allowed index is returned.
     * (e.g. Wrapping)
     * @param i The index.
     * @param n The size of the array the index belongs to.
     * @return Returns the index incremented by one with wrapping.
     */
    private static int getNextIndex(int i, int n)
    {
        return i+1 < n ? i+1 : 0;
    }
    
    /**
     * Checks two polygons to see if they can be merged.  If a merge is
     * allowed, provides data via the outResult argument.
     * <p>outResult will be an array of size 3 with the following
     * information:</p>
     * <p>0: The lenghtSq of the edge shared between the polygons.<br/>
     * 1: The index (not pointer) of the start of the shared edge in
     *    polygon A.<br/>
     * 2: The index (not pointer) of the start of the shared edge in
     *    polygon B.<br/>
     * </p>
     * <p>A value of -1 at index zero indicates one of the following:</p>
     * <ul>
     * <li>The polygons cannot be merged because they would contain too
     * many vertices.</li>
     * <li>The polygons do not have a shared edge.</li>
     * <li>Merging the polygons would result in a concave polygon.</li>
     * </ul>
     * <p>To convert the values at indices 1 and 2 to pointers:
     * (polyPointer + value)</p>
     * @param polyAPointer The pointer to the start of polygon A in the
     * polys argument.
     * @param polyBPointer The pointer to the start of polygon B in the
     * polys argument.
     * @param polys An array of polygons in the form:
     * (vert1, vert2, vert3, ..., vertN, NULL_INDEX).
     * The null index terminates every polygon.  This permits polygons
     * with different vertex counts.
     * @param verts  The vertex data associated with the polygons.
     * @param outResult An array of size three which contains merge information.
     */
    private static void getPolyMergeInfo(int polyAPointer
            , int polyBPointer
            , int[] polys
            , int[] verts
            , int maxVertsPerPoly
            , int[] outResult)
    {
        
        outResult[0] = -1;  // Default to invalid merge
        outResult[1] = -1;
        outResult[2] = -1;
        
        final int vertCountA = PolyMeshField.getPolyVertCount(polyAPointer
                , polys
                , maxVertsPerPoly);
        final int vertCountB = PolyMeshField.getPolyVertCount(polyBPointer
                , polys
                , maxVertsPerPoly);
        
        // If the merged polygon would would have to many vertices, do not
        // merge. Subtracting two since to take into account the effect of
        // a merge.
        if (vertCountA + vertCountB - 2 > maxVertsPerPoly)
            return;
        
        /*
         * Check if the polygons share an edge.
         * 
         * Loop through all of vertices for polygonA and extract its edge.
         * (vertA -> vertANext) Then loop through all vertices for polygonB
         * and check to see if any of its edges use the same vertices as
         * polygonA.
         */
        for (int iPolyVertA = 0; iPolyVertA < vertCountA; iPolyVertA++)
        {
            // Get the vertex indices for the polygonA edge
            final int iVertA = polys[polyAPointer+iPolyVertA];
            final int iVertANext = polys[polyAPointer
                                     + getNextIndex(iPolyVertA, vertCountA)];
            // Search polygonB for matches.
            for (int iPolyVertB = 0; iPolyVertB < vertCountB; iPolyVertB++)
            {
                // Get the vertex indices for the polygonB edge.
                final int iVertB = polys[polyBPointer+iPolyVertB];
                final int iVertBNext = polys[polyBPointer
                                     + getNextIndex(iPolyVertB, vertCountB)];
                if (iVertA == iVertBNext && iVertANext == iVertB)
                {
                    // The vertex indices for this edge are the same and
                    // sequenced in opposite order.  So the edge is shared.
                    outResult[1] = iPolyVertA;
                    outResult[2] = iPolyVertB;
                }
            }
        }
        
        if (outResult[1] == -1)
            // No common edge, cannot merge.
            return;
        
        /*
         * Check to see if the merged polygon would be convex.
         * 
         * Gets the vertices near the section where the merge would occur.
         * Do they form a concave section?  If so, the merge is invalid.
         * 
         * Note that the following algorithm is only valid for clockwise
         * wrapped convex polygons.
         */
        
        int pSharedVertMinus, pSharedVert, pSharedVertPlus;
        
        pSharedVertMinus = polys[polyAPointer
                             + getPreviousIndex(outResult[1], vertCountA)] * 3;
        pSharedVert = polys[polyAPointer + outResult[1]] * 3;
        pSharedVertPlus = polys[polyBPointer
                                + ((outResult[2]+2) % vertCountB)] * 3;
        if (!isLeft(verts[pSharedVert]
                          , verts[pSharedVert+2]
                          , verts[pSharedVertMinus]
                          , verts[pSharedVertMinus+2]
                          , verts[pSharedVertPlus]
                          , verts[pSharedVertPlus+2]))
            /*
             * The shared vertex (center) is not to the left of segment
             * vertMinus->vertPlus.  For a clockwise wrapped polygon, this
             * indicates a concave section.  Merged polygon would be concave.
             * Invalid merge.
             */
            return;
        
        pSharedVertMinus = polys[polyBPointer
                             + getPreviousIndex(outResult[2], vertCountB)] * 3;
        pSharedVert = polys[polyBPointer + outResult[2]] * 3;
        pSharedVertPlus = polys[polyAPointer
                                + ((outResult[1]+2) % vertCountA)] * 3;
        if (!isLeft(verts[pSharedVert]
                          , verts[pSharedVert+2]
                          , verts[pSharedVertMinus]
                          , verts[pSharedVertMinus+2]
                          , verts[pSharedVertPlus]
                          , verts[pSharedVertPlus+2]))
            /*
             * The shared vertex (center) is not to the left of segment
             * vertMinus->vertPlus.  For a clockwise wrapped polygon, this
             * indicates a concave section.  Merged polygon would be concave.
             * Invalid merge.
             */
            return;
        
        // Get the vertex indices that form the shared edge.
        pSharedVertMinus = polys[polyAPointer + outResult[1]] * 3;
        pSharedVert = polys[polyAPointer
                            + getNextIndex(outResult[1], vertCountA)] * 3;
        
        // Store the lengthSq of the shared edge.
        final int deltaX = verts[pSharedVertMinus+0] - verts[pSharedVert+0];
        final int deltaZ = verts[pSharedVertMinus+2] - verts[pSharedVert+2];
        outResult[0] = deltaX*deltaX + deltaZ*deltaZ;

    }
    
    /**
     * Returns the index decremented by one, or if the decrement causes an
     * out of range low the maximum allowed index is returned.  (e.g. Wrapping)
     * @param i The index.
     * @param n The size of the array the index belongs to.
     * @return Returns the index decremented by one with wrapping.
     */
    private static int getPreviousIndex(int i, int n)
    {
        return i-1 >= 0 ? i-1 : n-1;
    }
    
    /**
     * The absolute value of the returned value is two times the area of the
     * triangle defined by points (A, B, C).
     * <p>A positive value indicates:</p>
     * <ul>
     * <li>Counterclockwise wrapping of the points.</li>
     * <li>Point B lies to the right of line AC, looking from A to C.</li>
     * </ul>
     * <p>A negative value indicates:</p>
     * <ul>
     * <li>Clockwise wrapping of the points.</li>
     * <li>Point B lies to the left of line AC, looking from A to C.</li>
     * </ul>
     * <p>A value of zero indicates that all points are collinear or
     * represent the same point.</p>
     * <p>This is a fast operation.<p>
     * @param ax The x-value for point (ax, ay) for vertex A of the triangle.
     * @param ay The y-value for point (ax, ay) for vertex A of the triangle.
     * @param bx The x-value for point (bx, by) for vertex B of the triangle.
     * @param by The y-value for point (bx, by) for vertex B of the triangle.
     * @param cx The x-value for point (cx, cy) for vertex C of the triangle.
     * @param cy The y-value for point (cx, cy) for vertex C of the triangle.
     * @return The signed value of two times the area of the triangle defined
     * by the points (A, B, C).
     */
    private static int getSignedAreaX2(int ax, int ay
            , int bx, int by
            , int cx, int cy)
    {
        /*
         * References:
         * 
         * http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm
         *                                                  #Modern%20Triangles
         * http://mathworld.wolfram.com/TriangleArea.html (Search for "signed".)
         * 
         */
        return (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
    }
    
    /**
     * Returns TRUE if the line segment AB intersects any edges not already
     * connected to one of the two vertices.
     * <p>The test is only performed on the xz-plane.</p>
     * <p>Assumptions:</p>
     * <ul>
     * <li>The vertices and indices arguments define a valid simple polygon
     * with vertices wrapped clockwise.</li>
     * <li>indexA != indexB</li>
     * </ul>
     * <p>Behavior is undefined if the arguments to not meet these
     * assumptions</p>
     * @param indexA An polygon index of a vertex that will form segment AB.
     * @param indexB An polygon index of a vertex that will form segment AB.
     * @param verts The vertices array in the form (x, y, z, id).  The value
     * stored at the id position is not relevant to this operation.
     * @param indices A simplpe polygon wrapped clockwise.
     * @return TRUE if the line segment AB intersects any edges not already
     * connected to one of the two vertices.  Otherwise FALSE.
     */
    private static boolean hasIllegalEdgeIntersection(int indexA
            , int indexB
            , int[] verts
            , ArrayList<Integer> indices)
    {
        
        // Get pointers to the primary vertices being tested.
        int pVertA = (indices.get(indexA) & DEFLAG) * 4;
        int pVertB = (indices.get(indexB) & DEFLAG) * 4;
        
        // Loop through the polygon edges.
        for (int iPolyEdgeBegin = 0
                ; iPolyEdgeBegin < indices.size()
                ; iPolyEdgeBegin++)
        {
            int iPolyEdgeEnd = getNextIndex(iPolyEdgeBegin, indices.size());
            if (!(iPolyEdgeBegin == indexA
                    || iPolyEdgeBegin == indexB
                    || iPolyEdgeEnd == indexA
                    || iPolyEdgeEnd == indexB))
            {
                // Neither of the test indices are endpoints of this edge.
                // Get pointers for this edge's verts.
                int pEdgeVertBegin = (indices.get(iPolyEdgeBegin) & DEFLAG) * 4;
                int pEdgeVertEnd = (indices.get(iPolyEdgeEnd) & DEFLAG) * 4;
                if ((verts[pEdgeVertBegin] == verts[pVertA]
                                 && verts[pEdgeVertBegin+2] == verts[pVertA+2])
                        || (verts[pEdgeVertBegin] == verts[pVertB]
                                 && verts[pEdgeVertBegin+2] == verts[pVertB+2])
                        || (verts[pEdgeVertEnd] == verts[pVertA]
                                 && verts[pEdgeVertEnd+2] == verts[pVertA+2])
                        || (verts[pEdgeVertEnd] == verts[pVertB]
                                 && verts[pEdgeVertEnd+2] == verts[pVertB+2]))
                    /*
                     * One of the test vertices is co-located on the xz plane
                     * with one of the endpoints of this edge.  (This is a
                     * test of the actual position of the verts rather than
                     * simply the index check performed earlier.)
                     * Skip this edge.
                     */
                    continue;
                
                /*
                 * This edge is not connected to either of the test vertices.
                 * If line segment AB intersects  with this edge, then the
                 * intersection is illegal.
                 * I.e. New edges cannot cross existing edges.
                 */
                if (Geometry.segmentsIntersect(verts[pVertA]
                        , verts[pVertA+2]
                        , verts[pVertB]
                        , verts[pVertB+2]
                        , verts[pEdgeVertBegin]
                        , verts[pEdgeVertBegin+2]
                        , verts[pEdgeVertEnd]
                        , verts[pEdgeVertEnd+2]))
                    return true;
                
            }
        }
        
        return false;
    }
    
    /**
     * Returns TRUE if point P is to the left of line AB when looking
     * from A to B.
     * @param px The x-value of the point to test.
     * @param py The y-value of the point to test.
     * @param ax The x-value of the point (ax, ay) that is point A on line AB.
     * @param ay The y-value of the point (ax, ay) that is point A on line AB.
     * @param bx The x-value of the point (bx, by) that is point B on line AB.
     * @param by The y-value of the point (bx, by) that is point B on line AB.
     * @return TRUE if point P is to the left of line AB when looking
     * from A to B.  Otherwise FALSE.
     */
    private static boolean isLeft(int px, int py
            , int ax, int ay
            , int bx, int by)
    {
        return getSignedAreaX2(ax, ay, px, py, bx, by) < 0;
    }
    
    /**
     * Returns TRUE if point P is to the left of line AB when looking
     * from A to B or is collinear with line AB.
     * @param px The x-value of the point to test.
     * @param py The y-value of the point to test.
     * @param ax The x-value of the point (ax, ay) that is point A on line AB.
     * @param ay The y-value of the point (ax, ay) that is point A on line AB.
     * @param bx The x-value of the point (bx, by) that is point B on line AB.
     * @param by The y-value of the point (bx, by) that is point B on line AB.
     * @return TRUE if point P is to the left of line AB when looking
     * from A to B, or is collinear with line AB.  Otherwise FALSE.
     */
    private static boolean isLeftOrCollinear(int px, int py
            , int ax, int ay
            , int bx, int by)
    {
        return getSignedAreaX2(ax, ay, px, py, bx, by) <= 0;
    }
    
    /**
     * Returns TRUE if point P is to the right of line AB when looking
     * from A to B.
     * @param px The x-value of the point to test.
     * @param py The y-value of the point to test.
     * @param ax The x-value of the point (ax, ay) that is point A on line AB.
     * @param ay The y-value of the point (ax, ay) that is point A on line AB.
     * @param bx The x-value of the point (bx, by) that is point B on line AB.
     * @param by The y-value of the point (bx, by) that is point B on line AB.
     * @return TRUE if point P is to the right of line AB when looking
     * from A to B.
     */
    private static boolean isRight(int px, int py
            , int ax, int ay
            , int bx, int by)
    {
        return getSignedAreaX2(ax, ay, px, py, bx, by) > 0;
    }
    
    /**
     * Returns TRUE if point P is to the right of or on line AB when looking
     * from A to B.
     * @param px The x-value of the point to test.
     * @param py The y-value of the point to test.
     * @param ax The x-value of the point (ax, ay) that is point A on line AB.
     * @param ay The y-value of the point (ax, ay) that is point A on line AB.
     * @param bx The x-value of the point (bx, by) that is point B on line AB.
     * @param by The y-value of the point (bx, by) that is point B on line AB.
     * @return TRUE if point P is to the right of or on line AB when looking
     * from A to B.
     */
    private static boolean isRightOrCollinear(int px, int py
            , int ax, int ay
            , int bx, int by)
    {
        return getSignedAreaX2(ax, ay, px, py, bx, by) >= 0;
    }
    
    /**
     * Returns TRUE if the line segment formed by vertex A and vertex B will
     * form a valid partition of the polygon.
     * <p>I.e. New line segment AB is internal to the polygon and will not
     * cross existing line segments.</p>
     * <p>The test is only performed on the xz-plane.</p>
     * <p>Assumptions:</p>
     * <ul>
     * <li>The vertices and indices arguments define a valid simple polygon
     * with vertices wrapped clockwise.</li>
     * <li>indexA != indexB</li>
     * </ul>
     * <p>Behavior is undefined if the arguments to not meet these
     * assumptions</p>
     * @param indexA An polygon index of a vertex that will form segment AB.
     * @param indexB An polygon index of a vertex that will form segment AB.
     * @param verts The vertices array in the form (x, y, z, id).  The value
     * stored at the id position is not relevant to this operation.
     * @param indices A simplpe polygon wrapped clockwise.
     * @return TRUE if the line segment formed by vertex A and vertex B will
     * form a valid partition of the polygon.  Otherwise false.
     */
    private static boolean isValidPartition(int indexA
            , int indexB
            , int[] verts
            , ArrayList<Integer> indices)
    {
        /*
         *  First check whether the segment AB lies within the internal
         *  angle formed at A. (This is the faster check.)
         *  If it does, then perform the more costly check.
         */
        return liesWithinInternalAngle(indexA, indexB, verts, indices)
            && !hasIllegalEdgeIntersection(indexA, indexB, verts, indices);
    }
    
    /**
     * Returns TRUE if vertex B lies within the internal angle of the polygon
     * at vertex A.
     * 
     * <p>Vertex B does not have to be within the polygon border.  It just has
     * be be within the area encompassed by the internal angle formed at
     * vertex A.</p>
     * 
     * <p>This operation is a fast way of determining whether a line segment
     * can possibly form a valid polygon partition.  If this test returns
     * FALSE, then more expensive checks can be skipped.</p>
     * <a href="http://www.critterai.org/nmgen_polygen#anglecheck"
     * >Visualizations</a>
     * <p>Special case:
     * FALSE is returned if vertex B lies directly on either of the rays
     * cast from vertex A along its associated polygon edges.  So the test
     * on vertex B is exclusive of the polygon edges.</p>
     * <p>The test is only performed on the xz-plane.</p>
     * <p>Assumptions:</p>
     * <ul>
     * <li>The vertices and indices arguments define a valid simple polygon
     * with vertices wrapped clockwise.</li>
     * <li>indexA != indexB</li>
     * </ul>
     * <p>Behavior is undefined if the arguments to not meet these
     * assumptions</p>
     * @param indexA An polygon index of a vertex that will form segment AB.
     * @param indexB An polygon index of a vertex that will form segment AB.
     * @param verts The vertices array in the form (x, y, z, id).  The value
     * stored at the id position is not relevant to this operation.
     * @param indices A simplpe polygon wrapped clockwise.
     * @return Returns TRUE if vertex B lies within the internal angle of
     * the polygon at vertex A.
     */
    private static boolean liesWithinInternalAngle(int indexA
            , int indexB
            , int[] verts
            , ArrayList<Integer> indices)
    {
        
        // Get pointers to the main vertices being tested.
        int pVertA = (indices.get(indexA) & DEFLAG) * 4;
        int pVertB = (indices.get(indexB) & DEFLAG) * 4;
        
        // Get poitners to the vertices just before and just after vertA.
        int pVertAMinus // The vertex just before A.
            = (indices.get(getPreviousIndex(indexA, indices.size())) & DEFLAG)
                * 4;
        int pVertAPlus  // The vert just after A.
            = (indices.get(getNextIndex(indexA, indices.size())) & DEFLAG) * 4;

        /*
         * First, find which of the two angles formed by the line segments
         *  AMinus->A->APlus is internal to (pointing towards) the polygon.
         * Then test to see if B lies within the area formed by that angle.
         */
        
        
        // TRUE if A is left of or on line AMinus->APlus
        if (isLeftOrCollinear(verts[pVertA]
                   , verts[pVertA+2]
                   , verts[pVertAMinus]
                   , verts[pVertAMinus+2]
                   , verts[pVertAPlus]
                   , verts[pVertAPlus+2]))
            // The angle internal to the polygon is <= 180 degrees
            // (non-reflex angle).
            // Test to see if B lies within this angle.
            
            return isLeft(
                    // TRUE if B is left of line A->AMinus
                    verts[pVertB]
                        , verts[pVertB+2]
                        , verts[pVertA]
                        , verts[pVertA+2]
                        ,verts[pVertAMinus]
                        , verts[pVertAMinus+2])
                   // TRUE if B is right of line A->APlus
                   && isRight(verts[pVertB]
                         , verts[pVertB+2]
                         , verts[pVertA]
                         , verts[pVertA+2]
                         , verts[pVertAPlus]
                         , verts[pVertAPlus+2]);
            
        /*
         * The angle internal to the polygon is > 180 degrees (reflex angle).
         * Test to see if B lies within the external (<= 180 degree) angle and
         * flip the result.  (If B lies within the external angle, it can't
         * lie within the internal angle.)
         */
        
        
        return !(
                // TRUE if B is left of or on line A->APlus
                isLeftOrCollinear(verts[pVertB]
                     , verts[pVertB+2]
                      , verts[pVertA]
                      , verts[pVertA+2]
                      , verts[pVertAPlus]
                      , verts[pVertAPlus+2])
                 // TRUE if B is right of or on line A->AMinus
                 && isRightOrCollinear(verts[pVertB]
                      , verts[pVertB+2]
                      , verts[pVertA]
                      , verts[pVertA+2]
                      , verts[pVertAMinus]
                      , verts[pVertAMinus+2]));
    }
    
    /**
     * Attempts to triangluate a polygon.
     * <p>Assumes the verts and indices arguments define a valid simple
     * (concave or convex) polygon
     * with vertices wrapped clockwise. Otherwise behavior is undefined.</p>
     * @param verts The vertices that make up the polygon in the format
     * (x, y, z, id).  The value stored at the id position is not relevant to
     * this operation.
     * @param inoutIndices    A working array of indices that define the
     * polygon to be triangluated.  The content is manipulated during the
     * operation and it will be left in an undefined state at the end of
     * the operation. (I.e. Its content will no longer be of any use.)
     * @param outTriangles  The indices which define the triangles derived
     * from the original polygon in the form
     * (t1a, t1b, t1c, t2a, t2b, t2c, ..., tna, tnb, tnc).  The original
     * content of this argument is discarded prior to use.
     * @return The number of triangles generated.  Or, if triangluation
     * failed, a negative number.
     */
    private static int triangulate(int[] verts
            , ArrayList<Integer> inoutIndices
            , ArrayList<Integer> outTriangles)
    {
        
        outTriangles.clear();
        
        /*
         * Terminology, concepts and such:
         * 
         * This algorithm loops around the edges of a polygon looking for
         * new internal edges to add that will partition the polygon into a
         * new valid triangle internal to the starting polygon. During each
         * iteration the shortest potential new edge is selected to form that
         * iteration's new triangle.
         * 
         * Triangles will only be formed if a single new edge will create
         * a triangle.  Two new edges will never be added during a single
         * iteration.  This means that the triangulated portions of the
         * original polygon will only contain triangles and the only
         * non-triangle polygon will exist in the untrianglulated portion
         * of the original polygon.
         * 
         * "Partition edge" refers to a potential new edge that will form a
         * new valid triangle.
         * 
         * "Center" vertex refers to the vertex in a potential new triangle
         * which, if the triangle is formed, will be external to the
         * remaining untriangulated portion of the polygon.  Since is
         * is now external to the polygon, it can't be used to form any
         * new triangles.
         * 
         * Some documentation refers to "iPlus2" even though the variable is
         * not in scope or does not exist for that section of code. For
         * documentation purposes, iPlus2 refers to the 2nd vertex after the
         * primary vertex.
         * E.g.: i, iPlus1, and iPlus2.
         * 
         * Visualizations: http://www.critterai.org/nmgen_polygen#triangulation
         */
        
        // Loop through all vertices, flagging all indices that represent
        // a center vertex of a valid new triangle.
        for (int i = 0; i < inoutIndices.size(); i++)
        {
            final int iPlus1 = getNextIndex(i, inoutIndices.size());
            final int iPlus2 = getNextIndex(iPlus1, inoutIndices.size());
            if (isValidPartition(i, iPlus2, verts, inoutIndices))
            {
                // A triangle formed by i, iPlus1, and iPlus2 will result
                // in a valid internal triangle.
                // Flag the center vertex (iPlus1) to indicate a valid triangle
                // location.
                inoutIndices.set(iPlus1, inoutIndices.get(iPlus1) | FLAG);
            }
        }
        
        /*
         * Loop through the vertices creating triangles. When there is only a
         * single triangle left,  the operation is complete.
         * 
         * When a valid triangle is formed, remove its center vertex.  So for
         * each loop, a single vertex will be removed.
         * 
         * At the start of each iteration the indices list is in the following
         * state:
         * - Represents a simple polygon representing the un-triangulated
         *   portion of the original polygon.
         * - All valid center vertices are flagged.
         */
        while (inoutIndices.size() > 3)
        {
            
            // Find the shortest new valid edge.
            
            // The minimum length found.
            int minLengthSq = -1;
            // The index for the start of the minimum length edge.
            int iMinLengthSqVert = -1;
            
            // NOTE: i and iPlus1 are defined in two different scopes in
            // this section. So be careful.
            
            // Loop through all indices in the remaining polygon.
            for (int i = 0; i < inoutIndices.size(); i++)
            {
                final int iPlus1 = getNextIndex(i, inoutIndices.size());
                if ((inoutIndices.get(iPlus1) & FLAG) == FLAG)
                {
                    // Indices i, iPlus1, and iPlus2 are known to form a
                    // valid triangle.
                    final int vert
                        = (inoutIndices.get(i) & DEFLAG) * 4;
                    final int vertPlus2
                        = (inoutIndices.get(getNextIndex(iPlus1
                                , inoutIndices.size())) & DEFLAG) * 4;
                    
                    // Determine the length of the partition edge.
                    // (i -> iPlus2)
                    int deltaX = verts[vertPlus2] - verts[vert];
                    int deltaZ = verts[vertPlus2+2] - verts[vert+2];
                    int lengthSq = deltaX * deltaX + deltaZ * deltaZ;
                    
                    if (minLengthSq < 0 || lengthSq < minLengthSq)
                    {
                        // This is either the first valid new edge, or an edge
                        // that is shorter than others previously found.
                        // Select it.
                        minLengthSq = lengthSq;
                        iMinLengthSqVert = i;
                    }
                }
            }
            
            if (iMinLengthSqVert == -1)
                /*
                 * Could not find a new triangle.  Triangulation failed.
                 * This happens if there are three or more vertices
                 * left, but none of them are flagged as being a
                 * potential center vertex.
                 */
                return -(outTriangles.size()/3);

            int i = iMinLengthSqVert;
            int iPlus1 = getNextIndex(i, inoutIndices.size());
            
            // Add the new triangle to the output.
            outTriangles.add(inoutIndices.get(i) & DEFLAG);
            outTriangles.add(inoutIndices.get(iPlus1) & DEFLAG);
            outTriangles.add(
                    inoutIndices.get(getNextIndex(iPlus1
                            , inoutIndices.size())) & DEFLAG);
            
            /*
             * iPlus1, the "center" vert in the new triangle, is now external
             * to the untriangulated portion of the polygon.  Remove it from
             * the indices list since it cannot be a member of any new
             * triangles.
             */
            inoutIndices.remove(iPlus1);
            
            if (iPlus1 == 0 || iPlus1 >= inoutIndices.size())
            {
                /*
                 * The vertex removal has invalidated iPlus1 and/or i.  So
                 * force a wrap, fixing the indices so they reference the
                 * correct indices again. This only occurs when the new
                 * triangle is formed across the wrap location of the polygon.
                 * Case 1: i = 14, iPlus1 = 15, iPlus2 = 0
                 * Case 2: i = 15, iPlus1 = 0, iPlus2 = 1;
                 */
                i = inoutIndices.size() - 1;
                iPlus1 = 0;
            }
            
            /*
             *  At this point i and iPlus1 refer to the two indices from a
             * successful triangluation that will be part of another new
             * triangle.  We now need to re-check these indices to see if they
             * can now be the center index in a potential new partition.
             */
            if (isValidPartition(getPreviousIndex(i , inoutIndices.size())
                    , iPlus1
                    , verts
                    , inoutIndices))
                inoutIndices.set(i, inoutIndices.get(i) | FLAG);
            else
                inoutIndices.set(i, inoutIndices.get(i) & DEFLAG);
            
            if (isValidPartition(i
                    , getNextIndex(iPlus1, inoutIndices.size())
                    , verts
                    , inoutIndices))
                inoutIndices.set(iPlus1, inoutIndices.get(iPlus1) | FLAG);
            else
                inoutIndices.set(iPlus1, inoutIndices.get(iPlus1) & DEFLAG);
            
        }
        
        // Only three vertices remain.  Add their triangle to the output list.
        
        outTriangles.add(inoutIndices.get(0) & DEFLAG);
        outTriangles.add(inoutIndices.get(1) & DEFLAG);
        outTriangles.add(inoutIndices.get(2) & DEFLAG);
        
        return outTriangles.size() / 3;
    }
    
}

