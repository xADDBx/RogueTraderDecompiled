using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.SpaceCombat.Scrap;

[ComponentName("Add Scrap Modifier")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintFact))]
[AllowMultipleComponents]
[TypeId("1905a46a799252049b6085a82995ed40")]
public class ScrapModifier : BlueprintComponent
{
	[Serializable]
	public enum ModifierType
	{
		ShipRepairCost,
		EnemyKilledReward,
		SpaceCombatCompleteReward
	}

	[SerializeField]
	public ModifierType ModType;

	[SerializeField]
	public float ModValue;
}
