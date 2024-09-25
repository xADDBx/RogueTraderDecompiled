using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.AI.Blueprints.Components;

[TypeId("16f7086001390fd42b7ce10da913a224")]
public class WarhammerContextActionAddHatedTarget : ContextAction
{
	public override string GetCaption()
	{
		return "Add caster to hated targets";
	}

	protected override void RunAction()
	{
		base.TargetEntity.GetBrainOptional()?.AddCustomHatedTarget(base.Caster);
	}
}
