/*
 * This feature removed from v0.4.
 * 
 * Not tested.
 * 
 * TODO: EVAL: v0.5: Is this really useful?  
 * 
 */

//*
// * Copyright (c) 2012 Stephen A. Pratt
// * 
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the "Software"), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// * 
// * The above copyright notice and this permission notice shall be included in
// * all copies or substantial portions of the Software.
// * 
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// * THE SOFTWARE.
// */
//using System.Collections.Generic;
//using org.critterai.nmbuild;
//using org.critterai.nmbuild.u3d.editor;
//using UnityEngine;

//[System.Serializable]
//public sealed class ColliderCompiler
//    : InputBuildProcessor
//{
//    public bool allowMeshColocation = false;

//    public string Name { get { return name; } }
//    public override int Priority { get { return NMBuild.MinPriority - 1; } }
//    public override bool DuplicatesAllowed { get { return false; } }

//    public override bool ProcessInput(InputBuildContext context, InputBuildState state)
//    {
//        if (context != null)
//        {
//            switch (state)
//            {
//                case InputBuildState.CompileInput:

//                    Compile(context);
//                    break;

//                case InputBuildState.LoadComponents:

//                    Load(context);
//                    break;
//            }
//        }

//        return true;
//    }

//    private void Load(InputBuildContext context)
//    {
//        context.info.loaderCount++;

//        Collider[] items = context.GetFromScene<Collider>();

//        if (items.Length == 0)
//            return;

//        List<Collider> litems = new List<Collider>();

//        int colocated = 0;
//        int unknown = 0;

//        foreach (Collider item in items)
//        {
//            MeshFilter filter = item.GetComponent<MeshFilter>();

//            if (filter)
//            {
//                colocated++;
//                if (!allowMeshColocation)
//                    continue;
//            }

//            if (ColliderHelper.IsSupported(item))
//                litems.Add(item);
//            else
//                unknown++;
//        }

//        context.components.AddRange(litems.ToArray());

//        string msg = string.Format(
//            "{0}: Loaded {1} Colliders. Ignored {2} unsupported. {3} colocated with mesh filters."
//                + " Colocation allowed: {4}"
//            , Name, litems.Count, unknown, colocated, allowMeshColocation);

//        context.Log(msg, this);
//    }

//    private void Compile(InputBuildContext context)
//    {
//        context.info.compilerCount++;

//        ColliderHelper colliderHelper = new ColliderHelper();

//        InputGeometryCompiler inputCompiler = context.geomCompiler;

//        List<Component> master = new List<Component>(context.components);
//        List<byte> areas = new List<byte>(context.areas);

//        Queue<CombineInstance> combineInstances = new Queue<CombineInstance>();

//        int count = 0;
//        int ignored = 0;
//        while (master.Count > 0)
//        {
//            byte area = 0;

//            for (int i = master.Count - 1; i >= 0; i--)
//            {
//                Component item = master[i];

//                if (item is Collider && ColliderHelper.IsSupported((Collider)item))
//                {
//                    if (!allowMeshColocation && item.GetComponent<MeshFilter>())
//                    {
//                        ignored++;
//                        areas.RemoveAt(i);
//                        master.RemoveAt(i);
//                    }
//                    else
//                    {
//                        if (combineInstances.Count == 0)
//                            area = areas[i];

//                        if (areas[i] == area)
//                        {
//                            count++;

//                            CombineInstance ci;

//                            if (colliderHelper.Get((Collider)item, out ci))
//                                combineInstances.Enqueue(ci);

//                            areas.RemoveAt(i);
//                            master.RemoveAt(i);
//                        }
//                    }
//                }
//                else
//                {
//                    areas.RemoveAt(i);
//                    master.RemoveAt(i);
//                }
//            }

//            if (combineInstances.Count > 0)
//                org.critterai.nmbuild.u3d.MeshUtil.CombineMeshes(combineInstances, area, inputCompiler);

//            combineInstances.Clear();
//        }

//        colliderHelper.Dispose();

//        if (ignored > 0)
//        {
//            context.Log(string.Format("{0}: Ignored {1} colliders colocated with MeshFilters."
//                , Name, ignored)
//                , this);
//        }

//        context.Log(string.Format("{0}: Compiled {1} colliders.", Name, count), this);
//    }
//}
