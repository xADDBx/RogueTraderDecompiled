using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("7b639717dfd57bb469d3b87e09d5f61e")]
public class WarhammerContextActionSetStarshipDirection : ContextAction
{
	private enum RotationType
	{
		FixedAngle,
		RandomAngle
	}

	[SerializeField]
	private RotationType Rotation;

	[SerializeField]
	private PropertyCalculator Angle;

	[SerializeField]
	private int maximalTargetInertiaToApplyLowInertiaAngle = -1;

	[SerializeField]
	private PropertyCalculator LowInertiaAngle;

	[SerializeField]
	private ActionList ActionsOnClockwiseTurn;

	[SerializeField]
	private ActionList ActionsOnCounterTurn;

	[SerializeField]
	private ActionList ActionsOnNoTurn;

	[SerializeField]
	[Tooltip("Damage is done as percent of max HP, modified with already taken damage, one instance for each 45 turn, cumulative")]
	private int damageBaseMin;

	[SerializeField]
	private int damageBaseMax;

	public override string GetCaption()
	{
		return Rotation switch
		{
			RotationType.FixedAngle => $"Turn by an angle of {Angle}", 
			RotationType.RandomAngle => $"Turn by a random angle from 0 to {Angle}", 
			_ => "<unknown rotation type>", 
		};
	}

	protected override void RunAction()
	{
		if (!(base.Target.Entity is StarshipEntity starshipEntity) || !(base.Caster is StarshipEntity caster))
		{
			return;
		}
		int resultOrientation = GetResultOrientation(starshipEntity, out var angle);
		resultOrientation = GetAlignedOrientation(resultOrientation);
		starshipEntity.SetOrientation(resultOrientation);
		starshipEntity.SnapToGrid();
		if (damageBaseMin > 0 && !starshipEntity.Blueprint.IsSoftUnit)
		{
			for (int i = 40; i <= Math.Abs(angle); i += 45)
			{
				DoDamage(caster, starshipEntity);
			}
		}
		if (angle > 0)
		{
			ActionsOnClockwiseTurn.Run();
		}
		else if (angle < 0)
		{
			ActionsOnCounterTurn.Run();
		}
		else
		{
			ActionsOnNoTurn.Run();
		}
	}

	private void DoDamage(StarshipEntity caster, StarshipEntity target)
	{
		PartHealth health = target.Health;
		int num = health.Damage * damageBaseMin / 100;
		if (num > 0)
		{
			int max = health.Damage * damageBaseMax / 100;
			DamageData damage = new DamageData(DamageType.Warp, num, max);
			Rulebook.Trigger(new RuleDealDamage(caster, target, damage));
		}
	}

	private int GetResultOrientation(StarshipEntity target, out int angle)
	{
		PropertyContext context = new PropertyContext(base.Context.MaybeCaster, null, null, base.Context);
		angle = (((int)target.Stats.GetStat(StatType.Inertia) > maximalTargetInertiaToApplyLowInertiaAngle) ? Angle.GetValue(context) : LowInertiaAngle.GetValue(context));
		return Rotation switch
		{
			RotationType.FixedAngle => (int)target.Orientation + angle, 
			RotationType.RandomAngle => (int)target.Orientation + PFStatefulRandom.Mechanics.Range(0, angle + 1), 
			_ => 0, 
		};
	}

	private int GetAlignedOrientation(int orientation)
	{
		return (orientation + 360 + 22) / 45 * 45 % 360;
	}
}
