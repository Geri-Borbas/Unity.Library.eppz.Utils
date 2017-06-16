//
// Copyright (c) 2017 Geri Borbás http://www.twitter.com/_eppz
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;
using System.Collections;


namespace EPPZ.Utils.Editor
{


	public class Snap : ScriptableObject
	{	
		[MenuItem("eppz!/Snap/Snap center to Grid &%g")] // Alt + CMD + G
		static void MenuSnapToGrid()
		{
			foreach (Transform eachTransform in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable))
			{
				eachTransform.position = new Vector3(
					Mathf.Round(eachTransform.position.x / EditorPrefs.GetFloat("MoveSnapX")) * EditorPrefs.GetFloat("MoveSnapX"),
					Mathf.Round(eachTransform.position.y / EditorPrefs.GetFloat("MoveSnapY")) * EditorPrefs.GetFloat("MoveSnapY"),
					Mathf.Round(eachTransform.position.z / EditorPrefs.GetFloat("MoveSnapZ")) * EditorPrefs.GetFloat("MoveSnapZ")
					);
			}
		}

		[MenuItem("eppz!/Snap/Snap Bounds to Origin &%b")] // Alt + CMD + B
		static void MenuSnapBoundsToGrid()
		{
			foreach (Transform eachTransform in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable))
			{
				if (eachTransform.gameObject.GetComponent<Renderer>() == null) continue; // Only if any renderer

				eachTransform.position = new Vector3(
					eachTransform.position.x - eachTransform.gameObject.GetComponent<Renderer>().bounds.center.x,
					eachTransform.position.y - eachTransform.gameObject.GetComponent<Renderer>().bounds.center.y,
					eachTransform.position.z - eachTransform.gameObject.GetComponent<Renderer>().bounds.center.z
					);
			}
		}
	}
}