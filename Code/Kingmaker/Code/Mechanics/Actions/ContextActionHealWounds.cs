using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Code.Mechanics.Actions;

[Serializable]
[TypeId("f715d518d45e412f9dbf074e4d78c97d")]
public class ContextActionHealWounds : ContextAction
{
	public bool AllowOldWounds;

	public ContextValue Stacks;

	public override string GetCaption()
	{
		return "Heal Wounds" + (AllowOldWounds ? " [fresh and old]" : " [fresh only]") + (Stacks.IsZero ? " (all stacks)" : $" ({Stacks} stacks)");
	}

	protected override void RunAction()
	{
		PartHealth partHealth = base.Target.Entity?.GetHealthOptional();
		if (partHealth != null)
		{
			int num = Stacks.Calculate(base.Context);
			int woundFreshStacks = partHealth.WoundFreshStacks;
			partHealth.HealFreshWound(Stacks.IsZero ? int.MaxValue : Math.Min(num, woundFreshStacks));
			if (AllowOldWounds)
			{
				partHealth.HealOldWound(Stacks.IsZero ? int.MaxValue : Math.Max(0, num - woundFreshStacks));
			}
		}
	}
}
