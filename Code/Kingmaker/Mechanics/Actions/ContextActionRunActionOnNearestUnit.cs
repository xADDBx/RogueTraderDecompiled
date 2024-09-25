using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("b45b69ed39bb4d9eae68c2927e09cb33")]
public class ContextActionRunActionOnNearestUnit : ContextAction
{
	public ConditionsChecker Condition;

	public ActionList Actions;

	public bool RunOnAllTargets;

	public override string GetCaption()
	{
		return "Run action on nearest unit, that meets the conditions";
	}

	protected override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext == null)
		{
			return;
		}
		Vector3 target = mechanicsContext.MainTarget.Point;
		MechanicEntity targetEntity = mechanicsContext.MainTarget.Entity;
		List<MechanicEntity> targets = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && IsConditionPassed(base.Context, p)).Cast<MechanicEntity>().ToList();
		IEnumerable<MechanicEntity> enumerable = targets.Where((MechanicEntity p) => p.DistanceToInCells(target) == targets.Min((MechanicEntity p1) => p1.DistanceToInCells(target)) && p != targetEntity);
		if (!RunOnAllTargets)
		{
			MechanicEntity mechanicEntity = enumerable.Random(PFStatefulRandom.Mechanics);
			if (mechanicEntity == null)
			{
				return;
			}
			using (base.Context.GetDataScope((TargetWrapper)mechanicEntity))
			{
				Actions.Run();
				return;
			}
		}
		foreach (MechanicEntity item in enumerable)
		{
			using (base.Context.GetDataScope((TargetWrapper)item))
			{
				Actions.Run();
			}
		}
	}

	private bool IsConditionPassed(MechanicsContext context, BaseUnitEntity unit)
	{
		using (context.GetDataScope(unit.ToITargetWrapper()))
		{
			return Condition.Check();
		}
	}
}
