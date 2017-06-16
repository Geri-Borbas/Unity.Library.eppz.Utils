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