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


namespace EPPZ.Utils.Editor
{


		public class ScriptSurgery : EditorWindow
		{

				// Properties.
				[System.Serializable] public class Model : ScriptableObject
				{
						[System.Serializable] public class GUIDPair
						{
								public string previousGUID = "eddbe5f3f32be4ec98e40d4108ec673e";
								public string newGUID = "00997064b684b476cade2ba65d3f1c41";
						}

						public List<GUIDPair> GUIDs = new List<GUIDPair>();
				}
				public static Model model;

				// GUI.
				private static SerializedObject serializedModel;
				private Vector2 scroll;

				private int count { get; set; }
				private string verb { get; set; }
				private string report { get; set; }
				private string status { get; set; }

				// Asset.
				private string path;
				private TextureImporter textureImporter;


				// Window.
				[MenuItem("Window/eppz!/Script Surgery")]
				public static void ShowWindow()
				{
						// Show window.
						EditorWindow.GetWindow(typeof(ScriptSurgery), false, "Script Surgery");
				}

				void OnEnable()
				{
						// Model setup.
						model = new Model();
						serializedModel = new SerializedObject(model);
				}

				// GUI.
				void OnGUI()
				{
						// GUID pair array.
						EditorGUILayout.PropertyField(serializedModel.FindProperty("GUIDs"), new GUIContent("GUIDs"), true);
						serializedModel.ApplyModifiedProperties();

						if (GUILayout.Button("Preview"))
						{
							status = ""; report = ""; count = 0; // Reset
							Apply(true);
							WriteReportFile();
						}

						if (GUILayout.Button("Replace GUIDs"))
						{
							status = ""; report = ""; count = 0; // Reset
							Apply(false);
							WriteReportFile();
						}

						System.IO.File.WriteAllText(Application.dataPath+"report.txt", report);

						// Status (preserving scroll state).
						scroll = EditorGUILayout.BeginScrollView(scroll);
						EditorGUILayout.TextArea(status, EditorStyles.helpBox);
						EditorGUILayout.EndScrollView();
				}

				// Apply settings.
				void Apply(bool preview)
				{
						// Error.
						if (model.GUIDs.Count == 0)
						{
								status = "Please specify GUID pairs to replace.";
								return;
						}

						// Report.
						verb = (preview) ? "Should replace" : "Replaced";

						// Collect scene and prefab files.
						List<FileInfo> metaFileInfoList = new List<FileInfo>();
						CollectFilesOfType(metaFileInfoList, Application.dataPath, "*.prefab");
						CollectFilesOfType(metaFileInfoList, Application.dataPath, "*.unity");

						foreach (FileInfo eachMetaFileInfo in metaFileInfoList) 
						{
							foreach (Model.GUIDPair eachGUIDpair in model.GUIDs)
							{ ReplaceGUIDinFile(eachGUIDpair.previousGUID, eachGUIDpair.newGUID, eachMetaFileInfo, preview); }
						}

						// Branding.
						status += verb+" ("+count+") GUID occurences. \n";
						status += "Check `ScriptSurgery_report.txt` in project folder for further details. \n";
						status += "Brought to you by @_eppz";
				}

				void CollectFilesOfType(List<FileInfo> fileInfoList, string directoryPath, string fileExtension)
				{
					// Enumerate directories.
					string[] directoryPaths = Directory.GetDirectories(directoryPath);
					foreach (string eachDirectoryPath in directoryPaths)
					{ CollectFilesOfType(fileInfoList, eachDirectoryPath, fileExtension); }

					// Enumerate files.
					DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
					FileInfo[] metaFileInfos = directoryInfo.GetFiles(fileExtension);
					foreach (FileInfo eachMetaFileInfo in metaFileInfos)
					{ fileInfoList.Add(eachMetaFileInfo); } // Collect

				}

				void ReplaceGUIDinFile(string previousGUID, string newGUID, FileInfo fileInfo, bool preview)
				{
					string[] lines = File.ReadAllLines(fileInfo.FullName);
					bool changed = false;
					for(int i = 0; i < lines.Length; i++)
					{
						if(lines[i].Contains("guid: "))
						{
							int index = lines[i].IndexOf("guid: ") + 6;
							string foundGUID = lines[i].Substring(index, 32);

							bool reportGUIDoccurences = false;
							if (reportGUIDoccurences)
							{ report += "Found GUID `" + foundGUID + "` in `" + fileInfo.Name + "`. \n"; }

							if (previousGUID == foundGUID)
							{
								lines[i] = lines[i].Replace(foundGUID, newGUID);
								report += verb+" GUID `"+foundGUID+"` with GUID `"+newGUID+"` in file `"+fileInfo.Name+"` \n";
								count++;
								changed = true;
							}
						}
					}
					
					// Write changes if any.
					if (changed && !preview)
					{ File.WriteAllLines(fileInfo.FullName, lines); }
				}

				void WriteReportFile()
				{
					System.IO.File.WriteAllText(Application.dataPath+"/../ScriptSurgery_report.txt", report);
				}
		}
}