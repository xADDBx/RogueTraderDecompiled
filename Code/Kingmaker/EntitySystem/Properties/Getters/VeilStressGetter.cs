using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("f42258a78a4b4c8490804ac5e91d095c")]
public class VeilStressGetter : PropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Veil fracture";
	}

	protected override int GetBaseValue()
	{
		return Game.Instance.TurnController.VeilThicknessCounter.Value;
	}
}
