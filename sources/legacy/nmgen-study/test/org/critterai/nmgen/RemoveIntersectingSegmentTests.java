package org.critterai.nmgen;

import static org.junit.Assert.assertTrue;

import java.util.ArrayList;

import org.junit.Test;
import org.junit.Before;

/**
 * Test related to the ContourSetBuilder.removeIntersectingSegments()
 * operation.
 */
public class RemoveIntersectingSegmentTests
{

    /*
     * Design Notes:
     * 
     * The intersecting quads don't test realistic scenarios.  But
     * they are useful for testing basic operation of the algorithm
     * before the more complex realistic scenarios.
     */
    
    private static final int REGION_A = 1;
    private static final int REGION_B = 2;
    private static final int REGION_C = 3;
    private static final int REGION_S = 52;
    
    private final ArrayList<Integer> mBaseQuad = new ArrayList<Integer>(16);
    
    private final ArrayList<Integer> mIntersectionQuadA =
        new ArrayList<Integer>(12);
    
    private final ArrayList<Integer> mIntersectionQuadB = 
        new ArrayList<Integer>(12);
    
    @Before
    public void setup()
    {
        
        /*
         * 
         *     1 - 2
         *     |   |
         *     0 - 3
         */
        
        mBaseQuad.add(1);
        mBaseQuad.add(5);
        mBaseQuad.add(2);
        mBaseQuad.add(0);
        
        mBaseQuad.add(1);
        mBaseQuad.add(5);
        mBaseQuad.add(3);
        mBaseQuad.add(0);
        
        mBaseQuad.add(2);
        mBaseQuad.add(5);
        mBaseQuad.add(3);
        mBaseQuad.add(0);
        
        mBaseQuad.add(2);
        mBaseQuad.add(5);
        mBaseQuad.add(2);
        mBaseQuad.add(0);
        
        /*
         * 
         *     3   1
         *     | x |
         *     0   2
         */
        
        mIntersectionQuadA.add(1);
        mIntersectionQuadA.add(5);
        mIntersectionQuadA.add(2);
        mIntersectionQuadA.add(0);
        
        mIntersectionQuadA.add(2);
        mIntersectionQuadA.add(5);
        mIntersectionQuadA.add(3);
        mIntersectionQuadA.add(0);
        
        mIntersectionQuadA.add(2);
        mIntersectionQuadA.add(5);
        mIntersectionQuadA.add(2);
        mIntersectionQuadA.add(0);
        
        mIntersectionQuadA.add(1);
        mIntersectionQuadA.add(5);
        mIntersectionQuadA.add(3);
        mIntersectionQuadA.add(0);
        
        /*
         * 
         *     1   3
         *     | x |
         *     0   2
         */
        
        mIntersectionQuadB.add(1);
        mIntersectionQuadB.add(5);
        mIntersectionQuadB.add(2);
        mIntersectionQuadB.add(0);
        
        mIntersectionQuadB.add(1);
        mIntersectionQuadB.add(5);
        mIntersectionQuadB.add(3);
        mIntersectionQuadB.add(0);
        
        mIntersectionQuadB.add(3);
        mIntersectionQuadB.add(5);
        mIntersectionQuadB.add(2);
        mIntersectionQuadB.add(0);
        
        mIntersectionQuadB.add(3);
        mIntersectionQuadB.add(5);
        mIntersectionQuadB.add(3);
        mIntersectionQuadB.add(0);
        
    }
    
    @Test
    public void testBasicQuadOffset0()
    {
        // No change expected.
        mBaseQuad.set(1*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mBaseQuad);
        assertTrue(mBaseQuad.size() == 16); 
    }
    
    @Test
    public void testBasicQuadOffset1()
    {
        // No change expected.
        mBaseQuad.set(2*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mBaseQuad);
        assertTrue(mBaseQuad.size() == 16); 
    }
    
    @Test
    public void testBasicQuadOffset2()
    {
        // No change expected.
        mBaseQuad.set(3*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mBaseQuad);
        assertTrue(mBaseQuad.size() == 16); 
    }
    
    @Test
    public void testBasicQuadOffset3()
    {
        // No change expected.
        mBaseQuad.set(0*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mBaseQuad);
        assertTrue(mBaseQuad.size() == 16); 
    }
    
    @Test
    public void testDisallowedRemovalAll()
    {
        // No change expected.
        
        // Make both of the the intersecting segments
        // a region portal.
        mIntersectionQuadA.set(1*4+3, REGION_A);
        mIntersectionQuadA.set(3*4+3, REGION_B);
        ContourSetBuilder.removeIntersectingSegments(REGION_S
                , mIntersectionQuadA);
        assertTrue(mIntersectionQuadA.size() == 16);
  
    }
    
    @Test
    public void testDisallowedRemovalIndirect()
    {
        // No change expected.
        
        // The null edge can't be removed because
        // one of its vertices is on a portal edge.
        mIntersectionQuadA.set(1*4+3, REGION_A); // <- Tested edge.
        mIntersectionQuadA.set(0*4+3, REGION_B); // <- Want's to remove. Can't
        ContourSetBuilder.removeIntersectingSegments(REGION_S
                , mIntersectionQuadA);
        assertTrue(mIntersectionQuadA.size() == 16);
    }
    
    @Test
    public void testIntersectingQuadAOffset0()
    {

        mIntersectionQuadA.set(1*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mIntersectionQuadA);
        assertTrue(mIntersectionQuadA.size() == 12);
        
        assertTrue(mIntersectionQuadA.get(0) == 1);
        assertTrue(mIntersectionQuadA.get(2) == 2);
        
        assertTrue(mIntersectionQuadA.get(4) == 2);
        assertTrue(mIntersectionQuadA.get(6) == 3);

        assertTrue(mIntersectionQuadA.get(8) == 2);
        assertTrue(mIntersectionQuadA.get(10) == 2);
    }
    
    @Test
    public void testIntersectingQuadAOffset1()
    {
        // Expect no change to the quad.
        mIntersectionQuadA.set(2*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mIntersectionQuadA);
        assertTrue(mIntersectionQuadA.size() == 16);
    }
    
    @Test
    public void testIntersectingQuadAOffset2()
    {
        mIntersectionQuadA.set(3*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mIntersectionQuadA);
        
        assertTrue(mIntersectionQuadA.size() == 12);
        
        assertTrue(mIntersectionQuadA.get(0) == 1);
        assertTrue(mIntersectionQuadA.get(2) == 2);
        
        assertTrue(mIntersectionQuadA.get(4) == 2);
        assertTrue(mIntersectionQuadA.get(6) == 2);

        assertTrue(mIntersectionQuadA.get(8) == 1);
        assertTrue(mIntersectionQuadA.get(10) == 3);
    }
    
    @Test
    public void testIntersectingQuadAOffset3()
    {
        // Expect no change to the quad.
        mIntersectionQuadA.set(0*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mIntersectionQuadA);
        assertTrue(mIntersectionQuadA.size() == 16);
    }
    
    @Test
    public void testIntersectingQuadBOffset0()
    {
        // Expect no change to the quad.
        mIntersectionQuadB.set(1*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mIntersectionQuadB);
        assertTrue(mIntersectionQuadB.size() == 16);
    }
    
    @Test
    public void testIntersectingQuadBOffset1()
    {
        mIntersectionQuadB.set(2*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mIntersectionQuadB);
        assertTrue(mIntersectionQuadB.size() == 12);
        
        assertTrue(mIntersectionQuadB.get(0) == 1);
        assertTrue(mIntersectionQuadB.get(2) == 3);
        
        assertTrue(mIntersectionQuadB.get(4) == 3);
        assertTrue(mIntersectionQuadB.get(6) == 2);

        assertTrue(mIntersectionQuadB.get(8) == 3);
        assertTrue(mIntersectionQuadB.get(10) == 3);
    }
    
    @Test
    public void testIntersectingQuadBOffset2()
    {
        // Expect no change to the quad.
        mIntersectionQuadB.set(3*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mIntersectionQuadB);
        assertTrue(mIntersectionQuadB.size() == 16);
    }
    
    @Test
    public void testIntersectingQuadBOffset3()
    {
        mIntersectionQuadB.set(0*4+3, REGION_A);
        ContourSetBuilder.removeIntersectingSegments(REGION_S, mIntersectionQuadB);
        assertTrue(mIntersectionQuadB.size() == 12);
        
        assertTrue(mIntersectionQuadB.get(0) == 1);
        assertTrue(mIntersectionQuadB.get(2) == 2);
        
        assertTrue(mIntersectionQuadB.get(4) == 1);
        assertTrue(mIntersectionQuadB.get(6) == 3);

        assertTrue(mIntersectionQuadB.get(8) == 3);
        assertTrue(mIntersectionQuadB.get(10) == 3);
    }
    
    @Test
    public void testMultiIntersectA()
    {
     
        ArrayList<Integer> sourceContour = getMultiIntersectA();
        
        for (int sourceOffset = 0; sourceOffset < 10; sourceOffset++)
        {
            ArrayList<Integer> contour = new ArrayList<Integer>();
            contour.addAll(sourceContour);
            
            ContourSetBuilder.removeIntersectingSegments(REGION_S, contour);
            
            validateMAIAdjustment(sourceOffset, contour);
            
            ContourUtil.shiftContour(sourceContour);
        }
    }
    
    /**
     * Returns the vertex index to use for a MultiIntersectA
     * contour after its invalid vertices have been removed, 
     * taking into account various adjustments.
     * @param vertIndex The index to adjust.
     * @param sourceOffset The number of shifts performed
     * on the source contour.
     * @return The index of the vertex in the new contour.
     */
    private int getMIAAdjustedIndex(int vertIndex
            , int sourceOffset)
    {
        
        final int sourceVertCount = 10;
        
        int iVert = (vertIndex + sourceOffset) % sourceVertCount;
        int iBadVert01 = (3 + sourceOffset) % sourceVertCount;
        int iBadVert02 = (4 + sourceOffset) % sourceVertCount;
        int iBadVert03 = (5 + sourceOffset) % sourceVertCount;
        
        int offset = 0;
        if (iBadVert01 < iVert)
            offset--;
        if (iBadVert02 < iVert)
            offset--;
        if (iBadVert03 < iVert)
            offset--;
        
        return (iVert + offset);
    }
    
    private ArrayList<Integer> getMultiIntersectA()
    {
        // When testing against segment 0 -> 1, expect
        // vertices 3, 4, and 5 to be removed.
        
        ArrayList<Integer> contour = new ArrayList<Integer>(10*4);
        
        contour.add(2);
        contour.add(5);
        contour.add(1);
        contour.add(REGION_B);

        contour.add(2);
        contour.add(5);
        contour.add(10);
        contour.add(REGION_A);

        contour.add(3);
        contour.add(5);
        contour.add(9);
        contour.add(0);

        contour.add(2);
        contour.add(5);
        contour.add(8);
        contour.add(0);

        contour.add(1);
        contour.add(5);
        contour.add(8);
        contour.add(0);

        contour.add(1);
        contour.add(5);
        contour.add(7);
        contour.add(0);

        contour.add(4);
        contour.add(5);
        contour.add(7);
        contour.add(0);

        contour.add(6);
        contour.add(5);
        contour.add(5);
        contour.add(REGION_C);

        contour.add(4);
        contour.add(5);
        contour.add(5);
        contour.add(0);

        contour.add(4);
        contour.add(5);
        contour.add(3);
        contour.add(0);
        
        return contour;
    }
    
    /**
     * Checks to make sure a MultiIntersectA contour has been properly
     * cleaned up.
     * @param sourceOffset The number of shifts performed
     * on the source contour.
     * @param resultContour The contour after invalid vertices have been
     * removed.
     * @param startVertIndex The index of the start vertex with the source 
     * offset already taken into account.
     * @param endVertIndex The index of the end vertex with the source
     * offset already taken into account.
     * @param offset The offset returned by the contour cleanup operation.
     */
    private void validateMAIAdjustment(int sourceOffset
            , ArrayList<Integer> resultContour)
    {
        
        final int sourceVertCount = 10;
        
        // Expect three vertices were removed.
        int expectedVertCount = sourceVertCount - 3;
        assertTrue("Loop: " + sourceOffset
                , resultContour.size() == expectedVertCount * 4);

        int iVert =  getMIAAdjustedIndex(0, sourceOffset);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+0) == 2);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+2) == 1);

        iVert =  getMIAAdjustedIndex(1, sourceOffset);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+0) == 2);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+2) == 10);

        iVert = getMIAAdjustedIndex(2, sourceOffset);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+0) == 3);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+2) == 9);

        // Removed vertices: 3, 4, 5

        iVert = getMIAAdjustedIndex(6, sourceOffset);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+0) == 4);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+2) == 7);

        iVert = getMIAAdjustedIndex(7, sourceOffset);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+0) == 6);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+2) == 5);

        iVert = getMIAAdjustedIndex(8, sourceOffset);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+0) == 4);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+2) == 5);

        iVert = getMIAAdjustedIndex(9, sourceOffset);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+0) == 4);
        assertTrue("Loop: " + sourceOffset
                , resultContour.get(iVert*4+2) == 3);
    }
    
}
