﻿//
// Copyright (c) 2017 Geri Borbás http://www.twitter.com/_eppz
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


namespace EPPZ.Utils
{


	[InitializeOnLoad]
	public class ExecutionOrders
	{


		static ExecutionOrders()
		{
			foreach (MonoScript eachMonoScript in MonoImporter.GetAllRuntimeMonoScripts())
			{
				if (eachMonoScript.GetClass() != null)
				{
					Attribute[] eachAttributes = Attribute.GetCustomAttributes(eachMonoScript.GetClass(), typeof(ExecutionOrder));
					foreach (ExecutionOrder eachExecutionOrder in eachAttributes)
					{
						// Get orders.
						int currentExecutionOrder = MonoImporter.GetExecutionOrder(eachMonoScript);
						int newExecutionOrder = ((ExecutionOrder)eachExecutionOrder).executionOrder;

						// Set if override.
						if (currentExecutionOrder != newExecutionOrder)
						{ MonoImporter.SetExecutionOrder(eachMonoScript, newExecutionOrder); }
					}
				}
			}
		}
	}
}