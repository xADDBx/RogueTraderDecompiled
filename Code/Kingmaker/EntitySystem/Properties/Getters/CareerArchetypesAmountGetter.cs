using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("6d332ebc25604c2782874b52204b3d95")]
public class CareerArchetypesAmountGetter : PropertyGetter
{
	protected override int GetBaseValue()
	{
		return Game.Instance.BlueprintRoot.Progression.CareerPaths.Count();
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Amount of career archetypes";
	}
}
