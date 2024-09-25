using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.AreaLogic.SummonPool;

public interface ISummonPool
{
	BlueprintSummonPool Blueprint { get; }

	int Count { get; }

	IEnumerable<AbstractUnitEntity> Units { get; }

	void Register(AbstractUnitEntity unit);

	void Unregister(AbstractUnitEntity unit, bool keepReference = false);
}
