package org.critterai.nmgen;

import static org.junit.Assert.*;

import org.junit.Before;
import org.junit.Test;

/**
 * Tests related to the OpenHeightSpan class.
 */
public class OpenHeightSpanTests
{
    
    private static final int ROOT_REGION = 1;
    private static final int AXIS_NEGIHBOR0_REGION = 100;
    private static final int AXIS_NEGIHBOR1_REGION = 101;
    private static final int AXIS_NEGIHBOR2_REGION = 102;
    private static final int AXIS_NEGIHBOR3_REGION = 103;
    private static final int DIAG_NEGIHBOR4_REGION = 104;
    private static final int DIAG_NEGIHBOR5_REGION = 105;
    private static final int DIAG_NEGIHBOR6_REGION = 106;
    private static final int DIAG_NEGIHBOR7_REGION = 107;
    
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;    
    private static final int UNUSED_REGION = 9972;
    
    private final OpenHeightSpan mRootSpan = new OpenHeightSpan(4, 9);
    private final OpenHeightSpan mAxisNeighbor0 = new OpenHeightSpan(5, 10);
    private final OpenHeightSpan mAxisNeighbor1 = new OpenHeightSpan(5, 11);
    private final OpenHeightSpan mAxisNeighbor2 = new OpenHeightSpan(5, 12);
    private final OpenHeightSpan mAxisNeighbor3 = new OpenHeightSpan(5, 13);
    private final OpenHeightSpan mDiagNeighbor4 = new OpenHeightSpan(6, 14);
    private final OpenHeightSpan mDiagNeighbor5 = new OpenHeightSpan(6, 15);
    private final OpenHeightSpan mDiagNeighbor6 = new OpenHeightSpan(6, 16);
    private final OpenHeightSpan mDiagNeighbor7 = new OpenHeightSpan(6, 17);
    
    private final int[] mMap = new int[10];
    
    @Before
    public void setUp() throws Exception
    {
        mRootSpan.setRegionID(ROOT_REGION);
        mAxisNeighbor0.setRegionID(AXIS_NEGIHBOR0_REGION);
        mAxisNeighbor1.setRegionID(AXIS_NEGIHBOR1_REGION);
        mAxisNeighbor2.setRegionID(AXIS_NEGIHBOR2_REGION);
        mAxisNeighbor3.setRegionID(AXIS_NEGIHBOR3_REGION);
        mDiagNeighbor4.setRegionID(DIAG_NEGIHBOR4_REGION);
        mDiagNeighbor5.setRegionID(DIAG_NEGIHBOR5_REGION);
        mDiagNeighbor6.setRegionID(DIAG_NEGIHBOR6_REGION);
        mDiagNeighbor7.setRegionID(DIAG_NEGIHBOR7_REGION);
        
       mRootSpan.setNeighbor(0, mAxisNeighbor0);
       mRootSpan.setNeighbor(1, mAxisNeighbor1);
       mRootSpan.setNeighbor(2, mAxisNeighbor2);
       mRootSpan.setNeighbor(3, mAxisNeighbor3);
       
       mAxisNeighbor0.setNeighbor(1, mDiagNeighbor4);
       mAxisNeighbor0.setNeighbor(2, mRootSpan);
       mAxisNeighbor0.setNeighbor(3, mDiagNeighbor7);
        
       mAxisNeighbor1.setNeighbor(0, mDiagNeighbor4);
       mAxisNeighbor1.setNeighbor(2, mDiagNeighbor5);
       mAxisNeighbor1.setNeighbor(3, mRootSpan);
       
       mAxisNeighbor2.setNeighbor(0, mRootSpan);
       mAxisNeighbor2.setNeighbor(1, mDiagNeighbor5);
       mAxisNeighbor2.setNeighbor(3, mDiagNeighbor6);
       
       mAxisNeighbor3.setNeighbor(0, mDiagNeighbor7);
       mAxisNeighbor3.setNeighbor(1, mRootSpan);
       mAxisNeighbor3.setNeighbor(2, mDiagNeighbor6);
       
       mDiagNeighbor4.setNeighbor(2, mAxisNeighbor1);
       mDiagNeighbor4.setNeighbor(3, mAxisNeighbor0);
       
       mDiagNeighbor5.setNeighbor(1, mAxisNeighbor1);
       mDiagNeighbor5.setNeighbor(3, mAxisNeighbor2);

       mDiagNeighbor6.setNeighbor(0, mAxisNeighbor3);
       mDiagNeighbor6.setNeighbor(2, mAxisNeighbor2);

       mDiagNeighbor7.setNeighbor(1, mAxisNeighbor0);
       mDiagNeighbor7.setNeighbor(2, mAxisNeighbor3);
       
       // Corrupt the map.
       for (int i = 0; i < 10; i++)
           mMap[i] = UNUSED_REGION;      
       
    }
 
    @Test
    public void testGetDetailedRegionMapBase()
    {
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachN0()
    {
        mRootSpan.setNeighbor(0, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == NULL_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachN1()
    {
        mRootSpan.setNeighbor(1, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == NULL_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachN2()
    {
        mRootSpan.setNeighbor(2, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == NULL_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachN3()
    {
        mRootSpan.setNeighbor(3, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == NULL_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachN4()
    {
        mAxisNeighbor0.setNeighbor(1, null);
        mAxisNeighbor1.setNeighbor(0, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == NULL_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachN5()
    {
        mAxisNeighbor1.setNeighbor(2, null);
        mAxisNeighbor2.setNeighbor(1, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == NULL_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachN6()
    {
        mAxisNeighbor2.setNeighbor(3, null);
        mAxisNeighbor3.setNeighbor(2, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == NULL_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachN7()
    {
        mAxisNeighbor3.setNeighbor(0, null);
        mAxisNeighbor0.setNeighbor(3, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == NULL_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }
    
    @Test
    public void testGetDetailedRegionMapDetachPartial()
    {
        mAxisNeighbor0.setNeighbor(1, null);
        mAxisNeighbor0.setNeighbor(3, null);
        mRootSpan.getDetailedRegionMap(mMap, 1);

        assertTrue(mMap[1] == AXIS_NEGIHBOR0_REGION);
        assertTrue(mMap[2] == AXIS_NEGIHBOR1_REGION);
        assertTrue(mMap[3] == AXIS_NEGIHBOR2_REGION);
        assertTrue(mMap[4] == AXIS_NEGIHBOR3_REGION);
        assertTrue(mMap[5] == DIAG_NEGIHBOR4_REGION);
        assertTrue(mMap[6] == DIAG_NEGIHBOR5_REGION);
        assertTrue(mMap[7] == DIAG_NEGIHBOR6_REGION);
        assertTrue(mMap[8] == DIAG_NEGIHBOR7_REGION);
        
        assertTrue(mMap[0] == UNUSED_REGION);
        assertTrue(mMap[9] == UNUSED_REGION);
    }

}
