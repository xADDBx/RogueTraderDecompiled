using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("20eef6901e3c38a48b2e988dc13635a7")]
public class ContextActionRecalculateSelf : ContextAction
{
	public override void RunAction()
	{
		((EntityFact)((((object)ContextData<Feature.Data>.Current?.Feature) ?? ((object)ContextData<Buff.Data>.Current?.Buff)) ?? ContextData<Buff.Data>.Current?.Buff))?.Recalculate();
	}

	public override string GetCaption()
	{
		return "Recalculate this fact";
	}
}
