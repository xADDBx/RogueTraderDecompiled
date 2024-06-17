using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.AI;

public class AiCastData : ContextData<AiCastData>
{
	public BaseUnitEntity Unit { get; private set; }

	public BaseUnitEntity Target { get; private set; }

	public AiCastData Setup(BaseUnitEntity unit, BaseUnitEntity target)
	{
		Unit = unit;
		Target = target;
		return this;
	}

	protected override void Reset()
	{
		Unit = null;
		Target = null;
	}
}
