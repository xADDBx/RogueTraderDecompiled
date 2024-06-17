using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Mechanics.Actions;

[TypeId("7209261a10a047579fdaa45e3735454d")]
public class ContextActionSetDestructionStage : ContextAction
{
	public DestructionStage Stage;

	public override string GetCaption()
	{
		return $"Set destruction stage to {Stage}";
	}

	public override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return;
		}
		PartHealth required = base.Target.Entity.GetRequired<PartHealth>();
		int num = required.HitPoints;
		required.SetHitPointsLeft(Stage switch
		{
			DestructionStage.Whole => num, 
			DestructionStage.Damaged => num / 2 - 1, 
			DestructionStage.Destroyed => 0, 
			_ => throw new ArgumentException(string.Format("Invalid value {0} of {1}", Stage, "Stage")), 
		});
	}
}
