using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("1e0ebe55f7204066b7cdb0eb124b863a")]
public class ContextActionRemoveSelf : ContextAction
{
	public override string GetCaption()
	{
		return "Remove self";
	}

	public override void RunAction()
	{
		Buff.Data current = ContextData<Buff.Data>.Current;
		if (current != null)
		{
			current.Buff.Remove();
			return;
		}
		AreaEffectContextData current2 = ContextData<AreaEffectContextData>.Current;
		if (current2 != null)
		{
			current2.Entity.ForceEnd();
		}
		else
		{
			PFLog.Default.Error(this, $"RemoveSelf can only apply to buffs or area effects! Context.AssociatedBlueprint = {base.Context?.AssociatedBlueprint}");
		}
	}
}
