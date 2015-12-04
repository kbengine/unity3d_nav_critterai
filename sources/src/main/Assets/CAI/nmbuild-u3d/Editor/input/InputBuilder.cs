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
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
    internal sealed class InputBuilder
    {
        private InputBuildState mState;
        private readonly InputBuildContext mContext;
        private readonly IInputBuildProcessor[] mInputProcessors;

        private InputAssets mAssets = new InputAssets();

        public InputBuildState State { get { return mState; } }

        public int MessageCount
        {
            get { return (IsFinished ? mContext.MessageCount : 0); }
        }

        public string[] Messages
        {
            get { return (IsFinished ? mContext.GetMessages() : new string[0]); }
        }

        public InputAssets Result
        {
            get
            {
                return (mState == InputBuildState.Complete 
                    ? mAssets 
                    : new InputAssets());
            }
        }

        public bool IsFinished
        {
            get
            {
                return (mState == InputBuildState.Complete
                    || mState == InputBuildState.Aborted);
            }
        }

        private InputBuilder(InputBuildContext context, IInputBuildProcessor[] processors)
        {
            mContext = context;
            mInputProcessors = processors;

            System.Array.Sort(mInputProcessors, new PriorityComparer<IInputBuildProcessor>(true));

            mState = InputBuildState.LoadComponents;
        }

        public InputBuildState Build()
        {
            switch (mState)
            {
                case InputBuildState.LoadComponents:
                    LoadSceneItems();
                    break;
                case InputBuildState.FilterComponents:
                    FilterSceneItems();
                    break;
                case InputBuildState.ApplyAreaModifiers:
                    ApplyAreaModifiers();
                    break;
                case InputBuildState.CompileInput:
                    CompileInput();
                    break;
                case InputBuildState.PostProcess:
                    PostProcess();
                    break;
            }
            return mState;
        }

        public void BuildAll()
        {
            while (!IsFinished) { Build(); }
        }

        private void LoadSceneItems()
        {
            if (RunProcessors() && ValidateSceneItems())
            {
                mContext.Log(string.Format("{0} complete: {1} components."
                    , mState, mContext.components.Count)
                    , this);

                mState = InputBuildState.FilterComponents;
            }
        }

        private bool RunProcessors()
        {
            foreach (IInputBuildProcessor item in mInputProcessors)
            {
                if (!item.ProcessInput(mContext, mState))
                {
                    FinalizeOnAbort(string.Format("Processor requested abort: {0} ({1})"
                        , item.Name, mState));
                    return false;
                }
            }
            return true;
        }

        private bool ValidateSceneItems()
        {
            List<Component> items = mContext.components;

            int orig = items.Count;

            // Remove nulls.
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] == null)
                    items.RemoveAt(i);
            }

            int purgedNull = orig - items.Count;

            orig = items.Count;

            // Remove duplicates.
            for (int i = 0; i < items.Count; i++)
            {
                Component item = items[i];
                int j = items.LastIndexOf(item);
                while (j > i)
                {
                    items.RemoveAt(j);
                    j = items.LastIndexOf(item);
                }
            }

            int purgedDup = orig - items.Count;

            if (items.Count == 0)
            {
                FinalizeOnAbort(string.Format("No components available. ({0})", mState));
                return false;
            }

            if (purgedNull > 0)
            {
                mContext.Log(string.Format("Purged null components: {0} ({1})"
                    , purgedNull, mState)
                    , this);
            }

            if (purgedDup > 0)
            {
                mContext.Log(string.Format("Purged duplicate components: {0} ({1})"
                    , purgedDup, mState)
                    , this);
            }

            return true;
        }

        private void FilterSceneItems()
        {
            List<Component> items = mContext.components;

            int before = items.Count;
            mContext.info.compCountPre = before;

            if (RunProcessors() && ValidateSceneItems())
            {
                string msg = string.Format("{0} complete. {1} components. {2} removed."
                    , mState, mContext.components.Count, before - items.Count);

                mContext.Log(msg, this);

                mContext.info.compCountPost = items.Count;

                mState = InputBuildState.ApplyAreaModifiers;
            }
        }

        private void ApplyAreaModifiers()
        {
            List<Component> items = mContext.components;
            List<byte> areas = mContext.areas;
            int count = items.Count;

            areas.Clear();

            for (int i = 0; i < count; i++)
            {
                areas.Add(NMGen.MaxArea);
            }

            if (!RunProcessors())
                return;

            if (items.Count != count || areas.Count != count || items.Contains(null))
            {
                FinalizeOnAbort(string.Format("Custom processors corrupted component list."
                        + " (Detected nulls or count mismatch.)  ({0})"
                    , mState));
            }
            else
            {
                mContext.Log(mState + " complete.", this);
                mState = InputBuildState.CompileInput;
            }
        }

        private void CompileInput()
        {
            // Just in case input processors were naughty.
            mContext.processors.Clear();
            mContext.connCompiler.Reset();
            mContext.geomCompiler.Reset();

            if (RunProcessors())
            {
                mContext.Log(mState + " complete.", this);
                mState = InputBuildState.PostProcess;
            }
        }

        private void PostProcess()
        {
            if ((mContext.Options & InputBuildOption.AutoCleanGeometry) != 0)
            {
                int removed = mContext.geomCompiler.CleanTriangles();
                mContext.Log("Cleaned geometry. " + removed + " invalid triangles removed.", this);
            }

            if (RunProcessors())
            {
                mContext.Log(mState + " complete.", this);
                FinalizeBuild();
            }
        }

        private void ResetLocals()
        {
            mContext.connCompiler.Reset();
            mContext.geomCompiler.Reset();
            mContext.processors.Clear();
            mContext.areas.Clear();
            mContext.components.Clear();
        }

        private void FinalizeOnAbort(string message)
        {
            mAssets = new InputAssets();
            mContext.LogError(message, this);
            ResetLocals();
            mState = InputBuildState.Aborted;
        }

        private void FinalizeBuild()
        {
            mAssets.info = mContext.info;

            mAssets.geometry = mContext.geomCompiler.CreateGeometry(out mAssets.areas);

            if (mAssets.geometry == null)
            {
                FinalizeOnAbort(string.Format("No geometry was produced. ({0})", mState));
                return;
            }

            mAssets.processors = mContext.processors.ToArray();
            mAssets.conns = mContext.connCompiler.CreateConnectionSet();

            string msg = string.Format("Final geometry: Triangles: {0}, Vertices: {1}"
                , mAssets.geometry.triCount, mAssets.geometry.vertCount);

            mContext.Log(msg, this);

            mContext.Log("Final Off-Mesh Connections: " + mAssets.conns.Count, this);

            if (mAssets.processors.Length < 10)
            {
                foreach (INMGenProcessor p in mAssets.processors)
                {
                    mContext.Log("NMGen Processor: " + p.Name, this);
                }
            }
            else
                mContext.Log("NMGen processor count: " + mAssets.processors.Length, this);

            ResetLocals();
            mState = InputBuildState.Complete;
        }

        public static InputBuilder Create(ISceneQuery filter
            , IInputBuildProcessor[] processors
            , InputBuildOption options)
        {
            IInputBuildProcessor[] lprocessors = ArrayUtil.Compress(processors);

            if (lprocessors == null || lprocessors.Length == 0)
                return null;

            for (int i = lprocessors.Length - 1; i >= 0; i--)
            {
                IInputBuildProcessor processor = lprocessors[i];

                if (processor.DuplicatesAllowed)
                    continue;

                System.Type ta = processor.GetType();

                for (int j = i - 1; j >= 0; j--)
                {
                    System.Type tb = lprocessors[j].GetType();

                    if (ta.IsAssignableFrom(tb) || tb.IsAssignableFrom(ta))
                        return null;
                }
            }

            if (lprocessors == processors)
                lprocessors = (IInputBuildProcessor[])lprocessors.Clone();

            InputBuildContext context = new InputBuildContext(filter, options);

            return new InputBuilder(context, lprocessors);
        }

        public static float ToProgress(InputBuildState state)
        {
            float inc = 1 / 5f;
            switch (state)
            {
                case InputBuildState.LoadComponents:
                    return inc * 1;
                case InputBuildState.FilterComponents:
                    return inc * 2;
                case InputBuildState.ApplyAreaModifiers:
                    return inc * 3;
                case InputBuildState.CompileInput:
                    return inc * 4;
                case InputBuildState.PostProcess:
                    return inc * 5;
                case InputBuildState.Aborted:
                    return 1.0f;
                case InputBuildState.Complete:
                    return 1.0f;
            }
            return 0;
        }

        public static string ToLabel(InputBuildState state)
        {
            switch (state)
            {
                case InputBuildState.LoadComponents:
                    return "Loading scene objects...";
                case InputBuildState.FilterComponents:
                    return "Filtering scene objects...";
                case InputBuildState.ApplyAreaModifiers:
                    return "Applying area modifiers...";
                case InputBuildState.CompileInput:
                    return "Compiling input...";
                case InputBuildState.PostProcess:
                    return "Post-processing...";
                case InputBuildState.Aborted:
                    return "Aborted.";
                case InputBuildState.Complete:
                    return "Input gathered.";
            }
            return "Unhandled state: " + state;
        }
    }
}
