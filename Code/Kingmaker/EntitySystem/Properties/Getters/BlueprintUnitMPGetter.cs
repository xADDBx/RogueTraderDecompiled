using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("f74490d3e379431b88167eec6a09e4b7")]
public class BlueprintUnitMPGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.Blueprint.WarhammerInitialAPBlue;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "WarhammerInitialAPBlue from " + FormulaTargetScope.Current + " blueprint";
	}
}
