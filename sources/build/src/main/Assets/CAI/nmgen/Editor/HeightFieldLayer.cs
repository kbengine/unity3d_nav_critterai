/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using org.critterai.interop;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents a layer within a <see cref="HeightfieldLayerSet"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Undocumented: Equivalent to Recast: rcHeightfieldLayer.
    /// </para>
    /// <para>
    /// Instances of this class can only be obtained from a <see cref="HeightfieldLayerSet"/>.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class HeightfieldLayer
        : IManagedObject
    {

        // Field layout: rcHeightfieldLayer

        private Vector3 mBoundsMin;
        private Vector3 mBoundsMax;

        private float mXZCellSize;
        private float mYCellSize;

        private int mWidth;
        private int mDepth;
        private int mXMin;   // Bounding box of usable data.
        private int mXMax;
        private int mYMin;
        private int mYMax;
        private int mHeightMin;
        private int mHeightMax;
	
        // Height min/max
	    private IntPtr mHeights;			// byte[depth*width]
        private IntPtr mAreas;				// byte[depth*width]
        private IntPtr mCons;	            // byte[depth*width]

        /// <summary>
        /// The width of the layer. (Along the x-axis in cell units.)
        /// </summary>
        public int Width { get { return mWidth; } }

        /// <summary>
        /// The depth of the layer. (Along the z-axis in cell units.)
        /// </summary>
        public int Depth { get { return mDepth; } }

        /// <summary>
        /// The minimum bounds of the layer in world space.
        /// </summary>
        /// <returns>The minimum bounds of the layer.</returns>
        public Vector3 BoundsMin { get { return mBoundsMin; } }

        /// <summary>
        /// The maximum bounds of the layer in world space.
        /// </summary>
        /// <returns>The maximum bounds of the layer.</returns>
        public Vector3 BoundsMax { get { return mBoundsMax; } }

        /// <summary>
        /// The width/depth increment of each cell. (On the xz-plane.)
        /// </summary>
        public float XZCellSize { get { return mXZCellSize; } }

        /// <summary>
        /// The height increment of each cell. (On the y-axis.)
        /// </summary>
        public float YCellSize { get { return mYCellSize; } }

        /// <summary>
        /// The height maximum of the usable data.
        /// </summary>
        public int HeightMin { get { return mHeightMin; } }

        /// <summary>
        /// The hight minimum of the usable data.
        /// </summary>
        public int HeightMax { get { return mHeightMax; } }

        /// <summary>
        /// The x-minumum of the usuable data.
        /// </summary>
        public int XMin { get { return mXMin; } }

        /// <summary>
        /// The x-maximum of teh usable data.
        /// </summary>
        public int XMax { get { return mXMax; } }

        /// <summary>
        /// The z-minimum of the usable data.
        /// </summary>
        public int ZMin { get { return mYMin; } }

        /// <summary>
        /// The z-maximum of the usable data.
        /// </summary>
        public int ZMax { get { return mYMax; } }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mHeights == IntPtr.Zero); } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType
        {
            get { return AllocType.ExternallyManaged; }
        }

        internal HeightfieldLayer() { }

        internal void Reset()
        {
            mBoundsMin = Vector3Util.Zero;
            mBoundsMax = Vector3Util.Zero;
            mXZCellSize = 0;
            mYCellSize = 0;
            mWidth = 0;
            mDepth = 0;
            mXMin = 0;
            mXMax = 0;
            mYMin = 0;
            mYMax = 0;
            mHeightMin = 0;
            mHeightMax = 0;

            mHeights = IntPtr.Zero;
            mAreas = IntPtr.Zero;
            mCons = IntPtr.Zero;
        }

        /// <summary>
        /// Has no effect on the object. (The object owner will handle disposal.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <see cref="HeightfieldLayerSet"/> always owns and manages objects of this type.
        /// </para>
        /// </remarks>
        public void RequestDisposal()
        {
            // Always externally managed.  So don't do anything.
        }


        /// <summary>
        /// Loads the height data into the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the data into. [Size: >= Width * Depth]</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool GetHeightData(byte[] buffer)
        {
            if (IsDisposed || buffer.Length < mWidth * mDepth)
                return false;

            Marshal.Copy(mHeights, buffer, 0, mWidth * mDepth);

            return true;
        }

        /// <summary>
        /// Loads the area data into the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the data into. [Size: >= Width * Depth]</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool GetAreaData(byte[] buffer)
        {
            if (IsDisposed || buffer.Length < mWidth * mDepth)
                return false;

            Marshal.Copy(mAreas, buffer, 0, mWidth * mDepth);

            return true;
        }

        /// <summary>
        /// Loads the connection data into the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the data into. [Size: >= Width * Depth]</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool GetConnectionData(byte[] buffer)
        {
            if (IsDisposed || buffer.Length < mWidth * mDepth)
                return false;

            Marshal.Copy(mCons, buffer, 0, mWidth * mDepth);

            return true;
        }
    }
}
