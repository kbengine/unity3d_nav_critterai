/*
 * Copyright (c) 2012 Stephen A. Pratt
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
    /// A contiguous range of tiles, defined by tile grid indices.
    /// </summary>
	public struct TileZone
	{
        /// <summary>
        /// The minimum x-index of the zone.
        /// </summary>
        public int xmin;
        
        /// <summary>
        /// The mimimum z-index of the zone.
        /// </summary>
        public int zmin;

        /// <summary>
        /// The maximum x-index of the zone.
        /// </summary>
        public int xmax;

        /// <summary>
        /// The maximum z-index of the zone.
        /// </summary>
        public int zmax;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="xmin">The minimum x-index of the zone.</param>
        /// <param name="zmin">The mimimum z-index of the zone.</param>
        /// <param name="xmax">The maximum x-index of the zone.</param>
        /// <param name="zmax">The maximum z-index of the zone.</param>
        public TileZone(int xmin, int zmin, int xmax, int zmax)
        {
            this.xmin = xmin;
            this.zmin = zmin;
            this.xmax = xmax;
            this.zmax = zmax;
        }

        /// <summary>
        /// The number of tiles along the x-axis.
        /// </summary>
        public int Width { get { return (xmax - xmin) + 1; } }

        /// <summary>
        /// The number of tiles along the z-axis.
        /// </summary>
        public int Depth { get { return (zmax - zmin) + 1; } }
	}
}
