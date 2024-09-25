using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.View.Spawners;

public interface IUnitInitializer
{
	void OnSpawn(AbstractUnitEntity unit);

	void OnSpawn(IAbstractUnitEntity unit)
	{
		OnSpawn((AbstractUnitEntity)unit);
	}

	void OnInitialize(AbstractUnitEntity unit);

	void OnInitialize(IBaseUnitEntity unit)
	{
		OnInitialize((BaseUnitEntity)unit);
	}

	void OnInitialize(BaseUnitEntity unit)
	{
		OnInitialize((AbstractUnitEntity)unit);
	}

	void OnDispose(AbstractUnitEntity unit);

	void OnDispose(IAbstractUnitEntity unit)
	{
		OnDispose((AbstractUnitEntity)unit);
	}
}
