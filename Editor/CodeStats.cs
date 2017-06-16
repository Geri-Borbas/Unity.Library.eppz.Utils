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
using System.Linq;
using System.Collections.Generic;


namespace EPPZ.Utils.Editor
{


	public class CodeStats : EditorWindow 
	{


		System.Text.StringBuilder log;
		Vector2 scrollPosition = new Vector2(0,0);


		public class FileStats
		{
			public string name;
			public int lineCount;
			public int statementCount;
			public int usingStatementCount;
			public int ifStatementCount;


			public FileStats(string name)
			{
				this.name = name;
			}

			public void ProcessLine(string line)
			{
				lineCount++;
				if (ContainsStatement(line)) statementCount++;
				if (ContainsUsingStatement(line)) usingStatementCount++;
				if (ContainsIfStatement(line)) ifStatementCount++;
			}

			bool ContainsStatement(string line)
			{
				return (
					line.Contains(";") ||
					line.Contains("}")
				);
			}

			bool ContainsUsingStatement(string line)
			{ return line.Contains("using"); }

			bool ContainsIfStatement(string line)
			{ return line.Contains("if"); }
		}	


		void OnGUI()
		{
			if (GUILayout.Button("Recalculate"))
			{
				CalculateStatistics();
			}
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			EditorGUILayout.HelpBox(log.ToString(),MessageType.None);
			EditorGUILayout.EndScrollView();
		}


		[MenuItem("Window/eppz!/Code Stats")]
		public static void Init()
		{
			CodeStats window = EditorWindow.GetWindow<CodeStats>("Code Stats");
			window.Show();
			window.Focus();
			window.CalculateStatistics();
		}

		void CalculateStatistics()
		{		
			string folderPath = System.IO.Directory.GetCurrentDirectory();
			folderPath += @"/Assets";

			// Per file statistics.
			List<FileStats> codeStat = CodeStatsForFolder(folderPath);	

			// Overall.
			int totalLineCount = 0;
			int totalStatementCount = 0;
			foreach(FileStats eachFileStat in codeStat)
			{
				totalLineCount += eachFileStat.lineCount;
				totalStatementCount += eachFileStat.statementCount;
			}
			int averageLineCount = totalLineCount / codeStat.Count;
			int averageStatementCount = totalStatementCount / codeStat.Count;

			// Create new string.
			log = new System.Text.StringBuilder();
			log.Append("File count: " + codeStat.Count + "\n");
			log.Append("Line count: " + totalLineCount + "\n");
			log.Append("Statement count: " + totalStatementCount + "\n");
			log.Append("Average line count: " + averageLineCount + "\n");
			log.Append("Average statement count: " + averageStatementCount + "\n");

			int barLength = 50;

			// Order by statement count.
			List<FileStats> statementCountSortedCodeStat = codeStat.OrderBy(fileStat => fileStat.statementCount).ToList();
			int maximumStatementCount = statementCountSortedCodeStat.Last().statementCount;

			// Log per file statistics.
			foreach(FileStats eachFileStat in statementCountSortedCodeStat)
			{
				// A bar representing line count.
				int displayValue = Mathf.FloorToInt(((float)eachFileStat.statementCount / (float)maximumStatementCount) * (float)barLength);
				string bar = "";
				for (int i = 0; i < barLength; i++)
				{ bar += (i < displayValue) ? "█" : "░"; }

				log.Append(bar + " " + eachFileStat.name.Replace(folderPath, "") + ": " + eachFileStat.statementCount + " (" + eachFileStat.lineCount + ")\n");
			}	
		}

		static List<FileStats> CodeStatsForFolder(string folderPath)
		{	
			List<FileStats> codeStat = new List<FileStats>();

			string[] fileNames = System.IO.Directory.GetFiles(folderPath, "*.cs");
			foreach (string eachFileName in fileNames)
			{ codeStat.Add(FileStatForFile(eachFileName)); }

			// Collect from subfolders.
			string[] folders = System.IO.Directory.GetDirectories(folderPath);
			foreach (string eachFolder in folders)
			{ codeStat.AddRange(CodeStatsForFolder(eachFolder)); }

			return codeStat;
		}

		static FileStats FileStatForFile(string fileName)
		{
			FileStats fileStats = new FileStats(fileName);			
			System.IO.StreamReader reader = System.IO.File.OpenText(fileName);
			while (reader.Peek() >= 0)
			{ fileStats.ProcessLine(reader.ReadLine()); }
			reader.Close();			
			return fileStats;
		}	
	}
}