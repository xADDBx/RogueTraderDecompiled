using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("1be3e39bdea40cc4a8a1322ad5166eba")]
public class AbilityCustomStarshipBoardingTeam : AbilityCustomLogic, IAbilityTargetRestriction
{
	[SerializeField]
	private BlueprintProjectileReference m_ForwardProjectile;

	[SerializeField]
	private int ForwardCountMin;

	[SerializeField]
	private int ForwardCountMax;

	[SerializeField]
	private float ForwardIntervalMin;

	[SerializeField]
	private float ForwardIntervalMax;

	[SerializeField]
	private BlueprintProjectileReference m_ReturnProjectile;

	[SerializeField]
	private int ReturnCountMin;

	[SerializeField]
	private int ReturnCountMax;

	[SerializeField]
	private float ReturnIntervalMin;

	[SerializeField]
	private float ReturnIntervalMax;

	[SerializeField]
	private float delayBeforeReturning;

	[SerializeField]
	private float delayBeforeBoardingEnds;

	[SerializeField]
	private ActionList ActionsOnArrival;

	[SerializeField]
	private ActionList ActionsInTheMiddle;

	[SerializeField]
	private ActionList ActionsOnEnding;

	public BlueprintProjectile ForwardProjectile => m_ForwardProjectile?.Get();

	public BlueprintProjectile ReturnProjectile => m_ReturnProjectile?.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (!(maybeCaster is StarshipEntity starship))
		{
			yield break;
		}
		int projectilesInFlight = 0;
		double startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
		if ((bool)ForwardProjectile)
		{
			int projectilesLaunched = (projectilesInFlight = PFStatefulRandom.SpaceCombat.Range(ForwardCountMin, ForwardCountMax + 1));
			float totalPause = 0f;
			int q = 0;
			while (q < projectilesLaunched)
			{
				while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)totalPause)
				{
					yield return null;
				}
				FxBone fxBone = starship.View.ParticlesSnapMap["Locator_СenterFX_uniform"];
				new ProjectileLauncher(ForwardProjectile, starship, target).Ability(context.Ability).LaunchPosition(fxBone?.Transform.position).AttackResult(AttackResult.Hit)
					.OnHitCallback(delegate
					{
						int num3 = projectilesInFlight - 1;
						projectilesInFlight = num3;
					})
					.Launch();
				totalPause += PFStatefulRandom.SpaceCombat.Range(ForwardIntervalMin, ForwardIntervalMax);
				int num = q + 1;
				q = num;
			}
			while (projectilesInFlight > projectilesLaunched / 2)
			{
				yield return null;
			}
		}
		using (context.GetDataScope(target))
		{
			ActionsOnArrival.Run();
		}
		startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
		while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)(delayBeforeReturning / 2f))
		{
			yield return null;
		}
		using (context.GetDataScope(target))
		{
			ActionsInTheMiddle.Run();
		}
		while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)delayBeforeReturning)
		{
			yield return null;
		}
		projectilesInFlight = 0;
		if ((bool)ReturnProjectile)
		{
			projectilesInFlight = 1;
			int projectilesLaunched = (projectilesInFlight = PFStatefulRandom.SpaceCombat.Range(ReturnCountMin, ReturnCountMax + 1));
			float totalPause = delayBeforeReturning;
			int q = 0;
			while (q < projectilesLaunched)
			{
				while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)totalPause)
				{
					yield return null;
				}
				new ProjectileLauncher(ReturnProjectile, target, starship).Ability(context.Ability).AttackResult(AttackResult.Hit).OnHitCallback(delegate
				{
					int num2 = projectilesInFlight - 1;
					projectilesInFlight = num2;
				})
					.Launch();
				totalPause += PFStatefulRandom.SpaceCombat.Range(ForwardIntervalMin, ForwardIntervalMax);
				int num = q + 1;
				q = num;
			}
		}
		while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)(delayBeforeBoardingEnds + delayBeforeReturning))
		{
			yield return null;
		}
		using (context.GetDataScope(target))
		{
			ActionsOnEnding.Run();
		}
		while (projectilesInFlight > 0)
		{
			yield return null;
		}
		yield return new AbilityDeliveryTarget(target);
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (target.Entity is StarshipEntity starshipEntity)
		{
			return !starshipEntity.Blueprint.IsSoftUnit;
		}
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
	}
}
