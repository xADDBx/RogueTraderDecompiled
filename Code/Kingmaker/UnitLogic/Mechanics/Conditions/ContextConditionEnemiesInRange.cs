using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[TypeId("e2b6e577290c4805b118ca0dc9b901f7")]
public class ContextConditionEnemiesInRange : ContextCondition
{
	public int Range;

	public int MinimalEnemies;

	public int MaximalEnemies;

	protected override string GetConditionCaption()
	{
		return "Check if there is enough enemies in range";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity caster = base.Context.MaybeCaster;
		if (caster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return false;
		}
		int num = EntityBoundsHelper.FindUnitsInRange(caster.Position, Range.Cells().Meters).Count((BaseUnitEntity p) => p.CombatGroup.IsEnemy(caster) && !p.LifeState.IsDead);
		if (num <= MaximalEnemies || MaximalEnemies < 1)
		{
			return num >= MinimalEnemies;
		}
		return false;
	}
}
