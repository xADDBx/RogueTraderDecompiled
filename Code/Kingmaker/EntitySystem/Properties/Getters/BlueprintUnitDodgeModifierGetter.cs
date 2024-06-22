using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.DodgeChance;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("fae446546b404343be479f2b43ca6342")]
public class BlueprintUnitDodgeModifierGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.Blueprint.GetComponent<WarhammerDodgeChanceModifier>()?.DodgeChance.Calculate(base.PropertyContext.MechanicContext) ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "DodgeModifier from " + FormulaTargetScope.Current + " blueprint";
	}
}
