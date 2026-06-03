using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Group("Starship")]
[TypeId("7b639717dfd57bb469d3b87e09d5f61e")]
public class WarhammerContextActionSetStarshipDirection : ContextAction
{
	private enum RotationType
	{
		FixedAngle,
		RandomAngle
	}

	[Tooltip("If true will rotate the caster instead of the target")]
	[SerializeField]
	private bool RotateCaster;

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
		if (!(base.Target.Entity is StarshipEntity starshipEntity) || !(base.Caster is StarshipEntity starshipEntity2))
		{
			return;
		}
		StarshipEntity starshipEntity3 = (RotateCaster ? starshipEntity2 : starshipEntity);
		int resultOrientation = GetResultOrientation(starshipEntity3, out var angle);
		resultOrientation = GetAlignedOrientation(resultOrientation);
		if (!AbilityTargetCanTurn.CheckPositionAfterRotation(starshipEntity3, resultOrientation))
		{
			PFLog.Default.Error($"Cannot turn {starshipEntity} with WarhammerContextActionSetStarshipDirection by {starshipEntity2}");
		}
		starshipEntity3.SetOrientation(resultOrientation);
		starshipEntity3.SnapToGrid();
		if (damageBaseMin > 0 && !starshipEntity.Blueprint.IsSoftUnit)
		{
			for (int i = 40; i <= Math.Abs(angle); i += 45)
			{
				DoDamage(starshipEntity2, starshipEntity);
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

	private int GetResultOrientation(StarshipEntity rotationTarget, out int angle)
	{
		PropertyContext context = new PropertyContext(base.Context.MaybeCaster, null, null, base.Context);
		angle = (((int)rotationTarget.Stats.GetStat(StatType.Inertia) > maximalTargetInertiaToApplyLowInertiaAngle) ? Angle.GetValue(context) : LowInertiaAngle.GetValue(context));
		return Rotation switch
		{
			RotationType.FixedAngle => (int)rotationTarget.Orientation + angle, 
			RotationType.RandomAngle => (int)rotationTarget.Orientation + PFStatefulRandom.Mechanics.Range(0, angle + 1), 
			_ => 0, 
		};
	}

	private int GetAlignedOrientation(int orientation)
	{
		return (orientation + 360 + 22) / 45 * 45 % 360;
	}
}
