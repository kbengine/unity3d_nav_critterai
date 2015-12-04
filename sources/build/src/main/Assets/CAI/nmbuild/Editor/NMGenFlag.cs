/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Flags for common build options.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most builds have all flags set except <see cref="LedgeSpansNotWalkable"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="ProcessorSet.CreateStandard"/>
    /// <seealso cref="ProcessorSet.GetStandard"/>
    [System.Flags]
    public enum NMGenBuildFlag
    {
        /*
         * Design notes:
         * 
         * Keep the base type of this enum an integer in order to remain 
         * compatible with Unity serialization.
         * 
         */

        /// <summary>
        /// Include <see cref="org.critterai.nmgen.Heightfield.MarkLedgeSpansNotWalkable"/> 
        /// in the build.
        /// </summary>
        LedgeSpansNotWalkable = 0x010,

        /// <summary>
        /// Include <see cref="org.critterai.nmgen.Heightfield.MarkLowHeightSpansNotWalkable"/> 
        /// in the build.
        /// </summary>
        LowHeightSpansNotWalkable = 0x020,

        /// <summary>
        /// Include <see cref="org.critterai.nmgen.Heightfield.MarkLowObstaclesWalkable"/> 
        /// in the build.
        /// </summary>
        LowObstaclesWalkable = 0x040,

        /// <summary>
        /// Apply the 0x01 flag to all polygons in the <see cref="org.critterai.nmgen.PolyMesh"/> 
        /// object.
        /// </summary>
        ApplyPolyFlags = 0x080,

        /// <summary>
        /// Generate tile bounding volumns.
        /// </summary>
        BVTreeEnabled = 0x100
    }
}
