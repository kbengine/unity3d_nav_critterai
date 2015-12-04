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
using UnityEngine;
using org.critterai;

/// <summary>
/// Provides data used to assign an area during an NMGen build.
/// </summary>
public abstract class NMGenAreaMarker
    : NMGenComponent, IPriorityItem
{
    [SerializeField]
    private byte mArea = MaxArea;

    [SerializeField]
    private int mPriority = 100;

    /// <summary>
    /// The priority of the marker.
    /// </summary>
    public int Priority
    {
        get { return mPriority; }
        set { mPriority = Mathf.Max(ushort.MinValue, Mathf.Min(ushort.MaxValue, value)); }
    }

    /// <summary>
    /// The area to assign.
    /// </summary>
    public byte Area
    {
        get { return mArea; }
        set { mArea = ClampArea(value); }
    }

    /// <summary>
    /// The area to assign. (Integer version.)
    /// </summary>
    public int AreaInt
    {
        get { return mArea; }
        set { mArea = ClampArea(value); }
    }
}
