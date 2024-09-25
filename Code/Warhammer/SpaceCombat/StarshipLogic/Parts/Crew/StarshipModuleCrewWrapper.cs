using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Parts.Crew;

public class StarshipModuleCrewWrapper : IReadOnlyStarshipModuleCrewWrapper
{
	public delegate void DamageCrewDelegate();

	public delegate void HealCrewDelegate();

	private readonly DamageCrewDelegate m_OnDamage;

	private readonly HealCrewDelegate m_OnHeal;

	public readonly StarshipModuleCrewData Data;

	private readonly ShipModuleSettings m_Settings;

	private readonly StarshipEntity m_Owner;

	public int Max => Rulebook.Trigger(new RuleCalculateStarshipCrewMaxCount(m_Owner, m_Settings)).Result;

	public ShipModuleType ShipModuleType => m_Settings.ModuleType;

	public StarshipModuleCrewWrapper(StarshipEntity owner, StarshipModuleCrewData data, ShipModuleSettings settings, DamageCrewDelegate onDamage, HealCrewDelegate onHeal)
	{
		Data = data;
		m_Settings = settings;
		m_OnDamage = onDamage;
		m_OnHeal = onHeal;
		m_Owner = owner;
	}

	public bool CanMoveFrom()
	{
		return GetAliveCount(includeInTransition: false) > 0;
	}

	public bool CanMoveTo()
	{
		return !IsFull(includeTransitionCrew: true);
	}

	public int GetCountInTransitionToModule()
	{
		return Data.CountInTransitionToModule;
	}

	public int GetAvailableToMoveCount()
	{
		return GetAliveCount(includeInTransition: false);
	}

	public int FreePlace(bool includeTransitionCrew)
	{
		return Max - GetAliveCount(includeTransitionCrew);
	}

	public int GetAliveCount(bool includeInTransition)
	{
		if (includeInTransition)
		{
			return Max - Data.CountLost + Data.CountInTransitionToModule;
		}
		return Max - Data.CountLost;
	}

	public float GetRatio(bool includeTransitionCrew)
	{
		return (float)GetAliveCount(includeTransitionCrew) / (float)Max;
	}

	public void Remove(int count, bool isDamage)
	{
		int countLost = Data.CountLost;
		int num = Math.Clamp(Data.CountLost + count, 0, Max);
		Data.CountLost = num;
		if (isDamage)
		{
			Data.CountLostOnCurrentTurn += Math.Max(num - countLost, 0);
			m_OnDamage?.Invoke();
		}
	}

	public void Add(int count, bool isHeal)
	{
		int countLost = Data.CountLost;
		int num = Math.Clamp(Data.CountLost - count, 0, Max);
		Data.CountLost = num;
		if (isHeal)
		{
			Data.CountLostOnCurrentTurn -= Math.Max(num - countLost, 0);
			m_OnHeal?.Invoke();
		}
	}

	public void StartTransit(int count)
	{
		Data.CountInTransitionToModule += count;
	}

	public void CompleteTransit()
	{
		Add(Data.CountInTransitionToModule, isHeal: false);
		Data.CountInTransitionToModule = 0;
	}

	public ShipCrewModuleState GetState(bool includeInTransition)
	{
		float ratio = GetRatio(includeInTransition);
		if (!(ratio >= 2f / 3f))
		{
			if (ratio >= 1f / 3f)
			{
				return ShipCrewModuleState.UnderStaffed;
			}
			return ShipCrewModuleState.Unmanned;
		}
		return ShipCrewModuleState.FullyStaffed;
	}

	public void Restore()
	{
		Data.CountLost = 0;
		Data.CountInTransitionToModule = 0;
		Data.CountLostOnCurrentTurn = 0;
	}

	public bool IsFull(bool includeTransitionCrew)
	{
		return GetAliveCount(includeTransitionCrew) >= Max;
	}
}
