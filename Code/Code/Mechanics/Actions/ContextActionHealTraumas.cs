using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;

namespace Code.Mechanics.Actions;

[Serializable]
[TypeId("fc279f2920ce411bbc3ad0c56f2cdcbf")]
public class ContextActionHealTraumas : ContextAction
{
	public int Stacks;

	public override string GetCaption()
	{
		return "Heal Traumas " + ((Stacks > 0) ? $"({Stacks} stacks)" : "(all stacks)");
	}

	protected override void RunAction()
	{
		base.Target.Entity?.GetHealthOptional()?.HealTrauma((Stacks > 0) ? Stacks : int.MaxValue);
	}
}
