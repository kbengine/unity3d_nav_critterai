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
    /// Applies <see cref="Heightfield.MarkLowHeightSpansNotWalkable"/> to 
    /// a <see cref="Heightfield"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a system processor.
    /// </para>
    /// </remarks>
    public sealed class FilterLowHeightSpans
        : NMGenProcessor
    {
        private const int ProcessorPriority = FilterLedgeSpans.ProcessorPriority + 5;

        private static FilterLowHeightSpans mInstance = new FilterLowHeightSpans();

        private FilterLowHeightSpans()
            : base(typeof(FilterLowHeightSpans).Name, ProcessorPriority)
        {
        }

        /// <summary>
        /// Always threadsafe. (True)
        /// </summary>
        public override bool IsThreadSafe { get { return true; } }

        /// <summary>
        /// Process the build context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will be applied during the <see cref="NMGenState.HeightfieldBuild"/> state.
        /// </para>
        /// </remarks>
        /// <param name="state">The current build state.</param>
        /// <param name="context">The context to process.</param>
        /// <returns>False on error, otherwise true.</returns>
        public override bool ProcessBuild(NMGenContext context, NMGenState state)
        {

            if (state != NMGenState.HeightfieldBuild)
                return true;

            if (context.Heightfield.MarkLowHeightSpansNotWalkable(context
                , context.Config.WalkableHeight))
            {
                context.Log(Name + ": Marked low height spans as not walklable.", this);
                return true;
            }

            context.Log(Name + ": Mark low height spans failed.", this);
            return false;
        }

        /// <summary>
        /// The processor instance.
        /// </summary>
        public static FilterLowHeightSpans Instance { get { return mInstance; } }
    }
}
