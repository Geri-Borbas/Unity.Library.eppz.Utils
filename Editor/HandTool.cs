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


	[InitializeOnLoad]
	public class HandTool
	{


		public static Tool previousTool;
		public static bool spaceIsDown;


		static HandTool()
		{
			// Register callback.
			SceneView.onSceneGUIDelegate += _OnSceneGUI;
		}

		static void _OnSceneGUI(SceneView sceneView)
		{
			Event event_ = Event.current;
			bool space = (event_.keyCode == KeyCode.Space);

			// If space pressed.
			if (event_.type == EventType.KeyDown && space)
			{
				// Save current `Tool` selection (only at the first event).
				if (spaceIsDown == false) { previousTool = Tools.current; }

				Tools.current = Tool.View; // Set Hand tool
				Event.current.Use(); // Consume event
				spaceIsDown = true; // Flag
			}

			// If space released.
			if (event_.type == EventType.KeyUp && space)
			{
				Tools.current = previousTool;
				Event.current.Use(); // Consume event
				spaceIsDown = false; // Flag
			}
		}
	}
}