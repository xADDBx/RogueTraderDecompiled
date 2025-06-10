using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("ce71b10c9cf84ecaa85cccc35f272c00")]
public class WarhammerOverrideAbilityMeleeStat : BlueprintComponent
{
	[SerializeField]
	private StatType m_Stat;

	public StatType Stat => m_Stat;
}
