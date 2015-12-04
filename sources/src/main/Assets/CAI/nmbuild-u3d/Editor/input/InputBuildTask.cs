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
    internal sealed class InputBuildTask
        : BuildTask<InputGeometry>
    {
        private InputGeometryBuilder mBuilder;

        public override bool IsThreadSafe { get {return mBuilder.IsThreadSafe; } }

        private InputBuildTask(InputGeometryBuilder builder, int priority)
            : base(priority)
        {
            mBuilder = builder;
        }

        protected override bool  LocalUpdate()
        {
            return mBuilder.Build();
        }

        protected override bool GetResult(out InputGeometry result)
        {
            result = mBuilder.Result;
            return true;
        }

        protected override void FinalizeTask()
        {
            mBuilder = null;
        }

        public static InputBuildTask Create(InputGeometryBuilder builder, int priority)
        {
            if (builder == null || builder.IsFinished)
                return null;

            return new InputBuildTask(builder, priority);
        }
    }
}
