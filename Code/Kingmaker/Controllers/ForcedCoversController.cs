using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.View.Covers;

namespace Kingmaker.Controllers;

public class ForcedCoversController : IControllerTick, IController
{
	private class CoverProviderCachedValue
	{
		public NodeList NodeList;

		public CoverProviderCachedValue(NodeList nodeList)
		{
			NodeList = nodeList;
		}
	}

	private struct SingleValueOrList<T>
	{
		public bool HasFirstValue;

		public T FirstValue;

		public List<T> OtherValues;

		public int Count
		{
			get
			{
				if (!HasFirstValue)
				{
					return 0;
				}
				return (OtherValues?.Count ?? 0) + 1;
			}
		}

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
				{
					throw new IndexOutOfRangeException();
				}
				if (index != 0)
				{
					return OtherValues[index - 1];
				}
				return FirstValue;
			}
		}

		public void Add(T value)
		{
			if (!HasFirstValue)
			{
				FirstValue = value;
				HasFirstValue = true;
				return;
			}
			if (OtherValues == null)
			{
				OtherValues = new List<T>();
			}
			OtherValues.Add(value);
		}

		public bool Remove(T value)
		{
			if (!HasFirstValue)
			{
				return false;
			}
			if (object.Equals(FirstValue, value))
			{
				List<T> otherValues = OtherValues;
				if (otherValues != null && otherValues.Count > 0)
				{
					List<T> otherValues2 = OtherValues;
					FirstValue = otherValues2[otherValues2.Count - 1];
					if (OtherValues.Count == 1)
					{
						OtherValues = null;
					}
					else
					{
						OtherValues.RemoveAt(OtherValues.Count - 1);
					}
				}
				else
				{
					FirstValue = default(T);
					HasFirstValue = false;
				}
				return true;
			}
			if (OtherValues.Remove(value))
			{
				if (OtherValues.Count == 0)
				{
					OtherValues = null;
				}
				return true;
			}
			return false;
		}
	}

	private readonly Dictionary<IDynamicCoverProvider, CoverProviderCachedValue> m_RegisteredCoverProviders = new Dictionary<IDynamicCoverProvider, CoverProviderCachedValue>();

	private readonly Dictionary<CustomGridNodeBase, SingleValueOrList<IDynamicCoverProvider>> m_Cache = new Dictionary<CustomGridNodeBase, SingleValueOrList<IDynamicCoverProvider>>();

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (IDynamicCoverProvider key in m_RegisteredCoverProviders.Keys)
		{
			CoverProviderCachedValue coverProviderCachedValue = m_RegisteredCoverProviders[key];
			NodeList nodes = key.Nodes;
			NodeList nodeList = coverProviderCachedValue.NodeList;
			if (nodeList.Equals(nodes))
			{
				continue;
			}
			foreach (CustomGridNodeBase item in nodeList)
			{
				if (!nodes.Contains(item))
				{
					RemoveFromCache(item, key);
				}
			}
			foreach (CustomGridNodeBase item2 in nodes)
			{
				if (!nodeList.Contains(item2))
				{
					AddToCache(item2, key);
				}
			}
			coverProviderCachedValue.NodeList = nodes;
		}
	}

	private void AddToCache(CustomGridNodeBase cell, IDynamicCoverProvider coverProvider)
	{
		if (!m_Cache.TryGetValue(cell, out var value))
		{
			value = default(SingleValueOrList<IDynamicCoverProvider>);
		}
		value.Add(coverProvider);
		m_Cache[cell] = value;
	}

	private void RemoveFromCache(CustomGridNodeBase cell, IDynamicCoverProvider coverProvider)
	{
		if (m_Cache.TryGetValue(cell, out var value) && value.Remove(coverProvider))
		{
			if (value.Count == 0)
			{
				m_Cache.Remove(cell);
			}
			else
			{
				m_Cache[cell] = value;
			}
		}
	}

	public void RegisterCoverProvider(IDynamicCoverProvider coverProvider)
	{
		foreach (CustomGridNodeBase node in coverProvider.Nodes)
		{
			AddToCache(node, coverProvider);
		}
		m_RegisteredCoverProviders.Add(coverProvider, new CoverProviderCachedValue(coverProvider.Nodes));
	}

	public void UnregisterCoverProvider(IDynamicCoverProvider coverProvider)
	{
		if (!m_RegisteredCoverProviders.Remove(coverProvider, out var value))
		{
			return;
		}
		foreach (CustomGridNodeBase node in value.NodeList)
		{
			RemoveFromCache(node, coverProvider);
		}
	}

	public bool ContainsCoverProvider(IDynamicCoverProvider coverProvider)
	{
		return m_RegisteredCoverProviders.ContainsKey(coverProvider);
	}

	public bool TryGetCover(CustomGridNodeBase node, BaseUnitEntity entity, out LosCalculations.CoverType coverType)
	{
		if (!m_Cache.TryGetValue(node, out var value))
		{
			coverType = LosCalculations.CoverType.None;
			return false;
		}
		for (int i = 0; i < value.Count; i++)
		{
			IDynamicCoverProvider dynamicCoverProvider = value[i];
			if (dynamicCoverProvider.CanUseCover(entity))
			{
				coverType = dynamicCoverProvider.CoverType;
				return true;
			}
		}
		coverType = LosCalculations.CoverType.None;
		return false;
	}
}
