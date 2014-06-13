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

/**
 * Used to generate static triangle meshes representing the traversable
 * surfaces of arbitrary source geometry.
 * <p>When the build operation is provided with source geometry, several
 * meshes are generated which represent  the traversable (walkable)
 * surfaces of the source geometry.  A large number of configuration options
 * are used to adjust the final result.</p>
 * <p> <img src=
 * "http://www.critterai.org/projects/nmgen/images/stage_detail_mesh.jpg"/>
 * </p>
 * @see <a href="http://www.critterai.org/nmgen"
 * target="_parent">Project Home</a>
 * @see <a href="http://www.critterai.org/nmgen_overview"
 * target="_parent">Process Overview</a>
 * @see <a href="http://www.critterai.org/nmgen_config"
 * target="_parent">Configuration Options</a>
 */
public final class NavmeshGenerator
{
    
    /*
     * Design notes:
     * 
     * Recast reference: Sample_StatMeshSimple.cpp
     * 
     * Configuration getters will not be added until they are needed.
     * Do not add setters.  The design is meant to be thread friendly and
     * setters would compromise that.
     * Never add getters for the builder fields.  That would not be thread
     * friendly.
     */
    
    // The builders used by this class.
    private final SolidHeightfieldBuilder mSolidHeightFieldBuilder;
    private final OpenHeightfieldBuilder mOpenHeightFieldBuilder;
    private final ContourSetBuilder mContourSetBuilder;
    private final PolyMeshFieldBuilder mPolyMeshBuilder;
    private final DetailMeshBuilder mTriangleMeshBuilder;
    
    /**
     * Constructor
     * @param cellSize The width and depth resolution used when sampling
     * the source mesh.
     * <p>Constraints:  > 0</p>
     * 
     * @param cellHeight The height resolution used when sampling the
     * source mesh.
     * <p>Constraints:  > 0</p>
     * 
     * @param minTraversableHeight Represents the minimum floor to ceiling
     * height that will still allow the floor area to be considered walkable.
     * <p>Permits detection of overhangs in the geometry which make the
     * geometry below become unwalkable.</p>
     * <p>Constraints:  > 0</p>
     * 
     * @param maxTraversableStep Represents the maximum ledge height that
     * is considered to still be walkable.
     * <p>Prevents minor deviations in height from improperly showing
     * as obstructions. Permits detection of stair-like structures, curbs, etc.
     * </p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param maxTraversableSlope The maximum slope that is considered
     * walkable. (Degrees)
     * <p>Constraints:  0 <= value <= 85</p>
     * 
     * @param clipLedges Indicates whether ledges should be marked
     * as unwalkable.
     * <p>A ledge is a normally walkable voxel that has one or more
     * accessible neighbors with a  an un-steppable drop from voxel top to
     * voxel top.</p>
     * <p>E.g. If an agent using the navmesh were to travel down from the
     * ledge voxel to its neighbor voxel, it would  result in the maximum
     * traversable step distance being violated.  The agent cannot legally
     * "step down" from a ledge to its neighbor.</p>
     * 
     * @param traversableAreaBorderSize Represents the closest any part of
     * the navmesh can get to an obstruction in the source mesh.
     * <p>Usually set to the maximum bounding radius of entities utilizing
     * the navmesh for navigation decisions.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param smoothingThreshold The amount of smoothing to be performed
     * when generating the distance field.
     * <p>This value impacts region formation and border detection.  A
     * higher value results in generally larger regions and larger border
     * sizes.  A value of zero will disable smoothing.</p>
     * <p>Constraints: 0 <= value <= 4</p>
     * 
     * @param useConservativeExpansion Applies extra algorithms to regions
     * to help prevent poorly formed regions from forming.
     * <p>If the navigation mesh is missing sections that should be present,
     * then enabling this feature will likely fix the problem</p>
     * <p>Enabling this feature significantly increased processing cost.</p>
     * 
     * @param minUnconnectedRegionSize The minimum region size for
     * unconnected (island) regions. (Voxels)
     * <p>Any generated regions that are not connected to any other region
     * and are smaller than this size will be culled before final navmesh
     * generation.  I.e. No longer considered walkable.<p>
     * <p>Constraints:  > 0</p>
     * 
     * @param mergeRegionSize Any regions smaller than this size will, if
     * possible, be merged with larger regions. (Voxels)
     * <p>Helps reduce the number of unnecessarily small regions that can
     * be formed.  This is especially an issue in diagonal path regions
     * where inherent faults in the region generation algorithm can result in
     * unnecessarily small regions.</p>
     * <p>If a region cannot be legally merged with a neighbor region, then
     * it will be left alone.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param maxEdgeLength The maximum length of polygon edges that
     * represent the border of the navmesh.
     * <p>More vertices will be added to navmesh border edges if this value
     * is exceeded for a particular edge. In certain cases this will reduce
     * the number of thin, long triangles in the navmesh.</p>
     * <p>A value of zero will disable this feature.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param edgeMaxDeviation The maximum distance the edge of the navmesh
     *  may deviate from the source geometry.
     * <p>Setting this lower will result in the navmesh edges following the
     * geometry contour more accurately at the expense of an increased
     * triangle count.</p>
     * <p>Setting the value to zero is not recommended since it can result
     * in a large increase in the number of
     * triangles in the final navmesh at a high processing cost.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param maxVertsPerPoly The maximum number of vertices per polygon
     * for polygons generated during the voxel to polygon conversion stage.
     * <p>Higher values reduce performance, but can also result in better
     * formed triangles in the navmesh.  A value of around 6 is generally
     * adequate with diminishing returns for values higher than 6.</p>
     * <p>Contraints: >= 3</p>
     * 
     * @param contourSampleDistance Sets the sampling distance to use when
     * matching the navmesh to the surface of the original geometry.
     * <p>Impacts how well the final mesh conforms to the original
     * geometry's surface contour. Higher values result in a navmesh which
     * conforms more closely to the original geometry's surface at the cost
     * of a higher final  triangle count and higher processing cost.</p>
     * <p>Setting this argument to zero will disable this functionality.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param contourMaxDeviation The maximum distance the surface of the
     * navmesh may deviate from the surface of the original geometry.
     * <p>The accuracy of the algorithm which uses this value is impacted
     * by the value of the contour sample distance argument.</p>
     * <p>The value of this argument has no meaning if the contour sample
     * distance argument is set to zero.</p>
     * <p>Setting the value to zero is not recommended since it can result
     * in a large increase in the number of triangles in the final navmesh
     * at a high processing cost.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @throws IllegalArgumentException  If there are any unresolvable
     * argument errors.  (Most argument errors are avoided through value
     * clamping and other auto-correction methods.)
     * 
     * @see <a href="http://www.critterai.org/?q=nmgen_config"
     * target="_parent">Detailed parameter information.</a>
     */
    public NavmeshGenerator(float cellSize
                , float cellHeight
                , float minTraversableHeight
                , float maxTraversableStep
                , float maxTraversableSlope
                , boolean clipLedges
                , float traversableAreaBorderSize
                , int smoothingThreshold
                , boolean useConservativeExpansion
                , int minUnconnectedRegionSize
                , int mergeRegionSize
                , float maxEdgeLength
                , float edgeMaxDeviation
                , int maxVertsPerPoly
                , float contourSampleDistance
                , float contourMaxDeviation)
        throws IllegalArgumentException
    {
        
        // Convert certain values from world units to voxel units.
        int vxMinTraversableHeight = 1;
        if (minTraversableHeight != 0)
        {
            vxMinTraversableHeight = (int)Math.ceil(
                    Math.max(Float.MIN_VALUE, minTraversableHeight) /
                    Math.max(Float.MIN_VALUE, cellHeight));
        }
        
        int vxMaxTraversableStep = 0;
        if (maxTraversableStep != 0)
        {
            vxMaxTraversableStep = (int)Math.ceil(
                    Math.max(Float.MIN_VALUE, maxTraversableStep) /
                    Math.max(Float.MIN_VALUE, cellHeight));
        }
        
        int vxTraversableAreaBorderSize = 0;
        if (traversableAreaBorderSize != 0)
        {
            vxTraversableAreaBorderSize = (int)Math.ceil(
                    Math.max(Float.MIN_VALUE, traversableAreaBorderSize) /
                    Math.max(Float.MIN_VALUE, cellSize));
        }

        int vxMaxEdgeLength = 0;
        if (maxEdgeLength != 0)
        {
            vxMaxEdgeLength = (int)Math.ceil(
                    Math.max(Float.MIN_VALUE, maxEdgeLength) /
                    Math.max(Float.MIN_VALUE, cellSize));
        }

        // Construct the solid field builder.
        mSolidHeightFieldBuilder = new SolidHeightfieldBuilder(cellSize
                , cellHeight
                , vxMinTraversableHeight
                , vxMaxTraversableStep
                , maxTraversableSlope
                , clipLedges);
        
        // Construct the open field builder.
        // The order of the algorithms is the order they are applied.
        ArrayList<IOpenHeightFieldAlgorithm> regionAlgorithms =
            new ArrayList<IOpenHeightFieldAlgorithm>();
        if (vxTraversableAreaBorderSize > 0)
            // Since there will be a boarder around all null regions,
            // we can use the more efficient form of the algorithm.
            regionAlgorithms.add(new CleanNullRegionBorders(true));
        else
            regionAlgorithms.add(new CleanNullRegionBorders(false));
        regionAlgorithms.add(new FilterOutSmallRegions(minUnconnectedRegionSize
                         , mergeRegionSize));
        mOpenHeightFieldBuilder = new OpenHeightfieldBuilder(
                vxMinTraversableHeight
                , vxMaxTraversableStep
                , vxTraversableAreaBorderSize
                , smoothingThreshold
                , SpanFlags.WALKABLE
                , useConservativeExpansion
                , regionAlgorithms);
        
        // Construct the contour set builder.
        // The order of the algorithms is the order they are applied.
        ArrayList<IContourAlgorithm> contourAlgorithms =
            new ArrayList<IContourAlgorithm>();
        /*
         * Note: Semi-converting edgeMaxDeviation to voxel unit.
         * (It's still a float.)
         * This is needed because the null region edges algorithm has no
         * concept of world units. It only knows about the units it
         * is passed, and it is being passed voxel sized units.
         */
        contourAlgorithms.add(
                new MatchNullRegionEdges(edgeMaxDeviation / cellSize));
        contourAlgorithms.add(new NullRegionMaxEdge(vxMaxEdgeLength));
        mContourSetBuilder = new ContourSetBuilder(contourAlgorithms);
        
        // Construct the polymesh and triange mesh builders.
        mPolyMeshBuilder = new PolyMeshFieldBuilder(maxVertsPerPoly);
        mTriangleMeshBuilder = new DetailMeshBuilder(contourSampleDistance
                , contourMaxDeviation);
    }
    
    /**
     * Build a navigation mesh from the source geometry.
     * @param vertices  The source geometry vertices in the form (x, y, z)
     * @param indices The triangle mesh vertices in the form
     * (vertA, vertB, vertC), wrapped clockwise.
     * @param outIntermediateData  If non-null, the intermediate build
     * results will be added to this object.  If the build fails, the object
     * will contain all intermediate results which were successfully generated.
     * @return The generated navigation mesh, or null if generation failed.
     * @see <a href="http://www.critterai.org/?q=nmgen_overview"
     * target="_parent">Process Overview</a>
     */
    public TriangleMesh build(float[] vertices
            , int[] indices
            , IntermediateData outIntermediateData)
    {
        
        if (outIntermediateData != null)
            outIntermediateData.reset();
        
        long timerStart = 0;
        
        // Reference:  Heightfield overview
        // http://www.critterai.org/?q=nmgen_hfintro
        
        // Generate a height field representing obstructed (solid) space.
        if (outIntermediateData != null)
            timerStart = System.nanoTime();
        
        final SolidHeightfield solidField =
            mSolidHeightFieldBuilder.build(vertices, indices);
        if (solidField == null || !solidField.hasSpans())
            return null;
        
        if (outIntermediateData != null)
            outIntermediateData.voxelizationTime = 
                System.nanoTime() - timerStart;
        
        if (outIntermediateData != null)
            // Store intermediate data.
            outIntermediateData.setSolidHeightfield(solidField);
        
        /*
         * Generate a heightfield representing the open space
         * 
         * Instead of spans representing obstructed space, spans
         * represent open space above obstructed space and between obstructions.
         * 
         * Note: Only a partial build of the heightfield object is being
         * performed here.
         * The only reason for doing a partial build is for ease of prototyping.
         * I.e. Can comment out portions of the height field build without
         * touching the open heightfield builder class.
         */
        
        if (outIntermediateData != null)
            timerStart = System.nanoTime();
        
        final OpenHeightfield openField =
            mOpenHeightFieldBuilder.build(solidField, false);
        if (openField == null)
            return null;
        
        if (outIntermediateData != null)
            // Store intermediate data.
            outIntermediateData.setOpenHeightfield(openField);

        // Finish the build of the field.
        // Order is important.
        mOpenHeightFieldBuilder.generateNeighborLinks(openField);
        mOpenHeightFieldBuilder.generateDistanceField(openField);
        mOpenHeightFieldBuilder.blurDistanceField(openField);
        mOpenHeightFieldBuilder.generateRegions(openField);
        
        if (outIntermediateData != null)
            outIntermediateData.regionGenTime = System.nanoTime() - timerStart;
        
        // Terminology references:
        // http://en.wikipedia.org/wiki/Polygon
        // http://en.wikipedia.org/wiki    riangulation_%28advanced_geometry%29
        
        // Generate contours from the open heightfield's regions.
        // Contours are simply polygons that represent the edges of regions.
        

        if (outIntermediateData != null)
            timerStart = System.nanoTime();
        
        final ContourSet contours = mContourSetBuilder.build(openField);
        if (contours == null)
            return null;
        
        if (outIntermediateData != null)
            outIntermediateData.contourGenTime = System.nanoTime() - timerStart;
        
        if (outIntermediateData != null)
            // Store intermediate data.
            outIntermediateData.setContours(contours);
        
        // Generate a convex polygon mesh from the contours.
        // Converts contours (simple polygons) to convex polygons.
        
        if (outIntermediateData != null)
            timerStart = System.nanoTime();
        
        final PolyMeshField polyMesh = mPolyMeshBuilder.build(contours);
        if (polyMesh == null)
            return null;

        if (outIntermediateData != null)
            outIntermediateData.polyGenTime = System.nanoTime() - timerStart;
        
        if (outIntermediateData != null)
            // Store intermediate data.
            outIntermediateData.setPolyMesh(polyMesh);
        
        // Triangulate the convex polygon mesh.  This is where contour
        // matching is performed. Also referred to as tesselation.
      
        if (outIntermediateData != null)
            timerStart = System.nanoTime();
        
        TriangleMesh mesh = mTriangleMeshBuilder.build(polyMesh, openField);
        
        if (outIntermediateData != null && mesh != null)
            outIntermediateData.finalMeshGenTime = System.nanoTime() - timerStart;
            
         return mesh;
        
    }
    
}
