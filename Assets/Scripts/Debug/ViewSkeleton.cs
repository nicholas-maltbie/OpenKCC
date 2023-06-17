// Copyright (C) 2023 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

# if UNITY_EDITOR
using UnityEditor;
# endif

using UnityEngine;

public class ViewSkeleton : MonoBehaviour
{

    public Transform rootNode;
    public Transform[] childNodes;

    public void OnDrawGizmosSelected()
    {
        if (rootNode != null)
        {
            if (childNodes == null || childNodes.Length == 0)
            {
                //get all joints to draw
                PopulateChildren();
            }

            foreach (Transform child in childNodes)
            {

                if (child == rootNode)
                {
                    //list includes the root, if root then larger, green cube
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(child.position, new Vector3(.1f, .1f, .1f));
                }
                else
                {
                    Gizmos.color = Color.blue;
#if UNITY_EDITOR
                    Handles.DrawBezier(child.position, child.parent.position, child.position, child.parent.position, Color.blue, null, 5.0f);
#else
                    Gizmos.DrawLine(child.position, child.parent.position);
#endif
                    Gizmos.DrawSphere(child.position, 0.01f);
                }
            }
        }
    }

    public void PopulateChildren()
    {
        childNodes = rootNode.GetComponentsInChildren<Transform>();
    }
}
