using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("79fda134647d40bdbf0a98145b65e379")]
public class TutorialTriggerTorpedoSpawn : TutorialTrigger, ITorpedoSpawnHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IApplyAbilityEffectHandler, IHashable
{
	private enum Faction
	{
		IsPlayer,
		IsPlayerEnemy
	}

	[SerializeField]
	private Faction m_TorpedoCasterFaction;

	public void HandleTorpedoSpawn(BaseUnitEntity caster)
	{
		if (m_TorpedoCasterFaction == Faction.IsPlayerEnemy && caster.Faction.IsPlayerEnemy)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = caster;
			});
		}
	}

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		if (m_TorpedoCasterFaction != 0 || !context.Caster.IsInPlayerParty)
		{
			return;
		}
		ItemEntityStarshipWeapon starshipWeapon = context.Ability.StarshipWeapon;
		if (starshipWeapon != null && starshipWeapon.Blueprint.WeaponType == StarshipWeaponType.TorpedoTubes)
		{
			TryToTrigger(null, delegate(TutorialContext tutorialContext)
			{
				tutorialContext.SourceUnit = GameHelper.GetPlayerCharacter();
			});
		}
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
