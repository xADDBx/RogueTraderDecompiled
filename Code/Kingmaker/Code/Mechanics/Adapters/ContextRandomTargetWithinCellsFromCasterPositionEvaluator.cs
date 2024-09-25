using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("b29cfc3138924d069d58375def75a80e")]
public class ContextRandomTargetWithinCellsFromCasterPositionEvaluator : PositionEvaluator
{
	public ConditionsChecker ConditionsOnTarget;

	public ContextValue Range;

	public override string GetCaption()
	{
		return "Evaluate position of random enemy around caster";
	}

	protected override Vector3 GetValueInternal()
	{
		MechanicsContext context = ContextData<MechanicsContext.Data>.Current?.Context;
		MechanicEntity caster = context?.MaybeCaster;
		if (context == null || caster == null)
		{
			throw new FailToEvaluateException(this);
		}
		int range = Range.Calculate(context);
		return Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && IsConditionPassed(context, p) && caster.InRangeInCells(p, range)).Cast<MechanicEntity>().ToList()
			.Random(PFStatefulRandom.Mechanics)
			.Position;
	}

	private bool IsConditionPassed(MechanicsContext context, BaseUnitEntity unit)
	{
		using (context.GetDataScope(unit.ToITargetWrapper()))
		{
			return ConditionsOnTarget.Check();
		}
	}
}
