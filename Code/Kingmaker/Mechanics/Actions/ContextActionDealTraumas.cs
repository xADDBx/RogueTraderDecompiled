using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("4cede40e39bd4ca4ae1b3422486ddaaa")]
public class ContextActionDealTraumas : ContextAction
{
	public bool SpecificTraumas;

	[HideIf("SpecificTraumas")]
	[Tooltip("Count as 1 if 0")]
	public ContextValue Count;

	[ShowIf("SpecificTraumas")]
	public BlueprintBuffReference[] TraumaBuffs;

	public override string GetCaption()
	{
		if (!SpecificTraumas)
		{
			return $"Deal {(Count.IsZero ? ((ContextValue)1) : Count)} random trauma(s)";
		}
		return $"Deal {TraumaBuffs.Length} specific trauma(s)";
	}

	protected override void RunAction()
	{
		PartHealth healthOptional = base.TargetEntity.GetHealthOptional();
		if (healthOptional != null)
		{
			if (SpecificTraumas)
			{
				healthOptional.DealTraumas((IReadOnlyCollection<BlueprintBuffReference>)(object)TraumaBuffs);
			}
			else
			{
				healthOptional.DealTraumas(Math.Max(1, Count.Calculate(base.Context)));
			}
		}
	}
}
