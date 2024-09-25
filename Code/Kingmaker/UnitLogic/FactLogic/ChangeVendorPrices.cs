using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[TypeId("8a8f81395c3041fe8c492ebb93c84d3a")]
public class ChangeVendorPrices : BlueprintComponent
{
	[Serializable]
	public class Entry
	{
		[ValidateNotNull]
		[SerializeField]
		[FormerlySerializedAs("Item")]
		private BlueprintItemReference m_Item;

		public long CostOverride;

		public BlueprintItem Item => m_Item?.Get();
	}

	[SerializeField]
	private Entry[] m_Overrides;

	private Dictionary<BlueprintItem, long> m_ItemsToCosts;

	public Dictionary<BlueprintItem, long> Overrides
	{
		get
		{
			if (m_ItemsToCosts == null)
			{
				m_ItemsToCosts = new Dictionary<BlueprintItem, long>();
				Entry[] overrides = m_Overrides;
				foreach (Entry entry in overrides)
				{
					if ((bool)entry.Item)
					{
						m_ItemsToCosts[entry.Item] = entry.CostOverride;
					}
				}
			}
			return m_ItemsToCosts;
		}
	}

	public float GetProfitFactorCost(BlueprintItem item)
	{
		if (Overrides.TryGetValue(item, out var value))
		{
			return value;
		}
		return item.ProfitFactorCost;
	}
}
