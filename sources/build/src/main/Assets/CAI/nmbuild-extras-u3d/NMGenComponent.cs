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
using Math = System.Math;

/// <summary>
/// A scene component used during an NMGen build.
/// </summary>
public abstract class NMGenComponent
    : MonoBehaviour
{
    /// <summary>
    /// The maximum allowed area.
    /// </summary>
    public const byte MaxArea = 63;

    /// <summary>
    /// True if debug mode is enabled for all components.
    /// </summary>
    public static bool debugEnabled = false;

    /// <summary>
    /// True if debug mode is enabled for the current component.
    /// </summary>
    [System.NonSerialized]
    public bool debugEnabledLocal = false;

    /// <summary>
    /// Clamps the value to a valid area.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public static byte ClampArea(byte value)
    {
        return Math.Min(MaxArea, value);
    }

    /// <summary>
    /// Clamps the value to a valid area. (Integer version.)
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public static byte ClampArea(int value)
    {
        return (byte)Math.Min(MaxArea, Math.Max(0, value));
    }
}
