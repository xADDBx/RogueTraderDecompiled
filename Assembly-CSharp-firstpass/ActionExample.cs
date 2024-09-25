using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionExample : MonoBehaviour
{
	[Serializable]
	public class SimpleObject
	{
		public string stringProperty;

		public float floatProperty;

		public GameObject objectProperty;

		public void Reset()
		{
			stringProperty = "";
			floatProperty = 0f;
			objectProperty = null;
		}
	}

	public List<SimpleObject> simpleObjects;
}
