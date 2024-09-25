using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("41f618cc2e0246ad87356f71ffb72625")]
public class ContextActionDealWounds : ContextAction
{
	[Tooltip("Count as 1 if 0")]
	public ContextValue Count;

	public override string GetCaption()
	{
		return $"Deal {(Count.IsZero ? ((ContextValue)1) : Count)} wound(s)";
	}

	protected override void RunAction()
	{
		base.TargetEntity.GetHealthOptional()?.DealWounds(Math.Max(1, Count.Calculate(base.Context)));
	}
}
