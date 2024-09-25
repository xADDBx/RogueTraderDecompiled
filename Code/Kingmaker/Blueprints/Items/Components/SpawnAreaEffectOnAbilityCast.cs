using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
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
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
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

	[HideIf("m_OnUnit")]
	[SerializeField]
	private bool m_GetOrientationFromCaster;

	[SerializeField]
	private bool m_ShowPredictionForMelee;

	[ShowIf("m_ShowPredictionForMelee")]
	[SerializeField]
	private bool m_NeedCurrentTargetForPrediction;

	public ActionList ActionsOnAllTargetsOnApply = new ActionList();

	public bool GetOrientationFromCaster => m_GetOrientationFromCaster;

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

	public BlueprintAbilityAreaEffect GetBlueprintAbilityAreaEffect(AbilityData ability, CustomGridNodeBase casterNode)
	{
		if (!ability.IsMelee)
		{
			return null;
		}
		if (!m_NeedCurrentTargetForPrediction)
		{
			if (!m_Restrictions.IsPassed(base.Fact, base.Context, null, ability))
			{
				return null;
			}
			return AreaEffect;
		}
		PointerController clickEventsController = Game.Instance.ClickEventsController;
		TargetWrapper target = Game.Instance.SelectedAbilityHandler.GetTarget(clickEventsController.PointerOn, clickEventsController.WorldPosition, ability, ability.Caster.Position);
		if (target != null && target.Entity != null && target.Entity != ability.Caster && target.Entity.DistanceToInCells(casterNode.Vector3Position, ability.Caster.SizeRect) == 1)
		{
			if (!m_Restrictions.IsPassed(base.Fact, base.Context, null, ability, target.Entity))
			{
				return null;
			}
			return AreaEffect;
		}
		return null;
	}

	private void TryToTrigger(RulePerformAbility evt)
	{
		if (!m_Restrictions.IsPassed(base.Fact, evt, evt.Spell) || (bool)ContextData<UnitHelper.PreviewUnit>.Current || base.Owner.IsPreviewUnit)
		{
			return;
		}
		TimeSpan seconds = m_DurationValue.Calculate(base.Context).Seconds;
		AreaEffectEntity areaEffectEntity;
		if (!m_OnUnit)
		{
			MechanicsContext context = base.Context;
			BlueprintAbilityAreaEffect areaEffect = AreaEffect;
			TargetWrapper spellTarget = evt.SpellTarget;
			TimeSpan? duration = seconds;
			bool getOrientationFromCaster = m_GetOrientationFromCaster;
			areaEffectEntity = AreaEffectsController.Spawn(context, areaEffect, spellTarget, duration, null, getOrientationFromCaster);
		}
		else
		{
			areaEffectEntity = AreaEffectsController.SpawnAttachedToTarget(base.Context, AreaEffect, (BaseUnitEntity)evt.SpellTarget.Entity, seconds);
		}
		AreaEffectEntity areaEffectEntity2 = areaEffectEntity;
		if (areaEffectEntity2 == null)
		{
			return;
		}
		foreach (BaseUnitEntity u in Game.Instance.State.AllBaseUnits)
		{
			if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity2.Blueprint.IsAllArea && areaEffectEntity2.Contains(u) && (areaEffectEntity2.AffectEnemies || evt.Initiator.IsEnemy(u)))
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
