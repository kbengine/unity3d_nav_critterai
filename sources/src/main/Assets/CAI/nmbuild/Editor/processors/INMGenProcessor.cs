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
using org.critterai.nmgen;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// A processor used during the NMGen build process.
    /// </summary>
	public interface INMGenProcessor
        : IPriorityItem
    {
        /// <summary>
        /// The name of the processor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// True if the processor is safe to use across multiple threads at the same time.
        /// </summary>
        bool IsThreadSafe { get; }

        /// <summary>
        /// The build assets that should be preserved past their normal disposal point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Most processors will return zero. Use judiciously since it can result in large 
        /// increases in memory consumption during a build.
        /// </para>
        /// <para>
        /// Builders usually dispose of assets as soon as the asset is no longer needed for 
        /// a 'normal' build.  For example, the <see cref="Heightfield"/> is usually disposed as 
        /// soon as the <see cref="CompactHeightfield"/> is created.  So if a processor needs
        /// the heightfield for processing after heightfield post-processing, it should indicate 
        /// so through this field.
        /// </para>
        /// </remarks>
        NMGenAssetFlag PreserveAssets { get; }

        /// <summary>
        /// Process the build context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The processor will never return the context in an invalid state.  If it
        /// wants the build to abort it will return false rather than resetting or nulling 
        /// values in the context.
        /// </para>
        /// <para>
        /// The processor will always log a message to the context when it returns false.  
        /// It may also log summary messages for use in debugging.
        /// </para>
        /// </remarks>
        /// <param name="state">The current build state.</param>
        /// <param name="context">The context to process.</param>
        /// <returns>False if the build should abort.  Otherwise true.</returns>
        bool ProcessBuild(NMGenContext context, NMGenState state);
	}
}
