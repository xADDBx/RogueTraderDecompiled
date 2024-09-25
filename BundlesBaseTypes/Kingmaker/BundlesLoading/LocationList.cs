using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.BundlesLoading;

[Serializable]
public class LocationList : ISerializationCallbackReceiver
{
	public readonly Dictionary<string, string> GuidToBundle = new Dictionary<string, string>();

	[SerializeField]
	private List<string> m_Guids;

	[SerializeField]
	private List<string> m_Bundles;

	public List<string> Guids => m_Guids;

	public List<string> BundleNames => m_Bundles;

	public void OnBeforeSerialize()
	{
		m_Guids = new List<string>();
		m_Bundles = new List<string>();
		foreach (KeyValuePair<string, string> item in GuidToBundle)
		{
			m_Guids.Add(item.Key);
			m_Bundles.Add(item.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		for (int i = 0; i < m_Guids.Count; i++)
		{
			GuidToBundle[m_Guids[i]] = m_Bundles[i];
		}
	}
}
