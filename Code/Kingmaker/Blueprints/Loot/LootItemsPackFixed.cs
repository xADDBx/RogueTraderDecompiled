using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DLC;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[TypeId("9a9cba603f85c634690eb67962fdf792")]
public class LootItemsPackFixed : BlueprintLootComponent
{
	[SerializeField]
	private LootItem m_Item;

	[SerializeField]
	private int m_Count = 1;

	[SerializeField]
	private bool m_DlcCondition;

	[ShowIf("m_DlcCondition")]
	[SerializeField]
	private BlueprintDlcRewardReference m_DlcReward;

	public LootItem Item => m_Item;

	public int Count => m_Count;

	public bool DlcCondition => m_DlcCondition;

	public BlueprintDlcRewardReference DlcReward => m_DlcReward;

	public override void AddItemsTo(List<LootEntry> targetList)
	{
		if (m_DlcCondition)
		{
			BlueprintDlcReward blueprintDlcReward = m_DlcReward?.Get();
			if (blueprintDlcReward != null && !blueprintDlcReward.IsActive)
			{
				return;
			}
		}
		m_Item.AddItemTo(targetList, m_Count, base.ReputationPointsToUnlock);
	}

	public override float GetAverageProfitFactorPrice()
	{
		return (float)m_Count * m_Item.GetProfitFactorPrice();
	}

	public override IReadOnlyCollection<LootItem> GetPossibleLoot()
	{
		return new List<LootItem> { m_Item };
	}

	public void OverrideReputationPointsFromImport(int rep)
	{
		m_OverrideReputationPointsToUnlock = true;
		m_ReputationPointsToUnlock = rep;
	}

	public bool Same(VendorLootItem item)
	{
		if (item.Item == Item.Item)
		{
			return item.Diversity == Item.Diversity;
		}
		return false;
	}
}
