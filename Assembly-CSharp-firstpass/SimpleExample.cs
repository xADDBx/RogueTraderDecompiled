using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleExample : MonoBehaviour
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

	private static SimpleExample instance;

	public List<SimpleObject> simpleObjects;

	public static SimpleExample Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<SimpleExample>();
			}
			return instance;
		}
	}

	private void OnGUI()
	{
		GUILayout.Label("Select the SimpleExample scene object to visualize the table in the inspector");
	}
}
