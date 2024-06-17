using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("982407ed6d4c4b34c9e135d2d1c79859")]
public class StarshipActionsOnUnits : AbilityCustomLogic
{
	public bool ignoreSoft;

	public int repeatsMin;

	public int repeatsMax;

	[SerializeField]
	private BlueprintProjectileReference m_Projectile;

	public ActionList Actions;

	public BlueprintProjectile Projectile => m_Projectile?.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!(context.MaybeCaster is StarshipEntity starshipEntity))
		{
			yield break;
		}
		IEnumerable<AbstractUnitEntity> enumerable = Game.Instance.State.AllUnits.Where((AbstractUnitEntity u) => u is StarshipEntity && (!ignoreSoft || !(u as StarshipEntity).Blueprint.IsSoftUnit) && context.Ability.CanTarget(u));
		int projectilesInFlight = 0;
		foreach (AbstractUnitEntity targetShip in enumerable)
		{
			FxBone fxBone = starshipEntity.View.ParticlesSnapMap["Locator_СenterFX_uniform"];
			if (Projectile != null)
			{
				new ProjectileLauncher(Projectile, starshipEntity, targetShip).Ability(context.Ability).LaunchPosition(fxBone?.Transform.position).AttackResult(AttackResult.Hit)
					.OnHitCallback(delegate
					{
						RunActions(targetShip);
						int num3 = projectilesInFlight - 1;
						projectilesInFlight = num3;
					})
					.Launch();
				int num = projectilesInFlight + 1;
				projectilesInFlight = num;
			}
			else
			{
				RunActions(targetShip);
			}
		}
		while (projectilesInFlight > 0)
		{
			yield return null;
		}
		yield return null;
		yield return new AbilityDeliveryTarget(target);
		void RunActions(MechanicEntity ship)
		{
			int num2 = PFStatefulRandom.SpaceCombat.Range(repeatsMin, repeatsMax + 1);
			for (int i = 0; i < num2; i++)
			{
				using (context.GetDataScope(ship.ToITargetWrapper()))
				{
					Actions.Run();
				}
			}
		}
	}
}
