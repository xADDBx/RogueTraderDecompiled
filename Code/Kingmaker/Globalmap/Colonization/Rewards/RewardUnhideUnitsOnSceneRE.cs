using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.CombatRandomEncounters;
using Kingmaker.View.Spawners;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[TypeId("108f649ae88b4fc1aa06d5453b3071e9")]
public class RewardUnhideUnitsOnSceneRE : Reward
{
	[Serializable]
	private class SpawnersByEnterPoint
	{
		[SerializeField]
		private BlueprintAreaEnterPointReference m_EnterPoint;

		[SerializeField]
		[AllowedEntityType(typeof(UnitSpawnerBase))]
		public EntityReference Spawner;

		public BlueprintAreaEnterPoint EnterPoint => m_EnterPoint?.Get();
	}

	[SerializeField]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	private List<SpawnersByEnterPoint> m_Spawners;

	private BlueprintUnit Unit => m_Unit?.Get();

	public override void ReceiveReward(Colony colony = null)
	{
		if (m_Spawners == null || Unit == null)
		{
			return;
		}
		CombatRandomEncounterState combatRandomEncounterState = Game.Instance.Player.CombatRandomEncounterState;
		combatRandomEncounterState.AllyBlueprint = Unit;
		foreach (SpawnersByEnterPoint spawner in m_Spawners)
		{
			combatRandomEncounterState.AllySpawnersInAllAreas.Add(spawner.EnterPoint, spawner.Spawner);
		}
	}
}
