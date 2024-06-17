using System.Collections.Generic;
using Kingmaker.AI.Learning.Collectors;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.AI.Learning;

public class AiDataCollector : IAreaHandler, ISubscriber
{
	private List<UnitDataCollector> m_Collectors = new List<UnitDataCollector>();

	public void Subscribe()
	{
		EventBus.Subscribe(this);
	}

	public void Unsubscribe()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnAreaBeginUnloading()
	{
		UnsubscribeCollectors();
		Game.Instance.Player.AiCollectedDataStorage.Clear();
	}

	public void OnAreaDidLoad()
	{
		SubscribeCollectors();
	}

	private void SubscribeCollectors()
	{
		UnitDataStorage aiCollectedDataStorage = Game.Instance.Player.AiCollectedDataStorage;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			m_Collectors.Add(new AttackDataCollector(item, aiCollectedDataStorage));
		}
		foreach (UnitDataCollector collector in m_Collectors)
		{
			collector.Subscribe();
		}
	}

	private void UnsubscribeCollectors()
	{
		foreach (UnitDataCollector collector in m_Collectors)
		{
			collector.Unsubscribe();
		}
		m_Collectors.Clear();
	}
}
