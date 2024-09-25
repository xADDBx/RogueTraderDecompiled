using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[Serializable]
public class LootItem : ISerializationCallbackReceiver
{
	[SerializeField]
	private LootItemType m_Type;

	[SerializeField]
	private BlueprintItemReference m_Item;

	[SerializeField]
	private BlueprintUnitLootReference m_Loot;

	[SerializeField]
	[ShowIf("IsItem")]
	private bool m_OverrideProfitFactorCost;

	[SerializeField]
	[ShowIf("CanOverridePFCost")]
	private int m_ProfitFactorCost;

	[SerializeField]
	private int m_Diversity;

	public BlueprintItem Item => m_Item.Get();

	public int? ProfitFactorCostOverride
	{
		get
		{
			if (!m_OverrideProfitFactorCost)
			{
				return null;
			}
			return m_ProfitFactorCost;
		}
	}

	public bool IsItem => Item != null;

	public bool CanOverridePFCost
	{
		get
		{
			if (m_OverrideProfitFactorCost)
			{
				return Item != null;
			}
			return false;
		}
	}

	public int Diversity => m_Diversity;

	public void OverrideProfitFactorCostFromImport(int pf)
	{
		m_OverrideProfitFactorCost = true;
		m_ProfitFactorCost = pf;
	}

	public void AddItemTo(List<LootEntry> targetList, int count, int reputationPointsToUnlock)
	{
		if (m_Type == LootItemType.Item && m_Item != null)
		{
			LootEntry lootEntry = targetList.FirstOrDefault((LootEntry i) => m_Item.Is(i.Item));
			if (lootEntry == null)
			{
				targetList.Add(new LootEntry
				{
					Item = Item,
					Diversity = Diversity,
					Count = count,
					ReputationPointsToUnlock = reputationPointsToUnlock,
					ProfitFactorCostOverride = ProfitFactorCostOverride
				});
			}
			else
			{
				lootEntry.Count += count;
				lootEntry.ReputationPointsToUnlock = Math.Max(lootEntry.ReputationPointsToUnlock, reputationPointsToUnlock);
				if (ProfitFactorCostOverride.HasValue)
				{
					lootEntry.ProfitFactorCostOverride = (lootEntry.ProfitFactorCostOverride.HasValue ? new long?(Math.Max(lootEntry.ProfitFactorCostOverride.Value, ProfitFactorCostOverride.Value)) : ProfitFactorCostOverride);
				}
			}
		}
		else if (m_Type == LootItemType.Loot && !m_Loot.IsEmpty())
		{
			while (count-- > 0)
			{
				m_Loot.Get().AddItemsTo(targetList);
			}
		}
	}

	public void Validate(ValidationContext context)
	{
		if ((m_Type == LootItemType.Item && m_Item == null) || (m_Type == LootItemType.Loot && m_Loot == null))
		{
			context.AddError("Item is not specified");
		}
	}

	public void OnBeforeSerialize()
	{
		switch (m_Type)
		{
		case LootItemType.Item:
			m_Loot = null;
			break;
		case LootItemType.Loot:
			m_Item = null;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void OnAfterDeserialize()
	{
	}

	public float GetProfitFactorPrice()
	{
		if (!m_Item.IsEmpty() && m_Type == LootItemType.Item)
		{
			return ((float?)ProfitFactorCostOverride) ?? m_Item.Get().ProfitFactorCost;
		}
		if (!m_Loot.IsEmpty() && m_Type == LootItemType.Loot)
		{
			return m_Loot.Get().GetAverageProfitFactorPrice();
		}
		return 0f;
	}
}
