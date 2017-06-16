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
using System.Collections.Generic;


// Clip Surgery 0.9
// Easily fix broken paths, remap animation targets / properties, copy keyframe tangents, or copy entire curves.
//
// Next: Embed images, Clipboard icons, Header.
// Improvements:
// - Apply only changed curves !!!!
// - Remove original curves (instead of remove all)
// - Hook up http://docs.unity3d.com/ScriptReference/AnimationUtility-onCurveWasModified.html
// - Fix interpolation wroomer
// - Copy Curve / Paste Curve at the right of the curve cell.
// - Curve editor should be closer to the curve cell (Unfold below?)
// - Keyboard Shortcuts to clipboard functions
// - Indicate what is copied (New keyframe color)
// - Indicate if there is unapplied changes (color, or `*` at least)


namespace EPPZ.Utils.Editor
{


	public class ClipSurgery : EditorWindow
	{


		// Sizes.
		const float identationSize = 16.0f;
		const float padding = 8.0f;
		const float iconSize = 16.0f;
		const float keyframeIconSize = 11.0f;
		const float labelWidth = 64.0f;

		const float keyframesAreaHeight = 20.0f;
		const float keyframesLineHeight = 4.0f;
		const float keyframesMarkerWidth = 1.0f;

		static Texture2D _lightGrayTexture;
		static Texture2D lightGrayTexture
		{
			get
			{
				if (_lightGrayTexture == null)
				{ _lightGrayTexture = Resources.Load("lightGray") as Texture2D; }
				return _lightGrayTexture;
			}
		}

		static Texture2D _blueTexture;
		static Texture2D blueTexture
		{
			get
			{
				if (_blueTexture == null)
				{ _blueTexture = Resources.Load("blue") as Texture2D; }
				return _blueTexture;
			}
		}

		static Texture2D _transparentGrayTexture;
		static Texture2D transparentGrayTexture
		{
			get
			{
				if (_transparentGrayTexture == null)
				{ _transparentGrayTexture = Resources.Load("transparentGray") as Texture2D; }
				return _transparentGrayTexture;
			}
		}

		static Texture2D _lineTexture;
		static Texture2D lineTexture
		{
			get
			{
				if (_lineTexture == null)
				{ _lineTexture = Resources.Load("line") as Texture2D; }
				return _lineTexture;
			}
		}

		static Texture2D _keyframeTexture;
		static Texture2D keyframeTexture
		{
			get
			{
				if (_keyframeTexture == null)
				{ _keyframeTexture = Resources.Load("Keyframe") as Texture2D; }
				return _keyframeTexture;
			}
		}

		static Texture2D _keyframeSelectedTexture;
		static Texture2D keyframeSelectedTexture
		{
			get
			{
				if (_keyframeSelectedTexture == null)
				{ _keyframeSelectedTexture = Resources.Load("Keyframe (selected)") as Texture2D; }
				return _keyframeSelectedTexture;
			}
		}

		static GUIStyle lightGrayBackgroundStyle
		{
			get
			{
				GUIStyle style = new GUIStyle();
				style.normal.background = lightGrayTexture;
				return style;
			}
		}

		static GUIStyle blueBackgroundStyle
		{
			get
			{
				GUIStyle style = new GUIStyle();
				style.normal.background = blueTexture;
				return style;
			}
		}

		static GUIStyle smallFontStyle
		{
			get
			{
				GUIStyle style = new GUIStyle();
				style.fontSize = 9;
				style.alignment = TextAnchor.MiddleLeft;
				style.normal.textColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
				return style;
			}
		}

		static GUIStyle defaultStyle
		{
			get
			{
				return new GUIStyle();
			}
		}

		static Texture2D ColorTexture(Color color)
		{
			int size = 16;

			// Pixels.
			Color[] pixels = new Color[size*size];
			for(int i = 0; i < pixels.Length; i++)
			{ pixels[i] = color; }

			// Texture.
			Texture2D texture = new Texture2D(size, size);
			texture.SetPixels(pixels);
			texture.Apply();

			return texture;
		}


		// Model.
		GameObject _gameObject; // The `GameObject` using the `AnimationClip` 
		AnimationClip _clip; // The `AnimationClip` to edit
		List<Curve> _curves; // Animated properties (collected whenever clip changes)

		// UI.
		GameObject _previousGameObject; // To track if changed
		AnimationClip _previousClip; // To track if changed
		Vector2 _scrollPosition;

		// Temporary data.
		private class Clipboard
		{
			public Curve curve;
			public float inTangent;
			public float outTangent;
		}
		static Clipboard __clipboard;
		static Clipboard _clipboard
		{
			get
			{
				if (__clipboard == null) { __clipboard = new Clipboard(); }
				return __clipboard;
			}
		}
		static Curve _selectedCurve;
		static int _selectedKeyframeIndex;


		#region Model

		private class Curve
		{


			// References to outer scope.
			AnimationClip clip_;
			GameObject gameObject_;

			// Animation.
			private EditorCurveBinding binding;
			public string path { get { return binding.path; } }
			public string propertyName { get { return binding.propertyName; } }
			public AnimationCurve animationCurve;
			public System.Type type;

			private bool _changed;
			public bool changed { get { return _changed; } }

			// Processed.
			public float startTime;
			public float endTime;
			public string startTimeString;
			public string endTimeString;
			public float duration;

			// Drawing.
			public string niceName;
			public int indentation;
			public Color color;

			// UI State.
			public bool selected;

			private string[] paths;
			private string lastPathComponent;


			public Curve(AnimationClip clip_, GameObject gameObject_, EditorCurveBinding binding)
			{
				this.clip_ = clip_;
				this.gameObject_ = gameObject_;
				this.binding = binding;
				this.animationCurve = AnimationUtility.GetEditorCurve(clip_, binding);

				// Processing.
				_changed = false;
				ProcessPath();
				UpdateNiceName();
				ProcessKeyframes();
			}

			void UpdateNiceName()
			{
				// Defaults.
				string objectName = lastPathComponent;
				string typeName = "Object";
				string niceTypeName = "Object";
				string missing = " (Missing!)";
				color = Color.yellow;
				type = typeof(ScriptableObject);

				Object animatedObject = AnimationUtility.GetAnimatedObject(gameObject_, binding);
				if (animatedObject)
				{
					type = animatedObject.GetType();
					color = Color.black;
					niceTypeName = ObjectNames.NicifyVariableName(type.Name);
					objectName = animatedObject.name;
					missing = "";
				}

				// Nicify names.
				string nicePropertyName = ObjectNames.NicifyVariableName(binding.propertyName);
				string nicePathName = niceTypeName+"."+nicePropertyName;

				// Supress Transform names.
				if (typeName == "Transform" ||
					typeName == "RectTransform")
				{ nicePathName = nicePropertyName.Replace("Local ", ""); }

				// Indicate change.
				string changeIndicator = (_changed) ? " *" : "";

				// Assemble.
				niceName = objectName+" : "+nicePathName+missing+changeIndicator;
			}

			void ProcessKeyframes()
			{
				if (animationCurve == null) return; // Only if any

				startTime = Mathf.Infinity;
				endTime = 0.0f;
				foreach (Keyframe eachKeyframe in animationCurve.keys)
				{
					if (eachKeyframe.time < startTime) startTime = eachKeyframe.time;
					if (eachKeyframe.time > endTime) endTime = eachKeyframe.time;
				}
				duration = endTime - startTime;
				startTimeString = Mathf.FloorToInt(startTime)+":"+ Mathf.FloorToInt((startTime - Mathf.Floor(startTime)) * clip_.frameRate).ToString("00");
				endTimeString = Mathf.FloorToInt(endTime)+":"+ Mathf.FloorToInt((endTime - Mathf.Floor(endTime)) * clip_.frameRate).ToString("00");
			}

			void ProcessPath()
			{
				this.paths = this.path.Split('/');
				this.lastPathComponent = paths[paths.Length - 1];
				this.indentation = paths.Length + 1;
			}

			public void UpdatePath(string path_)
			{
				this.binding.path = path_;
				ProcessPath();
				UpdateNiceName();

				_changed = true;
			}

			public void UpdatePropertyName(string propertyName)
			{
				this.binding.propertyName = propertyName;
				UpdateNiceName();

				_changed = true;
			}

			public void PasteCurveData(Curve curve)
			{
				if (curve == null) return; // Only if any
				this.animationCurve = curve.animationCurve;
				ProcessKeyframes();

				_changed = true;
			}

			public void PasteKeyframeTangentsAtKeyframeIndex(float inTangent, float outTangent, int keyframeIndex)
			{
				Keyframe keyframe = animationCurve.keys[keyframeIndex];

				keyframe.inTangent = inTangent;
				keyframe.outTangent = outTangent;

				animationCurve.RemoveKey(keyframeIndex);
				animationCurve.AddKey(keyframe);

				_changed = true;
			}

			public void ApplyIfChanged()
			{
				if (_changed == false) return; // Only if changed

				AnimationUtility.SetEditorCurve(
					clip_,
					binding,
					animationCurve
				);

				_changed = false;
			}
		}

		// Create a dictionary of properties keyed by property path.
		void CollectCurves()
		{
			EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(_clip);

			// Create `Property` collection.
			_curves = new List<Curve>();
			foreach (EditorCurveBinding eachBinding in bindings)
			{
				Curve eachCurve = new Curve(_clip, _gameObject, eachBinding);
				_curves.Add(eachCurve);
			}

			// Sort by `indentation` (preserving original order otherwise).
			List<Curve> _original = new List<Curve>(_curves);
			_curves.Sort(delegate(Curve one, Curve other)
			{
				if (one.indentation == other.indentation) return _original.IndexOf(one).CompareTo(_original.IndexOf(other)); 
				return one.indentation.CompareTo(other.indentation);
			});

			// Select first curve.
			_curves[0].selected = true;
			_selectedCurve = _curves[0];
		}

		void ModelChanged()
		{
			Repaint();
		}

		// Apply to clip.
		void Apply()
		{
			foreach (Curve eachCurve in _curves)
			{ eachCurve.ApplyIfChanged(); }
		}

		void CopyKeyframeTangents(int keyframeIndex)
		{
			Keyframe keyframe = _selectedCurve.animationCurve.keys[keyframeIndex]; // Cast
			_clipboard.inTangent = keyframe.inTangent;
			_clipboard.outTangent = keyframe.outTangent;
		}

		void PasteKeyframeTangents(int keyframeIndex)
		{
			_selectedCurve.PasteKeyframeTangentsAtKeyframeIndex(_clipboard.inTangent, _clipboard.outTangent, keyframeIndex);
		}

		#endregion


		#region UI

		[MenuItem("Window/eppz!/Clip Surgery")]
		public static void ShowWindow()
		{ EditorWindow.GetWindow(typeof(ClipSurgery), false, "Clip Surgery"); }

		void OnGUI()
		{
			bool invalid = false;

			// Get object.
			_previousGameObject = _gameObject;
			_gameObject = EditorGUILayout.ObjectField(_gameObject, typeof(GameObject), true) as GameObject;
			if (_gameObject == null)
			{
				EditorGUILayout.HelpBox("Select the GameObject from the Scene using the Animation Clip below.", MessageType.Info, true);
				invalid = true;
			}

			// Get clip.
			_previousClip = _clip;
			_clip = EditorGUILayout.ObjectField(_clip, typeof(AnimationClip), false) as AnimationClip;
			if (_clip == null)
			{
				EditorGUILayout.HelpBox("Select Animation Clip to edit.", MessageType.Info, true);
				invalid = true;
			}

			// Early return if invalid.
			if (invalid)
			{
				_selectedCurve = null;
				return;
			}

			// Collect properties if clip has changed.
			if (_clip != _previousClip ||
				_gameObject != _previousGameObject)
			{
				CollectCurves();
				Repaint();
			}

			// Apply changes on actual clip.
			if (GUILayout.Button("Apply changes to Clip"))
			{ Apply(); }

			// Draw properties.
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			DrawCurves();
			EditorGUILayout.EndScrollView();

			// Draw curve editor.
			DrawCurveEditor();
		}

		void DrawCurves()
		{
			if (_curves == null) return; // Only if any

			// Draw properties.
			for (int index = 0; index < _curves.Count; index++)
			{
				// Model.
				Curve eachCurve = _curves[index];

				// Background color.
				bool odd = (index % 2 == 0);
				GUIStyle propertyRowStyle = (eachCurve.selected) ? blueBackgroundStyle : ((odd) ? lightGrayBackgroundStyle : defaultStyle);

				// Property row.
				Rect propertyRowArea = EditorGUILayout.BeginHorizontal(propertyRowStyle, GUILayout.ExpandWidth(true));

					// Identation.
					float indentationWidth = identationSize * eachCurve.indentation;
					GUILayout.Space(indentationWidth + iconSize);

					// Draw icon with nice name (similar to Animation Window).
					DrawIconForCurve(eachCurve, propertyRowArea, iconSize, indentationWidth);
					GUIStyle style = new GUIStyle();
					style.normal.textColor = eachCurve.color;
					EditorGUILayout.LabelField(eachCurve.niceName, style);

					// Select on click.
					if (IsRectClicked(propertyRowArea))
					{
						// Deselect previous, select clicked.
						_selectedCurve.selected = false;
						eachCurve.selected = true;
						_selectedCurve = eachCurve;
						Repaint();
					}

				EditorGUILayout.EndHorizontal();
			}
		}

		bool IsRectClicked(Rect rect)
		{
			if (Event.current == null) return false;
			if (Event.current.type != EventType.MouseDown) return false;
			if (Event.current.button != 0) return false;
			return rect.Contains(Event.current.mousePosition);
		}

		void DrawIconForCurve(Curve curve, Rect area, float width, float padding)
		{
			// Get.
			GUIContent content = EditorGUIUtility.ObjectContent(null, curve.type);
			Texture icon = content.image;
			if (icon == null)
			{
				content = EditorGUIUtility.ObjectContent(null, typeof(ScriptableObject));
				icon = content.image;
			}

			// Desired size.
			float iconHeight = width - 1.0f - 1.0f; // Row height.
			float iconAspect = (icon) ? icon.width / icon.height : 1.0f;
			float iconWidth = iconHeight * iconAspect;

			// Draw.
			if (icon)
			{
				GUI.DrawTexture(new Rect(
					area.x + padding + 1.0f,
					area.y + 1.0f,
					iconWidth,
					iconHeight
				), icon);
			}
		}

		void DrawCurveEditor()
		{
			if (_selectedCurve == null) return; // Only if any selected

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.Space(padding);

				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Space(padding);

					EditorGUILayout.LabelField("Path", GUILayout.Width(labelWidth));
					_selectedCurve.UpdatePath(EditorGUILayout.TextField(_selectedCurve.path));
					if (GUILayout.Button("Get From Scene Selection") && 
						Selection.activeTransform != null)
					{
						_selectedCurve.UpdatePath(AnimationUtility.CalculateTransformPath(Selection.activeTransform, Selection.activeTransform.root));
						Repaint();
					}

				GUILayout.Space(padding);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Space(padding);

					EditorGUILayout.LabelField("Property", GUILayout.Width(labelWidth));
					_selectedCurve.UpdatePropertyName(EditorGUILayout.TextField(_selectedCurve.propertyName));
				
				GUILayout.Space(padding);
				EditorGUILayout.EndHorizontal();

				// Curve editor box.
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.Space(padding);

					// Timestamps.
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Space(padding);

						GUIStyle style = smallFontStyle;
						EditorGUILayout.LabelField(_selectedCurve.startTimeString, style);
						GUILayout.FlexibleSpace();
						style.alignment = TextAnchor.MiddleRight;
						EditorGUILayout.LabelField(_selectedCurve.endTimeString, style);
					
					GUILayout.Space(padding);
					EditorGUILayout.EndHorizontal();

					// Keyframes.
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(keyframesAreaHeight));
					GUILayout.Space(padding);

						Rect keyframesArea = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(keyframesAreaHeight));	
						GUILayout.Space(padding);

							// Draw grid.
							GUI.DrawTexture(new Rect(keyframesArea.x, keyframesArea.y, keyframesArea.width, keyframesLineHeight), lineTexture);
							GUI.DrawTexture(new Rect(keyframesArea.x, keyframesArea.y + keyframesAreaHeight - keyframesLineHeight, keyframesArea.width, keyframesLineHeight), lineTexture);
							GUI.DrawTexture(new Rect(keyframesArea.x, keyframesArea.y, keyframesMarkerWidth, keyframesArea.height), transparentGrayTexture);
							GUI.DrawTexture(new Rect(keyframesArea.x + keyframesArea.width - keyframesMarkerWidth, keyframesArea.y, keyframesMarkerWidth, keyframesArea.height), transparentGrayTexture);

							// Actual keyframes.
							if (_selectedCurve.animationCurve != null)
							for (int eachKeyframeIndex = 0; eachKeyframeIndex < _selectedCurve.animationCurve.keys.Length; eachKeyframeIndex++)
							{
								// Model.
								Keyframe eachKeyframe = _selectedCurve.animationCurve.keys[eachKeyframeIndex];

								// Positioning.
								float left = (keyframesArea.width - 1.0f) * ((eachKeyframe.time - _selectedCurve.startTime) / _selectedCurve.duration);

								Rect eachKeyframeRect = new Rect(
									keyframesArea.x + left - Mathf.Floor(keyframeIconSize / 2.0f),
									keyframesArea.y + 5.0f,
									keyframeIconSize,
									keyframeIconSize
								);

								Rect eachKeyframeTextureRect = new Rect(
									eachKeyframeRect.x,
									eachKeyframeRect.y,
									iconSize,
									iconSize
								);

								// Draw.
								Texture2D eachTexture = keyframeTexture;
								bool isKeyframeSelected = (_selectedKeyframeIndex == eachKeyframeIndex);
								if (isKeyframeSelected) eachTexture = keyframeSelectedTexture;
								GUI.DrawTexture(eachKeyframeTextureRect, eachTexture);

								// Events.
								if (IsRectClicked(eachKeyframeRect))
								{
									// Select clicked.
									_selectedKeyframeIndex = eachKeyframeIndex;
									Repaint();
								}
							}

						GUILayout.Space(padding);
						EditorGUILayout.EndHorizontal();	
					
					GUILayout.Space(padding);
					EditorGUILayout.EndHorizontal();

					// Tangent Clipboard.
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Space(padding);

						if (GUILayout.Button("Copy Keyframe Tangents"))
						{
							CopyKeyframeTangents(_selectedKeyframeIndex);
						}

						bool keysChanged = false;
						if (GUILayout.Button("Paste Keyframe Tangents"))
						{
							if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint) 
							{
								PasteKeyframeTangents(_selectedKeyframeIndex); 
								keysChanged = true;
								ModelChanged();
							}
						}

					GUILayout.Space(padding);
					EditorGUILayout.EndHorizontal();

					// Actual curve.
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Space(padding);

						// Temporary model to avoid `CurveField` cache.
						AnimationCurve curve = _selectedCurve.animationCurve;

						if (keysChanged)
						{
							curve = new AnimationCurve();
							curve.keys = _selectedCurve.animationCurve.keys;
							curve.preWrapMode = _selectedCurve.animationCurve.preWrapMode;
							curve.postWrapMode = _selectedCurve.animationCurve.postWrapMode;
						}

						_selectedCurve.animationCurve = EditorGUILayout.CurveField(curve, GUILayout.Height(40.0f));
					
					GUILayout.Space(padding);
					EditorGUILayout.EndHorizontal();

					// Curve Clipboard.
					EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Space(padding);

						if (GUILayout.Button("Copy Entire Curve"))
						{
							_clipboard.curve = _selectedCurve;
						}

						if (GUILayout.Button("Paste Entire Curve"))
						{
							_selectedCurve.PasteCurveData(_clipboard.curve); 
							ModelChanged();
						}

					GUILayout.Space(padding);
					EditorGUILayout.EndHorizontal();
						
				GUILayout.Space(padding);
				EditorGUILayout.EndVertical(); // End of Keyframes

			EditorGUILayout.EndVertical(); // End of Curves
		}

		#endregion

	}
}