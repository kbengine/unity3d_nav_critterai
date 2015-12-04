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
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

// Note: The file name does not match the element name because Unity
// doesn't support multiple script files with the same name, and the
// entity name exists in multiple namespaces.

namespace org.critterai.nav.rcn
{
    internal struct InteropUtil
    {
    #if UNITY_IPHONE && !UNITY_EDITOR
        public const string PLATFORM_DLL = "__Internal";
    #else
        public const string PLATFORM_DLL = "cai-nav-rcn";
    #endif

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtvlVectorTest(
            [In] ref Vector3 vector3in
            , ref Vector3 vector3out);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtvlVectorArrayTest(
            [In] Vector3[] vector3in
            , int vectorCount
            , [In, Out] Vector3[] vector3out);
    }
}
