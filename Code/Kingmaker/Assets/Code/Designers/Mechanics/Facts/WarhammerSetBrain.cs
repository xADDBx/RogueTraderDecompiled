using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Kingmaker.Assets.Code.Designers.Mechanics.Facts;

[TypeId("91317c7faa9748f4180bca58bfbf2b1b")]
public class WarhammerSetBrain : ContextAction
{
	[Tooltip("New brain index from alternative brain list, or -1 to reset to default")]
	[SerializeField]
	private int newBrain = -1;

	public override string GetCaption()
	{
		return "Set new brain for unit";
	}

	public override void RunAction()
	{
		if (!(base.Target.Entity is BaseUnitEntity baseUnitEntity))
		{
			PFLog.Default.Error("Target is missing");
		}
		else if (newBrain < 0)
		{
			baseUnitEntity.Brain.SetBrain(baseUnitEntity.Blueprint.DefaultBrain);
		}
		else if (newBrain >= baseUnitEntity.Blueprint.AlternativeBrains.Length)
		{
			PFLog.Default.Error("StarshipSetBrain: Invalid brain index for unit \"" + baseUnitEntity.Name + "\"");
		}
		else
		{
			baseUnitEntity.Brain.SetBrain(baseUnitEntity.Blueprint.AlternativeBrains[newBrain]);
		}
	}
}
