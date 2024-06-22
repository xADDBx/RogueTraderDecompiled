using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("e10098e449dc0e3439e55de887154de2")]
public class UnitsWithFactGetter : PropertyGetter, PropertyContextAccessor.IOptionalTargetByType, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_Fact;

	public bool InMaximumDistance;

	[ShowIf("InMaximumDistance")]
	public int MaximumDistance;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Number of units with Fact in combat";
	}

	protected override int GetBaseValue()
	{
		int num = 0;
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (allUnit.IsInCombat && (!InMaximumDistance || base.CurrentEntity.DistanceToInCells(allUnit) <= MaximumDistance) && allUnit.Facts.Contains(Fact))
			{
				num++;
			}
		}
		return num;
	}
}
