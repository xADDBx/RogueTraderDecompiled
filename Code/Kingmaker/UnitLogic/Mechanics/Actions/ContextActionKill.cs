using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("930b32b3226bd714283798d0cc050b71")]
public class ContextActionKill : ContextAction
{
	public UnitDismemberType Dismember;

	protected override void RunAction()
	{
		PartLifeState partLifeState = base.Target.Entity?.GetLifeStateOptional();
		if (partLifeState == null)
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
			partLifeState.ForceDismember = Dismember;
		}
	}

	public override string GetCaption()
	{
		return $"Kill target. Dismember [{Dismember}]";
	}
}
