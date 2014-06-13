package org.critterai.nmgen;

import org.junit.runner.RunWith;
import org.junit.runners.Suite;
import org.junit.runners.Suite.SuiteClasses;

/**
 * Unit tests for all classes in the org.critterai.nav package.
 */
@RunWith(Suite.class)
@SuiteClasses( {GeometryTests.class
    , OpenHeightSpanTests.class
    , EncompassedNullRegionTests.class
    , NullRegionOuterCornerTipTests.class
    , NullRegionShortWrapTests.class
    , RemoveIntersectingSegmentTests.class
    , RemoveVerticalSegmentTests.class} )
public final class AllTests { }
