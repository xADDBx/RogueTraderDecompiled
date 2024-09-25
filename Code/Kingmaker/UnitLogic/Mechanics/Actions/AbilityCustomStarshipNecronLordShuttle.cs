using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Particles;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("f1745a9dceb1b0e40bec6d4e68eecaf0")]
public class AbilityCustomStarshipNecronLordShuttle : AbilityCustomLogic, IAbilityTargetRestriction
{
	private enum ActionMode
	{
		AimingAbility,
		ShuttleAbility
	}

	[SerializeField]
	private BlueprintFeatureReference m_NecronLordFeature;

	[SerializeField]
	private ActionMode m_ActionMode;

	[SerializeField]
	[ShowIf("IsShuttleMode")]
	private BlueprintProjectileReference m_EscapeProjectile;

	[SerializeField]
	private ActionList ActionsOnLaunch;

	[SerializeField]
	[ShowIf("IsShuttleMode")]
	private ActionList ActionsOnArrival;

	public BlueprintFeature NecronLordFeature => m_NecronLordFeature?.Get();

	public BlueprintProjectile EscapeProjectile => m_EscapeProjectile?.Get();

	public bool IsShuttleMode => m_ActionMode == ActionMode.ShuttleAbility;

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
		if (!IsShuttleMode)
		{
			using (context.GetDataScope(starship.ToITargetWrapper()))
			{
				ActionsOnLaunch.Run();
			}
			yield return new AbilityDeliveryTarget(starship);
		}
		List<StarshipEntity> list = (from u in GetCandidates(starship, starship.Position)
			select u as StarshipEntity).ToList();
		StarshipNecronLogic necronLogic = GetNecronLogic(starship);
		if (necronLogic != null)
		{
			list.Sort((StarshipEntity a, StarshipEntity b) => GetEffectiveHP(a, necronLogic).CompareTo(GetEffectiveHP(b, necronLogic)));
		}
		StarshipEntity client = list.FirstOrDefault();
		if (client == null)
		{
			yield break;
		}
		using (context.GetDataScope(client.ToITargetWrapper()))
		{
			ActionsOnLaunch.Run();
		}
		if (EscapeProjectile != null)
		{
			bool inFlight = true;
			FxBone fxBone = starship.View.ParticlesSnapMap["Locator_СenterFX_uniform"];
			new ProjectileLauncher(EscapeProjectile, starship, client).Ability(context.Ability).LaunchPosition(fxBone?.Transform.position).AttackResult(AttackResult.Hit)
				.OnHitCallback(delegate
				{
					inFlight = false;
				})
				.Launch();
			while (inFlight)
			{
				yield return null;
			}
		}
		using (context.GetDataScope(client.ToITargetWrapper()))
		{
			ActionsOnArrival.Run();
		}
		yield return new AbilityDeliveryTarget(client);
	}

	private StarshipNecronLogic GetNecronLogic(MechanicEntity unit)
	{
		return (unit as StarshipEntity).Blueprint.AddFacts.FirstOrDefault((BlueprintUnitFact fact) => fact as BlueprintFeature == NecronLordFeature)?.GetComponent<StarshipNecronLogic>();
	}

	private int GetEffectiveHP(StarshipEntity unit, StarshipNecronLogic necronLogic)
	{
		int num = unit.Health.HitPointsLeft;
		if (necronLogic != null && unit.Buffs.GetBuff(necronLogic.UndeathBuff) != null)
		{
			num -= unit.Health.MaxHitPoints * necronLogic.UndeathDamagePerTurnPct / 100;
		}
		return num;
	}

	private bool HaveReasonToShuttle(StarshipEntity caster, StarshipEntity friend, StarshipNecronLogic necronLogic)
	{
		return GetEffectiveHP(friend, necronLogic) > GetEffectiveHP(caster, necronLogic);
	}

	private IEnumerable<BaseUnitEntity> GetCandidates(MechanicEntity caster, Vector3 casterPosition)
	{
		StarshipNecronLogic necronLogic = GetNecronLogic(caster);
		if (necronLogic == null)
		{
			return null;
		}
		return Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity u) => u is StarshipEntity && u.IsAlly(caster) && u.Blueprint.AddFacts.Contains(NecronLordFeature) && WarhammerGeometryUtils.DistanceToInCells(casterPosition, default(IntRect), u.Position, default(IntRect)) <= necronLogic.LordEscapeMaxDistance && HaveReasonToShuttle(caster as StarshipEntity, u as StarshipEntity, necronLogic));
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		bool flag = GetCandidates(ability.Caster, casterPosition).Any();
		if (!IsShuttleMode)
		{
			return !flag;
		}
		return flag;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
	}
}
