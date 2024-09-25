using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.ActivatableAbilities;

public sealed class ActivatableAbility : UnitFact<BlueprintActivatableAbility>, IUnitCommandActHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitCommandEndHandler, IUnitRunCommandHandler, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, IInitiatorRulebookSubscriber, IEntitySubscriber, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, IUnitBuffHandler, IApplyAbilityEffectHandler, IHashable
{
	[JsonProperty]
	private bool m_IsOn;

	[JsonProperty]
	[CanBeNull]
	private Buff m_AppliedBuff;

	[JsonProperty]
	private TargetWrapper m_Target;

	[JsonProperty]
	private TimeSpan m_TurnOnTime;

	[JsonProperty]
	private bool m_WasInCombat;

	private bool m_ShouldBeDeactivatedInNextRound;

	private BlueprintComponentAndRuntime<ActivatableAbilityResourceLogic>[] m_CachedResourceLogic;

	private BlueprintComponentAndRuntime<ActivatableAbilityRestriction>[] m_CachedRestrictions;

	[JsonProperty]
	public float TimeToNextRound { get; set; }

	[JsonProperty]
	public bool IsStarted { get; private set; }

	[JsonProperty]
	public bool ReadyToStart { get; private set; }

	[JsonProperty]
	[CanBeNull]
	public Ability SelectTargetAbility { get; private set; }

	public bool IsOn
	{
		get
		{
			return m_IsOn;
		}
		set
		{
			SetIsOn(value, null);
		}
	}

	[CanBeNull]
	public Buff AppliedBuff => m_AppliedBuff;

	public bool IsAvailableByResources
	{
		get
		{
			m_CachedResourceLogic = m_CachedResourceLogic ?? SelectComponentsWithRuntime<ActivatableAbilityResourceLogic>().ToArray();
			return !m_CachedResourceLogic.HasItem((BlueprintComponentAndRuntime<ActivatableAbilityResourceLogic> i) => !i.Component.IsAvailable(i.Runtime));
		}
	}

	public bool IsAvailableByRestrictions
	{
		get
		{
			if (base.Blueprint.OnlyInCombat && !base.Owner.IsInCombat)
			{
				return false;
			}
			m_CachedRestrictions = m_CachedRestrictions ?? SelectComponentsWithRuntime<ActivatableAbilityRestriction>().ToArray();
			return !m_CachedRestrictions.HasItem((BlueprintComponentAndRuntime<ActivatableAbilityRestriction> i) => !i.Component.IsAvailable(i.Runtime));
		}
	}

	public int? ResourceCount
	{
		get
		{
			m_CachedResourceLogic = m_CachedResourceLogic ?? SelectComponentsWithRuntime<ActivatableAbilityResourceLogic>().ToArray();
			ActivatableAbilityResourceLogic component = m_CachedResourceLogic.FirstItem().Component;
			BlueprintAbilityResource blueprintAbilityResource = ((component != null && component.SpendType != 0) ? component.RequiredResource : null);
			int num = (blueprintAbilityResource ? base.Owner.AbilityResources.GetResourceAmount(blueprintAbilityResource) : ((base.SourceItem == null) ? (-1) : (base.SourceItem.IsSpendCharges ? base.SourceItem.Charges : (-1))));
			if (num >= 0)
			{
				return num;
			}
			return null;
		}
	}

	public bool IsAvailable
	{
		get
		{
			if (IsAvailableByResources)
			{
				return IsAvailableByRestrictions;
			}
			return false;
		}
	}

	public bool IsWaitingForTarget
	{
		get
		{
			if (base.Blueprint.IsTargeted && IsOn)
			{
				return m_Target == null;
			}
			return false;
		}
	}

	public ActivatableAbility(BlueprintActivatableAbility blueprint)
		: base(blueprint)
	{
	}

	[JsonConstructor]
	private ActivatableAbility(JsonConstructorMark _)
	{
	}

	private void SetIsOn(bool value, [CanBeNull] BaseUnitEntity target)
	{
		if (m_IsOn != value)
		{
			m_IsOn = value;
			m_TurnOnTime = Game.Instance.TimeController.RealTime;
			if (IsWaitingForTarget)
			{
				m_Target = target;
			}
			else if (target != null)
			{
				PFLog.Default.Error("ActivatableAbility.SetIsOn: !IsWaitingForTarget && target != null");
			}
			if (m_IsOn)
			{
				OnDidTurnOn();
			}
			else
			{
				OnDidTurnOff();
			}
			if (IsWaitingForTarget && SelectTargetAbility != null && base.Owner.IsDirectlyControllable)
			{
				Game.Instance.SelectedAbilityHandler.SetAbility(SelectTargetAbility.Data);
			}
		}
	}

	public void TurnOffImmediately()
	{
		SetIsOn(value: false, null);
	}

	public void SetIsOnWithTarget(bool value, [NotNull] BaseUnitEntity target)
	{
		SetIsOn(value, target);
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (base.Blueprint.IsTargeted)
		{
			SelectTargetAbility = base.Owner.Facts.Add(new Ability(base.Blueprint.SelectTargetAbility, base.Owner));
		}
	}

	protected override void OnDeactivate()
	{
		if (SelectTargetAbility != null)
		{
			base.Owner.Facts.Remove(SelectTargetAbility);
			SelectTargetAbility = null;
		}
		Stop();
		base.OnDeactivate();
	}

	public void OnNewRound()
	{
		m_WasInCombat |= base.Owner.IsInCombat;
		if (m_ShouldBeDeactivatedInNextRound || !IsOn || !IsAvailable || (base.Blueprint.DeactivateIfCombatEnded && !base.Owner.IsInCombat && (base.Blueprint.ActivateOnCombatStarts || m_WasInCombat)))
		{
			Stop();
		}
		else
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnNewRound();
			});
		}
		m_ShouldBeDeactivatedInNextRound = base.Blueprint.DeactivateAfterFirstRound;
	}

	public void HandleUnitJoinCombat()
	{
		if (base.Blueprint.ActivateOnCombatStarts && m_IsOn && !IsStarted && base.Blueprint.ActivateImmediately)
		{
			TryStart();
		}
	}

	public void HandleUnitLeaveCombat()
	{
		if (base.Blueprint.DeactivateIfCombatEnded && IsStarted && base.Blueprint.DeactivateImmediately)
		{
			Stop();
		}
	}

	public void HandleUnitRunCommand(AbstractUnitCommand command)
	{
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
	}

	public void TryStart()
	{
		if (IsStarted || IsWaitingForTarget || !IsAvailable)
		{
			return;
		}
		ReadyToStart = true;
		if (base.Blueprint.ActivateOnCombatStarts && !base.Owner.IsInCombat)
		{
			return;
		}
		if (base.Blueprint.Group != 0)
		{
			int num = 0;
			foreach (ActivatableAbility rawFact in base.Owner.ActivatableAbilities.RawFacts)
			{
				if (rawFact.Blueprint.Group == base.Blueprint.Group && (rawFact.IsStarted || rawFact.IsOn))
				{
					num += rawFact.Blueprint.WeightInGroup;
				}
			}
			int num2 = base.Owner.GetOptional<UnitPartActivatableAbility>()?.GetGroupSize(base.Blueprint.Group) ?? 1;
			if (num > num2)
			{
				return;
			}
		}
		m_WasInCombat = base.Owner.IsInCombat;
		IsStarted = true;
		ReapplyBuff();
		CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
		{
			l.OnStart();
		});
		TimeToNextRound = 0f;
		ReadyToStart = false;
	}

	public void Stop(bool forceRemovedBuff = false)
	{
		if (IsStarted)
		{
			EventBus.RaiseEvent(delegate(IActivatableAbilityWillStopHandler h)
			{
				h.HandleActivatableAbilityWillStop(this);
			});
			m_WasInCombat = false;
			IsStarted = false;
			TimeToNextRound = 0f;
			m_AppliedBuff?.Remove();
			m_AppliedBuff = null;
			if (m_IsOn && !IsAvailableByRestrictions)
			{
				m_IsOn = false;
				m_TurnOnTime = Game.Instance.TimeController.RealTime;
			}
		}
	}

	public void ReapplyBuff()
	{
		if (!IsOn || !IsStarted)
		{
			return;
		}
		if (m_AppliedBuff != null)
		{
			Buff appliedBuff = m_AppliedBuff;
			m_AppliedBuff = null;
			appliedBuff.Remove();
		}
		if (!base.Blueprint.Buff)
		{
			return;
		}
		if (base.Blueprint.Group == ActivatableAbilityGroup.BardicPerformance)
		{
			foreach (Buff rawFact in base.Owner.Buffs.RawFacts)
			{
				if (rawFact.Context.Root.AssociatedBlueprint is BlueprintActivatableAbility { Group: ActivatableAbilityGroup.BardicPerformance })
				{
					rawFact.Remove();
					break;
				}
			}
		}
		MechanicsContext parentContext = new MechanicsContext(base.Owner, null, base.Blueprint);
		m_AppliedBuff = base.Owner.Buffs.Add(base.Blueprint.Buff, parentContext);
		m_AppliedBuff?.AddSource(this);
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
		if (m_IsOn && base.Blueprint.ActivateOnUnitAction && base.Blueprint.ActivateOnUnitActionType == AbilityActivateOnUnitActionType.CastSpell)
		{
			TryStart();
		}
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
	}

	private void OnAttack(BlueprintItemWeapon weapon)
	{
		if (m_IsOn && IsStarted && IsAvailable)
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnAttack(weapon);
			});
			if (!IsAvailable && base.Blueprint.DeactivateImmediately)
			{
				Stop();
			}
		}
	}

	private void OnHit()
	{
		if (m_IsOn && IsStarted && IsAvailable)
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnHit();
			});
			if (!IsAvailable && base.Blueprint.DeactivateImmediately)
			{
				Stop();
			}
		}
	}

	private void OnCrit()
	{
		if (m_IsOn && IsStarted && IsAvailable)
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnCrit();
			});
			if (!IsAvailable && base.Blueprint.DeactivateImmediately)
			{
				Stop();
			}
		}
	}

	public void HandleBuffDidAdded(Buff buff)
	{
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (m_AppliedBuff == buff && IsStarted)
		{
			IsOn = false;
			if (IsStarted)
			{
				Stop();
			}
			m_ShouldBeDeactivatedInNextRound = false;
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}

	private void OnDidTurnOn()
	{
		if (IsWaitingForTarget)
		{
			return;
		}
		if (base.Blueprint.Group != 0)
		{
			ActivatableAbility[] array = (from a in base.Owner.ActivatableAbilities.Enumerable
				where a.Blueprint.Group == base.Blueprint.Group && a.IsOn
				orderby a.m_TurnOnTime
				select a).ToArray();
			int num = 0;
			ActivatableAbility[] array2 = array;
			foreach (ActivatableAbility activatableAbility in array2)
			{
				num += activatableAbility.Blueprint.WeightInGroup;
			}
			int num2 = base.Owner.GetOptional<UnitPartActivatableAbility>()?.GetGroupSize(base.Blueprint.Group) ?? 1;
			array2 = array;
			foreach (ActivatableAbility activatableAbility2 in array2)
			{
				if (num > num2)
				{
					activatableAbility2.IsOn = false;
					num -= activatableAbility2.Blueprint.WeightInGroup;
				}
			}
		}
		if (!IsStarted)
		{
			CallComponents(delegate(IActivatableAbilitySpendResourceLogic l)
			{
				l.OnAbilityTurnOn();
			});
			if (base.Blueprint.ActivateImmediately)
			{
				TryStart();
			}
		}
	}

	public bool IsActivateWithSameCommand(ActivatableAbility other)
	{
		if (base.Blueprint.Group == ActivatableAbilityGroup.Judgment)
		{
			return base.Blueprint.Group == other.Blueprint.Group;
		}
		return false;
	}

	private void OnDidTurnOff()
	{
		m_Target = null;
		ReadyToStart = false;
		if (IsStarted && base.Blueprint.DeactivateImmediately)
		{
			Stop();
		}
	}

	public void TurnOnWithTarget([NotNull] MechanicEntity target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (!IsWaitingForTarget)
		{
			throw new InvalidOperationException("Activatable ability is not waiting for target");
		}
		m_Target = target;
		OnDidTurnOn();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (m_IsOn && IsStarted && ((m_AppliedBuff != null && !m_AppliedBuff.Active && !m_AppliedBuff.IsSuppressed) || m_AppliedBuff?.Blueprint != base.Blueprint.Buff))
		{
			ReapplyBuff();
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		m_AppliedBuff = null;
		m_Target = null;
		m_CachedResourceLogic = null;
		m_CachedRestrictions = null;
	}

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		if (IsWaitingForTarget && context.Ability.Fact == SelectTargetAbility)
		{
			TurnOnWithTarget(target.Target.Entity);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		float val2 = TimeToNextRound;
		result.Append(ref val2);
		result.Append(ref m_IsOn);
		Hash128 val3 = ClassHasher<Buff>.GetHash128(m_AppliedBuff);
		result.Append(ref val3);
		bool val4 = IsStarted;
		result.Append(ref val4);
		bool val5 = ReadyToStart;
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<TargetWrapper>.GetHash128(m_Target);
		result.Append(ref val6);
		result.Append(ref m_TurnOnTime);
		result.Append(ref m_WasInCombat);
		Hash128 val7 = ClassHasher<Ability>.GetHash128(SelectTargetAbility);
		result.Append(ref val7);
		return result;
	}
}
