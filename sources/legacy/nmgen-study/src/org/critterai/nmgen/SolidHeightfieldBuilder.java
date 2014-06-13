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

import org.critterai.nmgen.SolidHeightfield.SolidHeightFieldIterator;

/**
 * A class used to build solid heightfields from source geometry using a
 * given configuration. The solid heightfield represents the space obstructed
 * by the source geometry.
 * <p>Each triangle in the source geometry is voxelized using conservative
 * voxelization and added to the field. Conservative voxelization is an
 * algorithm that ensures that polygon surfaces are completely encompassed
 * by the the generated voxels.</p>
 * <p>At the end of the process, spans with the {@link SpanFlags#WALKABLE}
 * flag have survived the following tests:</p>
 * <ul>
 * <li>The top of the span is at least a minimum distance from the bottom of
 * the span above it.  (The tallest agent can "stand" on the span without
 * colliding with an obstruction above.)</li>
 * <li>The top voxel of the span represents geometry with a slope below a
 * maximum allowed value. (The slope is low enough to be traversable by
 * agents.)</li>
 * <li>If ledge culling is enabled, the top of the span does not represent
 * a ledge.  (Agents can legally "step down" from the span to any of its
 * neighbors.)</li>
 * </ul>
 * @see <a href="http://www.critterai.org/nmgen_voxel"
 * target="_parent">The Voxelization Process</a>
 * @see <a href="http://www.critterai.org/nmgen_hfintro"
 * target="_parent">Introduction to Heightfields</a>
 * @see <a href="http://www.ecn.purdue.edu/purpl/level2/papers/consvox.pdf"
 * target="_blank">Conservative Voxelization (PDF)</a>
 */
public final class SolidHeightfieldBuilder
{

    /*
     * Design notes:
     * 
     * Recast reference:
     * rcCreateHeightfield in Recast.cpp
     * rcMarkWalkableTriangles in Recast.cpp
     * rcRasterizeTriangles in RecastRasterization.cpp
     * rcFilterLedgeSpans in RecastFilter.cpp
     * rcFilterWalkableLowHeightSpans in RecastFilter.cpp
     * 
     * Not adding configuration getters until they are needed.
     * Never add setters.  Configuration should remain immutable to keep
     * the class thread friendly.
     * 
     * TODO: EVAL: It may be better to implement post-processing as external
     * algorithms similar to what is done with the open heightfield and
     * contour classes.
     * 
     */
    
    // Configuration settings.
    
    private final boolean mClipLedges;
    private final int mMinTraversableHeight;
    private final int mMaxTraversableStep;
    
    /**
     * A derived value which represent the minimum y-normal permitted for
     * a polygon to be considered traversable.
     * <p>See the constructor for details.</p>
     */
    private final float mMinNormalY;
    
    /**
     * The cell size to use for all new fields.
     * <p>IMPORTANT: Only use this value for heightfield object initialization.
     * After that, use the value in the heightfield object.  (Since the
     * heightfield instance may place limitations on the value.)</p>
     */
    private final float mCellSize;
    
    /**
     * The cell height to use for all new fields.
     * <p>IMPORTANT: Only use this value for heightfield object initialization.
     * After that, use the value in the heightfield object.  (Since the height
     * field object may place limitations on the value.)</p>
     */
    private final float mCellHeight;
    
    /**
     * Constructor
     * @param cellSize The size of the cells.  (The grid that forms the base
     * of the field.)
     * <p>This value represents the x and z-axis sampling resolution to use
     * when generating voxels.</p>
     * 
     * @param cellHeight The height increment of the field.
     * <p>This value represents the y-axis sampling resolution to use when
     * generating voxels.</p>
     * 
     * @param minTraversableHeight Represents the minimum floor to ceiling
     * height that will still allow a floor area to be considered traversable.
     * <p>Permits detection of overhangs in the geometry which make the
     * geometry below become unwalkable.</p>
     * <p>Constraints:  > 0</p>
     * 
     * @param maxTraversableStep Represents the maximum ledge height that
     * is considered to still be traversable.
     * <p>Prevents minor deviations in height from improperly showing as
     * obstructions. Permits detection of stair-like structures, curbs, etc.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param maxTraversableSlope The maximum slope that is considered
     * traversable. (Degrees)
     * <p>Spans that are at or above this slope have the
     * {@link SpanFlags#WALKABLE} flag removed.</p>
     * <p>Constraints:  0 <= value <= 85</p>
     * 
     * @param clipLedges Indicates whether ledges should be marked as
     * unwalkable. I.e. The {@link SpanFlags#WALKABLE} flag will be removed.
     * <p>A ledge is a normally traversable span that has one or more
     * accessible neighbors with a  an un-steppable drop from span top to
     * span top.</p>
     * <p>E.g. If an agent using the navmesh were to travel down from the
     * ledge span to its neighbor span, it would result in the maximum
     * traversable step distance being violated.  The agent cannot legally
     * "step down" from a ledge to its neighbor.</p>
     */
    public SolidHeightfieldBuilder(float cellSize
            , float cellHeight
            , int minTraversableHeight
            , int maxTraversableStep
            , float maxTraversableSlope
            , boolean clipLedges)
    {

        mMinTraversableHeight = Math.max(1, minTraversableHeight);
        mMaxTraversableStep = Math.max(0, maxTraversableStep);
        maxTraversableSlope = Math.min(85, Math.max(0, maxTraversableSlope));
        mClipLedges = clipLedges;
        mCellSize = cellSize;
        mCellHeight = cellHeight;

        /*
         * Derive the minimum y-normal.
         * 
         * Base Reference: http://mathworld.wolfram.com/DihedralAngle.html
         * 
         * By ensuring n1 and n2 are both normalized before the calculation, the
         * denominator in the reference equations evaluate to 1 and
         * can be discarded. So the reference equation is simplified to...
         * 
         * cos theta = n1 dot n2
         * 
         * Using:
         * 
         *       n1 = (0, 1, 0) (Represents a flat surface on the (x,z) plane.)
         *       n2 = (x, y, z) Normalized. (A surface on an arbitrary plane.)
         * 
         * Simplify and solve for y:
         * 
         *       cos theta = 0x + 1y + 0z
         *       y = cos theta
         * 
         * We know theta.  It is the value of maxTraversableSlope after
         * conversion to radians. So we know what y-normal is at the walk
         * slope angle. If a polygon's y-normal is LESS THAN our calculated
         * y-normal, then we know we have exceeded the walk angle.
         */
        mMinNormalY = (float)Math.cos(Math.abs(
                        maxTraversableSlope)/180 * Math.PI);
    }
    
    /**
     * Generates a solid heightfield from the provided source geometry.  The
     * solid heightfield will represent the space obstructed by the source
     * geometry.
     * <p>The {@link SpanFlags#WALKABLE} will be applied to spans whose top
     * surface is considered traversable. See the class description for
     * details.</p>
     * @param vertices Source geometry vertices in the form (x, y, z).
     * @param indices Source geometry indices in the form (VertA, VertB, VertC).
     * Wrapped: Clockwise.
     * @return The generated solid heightfield, or null if the generation fails.
     */
    public SolidHeightfield build(float[] vertices, int[] indices)
    {
        // Perform basic checks.
        if (vertices == null
                || indices == null
                || vertices.length % 3 != 0
                || indices.length % 3 != 0)
            return null;
        
        // Initialize heightfield.
        final SolidHeightfield result =
            new SolidHeightfield(mCellSize, mCellHeight);
        
        // Pre-calculate values to save on the cost of division later.
        final float inverseCellSize = 1 / result.cellSize();
        final float inverseCellHeight = 1 / result.cellHeight();
        
        // Detect and set the bounds of the source geometry.
        // Default to the first vertex.
        float xmin = vertices[0];
        float ymin = vertices[1];
        float zmin = vertices[2];
        float xmax = vertices[0];
        float ymax = vertices[1];
        float zmax = vertices[2];
        // Loop through all vertices, expanding the bounds
        // as appropriate.
        for (int i = 3; i < vertices.length; i += 3)
        {
            xmax = Math.max(vertices[i], xmax);
            ymax = Math.max(vertices[i + 1], ymax);
            zmax = Math.max(vertices[i + 2], zmax);
            
            xmin = Math.min(vertices[i], xmin);
            ymin = Math.min(vertices[i + 1], ymin);
            zmin = Math.min(vertices[i + 2], zmin);
        }
        // Set the bounds.
        result.setBounds(xmin, ymin, zmin, xmax, ymax, zmax);
        
        // Detect which polygons in the source mesh have a slope
        // that low enough to be considered traversable.  (Agent can walk up
        // or down the slope.)
        final int[] polyFlags = markInputMeshWalkableFlags(vertices, indices);
        
        // For each polygon in the source mesh: Voxelize it and add the
        // resulting spans to the solid field.
        final int polyCount = indices.length / 3;
        for (int iPoly = 0; iPoly < polyCount; iPoly++)
        {
            voxelizeTriangle(iPoly
                    , vertices
                    , indices
                    , polyFlags[iPoly]
                    , inverseCellSize
                    , inverseCellHeight
                    , result);
        }
        
        // Remove the walkable flag from any span that has another span too
        // close above it.
        markLowHeightSpans(result);
        
        if (mClipLedges)
            // Remove the walkable flag from any span that is determined to
            // be a ledge.
            markLedgeSpans(result);
        
        return result;
        
    }
    
    /**
     * Checks the slope of each polygon against the maximum allowed.  Any
     * polygon whose slope is below the maximum permitted gets the
     * {@link SpanFlags#WALKABLE} flag.
     * @param vertices  The source geometry vertices in the form (x, y, z).
     * @param indices The source geometry indices in the form
     * (vertA, vertB, vertC), clockwise wrapped.
     * @return An array of flags in the form
     * (polyFlag0, polyFlag1, ..., polyFlagN), stride = 1.
     */
    private int[] markInputMeshWalkableFlags(float[] vertices, int[] indices)
    {
        
        // See mMinNormalY in constructor for more information on how this
        // works.
        
        final int[] flags = new int[indices.length / 3];
        
        // Working variables.  Content changes for every loop
        // and has no meaning outside the loop.
        float[] diffAB = new float[3];
        float[] diffAC = new float[3];
        float[] crossDiff = new float[3];
        
        // Loop through all polygons.
        int polyCount = indices.length / 3;
        for (int iPoly = 0; iPoly < polyCount; iPoly++)
        {
            // Get pointers to each polygon vertex.
            int pVertA = indices[iPoly*3]*3;
            int pVertB = indices[iPoly*3+1]*3;
            int pVertC = indices[iPoly*3+2]*3;

            // Determine the y-normal for the polygon.
            float normalY = getNormalY(
                    cross(subtract(pVertB, pVertA, vertices, diffAB)
                            , subtract(pVertC, pVertA, vertices, diffAC)
                            , crossDiff));
            
            if (normalY > mMinNormalY)
                // The slope of this polygon is acceptable.  Mark it as
                // walkable.
                flags[iPoly] = SpanFlags.WALKABLE;
        }
        
        return flags;
    }

    /**
     * Removes the traversable flag for any spans that represent a ledge.
     * A ledge occurs when stepping from the top of one span down to any of its
     * neighbor spans exceeds the allowed walk climb distance.  (i.e. Can't
     * legally "step down" to a neighbor span.)
     * @param field The field to operation on.
     */
    private void markLedgeSpans(SolidHeightfield field)
    {
        
        /*
         * Note: While this is a solid field representing obstructions, much
         * of this algorithm deals with the space between obstructions.
         * (The gaps.) So you will need to twist your thinking to gaps rather
         * than obstructions.
         * 
         * For visualization, see the @see in the class' javadoc.
         */
        
        // Loop through all spans.
        SolidHeightFieldIterator iter = field.dataIterator();
        while (iter.hasNext())
        {
            HeightSpan span = iter.next();

            if ((span.flags() & SpanFlags.WALKABLE) == 0)
                // Span is already known to be un-waklable.
                // Skip it.
                continue;
            
            final int widthIndex = iter.widthIndex();
            final int depthIndex = iter.depthIndex();
            
            // These values represent the gap (floor to ceiling) above the
            // current span.
            final int currFloor = span.max();
            final int currCeiling = (span.next() != null) ?
                            span.next().min() : Integer.MAX_VALUE;
            
            /*
             * Represents the minimum distance from the current span's floor
             * to a neighbor span's floor. A positive value indicates a step
             * up.  A negative distance represents a step down. If this
             * distance is too far down, then the current span is a ledge
             * and isn't traversable.
             * 
             * This algorithm only cares about drops.
             * Default to a maximum step up.
             */
            int minDistanceToNeighbor = Integer.MAX_VALUE;
            
            /*
             * The lowest possible floor is at -mMaxTraversableStep.  No span
             * can exist below the zero height index. So stepping from a span
             * whose floor is at zero to "empty space" will result in a drop
             * to -mMaxTraversableStep.
             */
            
            /*
             * Loop through all neighbor grid cells.
             */
            for (int dir = 0; dir < 4; dir++)
            {
                final int nWidthIndex = widthIndex
                        + BoundedField.getDirOffsetWidth(dir);
                final int nDepthIndex = depthIndex
                        + BoundedField.getDirOffsetDepth(dir);

                // Get the lowest span in this neighbor column.
                HeightSpan nSpan = field.getData(nWidthIndex, nDepthIndex);
                
                if (nSpan == null)
                {
                    // No neighbor on this side. Treat as the maximum drop.
                    // (Which is always considered a ledge.)
                    // TODO: EVAL: Should this be a break rather than a
                    // continue? (Detected too close to release to risk code
                    // changes.)
                    minDistanceToNeighbor = Math.min(minDistanceToNeighbor
                                    , -mMaxTraversableStep - currFloor);
                    continue;
                }

                /*
                 * First need to take into account the area below the lowest
                 * span in this neighbor column.
                 * 
                 * In this special case, the floor of this gap is the lowest
                 * possible value. The ceiling of this gap is the bottom of
                 * neighbor span.
                 */
                // Default to an excessive drop.
                int nFloor = -mMaxTraversableStep;
                int nCeiling = nSpan.min(); // The bottom of this first span.
                
                /*
                 * This check filters out the following:
                 * 
                 * The distance from the current span's floor this neighbor's
                 * ceiling  is not large enough to permit transit in that
                 * direction? (Agent will "bump its head" if it moves in this
                 * direction?)
                 * 
                 * The neighbor gap is entirely below the floor of the
                 * current span.
                 * 
                 * In such cases travel is not allowed to the neghbor gap, so
                 * it isn't taken into account.
                 */
                if (Math.min(currCeiling, nCeiling) - currFloor
                                > mMinTraversableHeight)
                    // Travel is permitted in this direction.  So take this
                    // neighbor gap into account.
                    minDistanceToNeighbor =
                        Math.min(minDistanceToNeighbor, (nFloor - currFloor));
                    
                /*
                 * Now process the rest of the gaps in this neighbor column
                 * normally.  E.g. The top of the span is the floor. The
                 * bottom of the next span is the ceiling.
                 */
                
                for (nSpan =  field.getData(nWidthIndex, nDepthIndex)
                        ; nSpan != null
                        ; nSpan = nSpan.next())
                {
                    nFloor = nSpan.max();
                    nCeiling = (nSpan.next() != null) ?
                                    nSpan.next().min() : Integer.MAX_VALUE;
                    /*
                     * This next check filters out the following:
                     * 
                     * The distance from the current span's floor this
                     * neighbor's ceiling  is not large enough to permit
                     * transit in that direction? (Agent will "bump its head"
                     * if it moves in this direction?)
                     * 
                     * The neighbor gap is entirely below the floor of the
                     * current span.
                     * 
                     * The neighbor gap is entirely above the ceiling of the
                     * current gap.
                     * 
                     * In such cases travel is not allowed to the neghbor gap,
                     * so it isn't taken into account.
                     */
                    if (Math.min(currCeiling, nCeiling)
                                    - Math.max(currFloor, nFloor)
                                            > mMinTraversableHeight)
                        // Potential travel to this neighbor span.
                        minDistanceToNeighbor =
                            Math.min(minDistanceToNeighbor
                                            , (nFloor - currFloor));
                }
                
            }
            
            // Remember:  A negative distance indicates a drop.
            if (minDistanceToNeighbor < -mMaxTraversableStep)
                // Can only drop by mMaxTraversableStep, but a neighbor has a
                // drop that exceeds this allowed drop.  Remove the walkable
                // flag.
                span.setFlags(span.flags() & ~SpanFlags.WALKABLE);
        }
    }
    
    /**
     * Remove the traversable flag from spans that have another span too
     * close above them.
     * @param field  The heightfield to operate on.
     */
    private void markLowHeightSpans(SolidHeightfield field)
    {
        // TODO: EVAL: Consider merging this operation with markLedgeSpans.
        
        // For visualization, see the @see in the class' javadoc.
        
        // Iterate through all spans in the field.
        SolidHeightFieldIterator iter = field.dataIterator();
        while (iter.hasNext())
        {
            HeightSpan span = iter.next();
            
            if ((span.flags() & SpanFlags.WALKABLE) == 0)
                // Span is already known to be un-waklable.
                // Skip it.
                continue;
            
            // Find the gap between the current span and the next higher span.
            // This represents the open space (floor to ceiling) above the
            // current span.
            int spanFloor = span.max();
            int spanCeiling = (span.next() != null) ?
                            span.next().min() : Integer.MAX_VALUE;
            
            if (spanCeiling - spanFloor <= mMinTraversableHeight)
                // Can't stand on this span.  Ceiling is too low.
                // Remove its walkable flag.
                span.setFlags(span.flags() & ~SpanFlags.WALKABLE);
        }
    }
    
    /**
     * Clamps the value to the specified range.
     * @param value The value to clamp.
     * @param minimum The minimum allowed value. (Inclusive.)
     * @param maximum The maximum allowed value. (Inclusive.)
     * @return If minimum <= value <= maximum, will return value.
     * If value < minimum, will return minimum.
     * If value > maximum, will return maximum.
     */
    private static int clamp(int value, int minimum, int maximum)
    {
        return (value < minimum
                        ? minimum : (value > maximum ? maximum : value));
    }
    
    private static int clipPoly(float[] in
                    , int inputVertCount
                    , float[] out
                    , float pnx
                    , float pnz
                    , float pd)
    {
        
        // TODO: DOC: Figure out what is going on here.  Not familiar with
        // algorithm. pnx and pnz are normals.
        
        float d[] = new float[inputVertCount];
        for (int vertIndex = 0; vertIndex < inputVertCount; ++vertIndex)
            d[vertIndex] = (pnx * in[vertIndex * 3])
                                + (pnz * in[vertIndex * 3 + 2]) + pd;
        
        int m = 0;
        for (int current = 0, previous = d.length - 1
                ; current < d.length
                ; previous=current, ++current)
        {
            boolean ina = d[previous] >= 0;
            boolean inb = d[current] >= 0;
            if (ina != inb)
            {
                float s = d[previous] / (d[previous] - d[current]);
                out[m*3+0] =
                    in[previous*3+0] + (in[current*3+0] - in[previous*3+0])*s;
                out[m*3+1] =
                    in[previous*3+1] + (in[current*3+1] - in[previous*3+1])*s;
                out[m*3+2] =
                    in[previous*3+2] + (in[current*3+2] - in[previous*3+2])*s;
                m++;
            }
            if (inb)
            {
                out[m*3+0] = in[current*3+0];
                out[m*3+1] = in[current*3+1];
                out[m*3+2] = in[current*3+2];
                m++;
            }
        }
        return m;
    }

    /**
     * Performs a cross product on the vectors u and v. (u x v)
     * @param u The first vector in the form (x, y, z)
     * @param v The second vector in the form (x, y, z)
     * @param out The array to be loaded with the result.
     * @return A reference to the out array loaded with the cross product.
     */
    private static float[] cross(float[] u, float[] v, float[] out)
    {
        // Reference: http://mathworld.wolfram.com/CrossProduct.html
        // Reference: http://en.wikipedia.org/wiki/Cross_product
        //                                       #Computing_the_cross_product
        out[0] = u[1] * v[2] - u[2] * v[1];
        out[1] = -u[0] * v[2] + u[2] * v[0];
        out[2] = u[0] * v[1] - u[1] * v[0];
        return out;
    }

    /**
     * Normalizes the provided vector and returns the y-value.
     * @param v The vector to normalize in the form: (x, y, z)
     * @return The y-value of the normalized vector.
     */
    private static float getNormalY(float[] v)
    {
        // This is just the standard normalization algorithm with
        // unneeded x and z calculations removed.
        
        final float epsilon = 0.0001f;
        
        float length =
            (float)Math.sqrt((v[0] * v[0]) + (v[1] * v[1]) + (v[2] * v[2]));
        if (length <= epsilon)
            length = 1;
        
        float y = v[1] / length;
        
        if (Math.abs(y) < epsilon)
            y = 0;

        return y;
    }
    
    /**
     * Performs vector subtraction on the vertices. (VertexA - VertexB)
     * @param pVertA The pointer to a valid vertex in the source vertices array.
     * @param pVertB The pointer to a valid vertex in the source vertices array.
     * @param out An array of size 3.  The array is loaded with the result of
     * the subtraction and returned.
     * @return  A reference to the out array loaded with the result of the
     * subtraction.
     */
    private static float[] subtract(int pVertA
                    , int pVertB
                    , float[] vertices
                    , float[] out)
    {
        out[0] = vertices[pVertA] - vertices[pVertB];
        out[1] = vertices[pVertA+1] - vertices[pVertB+1];
        out[2] = vertices[pVertA+2] - vertices[pVertB+2];
        return out;
    }
    
    /**
     * Voxelizes the chosen polygon and adds the resulting spans to the
     * heightfield.
     * <p>The inverse arguments are included for optimization.  (No need to
     * recalculate the values for every call.)</p>
     * <p>The heightfield will make the final decision on whether to apply
     * the flags provided in the arguments.  See the
     * {@link SolidHeightfield#addData(int, int, int, int, int) heightfield
     * add operation} for details</p>
     * @param polyIndex The polygon to voxelize.
     * @param vertices The vertices of the source geometry in the form
     * (x, y, z).
     * @param indices The indices of the source geometry in the form
     * (vertA, vertB, vertC), wrapped clockwise.
     * @param polyFlags The flags to apply to all new spans within the
     * heightfield.
     * @param inverseCellSize Inverse cell size. (1/cellSize)
     * @param inverseCellHeight Inverse cell height. (1/cellheight)
     * @param inoutField The heightfield to add new spans to.
     */
    private static void voxelizeTriangle(int polyIndex
            , float[] vertices
            , int[] indices
            , int polyFlags
            , float inverseCellSize
            , float inverseCellHeight
            , SolidHeightfield inoutField)
    {
        
        /*
         * Design notes:
         * 
         * There is significant processing going on here that is not
         * required since this is a private operation and the input is tightly
         * controlled. For example: We know that the heightfield is sized to
         * hold the source geometry, so bounds checks aren't really needed.
         * 
         * But, with the possible exception of object creation, the extra
         * cost is not big.  So I'm leaving the algorithm as it is just in
         * case it is converted to a public operation at a later date.
         */
        
        // Pointer to the polygon.
        final int pPoly = polyIndex*3;
        
        // Polygon vertices.
        final float[] triVerts = {
                      vertices[indices[pPoly]*3]    // VertA
                    , vertices[indices[pPoly]*3+1]
                    , vertices[indices[pPoly]*3+2]
                    , vertices[indices[pPoly+1]*3]    // VertB
                    , vertices[indices[pPoly+1]*3+1]
                    , vertices[indices[pPoly+1]*3+2]
                    , vertices[indices[pPoly+2]*3]    // VertC
                    , vertices[indices[pPoly+2]*3+1]
                    , vertices[indices[pPoly+2]*3+2]
                };
        
        // Determine the bounding box of the polygon.
        
        // Initialize bounds to the first triangle vertex.
        final float[] triBoundsMin = { triVerts[0], triVerts[1], triVerts[2] };
        final float[] triBoundsMax = { triVerts[0], triVerts[1], triVerts[2] };
        
        // Loop through all vertices to determine the actual bounding box.
        for (int vertPointer = 3; vertPointer < 9; vertPointer += 3)
        {
            triBoundsMin[0] = Math.min(triBoundsMin[0],
                            triVerts[vertPointer]);
            triBoundsMin[1] = Math.min(triBoundsMin[1],
                            triVerts[vertPointer + 1]);
            triBoundsMin[2] = Math.min(triBoundsMin[2],
                            triVerts[vertPointer + 2]);
            triBoundsMax[0] = Math.max(triBoundsMax[0],
                            triVerts[vertPointer]);
            triBoundsMax[1] = Math.max(triBoundsMax[1],
                            triVerts[vertPointer + 1]);
            triBoundsMax[2] = Math.max(triBoundsMax[2],
                            triVerts[vertPointer + 2]);
        }
        
        // If the triangle does not overlap the heightfield, then skip it.
        if (!inoutField.overlaps(triBoundsMin, triBoundsMax))
            return;
        
        /*
         * Determine footprint of triangle bounding box on the heightfield's
         * grid.
         * 
         * Notes:
         * 
         * The heightfield is an integer based grid with its origin at the
         * heightfield's minimum bounds.  I.e. Grid coordinate
         * (0, 0) => (heightField.minbounds.x, heightField.minbounds.z)
         * 
         * The heightfield width/depth values map to the (x, z) plane of the
         * triangle, not the (x, y) plane.
         */
        
        // First, convert the triangle bounds to field grid coordinates.
        int triWidthMin = (int)((triBoundsMin[0] - inoutField.boundsMin()[0])
                        * inverseCellSize);
        int triDepthMin = (int)((triBoundsMin[2] - inoutField.boundsMin()[2])
                        * inverseCellSize);
        int triWidthMax = (int)((triBoundsMax[0] - inoutField.boundsMin()[0])
                        * inverseCellSize);
        int triDepthMax = (int)((triBoundsMax[2] - inoutField.boundsMin()[2])
                        * inverseCellSize);
        
        // Snap the grid coordinates to the grid bounds.
        triWidthMin = clamp(triWidthMin, 0, inoutField.width() - 1);
        triDepthMin = clamp(triDepthMin, 0, inoutField.depth() - 1);
        triWidthMax = clamp(triWidthMax, 0, inoutField.width() - 1);
        triDepthMax = clamp(triDepthMax, 0, inoutField.depth() - 1);
        
        /*
         * "in" will contain the final data.
         * "out" and "inrow" are used for intermediate data.
         * "in" is initially seeded with the triangle vertices.
         * The arrays are sized to be 3 * 7.  This allows for the storage of
         * the maximum vertex count for a triangle clipped into a square (6)
         * with an extra triple.
         * (Don't know the purpose of the extra triple.)
         */
        final float in[] = new float[21];
        final float out[] = new float[21];
        final float inrow[] = new float[21];
        
        // The height of the heightfield.
        final float fieldHeight =
            inoutField.boundsMax()[1] - inoutField.boundsMin()[1];
        
        /*
         * Loop through all grid locations overlapped by the polygon.
         * (xz-plane only).
         * 
         * Clip the triangle into all grid cells it touches.
         * 
         * Any early exit from either of the loops means that the triangle
         * does not overlap the grid column, or is outside the height
         * bounds of the field.
         * 
         * All detailed clip data is discarded in the end.  The only information
         * preserved is the height information.
         * 
         * Dev Note:  I don't understand the mathematical algorithm used here.
         * So I've marked it as magic. But by tracing the process on paper
         * I've determined that the algorithm is finding all intersection
         * points between the grid column and the triangle face.
         * 
         * For visualization, see the @see in the class' javadoc.
         */
        for (int depthIndex = triDepthMin
                        ; depthIndex <= triDepthMax
                        ; ++depthIndex)
        {
            
            // Seed with the triangle vertices.
            System.arraycopy(triVerts, 0, in, 0, triVerts.length);
            
            // Do some magic.
            // Count of cell intersection vertices found.
            int intermediateVertCount = 3;
            final float rowWorldZ = inoutField.boundsMin()[2]
                                        + (depthIndex * inoutField.cellSize());
            intermediateVertCount = clipPoly(in
                            , intermediateVertCount
                            , out
                            , 0
                            , 1
                            , -rowWorldZ);
            if (intermediateVertCount < 3)
                continue;
            intermediateVertCount = clipPoly(out, intermediateVertCount
                            , inrow
                            , 0
                            , -1
                            , rowWorldZ + inoutField.cellSize());
            if (intermediateVertCount < 3)
                continue;
            
            for (int widthIndex = triWidthMin
                            ; widthIndex <= triWidthMax
                            ; ++widthIndex)
            {
                
                // Do some more magic.
                int vertCount = intermediateVertCount;
                final float colWorldX = inoutField.boundsMin()[0]
                                        + (widthIndex * inoutField.cellSize());
                vertCount = clipPoly(inrow, vertCount, out, 1, 0, -colWorldX);
                if (vertCount < 3)
                    continue;
                vertCount = clipPoly(out
                                , vertCount
                                , in
                                , -1
                                , 0
                                , colWorldX + inoutField.cellSize());
                if (vertCount < 3)
                    continue;
                
                // If got here, then "in" contains the definition for a poly
                // representing the portion
                // of the input triangle that overlaps the grid location.
                
                // Find the height (y-axis) range for this grid location.
                float heightMin = in[1];
                float heightMax = in[1];
                for (int i = 1; i < vertCount; ++i)
                {
                    heightMin = Math.min(heightMin, in[i*3+1]);
                    heightMax = Math.max(heightMax, in[i*3+1]);
                }
                // Convert to height above the "floor" of the heightfield.
                heightMin -= inoutField.boundsMin()[1];
                heightMax -= inoutField.boundsMin()[1];
                if (heightMax < 0.0f || heightMin > fieldHeight)
                    // The height of the potential span is entirely outside
                    // the bounds of the heightfield.
                    continue;
                // Clamp to the heightfield bounding box.
                if (heightMin < 0.0f)
                    heightMin = inoutField.boundsMin()[1];
                if (heightMax > fieldHeight)
                    heightMax = inoutField.boundsMax()[1];
                
                // Convert the min/max to height grid index.
                int heightIndexMin = clamp(
                          (int)Math.floor(heightMin * inverseCellHeight)
                        , 0
                        , Short.MAX_VALUE);
                int heightIndexMax = clamp(
                          (int)Math.ceil(heightMax * inverseCellHeight)
                        , 0
                        , Short.MAX_VALUE);
                
                // Add the span to the heightfield.
                inoutField.addData(widthIndex
                        , depthIndex
                        , heightIndexMin
                        , heightIndexMax
                        , polyFlags);
            }
        }
        
    }
    
}
