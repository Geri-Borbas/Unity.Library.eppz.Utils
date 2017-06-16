//
// Copyright (c) 2017 Geri Borbás http://www.twitter.com/_eppz
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;


namespace EPPZ.Utils.Editor
{


	public class SelectByLayer : EditorWindow 
	{


		static int layerIndex;


		[MenuItem("Window/eppz!/Select by Layer")]
		public static void Init()
		{
			SelectByLayer window = EditorWindow.GetWindow<SelectByLayer>("Select by Layer");
			window.Show();
			window.Focus();
		}

		void OnGUI()
		{
			// Layer index.	
			layerIndex = EditorGUILayout.IntField("Layer index", layerIndex);

			if (GUILayout.Button("Select all GameObjects (and Prefabs) on Layer"))
			{ FindAndSelectObjectsByLayer(); }
		}

		public static void FindAndSelectObjectsByLayer()
		{
			// Get all objects.
			GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(gameObject => gameObject.hideFlags == HideFlags.None).ToArray();

			// Match against layer.
			List<GameObject> matches = new List<GameObject>();
			foreach (GameObject eachGameObject in objects)
			{
				if (eachGameObject.layer == layerIndex)
				{ matches.Add(eachGameObject); }
			}

			// Select.
			Selection.objects = matches.ToArray();
		}
	}
}