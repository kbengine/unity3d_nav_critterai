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

namespace org.critterai.interop
{
    /// <summary>
    /// Provides various interop related utility methods.
    /// </summary>
    public static class UtilEx
    {
        /// <summary>
        /// Copies data from an unmanaged memory pointer to a ushort array.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method behaves the same as the Marshal.Copy methods.
        /// </para>
        /// </remarks>
        /// <param name="source">A memory pointer to copy from.</param>
        /// <param name="destination">The array to copy to.</param>
        /// <param name="length">The length of the copy.</param>
        public static void Copy(IntPtr source, ushort[] destination, int length)
        {
            int byteLength = sizeof(ushort) * length;
            byte[] tmp = new byte[byteLength];
            Marshal.Copy(source, tmp, 0, byteLength);
            Buffer.BlockCopy(tmp, 0, destination, 0, byteLength);
        }

        /// <summary>
        /// Copies data from an unmanaged memory pointer to a uint array.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method behaves the same as the Marshal.Copy methods.
        /// </para>
        /// </remarks>
        /// <param name="source">A memory pointer to copy from.</param>
        /// <param name="destination">The array to copy to.</param>
        /// <param name="length">The length of the copy.</param>
        public static void Copy(IntPtr source, uint[] destination, int length)
        {
            int byteLength = sizeof(uint) * length;
            byte[] tmp = new byte[byteLength];
            Marshal.Copy(source, tmp, 0, byteLength);
            Buffer.BlockCopy(tmp, 0, destination, 0, byteLength);
        }

        /// <summary>
        /// Copies the content of a one-dimentional array to an unmanaged memory pointer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method behaves the same as the Marshal.Copy methods.
        /// </para>
        /// </remarks>
        /// <param name="source">An array to copy from.</param>
        /// <param name="startIndex">The index where the copy should start.</param>
        /// <param name="destination">The memory pointer to copy to.</param>
        /// <param name="length">The length of the copy.</param>
        public static void Copy(ushort[] source, int startIndex, IntPtr destination, int length)
        {
            int size = sizeof(ushort);
            int byteLength = size * length;
            int byteStart = size * startIndex;
            byte[] tmp = new byte[byteLength];
            Buffer.BlockCopy(source, byteStart, tmp, 0, byteLength);
            Marshal.Copy(tmp, 0, destination, byteLength);
        }

        /// <summary>
        /// Copies the content of a one-dimentional array to an unmanaged memory pointer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method behaves the same as the Marshal.Copy methods.
        /// </para>
        /// </remarks>
        /// <param name="source">An array to copy from.</param>
        /// <param name="startIndex">The index where the copy should start.</param>
        /// <param name="destination">The memory pointer to copy to.</param>
        /// <param name="length">The length of the copy.</param>
        public static void Copy(uint[] source, int startIndex, IntPtr destination, int length)
        {
            int size = sizeof(uint);
            int byteLength = size * length;
            int byteStart = size * startIndex;
            byte[] tmp = new byte[byteLength];
            Buffer.BlockCopy(source, byteStart, tmp, 0, byteLength);
            Marshal.Copy(tmp, 0, destination, byteLength);
        }

        /// <summary>
        /// Gets a pointer to an allocated umanaged memory buffer.
        /// </summary>
        /// <param name="size">The size, in bytes, of the buffer.</param>
        /// <param name="zeroMemory">If true the the content of the buffer will be zeroed.</param>
        /// <returns>A pointer to an allocated unmanaged memory butter.</returns>
        public static IntPtr GetBuffer(int size, bool zeroMemory)
        {
            IntPtr result = Marshal.AllocHGlobal(size);
            if (zeroMemory)
                ZeroMemory(result, size);
            return result;
        }

        /// <summary>
        /// Gets a pointer to an unmanaged memory buffer filled from an array.
        /// </summary>
        /// <param name="source">The array used to build the buffer.</param>
        /// <param name="length">The number of elements to copy from the source.</param>
        /// <returns>A pointer to an unmanaged memory buffer filled from the source array.</returns>
        public static IntPtr GetFilledBuffer(ushort[] source, int length)
        {
            int size = sizeof(ushort) * length;
            IntPtr result = Marshal.AllocHGlobal(size);
            Copy(source, 0, result, length);
            return result;
        }

        /// <summary>
        /// Gets a pointer to an unmanaged memory buffer filled from an array.
        /// </summary>
        /// <param name="source">The array used to build the buffer.</param>
        /// <param name="length">The number of elements to copy from the source.</param>
        /// <returns>A pointer to an unmanaged memory buffer filled from the source array.</returns>
        public static IntPtr GetFilledBuffer(uint[] source, int length)
        {
            int size = sizeof(uint) * length;
            IntPtr result = Marshal.AllocHGlobal(size);
            Copy(source, 0, result, length);
            return result;
        }
        /// <summary>
        /// Gets a pointer to an unmanaged memory buffer filled from an array.
        /// </summary>
        /// <param name="source">The array used to build the buffer.</param>
        /// <param name="length">The number of elements to copy from the source.</param>
        /// <returns>A pointer to an unmanaged memory buffer filled from the source array.</returns>
        public static IntPtr GetFilledBuffer(float[] source, int length)
        {
            int size = sizeof(float) * length;
            IntPtr result = Marshal.AllocHGlobal(size);
            Marshal.Copy(source, 0, result, length);
            return result;
        }

        /// <summary>
        /// Gets a pointer to an unmanaged memory buffer filled from an array.
        /// </summary>
        /// <param name="source">The array used to build the buffer.</param>
        /// <param name="length">The number of elements to copy from the source.</param>
        /// <returns>A pointer to an unmanaged memory buffer filled from the source array.</returns>
        public static IntPtr GetFilledBuffer(int[] source, int length)
        {
            int size = sizeof(int) * length;
            IntPtr result = Marshal.AllocHGlobal(size);
            Marshal.Copy(source, 0, result, length);
            return result;
        }

        /// <summary>
        /// Gets a pointer to an unmanaged memory buffer filled from an array.
        /// </summary>
        /// <param name="source">The array used to build the buffer.</param>
        /// <param name="length">The number of elements to copy from the source.</param>
        /// <returns>A pointer to an unmanaged memory buffer filled from the source array.</returns>
        public static IntPtr GetFilledBuffer(byte[] source, int length)
        {
            IntPtr result = Marshal.AllocHGlobal(length);
            Marshal.Copy(source, 0, result, length);
            return result;
        }

        /// <summary>
        /// Returns an array filled from an unmanaged memory buffer.
        /// </summary>
        /// <param name="source">The pointer to an allocated unmanaged memory buffer.</param>
        /// <param name="length">The number of elements to copy into the return array.</param>
        /// <returns>A ushort array filled from the unmanaged memory buffer.</returns>
        public static ushort[] ExtractArrayUShort(IntPtr source, int length)
        {
            ushort[] result = new ushort[length];
            Copy(source, result, length);
            return result;
        }

        /// <summary>
        /// Returns an array filled from an unmanaged memory buffer.
        /// </summary>
        /// <param name="source">The pointer to an allocated unmanagedmemory buffer.</param>
        /// <param name="length">The number of elements to copy into the return array.</param>
        /// <returns>A uint array filled from the unmanaged memory buffer.</returns>
        public static uint[] ExtractArrayUInt(IntPtr source, int length)
        {
            uint[] result = new uint[length];
            Copy(source, result, length);
            return result;
        }

        /// <summary>
        /// Returns an array filled from an unmanaged memory buffer.
        /// </summary>
        /// <param name="source">The pointer to an allocated unmanagedmemory buffer.</param>
        /// <param name="length">The number of elements to copy into the return array.</param>
        /// <returns>An int array filled from the unmanaged memory buffer.</returns>
        public static int[] ExtractArrayInt(IntPtr source, int length)
        {
            int[] result = new int[length];
            Marshal.Copy(source, result, 0, length);
            return result;
        }

        /// <summary>
        /// Returns an array filled from an unmanaged memory buffer.
        /// </summary>
        /// <param name="source">The pointer to an allocated unmanaged memory buffer.</param>
        /// <param name="length">The number of elements to copy into the return array.</param>
        /// <returns>A byte array filled from the unmanaged memory buffer.</returns>
        public static byte[] ExtractArrayByte(IntPtr source, int length)
        {
            byte[] result = new byte[length];
            Marshal.Copy(source, result, 0, length);
            return result;
        }

        /// <summary>
        /// Returns an array filled from an unmanaged memory buffer.
        /// </summary>
        /// <param name="source">The pointer to an allocated unmanaged memory buffer.</param>
        /// <param name="length">The number of elements to copy into the return array.</param>
        /// <returns>A float array filled from the unmanaged memory buffer.</returns>
        public static float[] ExtractArrayFloat(IntPtr source, int length)
        {
            float[] result = new float[length];
            Marshal.Copy(source, result, 0, length);
            return result;
        }

        /// <summary>
        /// Zeros the memory of an allocated unmanaged memory buffer.
        /// </summary>
        /// <param name="target">A pointer to an allocated unmanaged memory buffer.</param>
        /// <param name="size">The number of bytes to zero.</param>
        public static void ZeroMemory(IntPtr target, int size)
        {
            byte[] tmp = new byte[size];
            Marshal.Copy(tmp, 0, target, size);
        }
    }
}
