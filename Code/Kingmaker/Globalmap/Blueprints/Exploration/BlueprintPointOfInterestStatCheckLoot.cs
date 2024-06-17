using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Globalmap.Exploration;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("c7217a916d3f4676859581eebc813640")]
public class BlueprintPointOfInterestStatCheckLoot : BlueprintPointOfInterest
{
	[SerializeField]
	public List<StatDC> Stats;

	[SerializeField]
	private List<LootEntry> m_CheckPassedLoot;

	[SerializeField]
	private List<LootEntry> m_CheckFailedLoot;

	public List<LootEntry> CheckPassedLoot => m_CheckPassedLoot;

	public List<LootEntry> CheckFailedLoot => m_CheckFailedLoot;
}
