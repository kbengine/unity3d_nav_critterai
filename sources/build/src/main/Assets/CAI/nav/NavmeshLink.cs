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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    /// <summary>
    /// Navigation mesh links. (Undocumented.)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshLink
    {
        /// <summary>
        /// The polygon reference of the neighbor.
        /// </summary>
        public uint polyRef;

        /// <summary>
        /// The index of the next link.
        /// </summary>
        public uint next;

        /// <summary>
        /// The index of the polygon edge that owns this link.
        /// </summary>
        public byte edge;
        
        /// <summary>
        /// If a boundary link, defines which side the link is.
        /// </summary>
        public byte side;

        /// <summary>
        /// If a boundary link, defines the sub-edge minimum.
        /// </summary>
        public byte boundsMin;

        /// <summary>
        /// If a boundary link, defines the sub-edge maximum.
        /// </summary>
        public byte boundsMax;
    }
}
