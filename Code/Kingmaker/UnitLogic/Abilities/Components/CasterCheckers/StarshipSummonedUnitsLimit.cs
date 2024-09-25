using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("859d4203501070e4b8671e7f49fb2da2")]
public class StarshipSummonedUnitsLimit : BlueprintComponent, IAbilityCasterRestriction
{
	[SerializeField]
	[Tooltip("Reference to blueprint of summoned unit to count")]
	private BlueprintStarshipReference unit;

	[SerializeField]
	private int limit;

	[SerializeField]
	private BlueprintFeatureReference m_ExpansionFeature;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		StarshipEntity starship = caster as StarshipEntity;
		if (starship == null)
		{
			return false;
		}
		int num = Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity u) => (u as StarshipEntity)?.Blueprint == unit?.Get() && u.GetOptional<UnitPartSummonedMonster>()?.Summoner == starship).Count();
		int num2 = (caster.Facts.Contains(m_ExpansionFeature?.Get()) ? 1 : 0);
		return num < limit + num2;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.CantLaunchMoreWings;
	}
}
