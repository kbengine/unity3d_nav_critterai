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
using System.Collections.Generic;
using org.critterai.nav;

/// <summary>
/// Global navigation settings. (Editor Only)
/// </summary>
/// <remarks>
/// <para>The values in this class can only be edited via the Unity Editor.</para>
/// </remarks>
[System.Serializable]
public sealed class CAINavEditorSettings
    : ScriptableObject
{
    /// <summary>
    /// The well known name for <see cref="Navmesh.NullArea"/>.
    /// </summary>
    public const string NotWalkable = "Not Walkable";

    /// <summary>
    /// The well known name for <see cref="Navmesh.MaxArea"/>.
    /// </summary>
    public const string Default = "Default";

    /// <summary>
    /// The area name for areas with no defined well known name.
    /// </summary>
    public const string Undefined = "<Undefined>";

    /// <summary>
    /// A value indicating that the area is unknown.
    /// </summary>
    /// <seealso cref="GetArea"/>
    public const byte UnknownArea = Navmesh.MaxArea + 1;

    [SerializeField]
    internal string[] areaNames;

    [SerializeField]
    internal string[] flagNames;

    [SerializeField]
    internal string[] avoidanceNames;

    void OnEnable()
    {
        if (areaNames == null || areaNames.Length == 0)
            // First startup.
            Reset();
    }

    /// <summary>
    /// Resets the settings to the default state.
    /// </summary>
    public void Reset()
    {
        areaNames = new string[Navmesh.MaxArea + 1];

        for (int i = 1; i < Navmesh.MaxArea; i++)
        {
            areaNames[i] = Undefined;
        }

        areaNames[Navmesh.NullArea] = NotWalkable;
        areaNames[Navmesh.MaxArea] = Default;
        areaNames[Navmesh.MaxArea - 1] = "Water";

        flagNames = new string[16];

        flagNames[0] = Default;
        flagNames[1] = "Jump";
        flagNames[2] = "Swim";
        flagNames[3] = "Blocked";

        for (int i = 4; i < flagNames.Length; i++)
        {
            flagNames[i] = string.Format("Flag 0x{0:X}", 1 << i);
        }

        avoidanceNames = new string[CrowdManager.MaxAvoidanceParams];

        avoidanceNames[0] = "Low";
        avoidanceNames[1] = "Medium";
        avoidanceNames[2] = "Good";
        avoidanceNames[3] = "High";

        for (int i = 4; i < avoidanceNames.Length; i++)
        {
            avoidanceNames[i] = "Quality " + i;
        }
    }

    /// <summary>
    /// Gets a clone of the area names.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the full array, including the <see cref="Undefined"/> values.
    /// </para>
    /// </remarks>
    /// <returns>A clone of the area names. [Length: <see cref="Navmesh.MaxArea"/> + 1]</returns>
    public string[] GetAreaNames()
    {
        return (string[])areaNames.Clone();
    }

    /// <summary>
    /// Gets the area associated with the well known area name.
    /// </summary>
    /// <param name="name">The well known area name.</param>
    /// <returns>Gets the area associated with the well known name, or 
    /// <see cref="UnknownArea"/> if the name is invalid.</returns>
    public byte GetArea(string name)
    {
        if (name == NotWalkable)
            return Navmesh.NullArea;

        for (int i = 0; i < areaNames.Length; i++)
        {
            if (areaNames[i] == name)
                return (byte)i;
        }

        return UnknownArea;
    }

    /// <summary>
    /// Gets a clone of the well known names for flags, indexed on the flag's bit.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All flags have a well known name.
    /// </para>
    /// <para>
    /// The order of the names match the order of the flags. So the name at index 5 is the name
    /// for flag (1 &lt;&lt; 5).
    /// </para>
    /// </remarks>
    /// <returns>A clone of the flag names. [Length: 16]</returns>
    public string[] GetFlagNames()
    {
        return (string[])flagNames.Clone();
    }

    /// <summary>
    /// Gets a clone of the well known crowd avoidance names.
    /// </summary>
    /// <returns>
    /// A clone of the avoidance names. [Length: <see cref="CrowdManager.MaxAvoidanceParams"/>]
    /// </returns>
    public string[] GetAvoidanceNames()
    {
        return (string[])avoidanceNames.Clone();
    }
}
