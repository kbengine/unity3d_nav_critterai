package org.critterai.nmgen;

import java.util.ArrayList;

/**
 * Provides utilities useful for Contour tests.
 */
public final class ContourUtil
{
    private ContourUtil() { }
    
    /**
     * Shifts the vertices one position higher in the list and
     * inserts the last vertex into the first position.
     * @param list The contour to shift. (Expecting a stride of 4.)
     */
    public static void shiftContour(ArrayList<Integer> list)
    {
        int size = list.size();
        
        int entry0 = list.get(size-4);
        int entry1 = list.get(size-3);
        int entry2 = list.get(size-2);
        int entry3 = list.get(size-1);
        
        for (int p = size - 5; p >= 0; p--)
        {
            list.set(p+4, list.get(p));
        }
        
        list.set(0, entry0);
        list.set(1, entry1);
        list.set(2, entry2);
        list.set(3, entry3);
        
    }
    
}
