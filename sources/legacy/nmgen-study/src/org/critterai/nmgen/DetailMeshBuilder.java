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

import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.logging.Logger;

/**
 * Builds an triangle mesh from {@link OpenHeightfield } and
 * {@link PolyMeshField} data.  The polygon mesh field is triangulated and
 * detail added as needed to match the surface of the  mesh to the surface
 * defined in the open heightfield.
 * <p>
 * <a href=
 * "http://www.critterai.org/projects/nmgen/images/stage_detail_mesh.png"
 * target="_parent">
 * <img class="insert" height="465" src=
 * "http://www.critterai.org/projects/nmgen/images/stage_detail_mesh.jpg"
 * width="620" />
 * </a></p>
 * @see <a href="http://www.critterai.org/nmgen_detailgen"
 * target="_parent">Detail Mesh Generation</a>
 * @see TriangleMesh
 */
public final class DetailMeshBuilder
{

    /*
     * Design notes:
     * 
     * Recast Reference: rcBuildPolyMeshDetail in RecastMeshDetail.cpp
     * 
     * Not adding configuration getters until they are needed.
     * Never add setters.  Configuration should remain immutable to keep
     * the class thread friendly.
     */
    
    /*
     * TODO: EVAL: Need to review the process of loading and getting values
     * from the height patch.
     * 
     * Current design assumes that there are natural special cases that
     * result in vertices  improperly showing as outside of the patch.  But
     * it may be a design issue that needs fixing rather than true special
     * cases such as floating point errors. See getHeightWithinField() for
     * descriptions of the special cases currently being taken into account.
     */
    
    /**
     * Represents height information for a portion (or patch) of a larger
     * heightfield.
     * <p>Various fields indicate which portion of the source heightfield is
     * represented.</p>
     * <p>Unlike normal heightfields, this class cannot represent overlaps.
     * Each grid location can only hold a single data point.</p>
     * <p>Data is filled using a flood method.  I.e. Start at a particular
     * span in the source height field, and flood outward to the patch's
     * boundaries.</p>
     * <p>Since the value {@link #UNSET} is used to indicate the lack of
     * data at a particular location, the effective maximum height value
     * is {@link Integer#MAX_VALUE} - 1.</p>
     * <p>WARNING: It is critical that public fields be set correctly.
     * Otherwise operation exceptions may be thrown.</p>
     */
    private class HeightPatch
    {
        /**
         * Indicates that data at a particular grid location has not been set.
         * (Has no value.)  After data has been loaded, this means
         * that there is no valid height information for the particular grid
         * location.
         */
        public static final int UNSET = Integer.MAX_VALUE;
        
        /**
         * The width index for the origin of this patch.
         * (Should be a valid width index within the larger height field.)
         */
        int minWidthIndex;
        
        /**
         * The depth index for the origin of this patch.
         * (Should be a valid depth index within the larger height field.)
         */
        int minDepthIndex;
        
        /**
         * The width of this patch as measured from the patch's origin.
         */
        int width;
        
        /**
         * The depth of this patch as measured from the patch's origin.
         */
        int depth;

        /**
         * Height data for this patch.
         * <p>Should normally use the {@link #setData(int, int, int) setData}
         * and {@link #getData(int, int) getData} operations to access data
         * in this array.</p>
         * <p>Storage: [(Depth Data) (Depth Data) ... (Depth Data)]
         * So the location of a particular data point is
         * widthIndex * {@link #depth} + depthIndex</p>
         * <p>Array should be sized such that it can hold at least
         * {@link #width} x {@link #depth} worth of data.</p>
         */
        int[] data;
        
        /**
         * Gets the height data for the patch location.
         * <p>The indices are auto-clamped.</p>
         * @param globalWidthIndex The width index of the grid location
         * from the source height field.
         * @param globalDepthIndex The depth index of the grid location
         * from the source height field.
         * @return The data stored a the grid location.
         */
        public int getData(int globalWidthIndex, int globalDepthIndex)
        {
            // Do not remove auto-clamping.  It is an important feature.
            // See special case comments in getHeightWithinField() for
            // details on why.
            int idx = Math.min(Math.max(
                            globalWidthIndex - minWidthIndex, 0)
                            , width - 1) * depth
                + Math.min(Math.max(
                            globalDepthIndex - minDepthIndex, 0)
                            , depth - 1);
            return data[idx];
        }
        
        /**
         * Indicates whether or not a global index (width, height) is
         * within the bounds of the patch.
         * @param globalWidthIndex The width index from the patch's
         * source heightfield.
         * @param globalDepthIndex The depth index from the patch's
         * source heightfield.
         * @return If the global index (width, height) is within the
         * bounds of the patch.
         */
        public boolean isInPatch(int globalWidthIndex, int globalDepthIndex)
        {
            return (globalWidthIndex >= minWidthIndex
                    && globalDepthIndex >= minDepthIndex
                    && globalWidthIndex < minWidthIndex + width
                    && globalDepthIndex < minDepthIndex + depth);
        }
        
        /**
         * Sets all values in the data array to {@link #UNSET}.
         */
        public void resetData()
        {
            if (data == null)
                return;
            for (int i = 0; i < data.length; i++)
                data[i] = UNSET;
        }
        
        /**
         * Sets the height data for the patch location.
         * <p>WARNING: No argument validation is performed. Calling code
         * should perform all needed validations prior to calling this
         * operation.</p>
         * @param globalWidthIndex The width index of the grid location
         * from the source height field.
         * @param globalDepthIndex The depth index of the grid location
         * from the source height field.
         * @param value The data to store at the grid location.
         */
        public void setData(int globalWidthIndex
                        , int globalDepthIndex
                        , int value)
        {
            data[(globalWidthIndex - minWidthIndex)*depth
                         +globalDepthIndex-minDepthIndex] = value;
        }
        
    }
    
    private static final Logger logger =
        Logger.getLogger(DetailMeshBuilder.class.getName());
    
    /**
     * Information is undefined.  (Not yet set.)
     */
    private static final int UNDEFINED = -1;
    
    /**
     * This side of the edge is external to the main polygon.  (It is part
     * of the polygon's hull.)
     * <p>Edges internal to the polygon (i.e. those created during
     * triangulation) should never have this value.</p>
     */
    private static final int HULL = -2;
    
    /**
     * The maximum number of vertices allowed for a triangulated polygon mesh.
     * <p>This value is arbitrary.</p>
     */
    private static final int MAX_VERTS = 256;
    
    /**
     * The maximum number of edges that a single edge can be broken into
     * during edge sampling.
     * <p>This value is arbitrary.</p>
     */
    private static final int MAX_EDGES = 64;

    /**
     * Contour matching sample resolution.
     * <p>Impacts how well the final mesh conforms to surface data in the
     * {@link OpenHeightfield} Higher values, closer conforming, higher
     * final triangle count.</p>
     */
    private final float mContourSampleDistance;
    
    /**
     * Contour matching maximum deviation.
     * <p>Impacts how well the final mesh conforms to the original meshes
     * surface contour. Lower values, closer conforming, higher final
     * triangle count.</p>
     */
    private final float mContourMaxDeviation;
    
    /**
     * 
     * @param contourSampleDistance Sets the sampling distance to use when
     * matching the final mesh to the surface defined by the
     * {@link OpenHeightfield}.
     * <p>Impacts how well the final mesh conforms to surface data in the
     * {@link OpenHeightfield} Higher values, closer conforming, higher
     * final triangle count.</p>
     * <p>Setting this argument to zero will disable this functionality.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param contourMaxDeviation The maximum distance the surface of the
     * navmesh may deviate from the surface data in the {@link OpenHeightfield}.
     * <p>The accuracy of the algorithm which uses this value is impacted by
     * the value of the contour sample distance argument.</p>
     * <p>The value of this argument has no meaning if the contour sample
     * distance argument is set to zero.</p>
     * <p>Setting the value to zero is not recommended since it can result
     * in a large increase in the number of triangles in the final navmesh
     * at a high processing cost.</p>
     * <p>Constraints:  >= 0</p>
     */
    public DetailMeshBuilder(float contourSampleDistance
            , float contourMaxDeviation)
    {
        mContourSampleDistance = Math.max(0, contourSampleDistance);
        mContourMaxDeviation = Math.max(0, contourMaxDeviation);
    }

    /**
     * Build a triangle mesh with detailed height information from the
     * provided polygon mesh.
     * <p>Concerning sampling: Sampling functionality will only work
     * correctly if the y-values in the source mesh are accurate to within
     * {@link OpenHeightfield#cellHeight()} of an associated span within the
     * provided height field.  Otherwise the algorithms used by
     * this operation may not be able to find accurate height information
     * from the {@link OpenHeightfield}.
     * @param sourceMesh The source polygon mesh to build the triangle
     * mesh from.
     * @param heightField The heightfield from which the {@link PolyMeshField}
     * was derived.
     * @return The generated triangle mesh.  Or null if there were errors
     * which prevented triangulation.
     */
    public TriangleMesh build(PolyMeshField sourceMesh
            , OpenHeightfield heightField)
    {
        if (sourceMesh == null
                || sourceMesh.vertCount() == 0
                || sourceMesh.polyCount() == 0)
            return null;

        // Create result object.
        final TriangleMesh mesh = new TriangleMesh();
        
        // Saves on divisions within loops.
        final int sourcePolyCount = sourceMesh.polyCount();
        
        // Convenience variables.
        final float cellSize = sourceMesh.cellSize();
        final float cellHeight = sourceMesh.cellHeight();
        final float[] minBounds = sourceMesh.boundsMin();
        final int maxVertsPerPoly = sourceMesh.maxVertsPerPoly();
        final int[] sourceVerts = sourceMesh.verts;
        final int[] sourcePolys = sourceMesh.polys;
        
        /*
         * Contains the xz-plane bounds of each polygon.
         * Entry format: (xmin, xmax, zmin, zmax)
         * Uses the same index as sourcePolys.
         */
        final int[] polyXZBounds = new int[sourcePolyCount * 4];
        
        // The total number of polygon vertices.
        // Needs to be calculated since the vertex count of the source
        // polygons vary.
        int totalPolyVertCount = 0;
        
        // The maximum width and depth found for the source polygons.
        int maxPolyWidth = 0;
        int maxPolyDepth = 0;
        
        /*
         * Gather data.
         * Loop through each polygon and find its xz bounds, the number of
         * vertices, and the overall maximum polygon width and depth.
         */
        for (int iPoly = 0; iPoly < sourcePolyCount; iPoly++)
        {
            final int pPoly = iPoly * maxVertsPerPoly * 2;
            // These next variables are pointers to this poly's bound fields
            // in polyXZBounds.
            final int pxmin = iPoly*4;
            final int pxmax = iPoly*4+1;
            final int pzmin = iPoly*4+2;
            final int pzmax = iPoly*4+3;
            // Initialize the bounds fields to their extremes.
            polyXZBounds[pxmin] = heightField.width();
            polyXZBounds[pxmax] = 0;
            polyXZBounds[pzmin] = heightField.depth();
            polyXZBounds[pzmax] = 0;
            // Loop through each vertex in the polygon, searching for
            // minimum/maximum vertex values.
            for (int vertOffset = 0; vertOffset < maxVertsPerPoly; vertOffset++)
            {
                if(sourcePolys[pPoly+vertOffset] == PolyMeshField.NULL_INDEX)
                    // Reached the end of this polygon's vertices.
                    break;
                final int pVert = sourcePolys[pPoly+vertOffset]*3;
                // Adjust the values of the bounds if this vertex
                // represents a new min/max.
                polyXZBounds[pxmin] = Math.min(polyXZBounds[pxmin]
                                                        , sourceVerts[pVert]);
                polyXZBounds[pxmax] = Math.max(polyXZBounds[pxmax]
                                                        , sourceVerts[pVert]);
                polyXZBounds[pzmin] = Math.min(polyXZBounds[pzmin]
                                                        , sourceVerts[pVert+2]);
                polyXZBounds[pzmax] = Math.max(polyXZBounds[pzmax]
                                                        , sourceVerts[pVert+2]);
                // Increment the total vertex count.
                totalPolyVertCount++;
            }
            /*
             * Clamp the values to one less than the minimum and one more
             * than the maximum while staying within the valid field bounds.
             * This ensures that when a height patch is created later, it is
             * guaranteed to encompass the entire polygon.
             */
            polyXZBounds[pxmin] = Math.max(0,polyXZBounds[pxmin] - 1);
            polyXZBounds[pxmax] = Math.min(heightField.width()
                    ,polyXZBounds[pxmax] + 1);
            polyXZBounds[pzmin] = Math.max(0,polyXZBounds[pzmin] - 1);
            polyXZBounds[pzmax] = Math.min(heightField.depth()
                    ,polyXZBounds[pzmax] + 1);
            if (polyXZBounds[pxmin] >= polyXZBounds[pxmax]
                                 || polyXZBounds[pzmin] >= polyXZBounds[pzmax])
                // NO chance of this polygon being the max width/depth polygon.
                continue;
            // Record whether this polygon has the largest width of depth
            // found so far.
            maxPolyWidth = Math.max(maxPolyWidth
                    , polyXZBounds[pxmax] - polyXZBounds[pxmin]);
            maxPolyDepth = Math.max(maxPolyDepth
                    , polyXZBounds[pzmax] - polyXZBounds[pzmin]);
        }
        
        // Holds the vertices of the current polygon to be triangulated.
        final float[] poly = new float[maxVertsPerPoly*3];
        int polyVertCount = 0;
        
        /*
         * Holds the triangle indices generated for the current polygon.
         * Form: (vertAIndex, vertBIndex, vertCIndex)
         * Initialized to allow for an increase in vertices during sampling.
         */
        ArrayList<Integer> polyTriangles
            = new ArrayList<Integer>(maxVertsPerPoly * 2 * 3);
        
        /*
         * Hold the vertex information for the triangles.
         * If sampling occurs, there will be more vertices in this array
         * than in the polygon array. Otherwise the content will be the same.
         */
        final float[] polyTriangleVerts = new float[MAX_VERTS*3];
        int polyTriangleVertCount = 0;
        
        final HeightPatch hfPatch = new HeightPatch();
        if (mContourSampleDistance > 0)
            /*
             * The height patch is only used in the polygon detail
             * operation if the sample distance is > 0.
             * Set the bounds of the patch to be searched.
             * Sized to hold the largest possible polygon.
             */
            hfPatch.data = new int[maxPolyWidth * maxPolyDepth];
            
        /*
         * Values within working variables are irrelevant outside the
         * operations  in which they are used. Their only purpose is to
         * save on object creation cost during loop iterations.
         * Most initialization sizes are arbitrary.
         */
        final ArrayDeque<Integer> workingStack =
            new ArrayDeque<Integer>(256);
        final ArrayDeque<OpenHeightSpan> workingSpanStack =
            new ArrayDeque<OpenHeightSpan>(128);
        final ArrayList<Integer> workingEdges =
            new ArrayList<Integer>(MAX_EDGES * 4);
        final ArrayList<Integer> workingSamples = new ArrayList<Integer>(512);
        
        /*
         * A working array used while building the height path.
         * Its data is not used within this operation.
         */
        final int[] workingWidthDepth = new int[2];
        
        /*
         * Holds the aggregate vertices for the entire mesh.
         * Form: (x, y, z)
         */
        final ArrayList<Float> globalVerts =
            new ArrayList<Float>(totalPolyVertCount * 2 * 3);
        
        /*
         * Holds the aggregate triangles for the entire mesh.
         * Format (vertAIndex, vertBIndex, vertCIndex, regionID)
         * where vertices are wrapped clockwise and regionID is the region
         * id of the source polygon the triangles were generated from.
         */
        final ArrayList<Integer> globalTriangles =
            new ArrayList<Integer>(totalPolyVertCount * 2 * 4);
        
        // Triangluate all polygons.
        for (int iPoly = 0; iPoly < sourcePolyCount; iPoly++)
        {
            final int pPoly = iPoly*maxVertsPerPoly*2;

            // Loop through all vertices in the current polygon and
            // load the working polygon array.
            polyVertCount = 0;
            for (int vertOffset = 0; vertOffset < maxVertsPerPoly; vertOffset++)
            {
                if(sourcePolys[pPoly+vertOffset] == PolyMeshField.NULL_INDEX)
                    // Reached the end of the polygon's verts.
                    break;
                
                final int pVert = sourcePolys[pPoly+vertOffset]*3;
                
                // Load the vertex information for the current polygon
                // into the working poly array.
                poly[vertOffset*3+0] = sourceVerts[pVert] * cellSize;
                poly[vertOffset*3+1] = sourceVerts[pVert+1] * cellHeight;
                poly[vertOffset*3+2] = sourceVerts[pVert+2] * cellSize;
                
                polyVertCount++;
                  
            }
            
            if (mContourSampleDistance > 0)
            {
                // The height patch is only used if the sample
                // distance is > 0.
                
                // Load height patch data for this polygon.
                
                // Set the bounds to the min/max for current polygon.
                hfPatch.minWidthIndex = polyXZBounds[iPoly*4];
                hfPatch.minDepthIndex = polyXZBounds[iPoly*4+2];
                hfPatch.width =
                    polyXZBounds[iPoly*4+1] - polyXZBounds[iPoly*4+0];
                hfPatch.depth =
                    polyXZBounds[iPoly*4+3] - polyXZBounds[iPoly*4+2];
                
                // Load the height data.
                loadHeightPatch(pPoly
                        , polyVertCount
                        , sourcePolys
                        , sourceVerts
                        , heightField
                        , hfPatch
                        , workingStack
                        , workingSpanStack
                        , workingWidthDepth);
            }
            
            // Triangulate this polygon.
            polyTriangleVertCount = buildPolyDetail(poly
                    , polyVertCount
                    , heightField
                    , hfPatch
                    , polyTriangleVerts
                    , polyTriangles
                    , workingEdges
                    , workingSamples);
            
            if (polyTriangleVertCount < 3)
            {
                logger.severe("Generation of detail polygon failed:"
                                + " Polygon lost. Region: "
                                + sourceMesh.getPolyRegion(iPoly)
                                + ", Polygon index: " + iPoly);
                continue;
            }
            
            // Make sure the global lists are able to handle the new data.
            globalVerts.ensureCapacity(
                    globalVerts.size() + polyTriangleVertCount * 3);
            globalTriangles.ensureCapacity(
                    globalTriangles.size() + polyTriangles.size() * 4 / 3);
            
            // Represents the next available vertex index.
            final int indexOffset = globalVerts.size() / 3;
            
            // Add all new vertices to the global vertices list.
            for (int iVert = 0; iVert < polyTriangleVertCount; iVert++)
            {
                // Note: Converting from height field to world coordinates.
                globalVerts.add(polyTriangleVerts[iVert*3] + minBounds[0]);
                globalVerts.add(polyTriangleVerts[iVert*3+1] + minBounds[1]);
                globalVerts.add(polyTriangleVerts[iVert*3+2] + minBounds[2]);
            }
            
            // Add all new triangles to the global triangles list.
            for (int pTriangle = 0
                    ; pTriangle < polyTriangles.size()
                    ; pTriangle += 3)
            {
                // Offset the original vertex index to match the index in
                // the global vertex list.
                globalTriangles.add(
                        polyTriangles.get(pTriangle) + indexOffset);
                globalTriangles.add(
                        polyTriangles.get(pTriangle+1) + indexOffset);
                globalTriangles.add(
                        polyTriangles.get(pTriangle+2) + indexOffset);
                // Record the region.
                globalTriangles.add(sourceMesh.getPolyRegion(iPoly));
            }
        }
        
        // Transfer the final results to the mesh object.
        
        // Load mesh object with vertex data.
        mesh.vertices = new float[globalVerts.size()];
        for (int i = 0; i < globalVerts.size(); i++)
            mesh.vertices[i] = globalVerts.get(i);
        
        // Load mesh object with the triangle indices and region information.
        mesh.indices = new int[globalTriangles.size() * 3 / 4];
        final int tcount = globalTriangles.size() / 4;
        mesh.triangleRegions = new int[tcount];
        for (int i = 0; i < tcount; i++)
        {
            // The index and region information is split and set to two
            // different locations in the mesh object.
            final int sourcePointer = i*4;
            final int destinationPointer = i*3;
            mesh.indices[destinationPointer] =
                globalTriangles.get(sourcePointer);
            mesh.indices[destinationPointer+1] =
                globalTriangles.get(sourcePointer+1);
            mesh.indices[destinationPointer+2] =
                globalTriangles.get(sourcePointer+2);
            mesh.triangleRegions[i] = globalTriangles.get(sourcePointer+3);
        }
        
        return mesh;
        
    }
    
    /**
     * Performs sampling and triangulation of the polygon.
     * <p>Sampling increases the detail of the polygon so that the height of
     * the final triangle mesh better follows the height of its section of
     * the heightfield.</p>
     * <p>If the sample distance is > 0, sampling will occur.  Otherwise
     * sampling will not occur and the number of vertices in the final mesh
     * will equal the number of vertices in the source polygon.</p>
     * @param sourcePoly  Represents a list of vertices in the format
     * (x, y, z) that represent a clockwise wrapped convex polygon.
     * @param sourceVertCount  The number of vertices in the source polygon.
     * @param heightField The height field from which the polygon was derived.
     * @param patch  The loaded height patch to use for sample vertex
     * height data.
     * <p>This parameter can be null if the the sample distance is <= 0.<p/>
     * @param outVerts The vertices for the triangle mesh generated from
     * the polygon.
     * <p>The array must be sized to be able to fit the maximum number of
     * vertices that can be generated.</p>
     * <p>The output may contain trash information.  Only vertex locations
     * which have an associated index in the output triangles list represent
     * real vertices.
     * @param outTriangles The indices of the triangle mesh generated from
     * the polygon. Its content is cleared before use.
     * @param workingEdges A working list used by this operation.  Its
     * content is undefined outside of this operation.  Its content is
     * cleared prior to use.
     * @param workingSamples A working list used by this operation.  Its
     * content is undefined outside of this operation.  Its content is
     * cleared prior to use.
     * @return The number of vertices in the outVerts array.
     * <p>For successful completion: Will equal the source vertex
     * count if not new vertices were added. Otherwise be greater
     * than the source vertex count by the number of vertices added.</p>
     * <p>For a failure: The value will be zero. The polygon should be
     * discarded.</p>
     */
    private int buildPolyDetail(float[] sourcePoly
            , int sourceVertCount
            , OpenHeightfield heightField
            , HeightPatch patch
            , float[] outVerts
            , ArrayList<Integer> outTriangles
            , ArrayList<Integer> workingEdges
            , ArrayList<Integer> workingSamples)
    {
        
        // There is no early exit for a source vertex count of 3
        // (a triangle) since sampling may increase the vertex count.
        
        // TODO: EVAL: There is a lot of array object creation going on here.
        // Investigate optimization.
        
        // TODO: EVAL: Should the height field reference be removed in favor
        // of integer parameters for cell size and such?  Parameter count is
        // getting a bit unruly.  Then again, this is a private operation.
        
        // Holds potential vertices for an edge that may be broken up into
        // smaller edges.
        final float[] workingVerts = new float[(MAX_EDGES+1)*3];
        
        // Holds the indices to the actual smoothed edge definition.
        // The indices reference vertices in the working vertices array.
        final int[] workingIndices = new int[MAX_EDGES];
        int workingIndicesCount = 0;
        
        /* Contains indices of vertices that represent the hull of the
         * polygon which the triangulation cannot alter. The content can be
         * considered as "seed" edges for the triangulation and is not
         * used unless the sample distance is > zero.
         * Points to vertices in the output vertex array.
         */
        final int[] hullIndices = new int[MAX_VERTS];
        int hullIndicesCount = 0;
        
        // Convenience variable and a variable to reduce number of divisions.
        final float cellSize = heightField.cellSize();
        final float inverseCellSize = 1.0f/heightField.cellSize();
        
        // Seed the output vertices array with the source vertices.
        System.arraycopy(sourcePoly, 0, outVerts, 0, sourceVertCount*3);
        
        // The number of vertices in the final detail polygon.  Will equal
        // the source vertex count if no new vertices are added.  This is
        // the return value.
        int outVertCount = sourceVertCount;
        
        // Any value sample vertex height value equal to or greater than
        // this value means a height could not be found in the patch.
        final float heightPathLimit = 
            HeightPatch.UNSET * heightField.cellHeight();
        
        if (mContourSampleDistance > 0)
        {
            
            /*
             * Create the mandatory hull edges.
             * 
             * The purpose of this algorithm is to better match the height
             * of the polygon edges to the height of the source field.
             * Vertices are added to edges so that the polygon's edges
             * better fit the contour of the height field.
             * 
             * This is the first of two sampling passes.  In this pass only
             * the edges are sampled.  In the second pass, the inside of the
             * polygon is sampled.
             * 
             * See: http://www.critterai.org/nmgen_detailgen#edgedetail
             */
            
            // Loop through all source polygon edges.
            for (int iSourceVertB = 0, iSourceVertA = sourceVertCount - 1
                    ; iSourceVertB < sourceVertCount
                    ; iSourceVertA = iSourceVertB++)
            {
                int pSourceVertA = iSourceVertA*3;
                int pSourceVertB = iSourceVertB*3;
                boolean swapped = false;
                /*
                 * Next section applies a consistent sort to the vertices so
                 * that segments are always processed in the same order.
                 * 
                 * I.e. If VertexA and VertexB represent an edge shared
                 * between two polygons, then when this operation is called
                 * for each polygon, the edge vertices  are processed in the
                 * same order no matter the order they are defined within
                 * each polygon.
                 * 
                 * This prevents seams from forming between polygons due to
                 * floating point errors.
                 */
                if (Math.abs(sourcePoly[pSourceVertA]
                                - sourcePoly[pSourceVertB]) < Float.MIN_VALUE)
                {
                    if (sourcePoly[pSourceVertA+2] > sourcePoly[pSourceVertB+2])
                    {
                        pSourceVertA = iSourceVertB*3;
                        pSourceVertB = iSourceVertA*3;
                        swapped = true;
                    }
                }
                else if (sourcePoly[pSourceVertA] > sourcePoly[pSourceVertB])
                {
                    pSourceVertA = iSourceVertB*3;
                    pSourceVertB = iSourceVertA*3;
                    swapped = true;
                }
                
                // Note: The ordering of the subtraction in these deltas
                // is important. Later code depends on this ordering.
                final float deltaX =
                    sourcePoly[pSourceVertB] - sourcePoly[pSourceVertA];
                final float deltaZ =
                    sourcePoly[pSourceVertB+2] - sourcePoly[pSourceVertA+2];
                final float edgeXZLength =
                    (float)Math.sqrt(deltaX * deltaX + deltaZ * deltaZ);
                
                // Get the maximum edge index.  (This is purposely not an
                // edge count.) This value is based on how many edge's this
                // edge is allowed to be broken into.
                int iMaxEdge =
                    1 + (int)Math.floor(edgeXZLength/mContourSampleDistance);
                // Clamp to max allowed edges.
                iMaxEdge = Math.min(iMaxEdge, MAX_EDGES);
                if (iMaxEdge + outVertCount >= MAX_VERTS)
                    // The addition of these new edges would result in
                    // too many vertices in the polygon. Adjust edge count
                    // so we don't exceed maximum allowed verts.
                    iMaxEdge = MAX_VERTS - 1 - outVertCount;
                
                /*
                 * Split the source edge into equally sized segments based
                 * on the maximum number of new edges allowed.
                 * 
                 * The edge is being build from vertex A toward vertex B.
                 */
                for (int iEdgeVert = 0; iEdgeVert <= iMaxEdge; iEdgeVert++)
                {
                    // This section of code depends on the delta's being A->B.
                    final float percentOffset = (float)iEdgeVert/iMaxEdge;
                    final int pEdge = iEdgeVert * 3;
                    workingVerts[pEdge] =
                        sourcePoly[pSourceVertA] + (deltaX * percentOffset);
                    workingVerts[pEdge+2] =
                        sourcePoly[pSourceVertA+2] + (deltaZ * percentOffset);
                    // Snap the y-value to a valid height in the height field.
                    workingVerts[pEdge+1]
                         = getHeightWithinField(workingVerts[pEdge]
                                                    , workingVerts[pEdge+2]
                                                    , cellSize
                                                    , inverseCellSize, patch)
                             * heightField.cellHeight();
                }
                
                /*
                 * The edges array now has a list of vertices that would
                 * represent the new edges if the source edge was to be
                 * broken up into the maximum number of edges allowed.
                 */
                
                // Seed with the first and last vertices.
                // The source edge's vertex A.
                workingIndices[0] = 0;
                // The sample vertex just before vertex B.
                workingIndices[1] = iMaxEdge;
                workingIndicesCount = 2;
                
                /*
                 * This loop incrementally inserts vertices into the
                 * working indices array when one is found to exceed the
                 * maximum allowed distance from the edge. Since the
                 * initialization of the working vertices array ensures
                 * that x and z-values will not deviate from the edge, this
                 * process only acts based on the y-value.
                 * 
                 * Note that the increment for this loop happens internal
                 * to the loop.
                 */
                for (int iWorkingIndex = 0
                        ; iWorkingIndex < workingIndicesCount - 1
                        ; )
                {
                    // Define the end points of the current working segment.
                    final int iWorkingVertA = workingIndices[iWorkingIndex];
                    final int iWorkingVertB = workingIndices[iWorkingIndex+1];
                    final int pWorkingVertA = iWorkingVertA*3;
                    final int pWorkingVertB = iWorkingVertB*3;
                    // Search the vertices that are between these end points.
                    // Find the vertex that is farthest from the segment.
                    // (Has the maximum deviation from the segment.)
                    float maxDistanceSq = 0;
                    int iMaxDistanceVert = -1;
                    for (int iTestVert = iWorkingVertA + 1
                            ; iTestVert < iWorkingVertB
                            ; iTestVert++)
                    {
                        if (workingVerts[iTestVert*3+1] >= heightPathLimit)
                        {
                            /*
                             * This vertex cannot be used.
                             * 
                             * Special case:  No valid height could be derived
                             * for the point.  This is not necessarily an error,
                             * though it does indicate a potential problem
                             * with the configuration used for generation
                             * of the navigation mesh.
                             * 
                             *  Several potential causes:
                             *   - The line segment crosses through a true null
                             *     region (a region with no spans).  E.g.
                             *     polygon mesh doesn't have enough detail
                             *     at null region border.
                             *   - Error in input data.
                             *   - Error in height patch build process.
                             */
                            logger.warning("Potential loss of polygon height"
                                    + "detail on polygon edge: Could not"
                                    + "determine height for sample vertex at (" 
                                    + workingVerts[iTestVert*3+0] + ", "
                                    + workingVerts[iTestVert*3+2] + ")."
                                    + " Heightpatch data not availalable.");
                            continue;
                        }
                        float distanceSq = Geometry.getPointSegmentDistanceSq(
                                        workingVerts[iTestVert*3]
                                      , workingVerts[iTestVert*3+1]
                                      , workingVerts[iTestVert*3+2]
                                      , workingVerts[pWorkingVertA]
                                      , workingVerts[pWorkingVertA+1]
                                      , workingVerts[pWorkingVertA+2]
                                      , workingVerts[pWorkingVertB]
                                      , workingVerts[pWorkingVertB+1]
                                      , workingVerts[pWorkingVertB+2]);
                        if (distanceSq > maxDistanceSq)
                        {
                            // Found a new maximum.
                            maxDistanceSq = distanceSq;
                            iMaxDistanceVert = iTestVert;
                        }
                    }
                    if (iMaxDistanceVert != -1
                            && maxDistanceSq
                                > mContourMaxDeviation * mContourMaxDeviation)
                    {
                        // A vertex was found which exceeded the maximum
                        // allowed deviation from the current segment.
                        // Insert the vertex.
                        for (int i = workingIndicesCount
                                ; i > iWorkingIndex
                                ; i--)
                            workingIndices[i] = workingIndices[i-1];
                        workingIndices[iWorkingIndex+1] = iMaxDistanceVert;
                        workingIndicesCount++;
                    }
                    else
                        iWorkingIndex++;
                }
                
                // The working indices array now contains a list of vertex
                // indices for an edge smoothed based on height.
                
                /*
                 * Add new vertices to the polygon.
                 * Build the hull.
                 * 
                 * Notes:
                 * 
                 * Remember that the output vertices array has already been
                 * seeded with the source indices in the same order as the
                 * source indices array.  So the indices match between
                 * the source and output arrays for all original vertices.
                 * 
                 * The new vertices are not added to the output vertex array
                 * in any particular order.   That is why the hull array is
                 * required to determine proper ordering for the polygon.
                 */
                
                // First add the start vertex for this new group of edges.
                hullIndices[hullIndicesCount++] = iSourceVertA;
                if (swapped)
                {
                    // The original vertices for this edge had to be
                    // reversed for the previous calculations. So they need
                    // to be added to the hull array in reverse order.
                    for (int iWorkingIndex = workingIndicesCount - 2
                            ; iWorkingIndex > 0
                            ; iWorkingIndex--)
                    {
                        outVerts[outVertCount*3] =
                            workingVerts[workingIndices[iWorkingIndex]*3];
                        outVerts[outVertCount*3+1] =
                            workingVerts[workingIndices[iWorkingIndex]*3+1];
                        outVerts[outVertCount*3+2] =
                            workingVerts[workingIndices[iWorkingIndex]*3+2];
                        hullIndices[hullIndicesCount++] = outVertCount;
                        outVertCount++;
                    }
                }
                else
                {
                    for (int iWorkingIndex = 1
                            ; iWorkingIndex < workingIndicesCount-1
                            ; iWorkingIndex++)
                    {
                        outVerts[outVertCount*3] =
                            workingVerts[workingIndices[iWorkingIndex]*3];
                        outVerts[outVertCount*3+1] =
                            workingVerts[workingIndices[iWorkingIndex]*3+1];
                        outVerts[outVertCount*3+2] =
                            workingVerts[workingIndices[iWorkingIndex]*3+2];
                        hullIndices[hullIndicesCount++] = outVertCount;
                        outVertCount++;
                    }
                }
            }
        }
        else
        {
            // There will be no adjustment to the edges of the polygon.
            // Just use the order of the output vertices array since it
            // contains a duplicate of the source polygon.
            for (int i = 0; i < outVertCount ; i++)
                hullIndices[i] = i;
            hullIndicesCount = outVertCount;
        }
        
        if (outVertCount > 3)
        {
            /*
             * Perform the triangulation.
             * Note: The only output expected is outTriangles.
             * The rest of the variables with the prefix "out" are only inputs
             * to this operation.
             */
            performDelaunayTriangulation(outVerts
                    , outVertCount
                    , hullIndices
                    , hullIndicesCount
                    , workingEdges, outTriangles);
        }
        else if (outVertCount == 3)
        {
            // The output vertices form a triangle.
            // Just copy it over.
            outTriangles.clear();
            outTriangles.add(0);
            outTriangles.add(1);
            outTriangles.add(2);
        }
        else
        {
            // Invalid output polygon due to bad input data.
            // Logging is handled by the caller.
            outTriangles.clear();
            return 0;
        }
        
        // Check validity of indices.
        int badIndicesCount =
            getInvalidIndicesCount(outTriangles, outVertCount);
        if (badIndicesCount > 0)
        {
            logger.severe("Delaunay triangulation failure: Invalid indices"
                            + " detected edge detail step. Bad indices"
                            + " detected: " + badIndicesCount);
            outTriangles.clear();
            return 0;
        }
        
        if (mContourSampleDistance > 0)
        {
            
            /*
             * The purpose of this second pass is to sample the inside of
             * the polygon and add internal triangles where the height field
             * deviates too far from the mesh.
             * 
             * This process has to be performed after the initial
             * triangulation in order to get accurate mesh distance values.
             */
            
            // TODO: EVAL: Can time be saved by using the patch bounds instead
            // of running these bounds calculations?
            
            // Get the bounds of the polygon in polygon space.
            float minX = sourcePoly[0];
            float minZ = sourcePoly[2];
            float maxX = minX;
            float maxZ = minZ;
            for (int iVert = 1; iVert < sourceVertCount; iVert++)
            {
                int pVert = iVert*3;
                minX = Math.min(minX, sourcePoly[pVert]);
                minZ = Math.min(minZ, sourcePoly[pVert+2]);
                maxX = Math.max(maxX, sourcePoly[pVert]);
                maxZ = Math.max(maxZ, sourcePoly[pVert+2]);
            }
            
            /*
             * Build the sample grid.
             * The next looping process builds a grid of points (x, y, z).
             * The x and z-values are snapped to a grid that encompasses
             * the entire source polygon and is incremented by the sample
             * distance.
             * The y-value is snapped to the closest height found in the
             * height patch at the grid's (x, z) location.
             */
            
            // Convert the polygon bounds to sample grid space bounds.
            final int x0 = (int)Math.floor(minX/mContourSampleDistance);
            final int z0 = (int)Math.floor(minZ/mContourSampleDistance);
            final int x1 = (int)Math.ceil(maxX/mContourSampleDistance);
            final int z1 = (int)Math.ceil(maxZ/mContourSampleDistance);
            
            workingSamples.clear();
            
            // Loop through all locations within the sample grid space
            // and create a vertex for each location that is inside the
            // source polygon.
            for (int z = z0; z < z1; z++)
            {
                for (int x = x0; x < x1; x++)
                {
                    // Need to figure out whether this grid location is
                    // outside or very close to the edge of the actual polygon.
                    // Converts back to polygon space.
                    final float vx = x * mContourSampleDistance;
                    // Converts back to polygon space.
                    final float vz = z * mContourSampleDistance;
                    
                    if (getSignedDistanceToPolygonSq(vx
                            , vz, sourcePoly
                            , sourceVertCount)
                                > -mContourSampleDistance/2)
                        // This location is either outside the polygon or
                        // very close to the the internal edge.  Skip it.
                        continue;
                    
                    // Add the sample vertex to the grid.
                    workingSamples.add(x);
                    workingSamples.add(getHeightWithinField(vx
                            , vz
                            , cellSize
                            , inverseCellSize
                            , patch));
                    workingSamples.add(z);
                }
            }
            
            final int sampleCount = workingSamples.size() / 3;
            
            // The only purpose of this outer loop is to provide a certain
            // number of iterations.  The inner loop does not depend in any way
            // on the iteration count of the outer loop.
            for (int iterationCount = 0
                    ; iterationCount < sampleCount
                    ; iterationCount++)
            {
                
                float selectedX = 0;
                float selectedY = 0;
                float selectedZ = 0;
                float maxDistance = 0;
                
                // Loop through all sample vertices.
                for (int iSampleVert = 0
                        ; iSampleVert < sampleCount
                        ; iSampleVert++)
                {
                    /*
                     * Design note:
                     * 
                     * There is a potential that the y-value for a sample
                     * vertex will be > Integer.MAX_VALUE because a height
                     * value for the vertex could not be found in the height
                     * patch.
                     * 
                     * Unlike for the edge vertices earlier, there is no known 
                     * valid reason for this to occur, so it is not being 
                     * checked and handled here.
                     * 
                     * If it does occur, the symptom will be the insertion of
                     * a vertex with a very high y-value that will disrupt
                     * the mesh at its location.
                     */
                    // Get the position of the sample in polygon space and
                    // its distance from the current mesh.
                    final float sampleX =
                        workingSamples.get(iSampleVert*3)
                            * mContourSampleDistance;
                    final float sampleY =
                        workingSamples.get(iSampleVert*3+1)
                            * heightField.cellHeight();
                    final float sampleZ =
                        workingSamples.get(iSampleVert*3+2)
                            * mContourSampleDistance;
                    final float sampleDistance = getInternalDistanceToMesh(
                            sampleX
                            , sampleY
                            , sampleZ
                            , outVerts
                            , outTriangles);
                    
                    if (sampleDistance == UNDEFINED)
                        // This sample vertex is outside of the triangle mesh.
                        continue;
                    
                    if (sampleDistance > maxDistance)
                    {
                        // This sample vertex is farther from the mesh than
                        // any other found so far.
                        maxDistance = sampleDistance;
                        selectedX = sampleX;
                        selectedY = sampleY;
                        selectedZ = sampleZ;
                    }
                }
                
                if (maxDistance <= mContourMaxDeviation)
                    // No sample vertex was found to be too far from the mesh.
                    // Can stop iterating early.
                    break;
                
                // Add this vertex to the output vertices.
                // Note that since this is an internal vertex it is not part of
                // a mandatory edge.  (Not part of the hull.)
                outVerts[outVertCount*3] = selectedX;
                outVerts[outVertCount*3+1] = selectedY;
                outVerts[outVertCount*3+2] = selectedZ;
                outVertCount++;
                
                // Re-perform the triangulation with the new vertex.
                // TODO: EVAL: A good candidate for optimizing.
                // E.g. Insert rather than full rebuild.
                performDelaunayTriangulation(outVerts
                        , outVertCount
                        , hullIndices
                        , hullIndicesCount
                        , workingEdges
                        , outTriangles);
                
                // Check validity of indices.
                badIndicesCount =
                    getInvalidIndicesCount(outTriangles, outVertCount);
                if (badIndicesCount > 0)
                {
                    logger.severe("Delaunay triangulation failure: Invalid "
                            + "indices detected during internal detail"
                            + " iteration. Iteration: " + iterationCount
                            + ", Bad indices detected: "+ badIndicesCount);
                    outTriangles.clear();
                    return 0;
                }
                
            }
        }
        
        return outVertCount;
    }
    
    /**
     * Generates data which represents the circumcircle of the triangle
     * formed by the three points (A, B, C).
     * @param triangleAreaX2 2x the area of the triangle formed by the
     * points.  The only reason for this parameter is for optimization.
     * The only operation that calls this operation has already performed
     * the area calculation, so why repeat it?
     * @param outCircle  The definition of the circumcircle in the form
     * (x, y, r) where (x, y) is the center point and r is the radius.
     * The value of (x, y) will be (0, 0) and r will be {@link #UNDEFINED} if
     * this operation returns FALSE.
     * @return TRUE if the circumcircle was successfully created.
     * Otherwise FALSE.
     */
    private static boolean buildCircumcircle(float ax
            , float ay
            , float bx
            , float by
            , float cx
            , float cy
            , float triangleAreaX2
            , float[] outCircle)
    {
        
        /*
         * References:
         * http://en.wikipedia.org/wiki/Circumcenter#Coordinates_of_circumcenter
         * http://mathworld.wolfram.com/Circumcircle.html
         */
        
        final float epsilon = 1e-6f;
        
        if (Math.abs(triangleAreaX2) > epsilon)
        {
            // Triangle has an area.  Calculate center point of circle.
            final float aLenSq = ax * ax + ay * ay;
            final float bLenSq = bx * bx + by * by;
            final float cLenSq = cx * cx + cy * cy;
            outCircle[0] = (aLenSq * (by - cy) + bLenSq
                    * (cy - ay) + cLenSq * (ay - by)) / (2* triangleAreaX2);
            outCircle[1] = (aLenSq * (cx - bx) + bLenSq
                    * (ax - cx) + cLenSq * (bx - ax)) / (2* triangleAreaX2);
            // Calculate the radius of the circle.  (Distance from center to
            // one of the supplied points.)
            outCircle[2] = (float)Math.sqrt(
                    getDistanceSq(outCircle[0], outCircle[1], ax, ay));
            return true;
        }
        
        // Invalid triangle.
        outCircle[0] = 0;
        outCircle[1] = 0;
        outCircle[2] = UNDEFINED;
        return false;
    }
    
    /**
     * Attempts to form a new triangle on an UNDEFINED side of the specified
     * edge.
     * <p>Will only attempt to form a new triangle for the first UNDEFINED
     * side that is found.</p>
     * <p>If a new triangle cannot be formed for the selected UNDEFINED side,
     * then that side will be set to the value HULL.</p>
     * <p>If a new triangle is formed, it is guaranteed to be complete.
     * (I.e. All necessary data to form the triangle will exist in the
     * edges list.)</p>
     * @param iEdge  The index of the edge to perform the operation on.
     * @param verts The available vertices in the format: (x, y, z)
     * @param vertCount The number of vertices in the vertices array.
     * @param currentTriangleCount The current number of triangles in the
     * edges list.
     * @param edges The edges list in the form:
     * (vertA, vertB, valueA, valueB) where valueA is the side to the left
     * of line segment vertA->vertB and valueB is the side to the left of
     * line segment vertB->vertA.
     * @return The new triangle count.  If the return value is the same as
     * currentTriangleCount then no new triangle could be formed.
     */
    private static int completeTriangle(int iEdge
            , float[] verts
            , int vertCount
            , int currentTriangleCount
            , ArrayList<Integer> edges)
    {

        int iVertA;
        int iVertB;
        
        if (edges.get(iEdge*4+2) == UNDEFINED)
        {
            // The side to the left of segment A->B is undefined.
            iVertA = edges.get(iEdge*4);
            iVertB = edges.get(iEdge*4+1);
        }
        else if (edges.get(iEdge*4+3) == UNDEFINED)
        {
            // The side to the left of segment B-A is undefined.
            iVertA = edges.get(iEdge*4+1);
            iVertB = edges.get(iEdge*4);
        }
        else
            // Edge is already completed.  No new faces.
            return currentTriangleCount;
        
        final int pVertA = iVertA * 3;
        final int pVertB = iVertB * 3;
        
        // The index of the best vertex on the left side of the edge.
        int iSelectedVert = UNDEFINED;
        
        /*
         * The definition of the selected circle in the format (x, z, r)
         * where (x, z) is the center point and r is the radius.
         * TODO: EVAL: Object creation.  Convert to a working parameter?
         */
        final float[] selectedCircle = { 0, 0, -1 };
        
        // Values used to take into account floating point errors.
        final float tolerance = 0.001f;
        final float epsilon = 1e-5f;
        
        /*
         * Loop through all the vertices.  Find the vertex that is to the
         * left of the edge (vertA->vertB) and forms the triangle with the
         * smallest circumcircle.
         * 
         * This process is difficult to optimize due to floating point
         * errors.  Especially when the source polygon is small in area.
         * So the optimizations were abandoned.
         */
        for (int iPotentialVert = 0
                ; iPotentialVert < vertCount
                ; iPotentialVert++)
        {
            if (iPotentialVert == iVertA || iPotentialVert == iVertB)
                // This vertex is one of the edge's vertices.  Skip it.
                continue;
            
            final int pPotentialVert = iPotentialVert * 3;
            
            final float area
                = getSignedAreaX2(verts[pVertA]
                                    , verts[pVertA+2]
                                    , verts[pVertB]
                                    , verts[pVertB+2]
                                    , verts[pPotentialVert]
                                    , verts[pPotentialVert+2]);
            
            if (area > epsilon)
            {
                // The three vertices form a triangle of adequate size AND the
                // current vertex is to the left of the line segment
                // vertA->vertB.
                
                if (selectedCircle[2] < 0)
                {
                    // This is the first potentially valid vertex combination
                    // found so far.
                    if (overlapsExistingEdge(iVertA
                                    , iPotentialVert
                                    , verts
                                    , edges)
                            || overlapsExistingEdge(iVertB
                                    , iPotentialVert
                                    , verts
                                    , edges))
                        // An overlap was found.  Can't use this vertex.
                        continue;
                    // Vertex combination is valid.  Try to use it.
                    if (buildCircumcircle(verts[pVertA]
                                          , verts[pVertA+2]
                                          , verts[pVertB]
                                          , verts[pVertB+2]
                                          , verts[pPotentialVert]
                                          , verts[pPotentialVert+2]
                                          , area
                                          , selectedCircle))
                        // Successfully created a circumcircle.
                        // Select this vertex.
                        iSelectedVert = iPotentialVert;
                    continue;
                }
                
                // This is not the first valid combination found.
                // Is it better than the existing?
                
                // Get the distance from the origin of the current
                // circumcircle to this vertex.
                final float distanceToOrigin =
                    (float)Math.sqrt(getDistanceSq(selectedCircle[0]
                                                 , selectedCircle[1]
                                                 , verts[pPotentialVert]
                                                 , verts[pPotentialVert+2]));
                if (distanceToOrigin > selectedCircle[2] * (1 + tolerance))
                {
                    // This vertex is outside the current circumcircle and
                    // can be ignored.
                    continue;
                }
                else
                {
                    /*
                     * The vertex is within, on, or almost on, the current
                     * circumcircle.
                     * 
                     * If it were not for floating point errors, we could
                     * automatically accept all vertices that showed as
                     * within the current circumcircle.  But floating point
                     * errors for small polygons prevent such a shortcut.
                     * 
                     * Need to check if new edges formed by the use of this
                     * vertex will conflict with  other edges already created.
                     */
                    if (overlapsExistingEdge(iVertA
                                    , iPotentialVert
                                    , verts
                                    , edges)
                            || overlapsExistingEdge(iVertB
                                    , iPotentialVert
                                    , verts
                                    , edges))
                        // An overlap was found.  Can't use this vertex.
                        continue;
                    // Using this vertex is valid.
                    if (buildCircumcircle(verts[pVertA]
                                              , verts[pVertA+2]
                                              , verts[pVertB]
                                              , verts[pVertB+2]
                                              , verts[pPotentialVert]
                                              , verts[pPotentialVert+2]
                                              , area
                                              , selectedCircle))
                        // Successfully created a circumcircle.
                        // Select this vertex.
                        iSelectedVert = iPotentialVert;
                }
            }
        }
        
        if (iSelectedVert != UNDEFINED)
        {
            // A new triangle can be formed.
            
            // Update triangle information of edge being completed.
            updateLeftFace(iEdge, iVertA, currentTriangleCount, edges);
            
            // Add a new edge (selectedVert->vertA) or update face info
            // of existing edge.
            int iSelectedEdge = getEdgeIndex(edges, iSelectedVert, iVertA);
            if (iSelectedEdge == UNDEFINED)
            {
                // This is a new edge.
                edges.add(iSelectedVert);
                edges.add(iVertA);
                edges.add(currentTriangleCount);
                edges.add(UNDEFINED);
            }
            else
                // Update the existing edge.
                updateLeftFace(iSelectedEdge
                        , iSelectedVert
                        , currentTriangleCount
                        , edges);
            
            // Add a new edge (vertB->selectedVert) or update face info
            // of existing edge.
            iSelectedEdge = getEdgeIndex(edges, iVertB, iSelectedVert);
            if (iSelectedEdge == UNDEFINED)
            {
                // This is a new edge.
                edges.add(iVertB);
                edges.add(iSelectedVert);
                edges.add(currentTriangleCount);
                edges.add(UNDEFINED);
            }
            else
                // Update the existing edge.
                updateLeftFace(iSelectedEdge
                        , iVertB
                        , currentTriangleCount
                        , edges);
            
            // Indicate a new face was created.
            currentTriangleCount++;
        }
        else
        {
            // A new face cannot be formed. Set the indicate the side of
            // the edge is a hull.
            updateLeftFace(iEdge, iVertA, HULL, edges);
        }
        
        return currentTriangleCount;
    }
    
    /**
     * Attempts to find a span in the heightfield associated with the
     * provided vertex.
     * <p>Spans that are within {@link OpenHeightfield#cellHeight()} of
     * the vertices y-value take precedence in the search.  Otherwise the
     * span closest in height is returned.</p>
     * @param vertX The x-value of the vertex (x, y, z).
     * @param vertY The y-value of the vertex (x, y, z).
     * @param vertZ The z-value of the vertex (x, y, z).
     * @param heightField The heightfield to search.
     * @param outWidthDepth The actual width and depth index of the selected
     * span in the form: (widthIndex, depthIndex).  The array must be
     * at least 2 in size.  Content is undefined if the return value
     * of the operation is null.
     * @return The span in the feightfield that is best associated with
     * the provided vertex.
     */
    private static OpenHeightSpan getBestSpan(int vertX
            , int vertY
            , int vertZ
            , OpenHeightfield heightField
            , int[] outWidthDepth)
    {
        /*
         * There are special cases which can result in the wrong span being
         * returned by a simple column search. These special cases are what
         * result in this a more complex search algorithm.
         * 
         * In the best case search: Search up the height field column
         * corresponding to  the vertices x and z-values and return the
         * span whose floor is within the  correct tolerance of the y-value.
         * 
         * While, technically, the best case search should always be
         * successful, floating point errors and other special cases can
         * result in a search failure. In such cases the search is expanded
         * to the 8-neighbor cells, resulting in a slower search.
         * 
         * If a search if forced to include neighbors, the search will
         * always be a full 8-neighbor search.  (No early exists.)
         * 
         * Known special case when this occurs:
         * 
         * The vertex lies on the outer edge of a border span. (The span
         * edge across which there is no other span, at any height.)
         * 
         * The vertex lies on the outer edge of a span without a connected
         * neighbor. (The cell across the span edge contains spans, but
         * none are connected as a neighbor to the span.)
         * 
         * The scope of these special cases may not be fully known since
         * a search failure can be hidden.  E.g. When building a height
         * patch, as long as one vertex in a polygon gets a good result,
         * the flood fill algoritm can still succeed. The search failures
         * will never be visible.  There may be other special cases
         * which are hidden in a similar manner.
         * 
         * The known special cases are much less likely to exhibit when
         * the source height field contains null (zero) region borders.
         * This is because a border forces all vertices in from the region
         * edges, which is where problems occur.
         */
        
        /*
         * Search order starts with zero offset.
         */
        final int[] targetOffset =
                { 0,0, -1,0, 0,-1, -1,-1, 1,-1, -1,1, 1,0, 1,1, 0,1 };
        
        OpenHeightSpan resultSpan = null;
        int minDistance = Integer.MAX_VALUE;
        // Loop through the offsets trying to find the best span match.
        // Priority and potential early exit is given to spans at zero offset.
        for (int p = 0; p < 17; p += 2)
        {
            int widthIndex = vertX + targetOffset[p];
            int depthIndex = vertZ + targetOffset[p+1];
            if (!heightField.isInBounds(widthIndex, depthIndex))
                // This neighbor is outside of the height field.
                continue;
            // Get the base span for this vertex from the height field.
            OpenHeightSpan span = heightField.getData(widthIndex, depthIndex);
            // Find the best span in the column. (Closest height match.)
            span = getBestSpan(span, vertY);
            if (span == null)
                // No spans at the target location.
                continue;
            else
            {
                // Found a span.
                int distance = Math.abs(vertY - span.floor());
                if (p == 0 && (distance <= heightField.cellHeight()))
                {
                    // Found a span at a good height at the zero offset
                    // grid location. Don't search further. (Early exit.)
                    outWidthDepth[0] = widthIndex;
                    outWidthDepth[1] = depthIndex;
                    return span;
                }
                else if (distance < minDistance)
                {
                    /*
                     * Could not find the "perfect" match.  So dropping back
                     * to the best possible match.
                     * This span's floor is the closest to the vertex found
                     * so far.
                     */
                    resultSpan = span;
                    outWidthDepth[0] = widthIndex;
                    outWidthDepth[1] = depthIndex;
                    minDistance = distance;
                }
            }
        }
        return resultSpan;
    }
    
    /**
     * Returns the span within the column whose floor is closest to the
     * provided height.
     * @param baseSpan Where to start the search.
     * @param targetHeight The height to find the closest match for.
     * @return The span whose floor is closest to the target height, or null
     * if no base span was provided.
     */
    private static OpenHeightSpan getBestSpan(OpenHeightSpan baseSpan
                    , int targetHeight)
    {
        int minDistance = Integer.MAX_VALUE;
        OpenHeightSpan result = null;
        // Loop up the column, starting at the base span.
        for (OpenHeightSpan span = baseSpan; span != null; span = span.next())
        {
            final int distance = Math.abs(targetHeight - span.floor());
            if (distance < minDistance)
            {
                // This span's floor is the closest to the vertex found so far.
                result = span;
                minDistance = distance;
            }
        }
        return result;
    }
    
    /**
     * Returns the square of the distance between two points.
     * @param ax The x-value of point (ax, ay).
     * @param ay The y-value of point (ax, ay).
     * @param bx The x-value of point (bx, by).
     * @param by The y-value of point (bx, by).
     * @return The square of the distance between two points.
     */
    private static float getDistanceSq(float ax, float ay, float bx, float by)
    {
        final float deltaX = (ax - bx);
        final float deltaY = (ay - by);
        return deltaX * deltaX + deltaY * deltaY;
    }
    
    /**
     * Gets the index of the edge defined by two indices.
     * @param edges  The edge list where each edge is in the form:
     * (vertIndex, vertIndex, value, value)
     * @param vertAIndex The index of one of the edge's vertices.
     * @param vertBIndex The index of the other of the edge's vertices.
     * @return The index of the edge in the edges list which matches the
     * provided vertices. Or {@link #UNDEFINED} if there is no corresponding
     * edge.
     */
    private static int getEdgeIndex(ArrayList<Integer> edges
                    , int vertAIndex
                    , int vertBIndex)
    {
        final int edgeCount = edges.size() / 4;
        for (int i = 0; i < edgeCount; i++)
        {
            final int u = edges.get(i*4);
            final int v = edges.get(i*4+1);
            if ((u == vertAIndex && v == vertBIndex)
                    || (u == vertBIndex && v == vertAIndex))
                return i;
        }
        return UNDEFINED;
    }
    
    /**
     * Get the height of the point within the height patch.
     * @param x  The world x position.
     * @param z  The world y position.
     * @param minBounds  The minimum bounds of the source field.
     * @param cellSize The cell size of the source field.
     * @param inverseCellSize  The inverse of the cell size of the source
     * field.  (Included only to improve performance by avoiding a division
     * within this operation.)
     * @param patch  The height patch to search.
     * @return The height of the location within the patch.  Or
     * {@link Float#MAX_VALUE} if the
     * search for a height fails.
     */
    private static int getHeightWithinField(float x
            , float z
            , float cellSize
            , float inverseCellSize
            , HeightPatch patch)
    {
        /*
         * There are two special cases when getting the height value:
         * 
         *   The x value is on the upper width edge of the source height field.
         *   The z value is on the upper depth edge of the source height field.
         * 
         * In these cases this algorithm will create invalid indices for
         * the height patch.  (Out of bounds high.)  Since the height patch
         * is a private class (under strict control) it is being left up to
         * the height patch class to clamp the index values so no exceptions
         * are thrown.
         * 
         * Note that these special cases refer to the source height field
         * edges, not the height patch edges.  This is because the height
         * patch creation process always creates the patch to be slightly
         * larger than the polygon being processed.  The problem is that
         * this slight expansion cannot occur when the height patch is up
         * against the edges of source height field.
         * 
         * The impact of these special cases is as follows:
         * 
         *   If x is on the upper width edge, then the same height value
         *   will be returned as is returned for (x - cellSize).
         * 
         *   If z is on the upper height edge, then the same value will be
         *   returned as is returned for (z - cellSize).
         * 
         * This is not a significant issue.
         */
        
        // Convert world coordinates to height field indices.
        final int widthIndex = (int)Math.floor(x * inverseCellSize + 0.01f);
        final int depthIndex = (int)Math.floor(z * inverseCellSize + 0.01f);
        // Get the height.
        int height = patch.getData(widthIndex, depthIndex);
        if (height == HeightPatch.UNSET)
        {
            /*
             * One of the following special cases exist:
             * - Data is bad.
             * - Floating point calculation errors.
             * - The vertex is in a true null region.  (A region without
             *   any spans.) This can happen since sloppiness is permitted
             *   around null regions.
             * Find nearest neighbor which has valid height.
             * This is an 8 neighbor search.
             */
            final int[] neighborOffset =
                    { -1,0, -1,-1, 0,-1, 1,-1, 1,0, 1,1, 0,1, -1,1};
            float minNeighborDistanceSq = Float.MAX_VALUE;
            for (int p = 0; p < 16; p += 2)
            {
                int nWidthIndex = widthIndex + neighborOffset[p];
                int nDepthIndex = depthIndex + neighborOffset[p+1];
                if (!patch.isInPatch(nWidthIndex, nDepthIndex))
                    // This neighbor is outside of the height patch.
                    continue;
                int nNeighborHeight = patch.getData(nWidthIndex, nDepthIndex);
                if (nNeighborHeight == HeightPatch.UNSET)
                    // This neighbor doesn't have a value either.
                    continue;
                // Get distance from this neighbor to the target location.
                float deltaWidth = (nWidthIndex + 0.5f) * cellSize - x;
                float deltaDepth = (nDepthIndex + 0.5f) * cellSize - z;
                float neighborDistanceSq =
                    deltaWidth*deltaWidth + deltaDepth*deltaDepth;
                if (neighborDistanceSq < minNeighborDistanceSq)
                {
                    // This is the closest neighbor found so far.
                    height = nNeighborHeight;
                    minNeighborDistanceSq = neighborDistanceSq;
                }
            }
        }
        return height;
    }

    /**
     * Returns the approximate y-axis distance of a point from the triangle
     * mesh.
     * <p> If the point is not within the (x, z) plane projection  of the
     * mesh then {@link #UNDEFINED} will be returned.</p>
     * @param px The x-value of the point to be tested. (px, py, pz)
     * @param py The y-value of the point to be tested. (px, py, pz)
     * @param pz The z-value of the point to be tested. (px, py, pz)
     * @param verts The vertices of the mesh to test against.
     * @param indices The indices for the mesh to test against.
     * @return The approximate y-axis distance of a point from the
     * triangle mesh.
     */
    private static float getInternalDistanceToMesh(float px
            , float py
            , float pz
            , float[] verts
            , ArrayList<Integer> indices)
    {
        
        float minDistance = Float.MAX_VALUE;
        
        final int triangleCount = indices.size() / 3;
        // Loop through all triangles in the mesh and get the point's y-distance
        // from any triangles the point lies within.  The goal is to find
        // the minimum (closest to the mesh) y-distance.
        for (int iTriangle = 0; iTriangle < triangleCount; iTriangle++)
        {
            final int pVertA = indices.get(iTriangle*3)*3;
            final int pVertB = indices.get(iTriangle*3+1)*3;
            final int pVertC = indices.get(iTriangle*3+2)*3;
            
            float distance = Float.MAX_VALUE;
            
            final float deltaACx = verts[pVertC] - verts[pVertA];
            final float deltaACy = verts[pVertC+1] - verts[pVertA+1];
            final float deltaACz = verts[pVertC+2] - verts[pVertA+2];
            
            final float deltaABx = verts[pVertB] - verts[pVertA];
            final float deltaABy = verts[pVertB+1] - verts[pVertA+1];
            final float deltaABz = verts[pVertB+2] - verts[pVertA+2];
            
            final float deltaAPx = px - verts[pVertA];
            final float deltaAPz = pz - verts[pVertA+2];
    
            final float dotACAC = deltaACx * deltaACx + deltaACz * deltaACz;
            final float dotACAB = deltaACx * deltaABx + deltaACz * deltaABz;
            final float dotACAP = deltaACx * deltaAPx + deltaACz * deltaAPz;
            final float dotABAB = deltaABx * deltaABx + deltaABz * deltaABz;
            final float dotABAP = deltaABx * deltaAPx + deltaABz * deltaAPz;
            
            // Compute barycentric coordinates
            final float inverseDenominator = 1.0f
                            / (dotACAC * dotABAB - dotACAB * dotACAB);
            final float u = (dotABAB * dotACAP - dotACAB * dotABAP)
                            * inverseDenominator;
            final float v = (dotACAC * dotABAP - dotACAB * dotACAP)
                            * inverseDenominator;
            
            final float tolerance = 1e-4f;
            if (u >= -tolerance && v >= -tolerance && (u + v) <= 1 + tolerance)
            {
                // The point lies inside the (x, z) plane projection of
                // the triangle. Interpolate the y value.
                final float y = verts[pVertA+1] + deltaACy * u + deltaABy * v;
                distance =  Math.abs(y - py);
            }
            
            if (distance < minDistance)
                minDistance = distance;
        }
        if (minDistance == Float.MAX_VALUE)
            // The point does not lie within the (x, z) plane projection
            // of the mesh.  So it is invalid.
            return UNDEFINED;
        
        return minDistance;
    }

    /**
     * Detects whether an indices list contains any obviously invalid
     * values.
     * <p>An invalid index:  index < 0 or index >= vertCount.
     * <p>
     * This check exists because the triangulation can be a bit dodgy when
     * dealing with very small triangles.  It helps detect these issues so
     * crashes can be avoided.
     * </p>
     * @param indices The detailed polygon indices in the form:
     * (vertA, vertB, vertC)
     * @param vertCount The number of vertices in the vertices array
     * which the indices refer to.
     * @return The number of invalid indices detected.
     */
    private static int getInvalidIndicesCount(ArrayList<Integer> indices
                    , int vertCount)
    {
        int badIndicesCount = 0;
        for (int i = 0; i < indices.size(); i++)
        {
           int index = indices.get(i);
           if (index < 0 || index >= vertCount)
               badIndicesCount++;
        }
        return badIndicesCount;
    }

    /**
     * Returns the distance squared from the point to the line segment.
     * @param px The x-value of point (px, py).
     * @param py The y-value of point (px, py)
     * @param ax The x-value of the line segment's vertex A.
     * @param ay The y-value of the line segment's vertex A.
     * @param bx The x-value of the line segment's vertex B.
     * @param by The y-value of the line segment's vertex B.
     * @return The distance squared from the point (px, py) to line segment AB.
     */
    private static float getPointSegmentDistanceSq(float px
                    , float py
                    , float ax
                    , float ay
                    , float bx
                    , float by)
    {
        
        /*
         * Reference: http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
         * 
         * The goal of the algorithm is to find the point on line segment
         * AB that is closest to P and then calculate the distance between
         * P and that point.
         */
        
        final float deltaABx = bx - ax;
        final float deltaABy = by - ay;
        final float deltaAPx = px - ax;
        final float deltaAPy = py - ay;
        
        final float segmentABLengthSq = deltaABx * deltaABx
                                            + deltaABy * deltaABy;
        
        if (segmentABLengthSq == 0)
            // AB is not a line segment.  So just return
            // distanceSq from P to A
            return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
            
        final float u = (deltaAPx * deltaABx + deltaAPy * deltaABy)
                            / segmentABLengthSq;
        
        if (u < 0)
            // Closest point on line AB is outside outside segment AB and
            // closer to A. So return distanceSq from P to A.
            return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
        else if (u > 1)
            // Closest point on line AB is outside segment AB and closer to B.
            // So return distanceSq from P to B.
            return (px - bx)*(px - bx) + (py - by)*(py - by);
        
        // Closest point on lineAB is inside segment AB.  So find the exact
        // point on AB and calculate the distanceSq from it to P.
        
        // The calculation in parenthesis is the location of the point on
        // the line segment.
        final float deltaX = (ax + u * deltaABx) - px;
        final float deltaY = (ay + u * deltaABy) - py;
    
        return deltaX*deltaX + deltaY*deltaY;
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
     * represent the same point.
     * <p>Each call to this operation results in 2 multiplications and 5
     * subtractions.<p>
     * @param ax The x-value for point (ax, ay) for vertex A of the triangle.
     * @param ay The y-value for point (ax, ay) for vertex A of the triangle.
     * @param bx The x-value for point (bx, by) for vertex B of the triangle.
     * @param by The y-value for point (bx, by) for vertex B of the triangle.
     * @param cx The x-value for point (cx, cy) for vertex C of the triangle.
     * @param cy The y-value for point (cx, cy) for vertex C of the triangle.
     * @return The signed value of two times the area of the triangle defined
     * by the points (A, B, C).
     */
    private static float getSignedAreaX2(float ax
                    , float ay
                    , float bx
                    , float by
                    , float cx
                    , float cy)
    {
        // References:
        // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm
        //                                                #Modern%20Triangles
        // http://mathworld.wolfram.com/TriangleArea.html (Search for "signed".)
        return (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
    }
    
    /**
     * Returns the distance squared from the point to the closest polygon
     * segment on the (x, z) plane.
     * If the return value is positive, the point is outside the polygon.
     * @param x The x-value of the test point (x, z).
     * @param z The y-value of the test point (x, z).
     * @param verts The polygon vertices in the form
     * (ax, ay, az, bx, by, bz, ..., nx, ny, nz,  trash)
     * @param vertCount The number of vertices in the polygon.
     * @return The distanceSq from the point to the closest polygon
     * segment on the (x, z) plane.
     */
    private static float getSignedDistanceToPolygonSq(float x
                    , float z
                    , float[] verts
                    , int vertCount)
    {
        float minDistance = Float.MAX_VALUE;
        int iVertB;
        int iVertA;
        boolean isInside = false;
        // Loop through all edges of the polygon and determine the distance
        // from (x, y) to the edge.
        for (iVertB = 0, iVertA = vertCount-1
                        ; iVertB < vertCount
                        ; iVertA = iVertB++)
        {
            final int pVertB = iVertB*3;
            final int pVertA = iVertA*3;
            if (((verts[pVertB+2] > z) != (verts[pVertA+2] > z))
                    && (x < (verts[pVertA]-verts[pVertB]) * (z-verts[pVertB+2])
                            / (verts[pVertA+2]-verts[pVertB+2])
                                    + verts[pVertB]) )
                // The point is inside the polygons (x,z) plane's column.
                isInside = true;
            // Get the distance from the point to this edge and compare it
            // to the current minimum distance.
            minDistance = Math.min(minDistance
                    , getPointSegmentDistanceSq(x
                            , z
                            , verts[pVertA]
                            , verts[pVertA+2]
                            , verts[pVertB]
                            , verts[pVertB+2]));
        }
        return isInside ? -minDistance : minDistance;
    }
    
    /**
     * Fills the data array of a height patch with height data.  Height
     * data is chosen from the heightfield based on the provided polygon's
     * vertices.
     * <p>The closest floor for each vertex is recorded, then this
     * operation floods fills outward to  all neighbors, recording neighbor
     * floor heights, out to the edges of the height patch.
     * @param polyPointer A pointer to the polygon whose vertices will be
     * used as seed information when building the height data.
     * @param vertCount The number of vertices in the polygon.
     * @param indices Polygon indices data.
     * @param verts Vertex data.
     * @param inoutPatch The section of the height field to find height
     * data for. Expects that the bounds data has been set.
     * Expects data array to be pre-sized such that it can fit the maximum
     * possible data. The data array values will be initialized to UNSET
     * before being filled.
     * @param gridIndexStack  A working stack.  Its data will be discarded
     * prior to use.  Content after the operation completes is undefined.
     * @param spanStack  A working stack.  Its data will be discarded prior
     * to use.  Content after the operation completes is undefined.
     * @param widthDepth A working array.  Expected to be of size 2.
     * Its content is undefined after operation completes.
     */
    private static void loadHeightPatch(int polyPointer
            , int vertCount
            , int[] indices
            , int[] verts
            , OpenHeightfield heightField
            , HeightPatch inoutPatch
            , ArrayDeque<Integer> gridIndexStack
            , ArrayDeque<OpenHeightSpan> spanStack
            , int[] widthDepth)
    {
        // Initialization
        inoutPatch.resetData();
        gridIndexStack.clear();
        spanStack.clear();
        
        /*
         * For each vertex, locate the span in the height field that is
         * closest to it. Push the spans onto the stack.
         * Only searching spans at the grid location of the vertex.
         * (E.g. In the height column of the vertex.)
         */
        
        for (int vertOffset = 0; vertOffset < vertCount; vertOffset++)
        {
            // The width within the height field.
            final int vertX = verts[indices[polyPointer+vertOffset]*3];
            // The height within the height field.
            final int vertY = verts[indices[polyPointer+vertOffset]*3+1];
            // The depth within the height field.
            final int vertZ = verts[indices[polyPointer+vertOffset]*3+2];
            
            // Search for the best span in the height field for this vertex.
            // Best span is the span whose floor area is closest to the
            // vertex location.
            final OpenHeightSpan selectedSpan =
                getBestSpan(vertX, vertY, vertZ, heightField, widthDepth);
            
            if (selectedSpan != null)
            {
                // Found a span for this vertex.  Push in onto the stack.
                gridIndexStack.push(widthDepth[0]);
                gridIndexStack.push(widthDepth[1]);
                spanStack.push(selectedSpan);
            }
        }
        
        // NOTE: If the polygon mesh was properly built, the stack should
        // always have a size greater than zero.
        
        /*
         * Using the spans that have been seeded into the stacks, flood
         * outward, recording the heights found for each grid location
         * within the bounds of the patch.
         */
        while(spanStack.size() > 0)
        {
            final int depthIndex = gridIndexStack.pop();
            final int widthIndex = gridIndexStack.pop();
            OpenHeightSpan span = spanStack.pop();
            
            if (inoutPatch.getData(widthIndex, depthIndex) != HeightPatch.UNSET)
                // This grid location was processed in an earlier iteration.
                continue;
             
            if (inoutPatch.isInPatch(widthIndex, depthIndex))
            {
                /*
                 * This span is in the height patch. Record the span's height.
                 * 
                 * Note: It is a valid situation for the span to NOT be in
                 * the height patch.  This can occur in the special cases
                 * described in detail in getHeightWithinField().  When this
                 * occurs, the algorithm depends on the neighbor search
                 * below to ensure the necessary flooding succeeds.
                 * E.g. One of the neighbors should end up within the
                 * bounds of the patch and continue the flooding.
                 */
                inoutPatch.setData(widthIndex, depthIndex, span.floor());
            }
            
            // "Flood" to the neighbors of this span. If a neighbor is within
            // the patch's grid, then put it in the stacks for processing.
            for (int dir = 0; dir < 4; dir++)
            {
                OpenHeightSpan nSpan = span.getNeighbor(dir);
                if (nSpan == null)
                    // No neighbor in this direction.
                    continue;
                
                final int nWidthIndex =
                    widthIndex + BoundedField.getDirOffsetWidth(dir);
                final int nDepthIndex =
                    depthIndex + BoundedField.getDirOffsetDepth(dir);
                
                if (!inoutPatch.isInPatch(nWidthIndex, nDepthIndex))
                    // This neighbor is outside the bounds of the patch.
                    // So skip it.
                    continue;
                
                if (inoutPatch.getData(nWidthIndex, nDepthIndex) !=
                                                            HeightPatch.UNSET)
                    // This grid location was processed in an earlier iteration.
                    continue;
                
                // Need to process this neighbor.
                gridIndexStack.push(nWidthIndex);
                gridIndexStack.push(nDepthIndex);
                spanStack.push(nSpan);
                
            }
        }
    }
    
    /**
     * Checks whether or not a potential new edge intersects with any
     * existing edge. Same and connected edges are ignored.
     * @param iVertA The first vertex index of the potential new edge.
     * @param iVertB The second vertex index of the potential new edge.
     * @param verts The available vertices in the form (x, y, z)
     * @param edges The edge definitions in the form
     * (vertAIndex, vertBIndex, valueA, valueB)
     * (valueA and valueB are not used by this operation.)
     * @return TRUE if the potential new edge inappropriately intersects
     * with an existing edge.
     * Otherwise FALSE.
     */
    private static boolean overlapsExistingEdge(int iVertA
            , int iVertB
            , float verts[]
            , ArrayList<Integer> edges)
    {
        // Loop through all edges.
        for (int pEdge = 0; pEdge < edges.size(); pEdge += 4)
        {
            final int iEdgeVertA = edges.get(pEdge);
            final int iEdgeVertB = edges.get(pEdge+1);
            if (iEdgeVertA == iVertA
                            || iEdgeVertA == iVertB
                            || iEdgeVertB == iVertA
                            || iEdgeVertB == iVertB)
                // Is same or connected edge. Ignore this edge.
                continue;
            if (segmentsOverlap(verts[iEdgeVertA*3]
                                      , verts[iEdgeVertA*3+2]
                                      , verts[iEdgeVertB*3]
                                      , verts[iEdgeVertB*3+2]
                                      , verts[iVertA*3]
                                      , verts[iVertA*3+2]
                                      , verts[iVertB*3]
                                      , verts[iVertB*3+2]))
                // The new edge overlaps this edge.
                return true;
        }
        // No intersections detected.
        return false;
    }
    
    /**
     * Attempts to perform a Delaunay triangulation on a group of vertices,
     * potentially restricted by the content of the hull argument.
     * @param verts  The vertices to triangulate in the form (x, y, z).
     * @param vertCount The number of vertices in the vertices array.
     * @param immutableHull The indices that make up the required hull
     * edges. These edges are guaranteed to be in the final triangulation.
     * <p>The indices in the hull array are expected to define a clockwise
     * wrapped convex polygon.  Behavior of the operation is undefined if
     * this is not the case.</p>
     * @param hullEdgeCount  The number of indices in the hull array.
     * If zero, then there will be no guaranteed edges.
     * @param workingEdges  A working list used for internal purposes.  Its
     * only purpose as an argument is to save on object creation time.
     * Its content is cleared before use.
     * @param outTriangles  The indices of the output triangle mesh in the
     * form (vertAIndex, vertBIndex, vertCIndex).  The indices refer to
     * vertices in the vertices array.
     * <p>The list is cleared prior to use by this operation.</p>
     */
    private static void performDelaunayTriangulation(float[] verts
            , int vertCount
            , int[] immutableHull
            , int hullEdgeCount
            , ArrayList<Integer> workingEdges
            , ArrayList<Integer> outTriangles)
    {
        
        int triangleCount = 0;
        workingEdges.clear();
        
        /*
         * General reference:
         *     http://en.wikipedia.org/wiki/Delaunay_triangulation
         * More references at:
         *     http://digestingduck.blogspot.com/2009/10/
         *         delaunay-triangulations.html
         */
        
        /*
         * Entries in the working edges list is as follows:
         * (vertA, vertB, valueA, valueB)
         * where valueA is the side to the left of line segment vertA->vertB
         * and valueB is the side to the left of line segment vertB->vertA
         */
        
        // Create an working edge entry for each hull edge.
        for (int iHullVertB = 0, iHullVertA = hullEdgeCount-1
                ; iHullVertB < hullEdgeCount
                ; iHullVertA = iHullVertB++)
        {
            workingEdges.add(immutableHull[iHullVertA]);
            workingEdges.add(immutableHull[iHullVertB]);
            // Since hull is expected to be clockwise wrapped, mark the
            // left side of the edge as a hull.
            workingEdges.add(HULL);
            // Don't know what is on the right side of the edge yet.
            // So default to undefined.
            workingEdges.add(UNDEFINED);
        }
        
        /*
         * Loop through edges until all UNDEFINED sides have been defined.
         * Notes:
         * - The looping is expected to continue for longer than the original
         *   edge count since new edges will be created by the triangle
         *   completion operation.
         * - No edge will ever be added to the edge list without at least
         *   one side already defined.  So each edge will, at most, need a
         *   single triangle built for it.
         */
        int iCurrentEdge = 0;
        while (iCurrentEdge * 4 < workingEdges.size())
        {
            if (workingEdges.get(iCurrentEdge*4+2) == UNDEFINED
                    || workingEdges.get(iCurrentEdge*4+3) == UNDEFINED)
                // Need to create a triangle for one of the sides.
                triangleCount = completeTriangle(iCurrentEdge
                        , verts
                        , vertCount
                        , triangleCount
                        , workingEdges);
            iCurrentEdge++;
        }
        
        /*
         * Unless there is a logic error, at this point no side value in the
         * edges array should have a value of UNDEFINED.  They should all
         * either be set to a triangle index or HULL.
         * The strict HULL equality tests below, rather than just testing
         * for >= 0, are meant to force logic errors to the surface.
         */
        
        // Fill the triangle list with the UNDEFINED value for each
        // expected entry.
        outTriangles.clear();
        outTriangles.ensureCapacity(triangleCount*3);
        for (int i = 0; i < triangleCount * 3; i++)
            outTriangles.add(UNDEFINED);
        
        // Loop through all edges.
        for (int pEdge = 0; pEdge < workingEdges.size(); pEdge += 4)
        {
            
            /*
             * This algorithm is based on the following assumptions:
             * 
             * Two of the three triangle vertices are known as soon as a
             * triangle is first detected.  Only the third vertex needs to
             * be found.
             * 
             * The edge building process guarantees that no partial
             * triangles exist in the data.
             */
            
            // This strict test is meant to force logic errors to the surface.
            // E.g. If an UNDEFINED value creeps into the working edges list.
            if (workingEdges.get(pEdge+3) != HULL)
            {
                /*
                 * The right side of edge A->B has a associated triangle.
                 * This will always be the case for hull edges.
                 * Also indicates that A->B is the clockwise wrapping direction.
                 */

                // Get a pointer to the triangle.
                final int pTriangle = workingEdges.get(pEdge+3)*3;
                
                if (outTriangles.get(pTriangle) == UNDEFINED)
                {

                    /*
                     * This is the first time this triangle has been seen.
                     * 
                     * Initialize this triangle by adding the edge's vertices
                     * to the triangle definition.
                     * 
                     * Wrap A->B for clockwise.
                     */
                    outTriangles.set(pTriangle, workingEdges.get(pEdge));
                    outTriangles.set(pTriangle+1, workingEdges.get(pEdge+1));
                }
                else if (outTriangles.get(pTriangle+2) == UNDEFINED)
                {
                    // This triangle's first two vertices have already been
                    // set.  Need to figure out which vertex in this edge
                    // is the final vertex.
                    if (workingEdges.get(pEdge).equals(
                                outTriangles.get(pTriangle))
                            || workingEdges.get(pEdge).equals(
                                    outTriangles.get(pTriangle+1)))
                        // The first vertex of this edge is already in the
                        // triangle. Add the second vertex of this edge to
                        // the triangle.
                        outTriangles.set(pTriangle+2
                                , workingEdges.get(pEdge+1));
                    else
                        /*
                         * The first vertex of this edge is NOT already in
                         * the triangle, so the 2nd vertex must already be
                         * in it. Add the first vertex of this edge to the
                         * triangle.
                         */
                        outTriangles.set(pTriangle+2, workingEdges.get(pEdge));
                }
            }
            if (workingEdges.get(pEdge+2) != HULL)
            {
                /*
                 * The left side of edge A->B has an associated triangle.
                 * 
                 * Indicates that B->A is the clockwise wrapping direction.
                 * 
                 * This will never be the case for an original edge since
                 * original edges always have their left sides set to HULL.
                 */
                
                final int pTriangle = workingEdges.get(pEdge+2)*3;
                
                if (outTriangles.get(pTriangle) == UNDEFINED)
                {

                    /*
                     * This is the first time this triangle has been seen.
                     * 
                     * Trivia: Will only get here for internal triangles that
                     * don't have a hull edge.
                     * 
                     * Initialize this triangle by adding the edge's vertices
                     * to the triangle definition.
                     * 
                     * Wrap B->A for clockwise.
                     */
                    outTriangles.set(pTriangle, workingEdges.get(pEdge+1));
                    outTriangles.set(pTriangle+1, workingEdges.get(pEdge));
                }
                else if (outTriangles.get(pTriangle+2) == UNDEFINED)
                {
                    // This triangle's first two vertices have already been
                    // set.  Need to figure out which vertex in this edge
                    // is the final vertex.
                    if (workingEdges.get(pEdge).equals(
                                    outTriangles.get(pTriangle))
                            || workingEdges.get(pEdge).equals(
                                    outTriangles.get(pTriangle+1)))
                        // The first vertex of this edge is already in the
                        // triangle. Add the second vertex of this edge to
                        // the triangle.
                        outTriangles.set(pTriangle+2
                                , workingEdges.get(pEdge+1));
                    else
                        /*
                         * The first vertex of this edge is NOT already in
                         * the triangle, so the 2nd vertex must already be
                         * in it.  Add the first vertex of this edge to
                         * the triangle.
                         */
                        outTriangles.set(pTriangle+2, workingEdges.get(pEdge));
                }
            }
        }
        
    }
    
    /**
     * Returns TRUE if the line segments AB and CD intersect at one or
     * more points.  Otherwise FALSE.
     * @param ax The x-value of point (ax, ay) for the line segment AB
     * @param ay The y-value of point (ax, ay) for the line segment AB
     * @param bx The x-value of point (bx, by) for the line segment AB
     * @param by The y-value of point (bx, by) for the line segment AB
     * @param cx The x-value of point (cx, cy) for the line segment CD
     * @param cy The y-value of point (cx, cy) for the line segment CD
     * @param dx The x-value of point (dx, dy) for the line segment CD
     * @param dy The y-value of point (dx, dy) for the line segment CD
     * @return TRUE if the line segments AB and CD intersect at one or
     * more points.  Otherwise FALSE.
     */
    private static boolean segmentsOverlap(float ax
            , float ay
            , float bx
            , float by
            , float cx
            , float cy
            , float dx
            , float dy)
    {
        final float deltaABx = bx - ax;
        final float deltaABy = by - ay;
        
        final float deltaCDx = dx - cx;
        final float deltaCDy = dy - cy;
        
        final float deltaCAx = ax - cx;
        final float deltaCAy = ay - cy;
        
        final float numerator = (deltaCAy * deltaCDx) - (deltaCAx * deltaCDy);
        final float denominator = (deltaABx * deltaCDy) - (deltaABy * deltaCDx);

        final float tolerance = 0.001f;
        
        if (denominator == 0)
        {
            if (numerator != 0)
                // Parallel and not colinear
                return false;
            /*
             * Lines are colinear.  But do the segments overlap?
             * 
             * Note: This design takes into account that it is a
             * logic error to call this operation for segments that share
             * end points.
             * 
             * Note: Since we know they are colinear, we only need to
             * check one axis for overlap.
             */
            if (Math.abs(cx - dx) < tolerance)
            {
                // Line is horizontal.  Use y-axis.
                if (Math.max(cy, dy) < Math.min(ay, by)
                                || Math.max(ay, by) < Math.min(cy, dy))
                    // The end points of the segments don't overlap.
                    // No intersection.
                    return false;
                else
                    // The end points of the segments overlap.  Intersection.
                    return true;
            }
            else
            {
                if (Math.max(cx, dx) < Math.min(ax, bx)
                                || Math.max(ax, bx) < Math.min(cx, dx))
                    // The end points of the segments don't overlap.
                    // No intersection.
                    return false;
                else
                    // The end points of the segments overlap.  Intersection.
                    return true;
            }
        }

        // Lines definitely intersect at a single point.
        
        float factorAB = numerator / denominator;
        float factorCD = ((deltaCAy * deltaABx) - (deltaCAx * deltaABy))
                                / denominator;

        // Determine the type of intersection
        if ((factorAB >= 0.0f)
                        && (factorAB <= 1.0f)
                        && (factorCD >= 0.0f)
                        && (factorCD <= 1.0f))
            // Segments intersect.
            return true;

        // Intersection is outside of one or both segments.
        return false;
        
    }
    
    /**
     * Sets the left face value of the specified edge to the specified value
     * if the value is not already set.
     * <p>Note that this means that once the value has been set to a value
     * other than UNDEFINED, this operation will not change the value.<p>
     * @param iEdge The index of the edge to update.
     * @param iStartVert The vertex that represents the start of the edge.
     * Used to determine which side of the edge is left.
     * @param faceValue The new value to apply.
     * @param edges The list of edges in the form
     * (vertAIndex, vertBIndex, valueA, valueB)
     * where valueA represents the left side of the edge A->B and valueB
     * represents the left side of the edge B->A.
     */
    private static void updateLeftFace(int iEdge
            , int iStartVert
            , int faceValue
            , ArrayList<Integer> edges)
    {
        final int pEdge = iEdge*4;
        if (edges.get(pEdge) == iStartVert
                && edges.get(pEdge+2) == UNDEFINED)
            edges.set(pEdge+2, faceValue);
        else if (edges.get(pEdge+1) == iStartVert
                && edges.get(pEdge+3) == UNDEFINED)
            edges.set(pEdge+3, faceValue);
    }
}
