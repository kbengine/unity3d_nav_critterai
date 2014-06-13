package org.critterai.nmgen;

/**
 * Utilities useful for tests involving the open height field.
 */
public final class OpenHeightFieldUtil
{
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;
     
    private OpenHeightFieldUtil() { }
    
    /**
     * Validates that the base spans in both regions are all in
     * the same layout and region.
     */
    public static boolean isSameRegionLayout(OpenHeightfield fieldA
            , OpenHeightfield fieldB)
    {
        if (fieldA.regionCount() != fieldB.regionCount())
            return false;
        if (fieldA.width() != fieldB.width()
                || fieldA.depth() != fieldB.depth())
            return false;
        
        for (int w = 0; w < fieldA.width(); w++)
        {
            for (int d = 0; d < fieldA.depth(); d++)
            {
                OpenHeightSpan spanA = fieldA.getData(w, d);
                OpenHeightSpan spanB = fieldB.getData(w, d);
                if (spanA == null && spanB != null
                        || spanB == null && spanA != null)
                    return false;
                if (spanA != null && spanA.regionID() != spanB.regionID())
                    return false;
            }
        }
        return true;
    }
    
    /**
     * Performs neighbor links of all base spans within a field. 
     * No checks are performed on validity neighbors. 
     */
    public static void linkAllBaseSpans(OpenHeightfield field)
    {
        for (int w = 0; w < field.width(); w++)
        {
            for (int d = 0; d < field.depth(); d++)
            {
                OpenHeightSpan span = field.getData(w, d);
                if (span == null)
                    continue;
                for (int dir = 0; dir < 4; dir++)
                {
                    int woff = w + OpenHeightfield.getDirOffsetWidth(dir);
                    int doff = d + OpenHeightfield.getDirOffsetDepth(dir);
                    if (woff < 0 || woff >= field.width()
                            || doff < 0 || doff >= field.depth())
                        continue;
                    OpenHeightSpan nSpan = field.getData(woff, doff);
                    if (nSpan != null)
                        span.setNeighbor(dir, nSpan);
                }
            }
        }
    }
    
    /**
     * A square single level patch with a null region fully encompassed 
     * by a single region. (RegionID = 1)
     * Only the region information and neighbor links are implemented.
     * 
     *              W
     *        0 1 2 3 4 5
     *        -----------
     *    5 | a a a a a a
     *    4 | a a a a a a
     *    3 | a a x x a a    x - null region span
     *  D 2 | a a x x a a    a - region 1 span
     *    1 | a a a a a a    All linked.
     *    0 | a a a a a a
     * 
     */
    public static OpenHeightfield getEncompassedNullRegionPatch()
    {
        
        float[] gridBoundsMin = { 0, 0, 0 };
        float[] gridBoundsMax = { 10, 10, 10 };
        
        OpenHeightfield field = new OpenHeightfield(gridBoundsMin
                , gridBoundsMax
                , 1
                , 1);
        
        for (int w = 0; w < 6; w++)
        {
            for (int d = 0; d < 6; d++)
            {
                OpenHeightSpan span = new OpenHeightSpan(w, d + 1);
                span.setRegionID(1);
                field.addData(w, d, span);
            }
        }
        linkAllBaseSpans(field);
        
        field.getData(2, 2).setRegionID(NULL_REGION);
        field.getData(2, 3).setRegionID(NULL_REGION);
        field.getData(3, 2).setRegionID(NULL_REGION);
        field.getData(3, 3).setRegionID(NULL_REGION);
        
        field.setRegionCount(2);
        
        return field;
    }
    
}
