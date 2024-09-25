using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.BundlesLoading;

[Serializable]
public class DependencyData : DependencyData.Interface, ISerializationCallbackReceiver
{
	[Serializable]
	public class Entry
	{
		public string Key;

		public List<string> Value;
	}

	public interface Interface
	{
		Dictionary<string, List<string>> BundleToDependencies { get; }
	}

	[SerializeField]
	private List<Entry> m_List = new List<Entry>();

	[JsonProperty]
	public Dictionary<string, List<string>> BundleToDependencies { get; private set; } = new Dictionary<string, List<string>>();


	public List<Entry> List => m_List;

	public void OnBeforeSerialize()
	{
		m_List = BundleToDependencies.Select((KeyValuePair<string, List<string>> p) => new Entry
		{
			Key = p.Key,
			Value = p.Value
		}).ToList();
	}

	public void OnAfterDeserialize()
	{
		BundleToDependencies = m_List.ToDictionary((Entry e) => e.Key, (Entry e) => e.Value);
		m_List = null;
	}
}
