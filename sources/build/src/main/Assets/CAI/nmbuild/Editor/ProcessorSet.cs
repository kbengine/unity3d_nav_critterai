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
using System.Collections.Generic;
using org.critterai.nmgen;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// A set of <see cref="INMGenProcessor"/> objects used in an NMGen build.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The processor set groups a set of processors for use in an NMGen build.  If all
    /// contained processors are thread-safe, then the process set can be used in a threaded build.
    /// </para>
    /// <para>
    /// To create an empty processor set, call <see cref="CreateStandard"/> with no
    /// options set.
    /// </para>
    /// </remarks>
    /// <seealso cref="INMGenProcessor"/>
    /// <seealso cref="NMGenProcessor"/>
    public sealed class ProcessorSet
    {
        /// <summary>
        /// The build options that most builds will require.
        /// </summary>
        public const NMGenBuildFlag StandardOptions = NMGenBuildFlag.ApplyPolyFlags
            | NMGenBuildFlag.LowHeightSpansNotWalkable
            | NMGenBuildFlag.LowObstaclesWalkable;

        private readonly bool mIsThreadSafe;
        private readonly INMGenProcessor[] mProcessors;
        private readonly NMGenAssetFlag mPreserveAssets;

        private ProcessorSet(INMGenProcessor[] processors) 
        {
            mProcessors = processors;

            PriorityComparer<INMGenProcessor> comp = 
                new PriorityComparer<INMGenProcessor>(true);

            System.Array.Sort(mProcessors, comp);

            mIsThreadSafe = true;  // This is correct.

            foreach (INMGenProcessor p in mProcessors)
            {
                mPreserveAssets |= p.PreserveAssets;

                if (!p.IsThreadSafe)
                    mIsThreadSafe = false;
            }
        }

        /// <summary>
        /// The build assets that should be preserved past their normal disposal point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is an aggregate of the <see cref="INMGenProcessor.PreserveAssets"/> for
        /// all contained processors.
        /// </para>
        /// </remarks>
        public NMGenAssetFlag PreserveAssets { get { return mPreserveAssets; } }

        /// <summary>
        /// True if all contained processors are threadsafe.
        /// </summary>
        public bool IsThreadSafe { get { return mIsThreadSafe; } }

        /// <summary>
        /// The number of processors in the set.
        /// </summary>
        public int Count { get { return mProcessors.Length; } }

        /// <summary>
        /// Runs all the processors in order of priority.  (Ascending)
        /// </summary>
        /// <remarks>
        /// <para>
        /// A return value of false indicates the build should be aborted.
        /// </para>
        /// </remarks>
        /// <param name="state">The current state of the build.</param>
        /// <param name="context">The build context.</param>
        /// <returns>False if the build should abort.</returns>
        public bool Process(NMGenContext context, NMGenState state)
        {
            foreach (INMGenProcessor p in mProcessors)
            {
                if (!p.ProcessBuild(context, state))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Logs processor information to the context for debug purposes.
        /// </summary>
        /// <param name="context">The build context.</param>
        public void LogProcessors(NMGenContext context)
        {
            foreach (INMGenProcessor p in mProcessors)
            {
                context.Log(string.Format("Processor: {0} ({1})", p.Name, p.GetType().Name)
                    , this);
            }
        }

        /// <summary>
        /// Gets standard processors based on the provided options.
        /// </summary>
        /// <param name="options">The processors to include.</param>
        /// <returns>
        /// The standard processors, or a zero length array if no processors selected.
        /// </returns>
        public static INMGenProcessor[] GetStandard(NMGenBuildFlag options)
        {
            List<INMGenProcessor> ps = new List<INMGenProcessor>();

            if ((options & NMGenBuildFlag.ApplyPolyFlags) != 0)
            {
                ps.Add(new ApplyPolygonFlags("ApplyDefaultPolyFlag"
                    , NMBuild.MinPriority, NMBuild.DefaultFlag));
            }

            if ((options & NMGenBuildFlag.LedgeSpansNotWalkable) != 0)
                ps.Add(FilterLedgeSpans.Instance);

            if ((options & NMGenBuildFlag.LowHeightSpansNotWalkable) != 0)
                ps.Add(FilterLowHeightSpans.Instance);

            if ((options & NMGenBuildFlag.LowObstaclesWalkable) != 0)
                ps.Add(LowObstaclesWalkable.Instance);

            return ps.ToArray();
        }

        /// <summary>
        /// Creates a processor set based on the provided options.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An empty processor set will be created if <paramref name="options"/> is zero.
        /// </para>
        /// </remarks>
        /// <param name="options">The processors to include.</param>
        /// <returns>A processor set with the standard processors.</returns>
        public static ProcessorSet CreateStandard(NMGenBuildFlag options)
        {
            return Create(GetStandard(options));
        }
        
        /// <summary>
        /// Creates a processor set loaded with the provided processors.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An empty processor set will be created if <paramref name="processors"/> is null
        /// or contains no processors.
        /// </para>
        /// </remarks>
        /// <param name="processors">The processors to include in the set.</param>
        /// <returns>The processor set</returns>
        public static ProcessorSet Create(INMGenProcessor[] processors)
        {
            INMGenProcessor[] lprocessors = ArrayUtil.Compress(processors);

            if (lprocessors == null)
                lprocessors = new INMGenProcessor[0];
            else if (lprocessors == processors)
                lprocessors = (INMGenProcessor[])processors.Clone();

            return new ProcessorSet(lprocessors);
        }
    }
}
