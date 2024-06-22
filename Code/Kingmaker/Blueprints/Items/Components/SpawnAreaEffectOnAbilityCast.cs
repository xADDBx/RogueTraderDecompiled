using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("0368d351dda74eafa920effae9c1998d")]
public class SpawnAreaEffectOnAbilityCast : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_AreaEffect = new BlueprintAbilityAreaEffectReference();

	[SerializeField]
	private ContextDurationValue m_DurationValue;

	[SerializeField]
	private bool m_OnUnit;

	[SerializeField]
	private bool m_ShowPredictionForMelee;

	public ActionList ActionsOnAllTargetsOnApply = new ActionList();

	private BlueprintAbilityAreaEffect AreaEffect => m_AreaEffect.Get();

	protected override void OnActivateOrPostLoad()
	{
		if (m_ShowPredictionForMelee)
		{
			base.Owner.GetOrCreate<PartAbilityPredictionForAreaEffect>().Add(this);
		}
	}

	protected override void OnDeactivate()
	{
		if (m_ShowPredictionForMelee)
		{
			base.Owner.GetOptional<PartAbilityPredictionForAreaEffect>()?.Remove(this);
		}
	}

	public BlueprintAbilityAreaEffect GetBlueprintAbilityAreaEffect(AbilityData ability)
	{
		if (!m_Restrictions.IsPassed(base.Fact, base.Context, null, ability))
		{
			return null;
		}
		return AreaEffect;
	}

	private void TryToTrigger(RulePerformAbility evt)
	{
		using EntityFactComponentLoopGuard entityFactComponentLoopGuard = base.Runtime.RequestLoopGuard();
		if (entityFactComponentLoopGuard.Blocked || !m_Restrictions.IsPassed(base.Fact, evt, evt.Spell) || (bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			return;
		}
		TimeSpan seconds = m_DurationValue.Calculate(base.Context).Seconds;
		AreaEffectEntity areaEffectEntity = (m_OnUnit ? AreaEffectsController.SpawnAttachedToTarget(base.Context, AreaEffect, (BaseUnitEntity)evt.SpellTarget.Entity, seconds) : AreaEffectsController.Spawn(base.Context, AreaEffect, evt.SpellTarget, seconds));
		if (areaEffectEntity == null)
		{
			return;
		}
		foreach (BaseUnitEntity u in Game.Instance.State.AllBaseUnits)
		{
			if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity.Blueprint.IsAllArea && areaEffectEntity.Contains(u) && (areaEffectEntity.AffectEnemies || evt.Initiator.IsEnemy(u)))
			{
				EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
				{
					h.OnTryToApplyAbilityEffect(evt.Context, new AbilityDeliveryTarget(u));
				});
				if (ActionsOnAllTargetsOnApply.HasActions)
				{
					base.Fact.RunActionInContext(ActionsOnAllTargetsOnApply, u);
				}
			}
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
		TryToTrigger(evt);
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
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
