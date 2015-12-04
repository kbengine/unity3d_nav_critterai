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

namespace org.critterai.nmbuild.u3d.editor
{
	internal sealed class MatchPredicate
	{
        private readonly Object mRoot;
        private readonly MatchType mType;
        private readonly bool mInvert;

        public MatchPredicate(Object root, MatchType type, bool invert)
        {
            mRoot = root;
            mType = type;
            mInvert = invert;
        }

        public bool Matches(Object item)
        {
            if (mInvert)
                return Matches(mRoot, item, mType);
            else
                return Matches(item, mRoot, mType);
        }

        // Not commutative
        public static bool Matches(Object target, Object source, MatchType option)
        {
            if (target == null || source == null)
                return false;

            switch (option)
            {
                case MatchType.Strict:

                    return (target == source);

                case MatchType.NameBeginsWith:

                    return (target.name.StartsWith(source.name));

                case MatchType.AnyInstance:

                    return (target == source
                        || (target.name.StartsWith(source.name + " ")
                            && target.name.Contains("Instance")));
            }
            return false;
        }

    }
}
