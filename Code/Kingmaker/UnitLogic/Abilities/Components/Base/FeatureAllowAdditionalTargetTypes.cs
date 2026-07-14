using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintFeatureBase))]
[TypeId("07a03251aa8f49e44801564ba06204f1")]
public class FeatureAllowAdditionalTargetTypes : UnitFactComponentDelegate, IAbilityAllowTargetingType, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions;

	[SerializeField]
	private bool m_UseAbilityGroups;

	[SerializeField]
	[HideIf("m_UseAbilityGroups")]
	private BlueprintAbilityReference[] m_Abilities;

	[SerializeField]
	[ShowIf("m_UseAbilityGroups")]
	private BlueprintAbilityGroupReference[] m_AbilityGroups;

	[SerializeField]
	private IAbilityAllowTargetingType.TargetTypeEnum m_TargetType;

	public IAbilityAllowTargetingType.TargetTypeEnum TargetType => m_TargetType;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityTargetExtension>().Register(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOrCreate<PartAbilityTargetExtension>().Unregister(base.Fact, this);
	}

	public bool IsRestrictionPassed(AbilityData abilityData)
	{
		if (!IsValidAbility(abilityData))
		{
			return false;
		}
		PropertyContext context = new PropertyContext(abilityData);
		m_Restrictions.IsPassed(context);
		return true;
	}

	private bool IsValidAbility(AbilityData abilityData)
	{
		if (m_UseAbilityGroups)
		{
			BlueprintAbilityGroupReference[] abilityGroups = m_AbilityGroups;
			for (int i = 0; i < abilityGroups.Length; i++)
			{
				BlueprintAbilityGroup bp = abilityGroups[i].Get();
				if (abilityData.Blueprint.AbilityGroups.Contains(bp))
				{
					return true;
				}
			}
			return false;
		}
		BlueprintAbilityReference[] abilities = m_Abilities;
		for (int i = 0; i < abilities.Length; i++)
		{
			BlueprintAbility blueprintAbility = abilities[i].Get();
			if (abilityData.Blueprint == blueprintAbility)
			{
				return true;
			}
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
