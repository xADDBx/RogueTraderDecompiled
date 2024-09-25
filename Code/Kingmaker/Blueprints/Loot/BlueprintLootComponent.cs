using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[AllowedOn(typeof(BlueprintUnitLoot))]
[AllowMultipleComponents]
public abstract class BlueprintLootComponent : BlueprintComponent
{
	[SerializeField]
	protected bool m_OverrideReputationPointsToUnlock;

	[SerializeField]
	[ShowIf("OverrideReputationPointsToUnlock")]
	protected int m_ReputationPointsToUnlock;

	public int ReputationPointsToUnlock
	{
		get
		{
			if (!m_OverrideReputationPointsToUnlock)
			{
				return 0;
			}
			return m_ReputationPointsToUnlock;
		}
	}

	public bool OverrideReputationPointsToUnlock => m_OverrideReputationPointsToUnlock;

	public abstract void AddItemsTo(List<LootEntry> targetList);

	public abstract float GetAverageProfitFactorPrice();

	public abstract IReadOnlyCollection<LootItem> GetPossibleLoot();
}
