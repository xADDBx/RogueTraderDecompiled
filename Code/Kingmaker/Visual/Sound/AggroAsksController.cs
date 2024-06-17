using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class AggroAsksController : ITickUnitAsksController, IUnitAsksController, IDisposable, IPartyCombatHandler, ISubscriber
{
	private bool m_CheckAggro;

	public AggroAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
		m_CheckAggro = false;
	}

	void IPartyCombatHandler.HandlePartyCombatStateChanged(bool inCombat)
	{
		m_CheckAggro = inCombat;
	}

	public void Tick()
	{
		if (!m_CheckAggro)
		{
			return;
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		if (mainCharacterEntity.IsInCombat && mainCharacterEntity.LifeState.IsConscious && mainCharacterEntity.View != null && mainCharacterEntity.View.Asks != null)
		{
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
			{
				if (!allBaseAwakeUnit.IsPlayerEnemy || !allBaseAwakeUnit.IsInCombat)
				{
					continue;
				}
				BarkWrapper barkWrapper = null;
				BlueprintArmyDescription army = allBaseAwakeUnit.Blueprint.Army;
				if (army != null)
				{
					if (army.IsHuman)
					{
						barkWrapper = mainCharacterEntity.View.Asks.AggroQuestionHuman;
					}
					else if (army.IsXenos)
					{
						barkWrapper = mainCharacterEntity.View.Asks.AggroQuestionXenos;
					}
					else if (army.IsDaemon)
					{
						barkWrapper = mainCharacterEntity.View.Asks.AggroQuestionChaos;
					}
					if (barkWrapper != null && barkWrapper.Schedule(is2D: false, delegate
					{
						AggroCallback(isRaceAnswer: true);
					}))
					{
						m_CheckAggro = false;
						return;
					}
				}
			}
		}
		UnitAsksHelper.GetRandomPartyEntity((BaseUnitEntity x) => x.IsInCombat && x.LifeState.IsConscious && x.View != null && x.View.Asks != null && x.View.Asks.Aggro.HasBarks)?.View.Asks.Aggro.Schedule(is2D: false, delegate
		{
			AggroCallback(isRaceAnswer: false);
		});
		m_CheckAggro = false;
	}

	private static void AggroCallback(bool isRaceAnswer)
	{
		if (isRaceAnswer)
		{
			BaseUnitEntity randomEntity = UnitAsksHelper.GetRandomEntity((BaseUnitEntity x) => x.Faction.IsPlayerEnemy && x.IsInCombat && x.LifeState.IsConscious && x.View != null && x.View.Asks != null && x.View.Asks.AggroRaceAnswer.HasBarks);
			if (randomEntity != null && randomEntity.View.Asks != null && randomEntity.View.Asks.AggroRaceAnswer.Schedule())
			{
				return;
			}
		}
		UnitAsksHelper.GetRandomEntity((BaseUnitEntity x) => x.Faction.IsPlayerEnemy && x.IsInCombat && x.LifeState.IsConscious && x.View != null && x.View.Asks != null && x.View.Asks.Aggro.HasBarks)?.View.Asks?.Aggro.Schedule();
	}
}
