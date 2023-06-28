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

public class DebugRaycastView : MonoBehaviour
{

    public Color color = Color.red;
    public Color colorNormal = Color.blue;

    [Range(1, 100)]
    public float lineThickness = 25.0f;

    private Transform source, dest;

    public void SetupGizmos()
    {
        if (source == null)
        {
            source = new GameObject("Source").transform;
            dest = new GameObject("Dest").transform;
            source.transform.position = transform.position;
            dest.transform.position = transform.position;
            source.transform.SetParent(transform);
            dest.transform.SetParent(transform);
        }
    }

    public void OnDrawGizmos()
    {
        SetupGizmos();

        Vector3 delta = dest.position - source.position;
        bool didHit = Physics.Raycast(new Ray(source.position, delta.normalized), out RaycastHit hit, delta.magnitude);
# if UNITY_EDITOR
        if (didHit)
        {
            Handles.color = color;
            Handles.DrawLine(source.position, hit.point, lineThickness);
            Handles.DrawSolidDisc(hit.point, hit.normal, 0.1f);
            Handles.color = Color.Lerp(color, Color.black, 0.9f);
            Handles.DrawWireDisc(hit.point, hit.normal, 0.1f);
            Gizmos.color = Color.Lerp(color, Color.black, 0.9f);
            Gizmos.DrawWireSphere(hit.point, 0.05f);

            Handles.color = Color.Lerp(color, Color.white, 0.9f);
            Handles.DrawLine(hit.point, dest.position, lineThickness);

            Handles.color = colorNormal;
            Handles.DrawLine(hit.point, hit.point + hit.normal * 0.25f, lineThickness);
        }
        else
        {
            Handles.color = color;
            Handles.DrawLine(source.position, dest.position, lineThickness);
        }
# endif
    }
}
