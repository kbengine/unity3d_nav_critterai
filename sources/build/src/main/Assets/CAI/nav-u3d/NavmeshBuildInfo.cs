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

namespace org.critterai.nav.u3d
{
    /// <summary>
    /// Represents the core configuration values used during the build of a <see cref="Navmesh"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This information is used when doing a partial rebuild of the mesh.
    /// </para>
    /// <para>
    /// This is a class with public fields in order to support Unity serialization.
    /// </para>
    /// </remarks>
    [System.Serializable]
    public sealed class NavmeshBuildInfo
    {
        /// <summary>
        /// Tile size.
        /// </summary>
        public int tileSize;

        /// <summary>
        /// XZ-plane cell size.
        /// </summary>
        public float xzCellSize;

        /// <summary>
        /// Y-axis cell size.
        /// </summary>
        public float yCellSize;

        /// <summary>
        /// Walkable height.
        /// </summary>
        public int walkableHeight;

        /// <summary>
        /// Walkable step.
        /// </summary>
        public int walkableStep;

        /// <summary>
        /// Walkable radius.
        /// </summary>
        public int walkableRadius;

        /// <summary>
        /// Border size.
        /// </summary>
        public int borderSize;

        /// <summary>
        /// The full path to the scene used to build the mesh.
        /// </summary>
        public string inputScene;

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public NavmeshBuildInfo Clone()
        {
            NavmeshBuildInfo result = new NavmeshBuildInfo();
            result.tileSize = tileSize;
            result.borderSize = borderSize;
            result.walkableHeight = walkableHeight;
            result.walkableRadius = walkableRadius;
            result.walkableStep = walkableStep;
            result.xzCellSize = xzCellSize;
            result.yCellSize = yCellSize;
            result.inputScene = inputScene;
            return result;
        }
    }
}
