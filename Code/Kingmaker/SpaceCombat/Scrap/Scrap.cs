using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Starships;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.SpaceCombat.Scrap;

public class Scrap : IEndSpaceCombatHandler, ISubscriber, IUnitDeathHandler, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, IHashable
{
	[JsonProperty]
	private int m_Value;

	[JsonProperty]
	private int m_PotentialScrap;

	public int ScrapNeededForFullRepair => Mathf.RoundToInt((float)PlayerShip.Health.Damage * ScrapToRegenOneHp);

	private float ScrapToRegenOneHp => (float)BlueprintWarhammerRoot.Instance.BlueprintScrapRoot.ScrapToRegenOneHp * ShipRepairCostModifiers * SpacecombatDifficultyHelper.RepairCostMod();

	public float ShipRepairCostModifiers => GetModifierOfType(ScrapModifier.ModifierType.ShipRepairCost);

	private StarshipEntity PlayerShip => Game.Instance.Player.PlayerShip;

	public void Initialize()
	{
		m_Value = BlueprintWarhammerRoot.Instance.BlueprintScrapRoot.InitScrap;
		m_PotentialScrap = 0;
	}

	public int ReceiveWithModifiers(int scrap, ScrapModifier.ModifierType applyModifiers)
	{
		int num = Mathf.RoundToInt((float)scrap * GetModifierOfType(applyModifiers));
		if (num != scrap)
		{
			Receive(scrap);
			Receive(num - scrap);
		}
		else
		{
			Receive(num);
		}
		return num;
	}

	public void Receive(int scrap)
	{
		m_Value = Math.Min(int.MaxValue, m_Value + scrap);
		EventBus.RaiseEvent(delegate(IScrapChangedHandler h)
		{
			h.HandleScrapGained(scrap);
		});
	}

	public void Spend(int scrap)
	{
		m_Value = Math.Max(0, m_Value - scrap);
		EventBus.RaiseEvent(delegate(IScrapChangedHandler h)
		{
			h.HandleScrapSpend(scrap);
		});
	}

	public static implicit operator int(Scrap value)
	{
		return value.m_Value;
	}

	public void RepairShipFull()
	{
		RepairShip(Game.Instance.Player.PlayerShip.Health.Damage);
	}

	public void RepairShipForAllScrap()
	{
		double num = Math.Floor((double)m_Value / (double)ScrapToRegenOneHp);
		int damage = Game.Instance.Player.PlayerShip.Health.Damage;
		if (num < (double)damage)
		{
			EventBus.RaiseEvent(delegate(IRepairShipHandler h)
			{
				h.HandleCantFullyRepairShip();
			});
		}
		int restoreHealth = Math.Min(damage, (int)num);
		RepairShip(restoreHealth);
	}

	public void RepairShip(int restoreHealth)
	{
		Game.Instance.GameCommandQueue.RepairShip(restoreHealth);
	}

	public void RepairShipInternal(int restoreHealth)
	{
		int num = Math.Min(restoreHealth, Game.Instance.Player.PlayerShip.Health.Damage);
		int num2 = Mathf.RoundToInt((float)num * ScrapToRegenOneHp);
		if (m_Value >= num2)
		{
			Spend(num2);
			Game.Instance.Player.PlayerShip.Health.HealDamage(num);
		}
	}

	private float GetModifierOfType(ScrapModifier.ModifierType type)
	{
		return (from x in PlayerShip.Facts.GetComponents<ScrapModifier>()
			where x.ModType == type
			select x.ModValue).Aggregate(1f, (float x, float y) => x + y);
	}

	public void HandleEndSpaceCombat()
	{
		m_PotentialScrap = 0;
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity is StarshipEntity starshipEntity && Game.Instance.Player.PlayerShip.IsEnemy(starshipEntity))
		{
			m_PotentialScrap += starshipEntity.Health.MaxHitPoints;
		}
	}

	public void HandleUnitJoinCombat()
	{
		if (EventInvokerExtensions.BaseUnitEntity as StarshipEntity == Game.Instance.Player.PlayerShip)
		{
			m_PotentialScrap = 0;
		}
	}

	public void HandleUnitLeaveCombat()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_Value);
		result.Append(ref m_PotentialScrap);
		return result;
	}
}
