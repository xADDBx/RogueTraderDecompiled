using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace RogueTrader.Editor.CameraRecorder;

[Serializable]
[TypeId("084826bccf334e3a892863750a393dc1")]
public class CameraRecorderData : BlueprintScriptableObject
{
	[Serializable]
	public readonly struct Delta
	{
		public readonly float Timestamp;

		public readonly Vector3 Position;

		public readonly float RotationY;

		public readonly float Zoom;

		public Delta(float timestamp, Vector3 position, float rotationY, float zoom)
		{
			Timestamp = timestamp;
			Position = position;
			RotationY = rotationY;
			Zoom = zoom;
		}
	}

	[Serializable]
	public class Record
	{
		public bool Bookmark;

		public string Name;

		public Delta Origin;

		public float DelaySeconds;

		[InspectorReadOnly]
		public List<Delta> DeltaList = new List<Delta>();

		public void Clear()
		{
			DeltaList.Clear();
		}
	}

	private const int HistoryLength = 20;

	private const string DefaultRecordName = "Record ";

	[SerializeField]
	private List<Record> m_Records = new List<Record>();

	public ReadonlyList<Record> Records => m_Records;

	private string GetDefaultRecordName()
	{
		return "Record " + DateTime.Now.ToString("yy/MM/dd") + " " + DateTime.Now.ToString("hh:mm");
	}

	public static bool IsDefaultRecordName(string name)
	{
		return name.StartsWith("Record ");
	}

	public Record CreateNewRecord()
	{
		return null;
	}

	public void RemoveRecord(Record record)
	{
	}
}
