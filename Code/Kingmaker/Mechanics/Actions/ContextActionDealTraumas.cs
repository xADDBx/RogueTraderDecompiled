using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("4cede40e39bd4ca4ae1b3422486ddaaa")]
public class ContextActionDealTraumas : ContextAction
{
	[Tooltip("Count as 1 if 0")]
	public ContextValue Count;

	public override string GetCaption()
	{
		return $"Deal {(Count.IsZero ? ((ContextValue)1) : Count)} trauma(s)";
	}

	protected override void RunAction()
	{
		base.TargetEntity.GetHealthOptional()?.DealTraumas(Math.Max(1, Count.Calculate(base.Context)));
	}
}
