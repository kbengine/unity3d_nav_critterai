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
using System;
using org.critterai.nmgen;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// The base class for markers that apply an area id during the
    /// <see cref="NMGenState.CompactFieldBuild"/> state of an NMGen build.
    /// </summary>
	public abstract class AreaMarker
        : NMGenProcessor
	{
        private readonly byte mArea;

        /// <summary>
        /// The area to apply.
        /// </summary>
        public byte Area { get { return mArea; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The processor name.</param>
        /// <param name="priority">The processor priority.</param>
        /// <param name="area">The area to apply.</param>
        public AreaMarker(string name, int priority, byte area)
            : base(name, priority)
        {
            mArea = NMGen.ClampArea(area);
        }
    }
}
