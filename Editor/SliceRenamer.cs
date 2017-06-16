//
// Copyright (c) 2017 Geri Borbás http://www.twitter.com/_eppz
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace EPPZ.EditorUtils
{


	public class EmbeddedImage
	{


		// Use something like: Debug.Log(embeddedImage.ChopStringWithNewLines(embeddedImage.StringFromTexture(header), 256));


		// Test at http://www.askapache.com/online-tools/base64-image-converter/
		public string StringFromTexture(Texture2D texture)
		{
			byte[] bytes;
			bytes = texture.EncodeToPNG();
			string encodedString = Convert.ToBase64String(bytes);
			return encodedString;
		}

		public Texture2D TextureFromString(string encodedString)
		{
			string cleanedUpString = encodedString.TrimEnd(new char[] { '\r', '\n' }); // Remove carriage return and newlines.
			byte[] bytes = System.Convert.FromBase64String(cleanedUpString); 
			Texture2D texture = new Texture2D(1,1);
			texture.LoadImage(bytes);
			return texture;
		}

		// Easier to embed into code when split to new lines.
		public string ChopStringWithNewLines(string string_, int size)
		{
			string[] chops = Chop(string_, size);
			string glued = "";
			foreach (string eachChop in chops)
			{ glued += eachChop + "\n"; }
			return glued;
		}

		public static string[] Chop(string string_, int size)
		{
			int remainingLength = string_.Length;
			int chopCount = (remainingLength + size - 1) / size;
			string[] chops = new string[chopCount];
			for (int index = 0; index < chopCount; ++index)
			{
				chops[index] = string_.Substring(index * size, Mathf.Min(size, remainingLength));
				remainingLength -= size;
			}
			return chops;
		}

	}


	public class SliceRenamer : EditorWindow
	{

		// Properties.
		public Texture2D texture;
		public string format = "slice_{0:00}";
		[System.Serializable] public class Model : ScriptableObject
		{
			[System.Serializable] public class Argument
			{
				public int start = 0;
				public int incrementEveryNth = 1;
				public int resetEveryNth = 0;

				public int valueForIndex(int index)
				{
					int value = 0;

					// Increment.
					if (incrementEveryNth == 0)
					{ value = start + index; } // Simply increment
					else
					{ value = start + (index / incrementEveryNth); } // Increment every Nth only

					// Reset (if requested).
					if (resetEveryNth > 0)
					{ value = value % resetEveryNth; }

					return value;
				}
			}

			public List<Argument> arguments = new List<Argument>();
		}
		public static Model model;

		// GUI.
		private static SerializedObject serializedModel;
		private Vector2 scroll;
		private string status = "";
		private Texture2D header = null;
		private Texture2D background = null;

		// Asset.
		private string path;
		private TextureImporter textureImporter;


		// Window.
		[MenuItem("Window/eppz!/Slice Renamer")]
		public static void ShowWindow()
		{
			// Show window.
			EditorWindow.GetWindow(typeof(SliceRenamer), false,  "Slice Renamer");
		}

		void OnEnable()
		{
			// Model setup.
			model = new Model();
			model.arguments.Add(new Model.Argument()); // Default with a single incrementing argument
			serializedModel = new SerializedObject(model);

			EmbeddedImage embeddedImage = new EmbeddedImage();

			// UI.
			header = embeddedImage.TextureFromString(Images.headerEncoded); // (Texture2D)Resources.Load("sr", typeof(Texture2D));
			background = embeddedImage.TextureFromString(Images.backgroundEncoded); // (Texture2D)Resources.Load("srb", typeof(Texture2D));
		}


		// GUI.
		void OnGUI()
		{
			// Header.
			GUILayout.BeginHorizontal();

				GUIStyle style = new GUIStyle(GUI.skin.label);
				style.margin = new RectOffset(0, 0, 0, 0);
				style.padding = new RectOffset(0, 0, 0, 0);

				GUILayout.Label(header,
				                style,
				                GUILayout.ExpandWidth(false),
				                GUILayout.ExpandHeight(false),
				                GUILayout.Width(512),
			                	GUILayout.Height(32));

				style.normal.background = background;
				style.normal.background.wrapMode = TextureWrapMode.Repeat;

				GUILayout.Label("",
				                style,
			                	GUILayout.ExpandWidth(true),
				              	GUILayout.ExpandHeight(false),
			                	GUILayout.Height(32));

			GUILayout.EndHorizontal();

			// Texture asset.
			string textureName = (texture == null) ? "No Texture selected" : texture.name;
			EditorGUILayout.LabelField(textureName, EditorStyles.boldLabel);
			texture = EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), true) as Texture2D;

			// Format string.
			format = EditorGUILayout.TextField("Format", format);

			// Arguments array.
			EditorGUILayout.PropertyField(serializedModel.FindProperty("arguments"), new GUIContent("Arguments"), true);
			serializedModel.ApplyModifiedProperties();

			if (GUILayout.Button("Preview"))
			{ Apply(true); }

			if (GUILayout.Button("Rename slices"))
			{ Apply(false); }

			// Status.
			scroll = EditorGUILayout.BeginScrollView(scroll);
			EditorGUILayout.LabelField(status, EditorStyles.helpBox);
			EditorGUILayout.EndScrollView();
		}

		// Apply settings.
		void Apply(bool preview)
		{
			// Error.
			if (texture == null)
			{
				status = "Drag a texture into the slot.";
				return;
			}

			// Locate asset, get meta.
			path = AssetDatabase.GetAssetPath(texture);
			textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			SpriteMetaData[] sliceMetaData = textureImporter.spritesheet;

			// Error.
			if (sliceMetaData == null || sliceMetaData.Length == 0)
			{
				status = "Seems no slices defined in texture.";
				return;
			}

			// Naming loop.
			int index = 0;
			status = "";
			if (model.arguments.Count > 3) { status += "Exceeded argument limit of 3. Only first 3 arguments will be used.\n\n"; }
			foreach (SpriteMetaData eachSliceMetaData in sliceMetaData)
			{
				string eachName = "";
				
				// Create string.
				switch (model.arguments.Count)
				{
				case 0: eachName = format; break;
				case 1: eachName = string.Format(format, model.arguments[0].valueForIndex(index)); break;
				case 2: eachName = string.Format(format, model.arguments[0].valueForIndex(index), model.arguments[1].valueForIndex(index)); break;
				case 3: eachName = string.Format(format, model.arguments[0].valueForIndex(index), model.arguments[1].valueForIndex(index), model.arguments[2].valueForIndex(index)); break;
				}
				
				// Assemble name.
				string verb = (preview) ? "Rename" : "Renamed";
				if (index > 0) status += "\n";
				status += verb+" `"+eachSliceMetaData.name+"` to `"+eachName+"`.";
				
				// Assign.
				if (preview == false)
				{ sliceMetaData[index].name = eachName; }
				
				index++;
			}

			// Branding.
			status += "\n\nBrought to you by @_eppz";

			// Apply.
			if (preview == false)
			{
				// Save settings.
				textureImporter.spritesheet = sliceMetaData;
				EditorUtility.SetDirty(textureImporter);
				textureImporter.SaveAndReimport();
				
				// Reimport asset.
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
		}


		// Header images.
		private static class Images
		{
			public const string backgroundEncoded = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAAHklEQVQoFWOMiIhgIAUwkaIYpHZUAzEhNhpKgyOUAFqtASiOgjFIAAAAAElFTkSuQmCC";
			public const string headerEncoded = @"iVBORw0KGgoAAAANSUhEUgAAAgAAAAAgCAIAAADrKVeyAAAU/ElEQVR4Ae3dd5BVxbYG8DOJIaiAwCBREBUxYs6KevWZc/bpM6cyVvlK/UPLVNdSS31l6dNSy5y9ZZlzfmbFHMEsCAKKYAIGZt6vd5+z2XPAQSmvdy72vt49vbtXr1797bO/tXr1Poeahgv+u6amVGotlWpKrc5zj6yq1FoTmjXFy7nNlVJZLLsMkpku56i0tNvocVllOiUEEgIJ
gYRAx0Kgljm4HYtX2F8xkHflqAmtlbpKfRAvEH2Jk8j8RJSrSAUVxXJFZfqbEEgIJAQSAh0AAQ6gwu6ZNTWBzMt2KSjGBUCsIhobM6FQjJdhfdCqHKqDulDdWlQVu6dzQiAhkBBICHQcBOojh2esHbg+BPtlCo/cPjf8Z3QmoDksCyqX/mY1oSIUA/eXi5VSdplOCYGEQEIgIdChECivAFB1iPTRd4jbg4VIXFwfqL1sb2D1QkVsD22VHlm5vAKg
pNIvVKcjIZAQSAgkBDocAmEPIGP7EOlnpB5ZPtB9RuKxOrB8qAlnuZ1w5BSfiWY1wUMolAVCIXmBAEw6EgIJgYRAR0SgNuN0PB2YvfJfmbYzNi83Zd4hSDiy8txkfz6tKEOVQlwrZBry9n/XwuKLL94RTO/evXvXrl07giXJhoRAQmDRQKAWWcc4XUQfg3qXWSHWZ/F9JlEVzhecQQj8o9MoyMSKKPVvjBUoTjjhhOWWW+5fO4devXodddRRhx56
aENDw7/WkjR6QiAhsMggUN4EjvNpG7a3ifHra+q6NjQ01tfXZVQ/u7Xll9nNPzc3VzZ9Rf3hqIT82F9F9AFR9z/xjKPXWmutTp06Pf/884apq6urra1tbm7+o4bs27fvHx56NzY2zpw587db2Llz54EDB5qUqZnv2muvvf322z/88MMvvPDCb1eSJBMCCYGEQBGB+kDVGXlXuDu8vlmoqME3q/fpv9GAIWv3G7Rsj17d6hu0fjdzxruTvn70i7HP
jv9s6swZMfBvzZP/YYQ/zwfgxJ49e3br1i1ObPnllx82bNj9999fnGeHKvfu3Xurrba65557fvrpp4UwjDNYaaWVll122S222CI5gIUAMHVJCCQEIgL1ketjtF6m/kjdqmpqutbVH7HqevuvvPbw3k31tXVF1NYftMyuI0Ze+9ZLF4/+v8kzfolNWVcqy+xfyAgVu/7B5Tlz5jz77LO50qWXXnr99dfvyA5gyJAhlixsXjgHYOnw4osvOr/55pv5
rFMhIZAQSAj8XgTq6rbaMLK/nvjaf5WlQE1Dbe2xq294ygZ/G9S9Z21NeF/ojU/H7nTuaf179lq+/0CXXRs6rdrUf/yUyW99O6EljJxH/cGt8B9UrThhemhp99hggw3WWWedPn36fPPNN7NnzyYrQzJo0KBp06YNHjx4o402Wmqppb777rtZs2ZFNZI8wvxvv/12lVVW0feLL76QpVGJT23Yrrvuuv37958wYQJVv/wSPBNLJPF5hX79+hX1VBll
0JEjR6633nrEfvjhh7zvtttu+/rrr8vCb7LJJuyZOnVqbMq7C8Y33HDDAQMGTJkyJTcytrLQuBYo48ePjzVLLrnkqquuyh6TNYUqeTJyWczQa5lllvkxO1QuscQSG2+8cUtLy+OPP25eM2bMsA54++231US1ENh8883B4pKFWTYvtpQsF2gbOnSo4XQs17b9E/FceeWVeaavv/46JtB0Ab4mRroXsUePHj3M10zzVsP9/PPPuT73cdNNN11ttdW6
dOmiKd5Qd4fbcyMUtELAio0SNSuuuCL0JLjMtegRdSfmbsI8t5w8/TZC6uvrt9xyS7ds0qRJ+dCpkBBICPwuBOwB2OyNX+zKSQOPBy5fp+/A/1p57e6NXXKN1zzx4IdffTn267k/77NYY+fdho64e/RLk7t1ag0+IvSN58yRZAuMrGq+p6ampiOOOMLDL571VO+www4XXnjhxIkTPfmHHXbYM888gxoE+JI8++6772233SbLj1Bw9F577YXKsR7q
efnll/EUgr700ktHjRqFpqk65phj8ONll12GR2yfjhgxAvfRs99++9Ej+qa2aJIuRx99dBRDLltvvfWNN9747rvvkqFhn332QbVG9CoOf3DLLbcYVJPLAw44ANkxA7XtscceemkibGp2j2V7jEv5/vvvf/nll7/33ntoy0GnGeHWq6++umgJeTu93AYNrKXwvvvuu/vuu4umKoPFuCj15ptv5jDsB+y6667ZfWzVi7tiBgdD27HHHsvZ0KZ17733
vv322x977LFIyrlOIwJw3LhxbgRt2N8KgzAwo4zuHA/ceILddtuNp3nnnXdgZcqOnXfe+aqrrnrllVcMseeee2633XZs0EXTZ599dt1113388cfcxmmnncYro29zJ+B46623vv/+e/crCnOr9OToHXfccVxgvPs77rjj9ddfP3r06MUWW+ycc84xNXroZAazKcnnEgsscROVi1tBRlRJoSOKxRqjVwFSpS1dJgQWVQTCQxKZOsv/hOxNDOMb6+q3
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";
		}
	}
}