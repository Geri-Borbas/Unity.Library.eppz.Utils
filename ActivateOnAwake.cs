using UnityEngine;
using System.Collections;


namespace EPPZ.Utils
{


	public class ActivateOnAwake : MonoBehaviour
	{


		public GameObject[] targets;


		void Awake()
		{
			foreach (GameObject eachTarget in targets)
			{
				if (eachTarget == null) continue;
				eachTarget.SetActive(true);
			}
		}
	}
}
