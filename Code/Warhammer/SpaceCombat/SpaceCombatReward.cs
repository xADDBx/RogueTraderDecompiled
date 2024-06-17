using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.Scrap;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat;

[AllowedOn(typeof(BlueprintArea))]
[AllowedOn(typeof(BlueprintStarship))]
[AllowMultipleComponents]
[TypeId("a1ed0cc8887c45d299b2774d4c525329")]
public class SpaceCombatReward : EntityFactComponentDelegate, IEndSpaceCombatHandler, ISubscriber, IUnitDeathHandler, IHashable
{
	[Serializable]
	public class Reward
	{
		[Serializable]
		public struct RewardCondition
		{
			[SerializeField]
			public bool Not;

			[SerializeField]
			public bool HasBuff;

			[SerializeField]
			public bool CheckOnPlayerShipInstead;

			[SerializeField]
			[ShowIf("HasBuff")]
			private BlueprintBuffReference m_Buff;

			public BlueprintBuff Buff => m_Buff?.Get();

			public bool Check()
			{
				bool flag = CheckBuff();
				if (!Not)
				{
					return flag;
				}
				return !flag;
			}

			private bool CheckBuff()
			{
				if (!HasBuff)
				{
					return true;
				}
				StarshipEntity starshipEntity = ComponentEventContext.CurrentRuntime.Owner as StarshipEntity;
				if (CheckOnPlayerShipInstead)
				{
					starshipEntity = Game.Instance.Player.PlayerShip;
				}
				if (starshipEntity == null)
				{
					return false;
				}
				foreach (Buff buff in starshipEntity.Buffs)
				{
					if (buff.Blueprint == Buff)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<BlueprintItemReference> Items;

		public List<int> ItemCounts;

		public List<BlueprintCargoReference> Cargoes;

		public int Scrap;

		[SerializeField]
		public List<RewardCondition> Condition;

		public bool IsAvailable()
		{
			if (Condition == null)
			{
				return true;
			}
			if (Condition.Count == 0)
			{
				return true;
			}
			bool flag = true;
			foreach (RewardCondition item in Condition)
			{
				flag &= item.Check();
			}
			return flag;
		}
	}

	[SerializeField]
	public List<Reward> Rewards;

	public List<Reward> AvailableRewards => Rewards.Where((Reward x) => x.IsAvailable()).ToList();

	public void HandleEndSpaceCombat()
	{
		if (base.Owner is StarshipEntity)
		{
			return;
		}
		PFLog.Default.Log($"Space Combat Reward: Rewards - {AvailableRewards.Count}");
		foreach (Reward availableReward in AvailableRewards)
		{
			AddReward(availableReward);
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unit)
	{
		if (base.Owner != unit)
		{
			return;
		}
		PFLog.Default.Log("Space Combat Ship Death Reward for " + unit.Name);
		foreach (Reward availableReward in AvailableRewards)
		{
			AddReward(availableReward);
		}
	}

	private void AddReward(Reward reward)
	{
		Player player = Game.Instance.Player;
		for (int i = 0; i < reward.Items.Count; i++)
		{
			ItemEntity itemEntity = reward.Items[i].Get().CreateEntity();
			if (reward.ItemCounts.Count > i)
			{
				itemEntity.SetCount(reward.ItemCounts[i]);
			}
			if (CargoHelper.IsTrashItem(itemEntity) && CargoHelper.CanTransferToCargo(itemEntity))
			{
				player.CargoState.AddToCargo(itemEntity);
			}
			else
			{
				player.Inventory.Add(itemEntity);
			}
		}
		foreach (BlueprintCargoReference cargo in reward.Cargoes)
		{
			player.CargoState.Create(cargo);
		}
		ScrapModifier.ModifierType applyModifiers = ((base.Owner is StarshipEntity) ? ScrapModifier.ModifierType.EnemyKilledReward : ScrapModifier.ModifierType.SpaceCombatCompleteReward);
		int scrapWithModifiers = player.Scrap.ReceiveWithModifiers(reward.Scrap, applyModifiers);
		EventBus.RaiseEvent(delegate(ISpaceCombatRewardUIHandler h)
		{
			h.HandleSpaceCombatReward(reward.Items, reward.ItemCounts, reward.Cargoes, scrapWithModifiers);
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
