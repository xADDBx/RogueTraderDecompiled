using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("e5951888d1734aacafb4e0f0f3bb50be")]
public class WarhammerAbilityHasPriorityTarget : BlueprintComponent, IAbilityCasterRestriction
{
	[SerializeField]
	private BlueprintBuffReference m_TargetBuff;

	public bool HasAlternativeBuff;

	[SerializeField]
	[ShowIf("HasAlternativeBuff")]
	private BlueprintBuffReference m_AlternativeTargetBuff;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public BlueprintBuff AlternativeTargetBuff => m_AlternativeTargetBuff?.Get();

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		BaseUnitEntity obj = caster.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
		BaseUnitEntity baseUnitEntity = caster.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(AlternativeTargetBuff);
		if (obj == null)
		{
			if (HasAlternativeBuff)
			{
				return baseUnitEntity != null;
			}
			return false;
		}
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.HasNoPriorityTarget;
	}
}
