using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("3244a7f6b105c654db650034076be4a5")]
public class AbilityCasterHasFacts : BlueprintComponent, IAbilityCasterRestriction
{
	[SerializeField]
	[FormerlySerializedAs("Facts")]
	private BlueprintUnitFactReference[] m_Facts;

	public bool NeedsAll;

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		bool flag = NeedsAll;
		foreach (BlueprintUnitFact fact in Facts)
		{
			flag = caster.Facts.Contains(fact);
			if (flag && !NeedsAll)
			{
				return true;
			}
		}
		return flag;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}
}
