using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Mechanics.Actions;

[TypeId("713a3fe5623741bdbbd8754f752fc9c0")]
public class ContextActionKillWithoutTrauma : ContextAction
{
	public override string GetCaption()
	{
		return "Kill target without trauma.";
	}

	protected override void RunAction()
	{
		PartLifeState partLifeState = base.Target.Entity?.GetLifeStateOptional();
		PartHealth partHealth = base.Target.Entity?.GetHealthOptional();
		if (partLifeState == null || partHealth == null)
		{
			Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
		}
		else if (base.Context.MaybeCaster == null || !base.Context.MaybeCaster.IsAttackingGreenNPC(base.Target.Entity))
		{
			EventBus.RaiseEvent(delegate(IUIContextActionKillHandler h)
			{
				h.HandleOnContextActionKill(base.Context.MaybeCaster, base.Target.Entity, base.Context.AssociatedBlueprint as BlueprintMechanicEntityFact, base.Context.SavingThrow);
			});
			partLifeState.MarkedForDeath = true;
			partHealth.DiscardTrauma = true;
		}
	}
}
