using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Visual.Sound;

public class LifeStateAsksController : ITickUnitAsksController, IUnitAsksController, IDisposable, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IDamageHandler
{
	private class DamageData
	{
		public readonly RuleDealDamage LastHandledDamage;

		public readonly UnitLifeState UnitLifeState;

		public readonly bool IsEnemyOfInitiator;

		public readonly bool IsPlayerFaction;

		public DamageData(RuleDealDamage lastHandledDamage, BaseUnitEntity target)
		{
			LastHandledDamage = lastHandledDamage;
			UnitLifeState = target.LifeState.State;
			IsEnemyOfInitiator = lastHandledDamage != null && lastHandledDamage.InitiatorUnit != null && lastHandledDamage.Target.IsEnemy(lastHandledDamage.InitiatorUnit);
			IsPlayerFaction = target.IsPlayerFaction;
		}
	}

	private readonly Dictionary<BaseUnitEntity, DamageData> m_DamagedUnits = new Dictionary<BaseUnitEntity, DamageData>();

	private readonly Dictionary<BaseUnitEntity, int> m_EnemyKillsCounter = new Dictionary<BaseUnitEntity, int>();

	private readonly List<BarkWrapper> m_Barks = new List<BarkWrapper>();

	private readonly List<BarkWrapper> m_PersonalizedBarks = new List<BarkWrapper>();

	public LifeStateAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
		m_DamagedUnits.Clear();
		m_EnemyKillsCounter.Clear();
		m_Barks.Clear();
		m_PersonalizedBarks.Clear();
	}

	private void CacheDamage(BaseUnitEntity entity, DamageData lastDamageData)
	{
		if (lastDamageData != null)
		{
			if (m_DamagedUnits.ContainsKey(entity))
			{
				m_DamagedUnits[entity] = lastDamageData;
			}
			else
			{
				m_DamagedUnits.Add(entity, lastDamageData);
			}
		}
	}

	void IUnitLifeStateChanged.HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity is UnitEntity && prevLifeState == UnitLifeState.Conscious && baseUnitEntity.LifeState.State != 0)
		{
			CacheDamage(EventInvokerExtensions.BaseUnitEntity, new DamageData(baseUnitEntity.Health.LastHandledDamage, baseUnitEntity));
		}
	}

	void IDamageHandler.HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if ((bool)dealDamage.ConcreteTarget.View && dealDamage.ConcreteTarget is UnitEntity unitEntity)
		{
			CacheDamage(unitEntity, new DamageData(dealDamage, unitEntity));
		}
	}

	public void Tick()
	{
		BaseUnitEntity key;
		foreach (KeyValuePair<BaseUnitEntity, DamageData> damagedUnit in m_DamagedUnits)
		{
			damagedUnit.Deconstruct(out key, out var value);
			BaseUnitEntity baseUnitEntity = key;
			DamageData damageData = value;
			if (baseUnitEntity == null)
			{
				continue;
			}
			CacheEnemyKills(damageData);
			if (!baseUnitEntity.IsDisposed && !baseUnitEntity.IsDisposingNow)
			{
				bool unitCanSpeak = baseUnitEntity.CanSpeakAsks() && !(damageData.LastHandledDamage?.DisableFxAndSound ?? false);
				if (baseUnitEntity.LifeState.IsConscious)
				{
					SpeakGetDamage(unitCanSpeak, baseUnitEntity);
				}
				else if (baseUnitEntity.LifeState.IsUnconscious)
				{
					SpeakBecomeUnconscious(unitCanSpeak, baseUnitEntity, damageData);
				}
				else if (baseUnitEntity.LifeState.IsDead)
				{
					SpeakBecomeDead(unitCanSpeak, baseUnitEntity);
				}
			}
		}
		m_DamagedUnits.Clear();
		foreach (KeyValuePair<BaseUnitEntity, int> item in m_EnemyKillsCounter)
		{
			item.Deconstruct(out key, out var value2);
			BaseUnitEntity baseUnitEntity2 = key;
			int num = value2;
			if (!(baseUnitEntity2?.View == null) && baseUnitEntity2.View.Asks != null)
			{
				BarkWrapper barkWrapper = baseUnitEntity2.View.Asks.EnemyDeath;
				if (num >= UnitAsksHelper.EnemyMassDeathKillsCount && baseUnitEntity2.View.Asks.EnemyMassDeath.HasBarks)
				{
					barkWrapper = baseUnitEntity2.View.Asks?.EnemyMassDeath;
				}
				barkWrapper.Schedule();
			}
		}
		m_EnemyKillsCounter.Clear();
	}

	private static void SpeakBecomeDead(bool unitCanSpeak, BaseUnitEntity unit)
	{
		if (unitCanSpeak)
		{
			unit.View.Asks?.Death.Schedule();
		}
	}

	private void SpeakBecomeUnconscious(bool unitCanSpeak, BaseUnitEntity unit, DamageData damageData)
	{
		if (damageData.IsPlayerFaction)
		{
			HandlePartyMemberUnconscious(unit);
			unit.View.Asks?.Unconscious.Schedule();
		}
		else if (unitCanSpeak)
		{
			unit.View.Asks?.Unconscious.Schedule();
		}
	}

	private static void SpeakGetDamage(bool unitCanSpeak, BaseUnitEntity unit)
	{
		if (unitCanSpeak && unit.View.Asks != null)
		{
			(((float)unit.Health.HitPointsLeft < (float)unit.Health.MaxHitPoints * UnitAsksHelper.LowHealthBarkHPPercent) ? unit.View.Asks.LowHealth : unit.View.Asks.Pain)?.Schedule();
		}
	}

	private void CacheEnemyKills(DamageData damage)
	{
		RuleDealDamage lastHandledDamage = damage.LastHandledDamage;
		if (lastHandledDamage != null && damage.UnitLifeState == UnitLifeState.Dead && damage.IsEnemyOfInitiator && lastHandledDamage.InitiatorUnit != null && lastHandledDamage.InitiatorUnit.CanSpeakAsks())
		{
			if (m_EnemyKillsCounter.ContainsKey(lastHandledDamage.InitiatorUnit))
			{
				m_EnemyKillsCounter[lastHandledDamage.InitiatorUnit]++;
			}
			else
			{
				m_EnemyKillsCounter.Add(lastHandledDamage.InitiatorUnit, 1);
			}
		}
	}

	private static BarkWrapper GetPartyMemberUnconsciousBark(UnitBarksManager asksComponent, MechanicEntity unconsciousUnit)
	{
		BarkWrapper[] partyMemberUnconsciousPersonalized = asksComponent.PartyMemberUnconsciousPersonalized;
		foreach (BarkWrapper barkWrapper in partyMemberUnconsciousPersonalized)
		{
			if (barkWrapper.HasBarks && !barkWrapper.IsOnCooldown && barkWrapper.Bark is UnitAsksComponent.PersonalizedBark personalizedBark && Enumerable.Any(personalizedBark.UnitReferences, (BlueprintUnitReference unitReference) => unitReference.Get() == unconsciousUnit.Blueprint))
			{
				return barkWrapper;
			}
		}
		return asksComponent.PartyMemberUnconscious;
	}

	private void HandlePartyMemberUnconscious(BaseUnitEntity unit)
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.State.PlayerState.PartyAndPets)
		{
			if (partyAndPet == null || !partyAndPet.LifeState.IsConscious || partyAndPet.View == null || partyAndPet.View.Asks == null)
			{
				continue;
			}
			BarkWrapper partyMemberUnconsciousBark = GetPartyMemberUnconsciousBark(partyAndPet.View.Asks, unit);
			if (partyMemberUnconsciousBark != null)
			{
				if (partyMemberUnconsciousBark.Bark is UnitAsksComponent.PersonalizedBark)
				{
					m_PersonalizedBarks.Add(partyMemberUnconsciousBark);
				}
				else
				{
					m_Barks.Add(partyMemberUnconsciousBark);
				}
			}
		}
		(m_PersonalizedBarks.Random(PFStatefulRandom.Visuals.UnitAsks) ?? m_Barks.Random(PFStatefulRandom.Visuals.UnitAsks))?.Schedule();
		m_Barks.Clear();
		m_PersonalizedBarks.Clear();
	}
}
