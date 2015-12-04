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
    /// The state of a <see cref="NavmeshBuild"/> asset. (Editor Only)
    /// </summary>
    public enum NavmeshBuildState
    {
        /// <summary>
        /// The build is missing core references needed to perform the build.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In general, no build operations can be performed until the issues are resolved.
        /// </para>
        /// </remarks>
        Invalid,

        /// <summary>
        /// Build data is available, but there is no input data. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The build data may have been left in place purposefully or accidentally.
        /// (E.g. An editor reset, user forgot to exit the build, etc.)
        /// </para>
        /// <para>
        /// The next expected action is to either reset the build or set the input data.
        /// </para>
        /// </remarks>
        NeedsRecovery,

        /// <summary>
        /// The build has not been started. (Has no input data and no build data.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The next step is to set the input data.
        /// </para>
        /// </remarks>
        Inactive,

        /// <summary>
        /// The build has input data, but no build data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The next step is to set the configuration and prepare the build.
        /// </para>
        /// </remarks>
        InputCompiled,

        /// <summary>
        /// Build operations are supported.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The input and build data are available for use in builds.
        /// </para>
        /// </remarks>
        Buildable,
    }
}