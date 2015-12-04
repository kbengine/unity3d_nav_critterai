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
using Math = System.Math;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Provides utilities related to navigation.
    /// </summary>
    public static class NavUtil
    {
        /// <summary>
        /// Returns true if the status includes the success flag.
        /// </summary>
        /// <param name="status">The status to check.</param>
        /// <returns>True if the status includes the success flag.</returns>
        public static bool Succeeded(NavStatus status)
        {
            return (status & NavStatus.Sucess) != 0;
        }

        /// <summary>
        /// Returns true if the status includes the failure flag.
        /// </summary>
        /// <param name="status">The status to check.</param>
        /// <returns>True if the status includes the failure flag.</returns>
        public static bool Failed(NavStatus status)
        {
            return (status & NavStatus.Failure) != 0;
        }

        /// <summary>
        /// Returns true if the status includes the in-progress flag.
        /// </summary>
        /// <param name="status">The status to check.</param>
        /// <returns>True if the status includes the in-progress flag.
        /// </returns>
        public static bool IsInProgress(NavStatus status)
        {
            return (status & NavStatus.InProgress) != 0;
        }

        /// <summary>
        /// Clamps the value to the valid area range. 
        /// (0 &lt;= value &lt;= <see cref="Navmesh.MaxArea"/>)
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>A valid area.</returns>
        public static byte ClampArea(byte value)
        {
            return Math.Min(Navmesh.MaxArea, value);
        }

        /// <summary>
        /// Clamps the value to the valid area range. 
        /// (0 &lt;= value &lt;= <see cref="Navmesh.MaxArea"/>)
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>A valid area.</returns>
        public static byte ClampArea(int value)
        {
            return (byte)Math.Min(Navmesh.MaxArea, Math.Max(0, value));
        }

        /// <summary>
        /// Derives the <see cref="NavmeshParams"/> for a tile.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is useful for getting the configuration required to build a single-tile
        /// navigation mesh for the tile.
        /// </para>
        /// </remarks>
        /// <param name="tile">The tile.</param>
        /// <returns>The <see cref="NavmeshParams"/> for the tile.</returns>
        public static NavmeshParams DeriveConfig(NavmeshTileData tile)
        {
            NavmeshTileHeader header = tile.GetHeader();

            return new NavmeshParams(header.boundsMin
                    , header.boundsMax.x - header.boundsMin.x
                    , header.boundsMax.z - header.boundsMin.z
                    , 1  // Max tiles.
                    , header.polyCount);
        }

        /// <summary>
        /// Tests that vector interop behaves as expected.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the test is successful the input and return vectors will be equal in value.
        /// </para>
        /// <para>
        /// This method is used to validate that builds using custom vectors behaves correctly
        /// with interop.  (I.e. The custom vector is data compatible.)
        /// </para>
        /// </remarks>
        /// <param name="v">The input vector.</param>
        /// <returns>
        /// A vector equal to the input vector if interop is functioning as expected.
        /// </returns>
        public static Vector3 TestVector(Vector3 v)
        {
            Vector3 result = new Vector3();
            rcn.InteropUtil.dtvlVectorTest(ref v, ref result);
            return result;
        }

        /// <summary>
        /// Tests that vector interop behaves as expected for vector arrays.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the test is successful the input and result arrays will be equal in length and
        /// element values.  (E.g. vectors[i] == result[i])
        /// </para>
        /// <para>
        /// This method is used to validate that builds using custom vectors behaves correctly
        /// with interop.  (I.e. The custom vector is data compatible.)
        /// </para>
        /// </remarks>
        /// <param name="vectors">
        /// The input vector array. [Length: >= <paramref name="vectorCount"/>]
        /// </param>
        /// <param name="vectorCount">The number of vectors in the array.</param>
        /// <param name="result">
        /// The array to load the result into. [Length: >= <paramref name="vectorCount"/>]
        /// </param>
        public static void TestVectorArray(Vector3[] vectors, int vectorCount, Vector3[] result)
        {
            rcn.InteropUtil.dtvlVectorArrayTest(vectors, vectorCount, result);
        }
    }
}
