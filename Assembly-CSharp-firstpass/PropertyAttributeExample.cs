using System;
using System.Collections.Generic;
using EditorGUITable;
using UnityEngine;

public class PropertyAttributeExample : MonoBehaviour
{
	[Serializable]
	public class SimpleObject
	{
		public string stringProperty;

		public float floatProperty;

		public GameObject objectProperty;

		public Vector2 v2Property;
	}

	public List<SimpleObject> simpleObjectsDefaultDisplay;

	[Table]
	public List<SimpleObject> simpleObjectsUsingTableAttribute;

	[ReorderableTable(new string[] { "stringProperty", "floatProperty:Width(40),Title(float)", "v2Property" }, new string[] { "RowHeight(22)" })]
	public List<SimpleObject> simpleObjectsUsingReorderableTableAttribute;

	[Table]
	public List<Enemy> enemies;

	private void OnGUI()
	{
		GUILayout.Label("Select the PropertyAttribute scene object to visualize the table in the inspector");
	}
}
