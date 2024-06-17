using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.AI.Blueprints.Components;

[TypeId("e03fd4072db5e6c4e81e5ec7588fec58")]
public class WarhammerContextActionRemoveHatedTarget : ContextAction
{
	public override string GetCaption()
	{
		return "Remove caster from hated targets";
	}

	public override void RunAction()
	{
		base.TargetEntity.GetBrainOptional()?.RemoveCustomHatedTarget(base.Caster);
	}
}
