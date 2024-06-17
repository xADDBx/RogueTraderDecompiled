using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Groups;

namespace Kingmaker.Controllers.Combat;

public class UnitCombatJoinController : IControllerEnable, IController, IControllerTick, IControllerDisable, IControllerStop, IUnitMakeOffensiveActionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private readonly List<(BaseUnitEntity Attacker, BaseUnitEntity Target)> m_TryStartCombat = new List<(BaseUnitEntity, BaseUnitEntity)>();

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		try
		{
			bool isInCombat = Game.Instance.Player.IsInCombat;
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
			{
				TickUnit(allBaseAwakeUnit);
			}
			Game.Instance.Player.UpdateIsInCombat();
			for (int i = 0; i < m_TryStartCombat.Count; i++)
			{
				var (attacker, target) = m_TryStartCombat[i];
				TryStartCombat(attacker, target);
			}
			Game.Instance.Player.UpdateIsInCombat();
			if (isInCombat != Game.Instance.Player.IsInCombat)
			{
				EventBus.RaiseEvent(delegate(IPartyCombatHandler h)
				{
					h.HandlePartyCombatStateChanged(Game.Instance.Player.IsInCombat);
				});
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		finally
		{
			m_TryStartCombat.Clear();
		}
	}

	public void OnEnable()
	{
		UpdatePlayerCombatState();
	}

	public void OnDisable()
	{
		UpdatePlayerCombatState();
	}

	private void UpdatePlayerCombatState()
	{
		bool isInCombat = Game.Instance.Player.IsInCombat;
		Game.Instance.Player.UpdateIsInCombat();
		if (isInCombat != Game.Instance.Player.IsInCombat)
		{
			EventBus.RaiseEvent(delegate(IPartyCombatHandler h)
			{
				h.HandlePartyCombatStateChanged(Game.Instance.Player.IsInCombat);
			});
		}
	}

	public void OnStop()
	{
		m_TryStartCombat.Clear();
	}

	private static void TickUnit(BaseUnitEntity unit)
	{
		if (!CanJoinCombat(unit))
		{
			return;
		}
		if (unit.IsPlayerFaction)
		{
			if (!unit.IsInCombat && Game.Instance.Player.IsInCombat)
			{
				unit.CombatState.JoinCombat();
			}
			return;
		}
		foreach (BaseUnitEntity item in unit.Vision.CanBeInRange)
		{
			if ((!unit.IsInCombat || !item.IsInCombat) && CanJoinCombat(item) && ShouldStartCombat(unit, item) && unit.HasLOS(item))
			{
				if (!unit.IsInCombat)
				{
					JoinCombat(unit, item);
				}
				if (!item.IsInCombat)
				{
					JoinCombat(item, unit);
				}
				break;
			}
		}
	}

	private static void TryStartCombat(BaseUnitEntity attacker, BaseUnitEntity target)
	{
		target = (CanJoinCombat(target) ? target : (target.CombatGroup.Group.Units.FirstItem(delegate(UnitReference i)
		{
			BaseUnitEntity baseUnitEntity = i.Entity.ToBaseUnitEntity();
			return baseUnitEntity != null && !baseUnitEntity.IsInCombat && baseUnitEntity.IsInGame && CanJoinCombat(i.Entity.ToBaseUnitEntity());
		}).Entity.ToBaseUnitEntity() ?? target));
		if (attacker.IsInGame && target.IsInGame && (!attacker.IsInCombat || !target.IsInCombat) && (TurnController.IsInTurnBasedCombat() || attacker.IsPlayerFaction || target.IsPlayerFaction))
		{
			UnitAggroFilter component = target.Blueprint.GetComponent<UnitAggroFilter>();
			component?.OnAggroAction(target, attacker);
			if (target.Faction.Neutral && attacker != target && (component == null || component.ShouldAggro(target, attacker)))
			{
				BlueprintFaction blueprint = attacker.Faction.Blueprint;
				target.Faction.AttackFactions.Add(blueprint);
			}
			if (CanJoinCombat(attacker) && CanJoinCombat(target))
			{
				JoinCombat(attacker, target);
				JoinCombat(target, attacker);
			}
		}
	}

	public static bool CanJoinCombat(BaseUnitEntity unit)
	{
		if (unit.LifeState.IsConscious && !unit.IsExtra && !unit.Features.IsIgnoredByCombat && !unit.Passive && !unit.Faction.NeverJoinCombat)
		{
			return !unit.IsInLockControlCutscene;
		}
		return false;
	}

	private static void JoinCombat(BaseUnitEntity unit, BaseUnitEntity cause, bool gotAmbushed = false)
	{
		if (unit.IsExtra || cause.IsExtra)
		{
			throw new InvalidOperationException("Extra units can't participate in combat");
		}
		UnitGroupEnumerator enumerator = cause.CombatGroup.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BaseUnitEntity current = enumerator.Current;
			Game.Instance.UnitMemoryController.AddToMemory(unit, current);
		}
		bool surprised = CalculateIsSurprised(unit) || gotAmbushed;
		enumerator = unit.CombatGroup.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BaseUnitEntity current2 = enumerator.Current;
			if (current2 != null && !current2.IsInCombat && CanJoinCombat(current2))
			{
				current2.CombatState.JoinCombat(surprised);
				Game.Instance.UnitMemoryController.UpdateUnit(current2);
			}
		}
	}

	private void ForceCombatMemory(BaseUnitEntity unit, BaseUnitEntity cause)
	{
		if (unit.IsExtra || cause.IsExtra)
		{
			throw new InvalidOperationException("Extra units can't participate in combat");
		}
		UnitGroupEnumerator enumerator = cause.CombatGroup.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BaseUnitEntity current = enumerator.Current;
			Game.Instance.UnitMemoryController.TryForceMemoryPair(unit, current);
		}
	}

	private static bool ShouldStartCombat(BaseUnitEntity unit, BaseUnitEntity enemy)
	{
		if (unit.IsPlayerFaction)
		{
			return false;
		}
		if (unit.IsExtra || enemy.IsExtra)
		{
			return false;
		}
		if (enemy.LifeState.IsDead || (bool)enemy.Features.IsIgnoredByCombat || (bool)enemy.Passive)
		{
			return false;
		}
		if (!unit.IsEnemy(enemy))
		{
			return false;
		}
		if (unit.IsInFogOfWar && !enemy.Faction.IsPlayer && (!enemy.IsSummoned(out var caster) || caster == null || !caster.IsPlayerFaction))
		{
			return false;
		}
		if (unit.Stealth.Active && unit.Stealth.InAmbush && unit.DistanceTo(enemy) >= unit.Stealth.AmbushJoinCombatDistance && !unit.Stealth.IsSpottedBy(enemy.CombatGroup.Group))
		{
			return false;
		}
		if (SettingsRoot.Game.TurnBased.EnableTurnBasedMode)
		{
			return enemy.IsAwake;
		}
		return true;
	}

	private static bool CalculateIsSurprised(BaseUnitEntity unit)
	{
		return unit.CombatGroup.All((BaseUnitEntity u) => u.CombatState.Surprised);
	}

	public void StartScriptedCombat(BaseUnitEntity unit, BaseUnitEntity target, bool targetGotAmbushed = false)
	{
		JoinCombat(unit, target);
		JoinCombat(target, unit, targetGotAmbushed);
		ForceCombatMemory(unit, target);
		ForceCombatMemory(target, unit);
		if (CanJoinCombat(unit))
		{
			unit.SnapToGrid();
		}
	}

	public void HandleUnitMakeOffensiveAction(BaseUnitEntity target)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if ((!baseUnitEntity.IsInCombat || !target.IsInCombat) && CanJoinCombat(baseUnitEntity) && CanJoinCombat(target))
		{
			m_TryStartCombat.Add((baseUnitEntity, target));
		}
	}

	public static void JoinPartyToCombat()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			item.CombatState.JoinCombat();
		}
		Game.Instance.Player.UpdateIsInCombat();
	}
}
