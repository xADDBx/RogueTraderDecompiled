using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b9888648240744caaa14e5cb18b734cd")]
public class CheckSourceAbilityGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptionalRule
{
	[SerializeField]
	private BlueprintAbilityReference[] m_Abilities;

	public ReferenceArrayProxy<BlueprintAbility> Abilities
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] abilities = m_Abilities;
			return abilities;
		}
	}

	protected override string GetInnerCaption()
	{
		return "Source ability is " + string.Join("|", from i in Abilities
			where i != null
			select i.ToString());
	}

	protected override int GetBaseValue()
	{
		if (!Abilities.HasReference(ContextData<MechanicsContext.Data>.Current?.Context.SourceAbility) && !Abilities.HasReference(this.GetAbility()?.Blueprint) && !Abilities.HasReference(this.GetMechanicContext()?.SourceAbility) && !Abilities.HasReference(this.GetRule()?.Reason.Context?.SourceAbility) && (!(this.GetRule() is RulePerformAbility { Spell: { Blueprint: { } blueprint } }) || !Abilities.HasReference(blueprint)))
		{
			return 0;
		}
		return 1;
	}
}
