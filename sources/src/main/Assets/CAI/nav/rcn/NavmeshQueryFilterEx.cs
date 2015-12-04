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

namespace org.critterai.nav.rcn
{
    internal static class NavmeshQueryFilterEx
    {
        /*
         * Design note: In order to stay compatible with Unity iOS, all
         * extern methods must be unique and match DLL entry point.
         * (Can't use EntryPoint.)
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtqfAlloc();

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtqfFree(IntPtr filter);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtqfSetAreaCost(IntPtr filter
            , int index
            , float cost);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern float dtqfGetAreaCost(IntPtr filter
            , int index);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtqfSetIncludeFlags(IntPtr filter
            , ushort flags);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern ushort dtqfGetIncludeFlags(IntPtr filter);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtqfSetExcludeFlags(IntPtr filter
            , ushort flags);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern ushort dtqfGetExcludeFlags(IntPtr filter);
    }
}
