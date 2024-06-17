using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintAbilityAreaEffect))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintItemEnchantment))]
[TypeId("a6777ad03deb60a4e8ef6b7067ac836b")]
public class ContextCalculateSharedValue : BlueprintComponent
{
	public AbilitySharedValue ValueType;

	public ContextDiceValue Value;

	public double Modifier = 1.0;

	public int Calculate(MechanicsContext context)
	{
		return (int)((double)Value.Calculate(context) * Modifier);
	}
}
