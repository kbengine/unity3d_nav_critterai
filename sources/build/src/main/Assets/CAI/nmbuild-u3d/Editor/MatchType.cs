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
namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// Match behavior type. (Editor Only)
    /// </summary>
    public enum MatchType
	{
        /// <summary>
        /// The compared object references must be equal. (source == target)
        /// </summary>
        Strict = 0,

        /// <summary>
        /// The target object's name must match the instance pattern of the source.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The instance pattern is the source name, followed by a space, and containing the
        /// word "instance". The check is lazy.  If the source name is "SwampMesh", then 
        /// both "SwampMesh Instance" and "SwampMesh NotReallyAnInstance" will match.
        /// </para>
        /// </remarks>
        AnyInstance,

        /// <summary>
        /// The target object's name begins with the same string as the entire name of the source.
        /// </summary>
        /// <remarks>
        /// Example: If the source object is "Column", then "ColumnWooden" and "Column Iron" will
        /// match.
        /// </remarks>
        NameBeginsWith
	}
}
