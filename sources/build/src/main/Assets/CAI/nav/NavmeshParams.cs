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
using System;
using System.Runtime.InteropServices;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Navigation mesh configuration parameters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implemented as a class with public fields in order to support Unity serialization.  
    /// Care must be taken not to set the fields to invalid values.
    /// </para>
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NavmeshParams
    {
        /// <summary>
        /// The minimumn allowed tile size.
        /// </summary>
        public const float MinTileSize = 0.1f;

        /// <summary>
        /// The world-space origin of the navigation mesh.
        /// </summary>
        public Vector3 origin;

        /// <summary>
        /// The width of each tile. (Along the x-axis.)
        /// </summary>
        public float tileWidth = 0;

        /// <summary>
        /// The depth of each tile. (Along the z-axis.)
        /// </summary>
        public float tileDepth = 0;	

        /// <summary>
        /// The maximum number of tiles the navigation mesh can contain.
        /// </summary>
        public int maxTiles = 0;

        /// <summary>
        /// The maximum number of polygons each tile can contain.
        /// </summary>
        public int maxPolysPerTile = 0;

        /// <summary>
        /// Constructs and initializes the structure.
        /// </summary>
        /// <param name="origin">The tile space origin.</param>
        /// <param name="tileWidth">The width of each tile. (Along the x-axis.)</param>
        /// <param name="tileDepth">The depth of each tile. (Along the z-axis.)</param>
        /// <param name="maxTiles">
        /// The maximum number of tiles the navigation mesh can contain.
        /// </param>
        /// <param name="maxPolysPerTile"> 
        /// The maximum number of polygons each tile can contain.
        /// </param>
        public NavmeshParams(Vector3 origin
            , float tileWidth, float tileDepth
            , int maxTiles, int maxPolysPerTile)
        {
            this.origin = origin;
            this.tileWidth = Math.Max(MinTileSize, tileWidth);
            this.tileDepth = Math.Max(MinTileSize, tileDepth);
            this.maxTiles = Math.Max(1, maxTiles);
            this.maxPolysPerTile = Math.Max(1, maxPolysPerTile);
        }

        internal NavmeshParams() { }

        /// <summary>
        /// A clone of the parameters.
        /// </summary>
        /// <returns>A clone of the parameters.</returns>
        public NavmeshParams Clone()
        {
            return new NavmeshParams(origin, tileWidth, tileDepth, maxTiles, maxPolysPerTile);
        }
    }
}
