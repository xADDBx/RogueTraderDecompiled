using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.AI.Learning;

public abstract class UnitDataCollector
{
	private readonly BaseUnitEntity m_Unit;

	private readonly UnitDataStorage m_Storage;

	protected UnitDataStorage Storage => m_Storage;

	public IEntity GetSubscribingEntity()
	{
		return m_Unit;
	}

	public UnitDataCollector(BaseUnitEntity unit, UnitDataStorage storage)
	{
		m_Unit = unit;
		m_Storage = storage;
	}

	public void Subscribe()
	{
		EventBus.Subscribe(this);
	}

	public void Unsubscribe()
	{
		EventBus.Unsubscribe(this);
	}
}
