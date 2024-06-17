using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace RogueTrader.SharedTypes;

[CreateAssetMenu]
public class BlueprintReferencedAssets : ScriptableObject
{
	[Serializable]
	private struct Entry
	{
		public string AssetId;

		public long FileId;

		public UnityEngine.Object Asset;
	}

	[SerializeField]
	private List<Entry> m_Entries = new List<Entry>();

	public void Add(UnityEngine.Object asset, string assetId, long fileId)
	{
		foreach (Entry entry in m_Entries)
		{
			if (entry.Asset == asset)
			{
				return;
			}
		}
		if (!string.IsNullOrEmpty(assetId))
		{
			m_Entries.Add(new Entry
			{
				AssetId = assetId,
				FileId = fileId,
				Asset = asset
			});
		}
	}

	public UnityEngine.Object Get(string assetId, long fileId)
	{
		return FirstItem(m_Entries, (Entry e) => e.AssetId.Equals(assetId, StringComparison.Ordinal) && e.FileId == fileId).Asset;
	}

	public int IndexOf(UnityEngine.Object o)
	{
		return m_Entries.FindIndex((Entry e) => e.Asset == o);
	}

	public UnityEngine.Object Get(int index)
	{
		if (m_Entries == null)
		{
			return null;
		}
		if (index >= 0 && index < m_Entries.Count)
		{
			return m_Entries[index].Asset;
		}
		Debug.LogError("no asset with ID " + index);
		return null;
	}

	private static bool HasItem<TValue>([CanBeNull] IList<TValue> source, Func<TValue, bool> pred)
	{
		if (source == null)
		{
			return false;
		}
		for (int i = 0; i < source.Count; i++)
		{
			if (pred(source[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static TValue FirstItem<TValue>([CanBeNull] IList<TValue> source, Func<TValue, bool> pred)
	{
		if (source == null)
		{
			return default(TValue);
		}
		for (int i = 0; i < source.Count; i++)
		{
			if (pred(source[i]))
			{
				return source[i];
			}
		}
		return default(TValue);
	}

	public (string AssetId, long FileId)? GetAssetId(UnityEngine.Object asset)
	{
		Entry? entry = null;
		foreach (Entry entry2 in m_Entries)
		{
			if (entry2.Asset == asset)
			{
				entry = entry2;
				break;
			}
		}
		if (!entry.HasValue)
		{
			return null;
		}
		return (entry.Value.AssetId, entry.Value.FileId);
	}
}
