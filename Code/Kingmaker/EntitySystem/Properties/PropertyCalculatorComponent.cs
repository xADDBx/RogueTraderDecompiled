using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
[TypeId("f9bf4ae9ccd847689d5cdf2e86bc51ca")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintPlayerUpgrader))]
[AllowedOn(typeof(BlueprintAbilityAdditionalEffect))]
[AllowMultipleComponents]
public class PropertyCalculatorComponent : BlueprintComponent
{
	public enum SaveToContextType
	{
		No,
		ForCaster,
		ForMainTarget
	}

	public ContextPropertyName Name;

	public SaveToContextType SaveToContext;

	public PropertyCalculator Value;

	public int GetValue(PropertyContext context)
	{
		return Value.GetValue(context);
	}
}
