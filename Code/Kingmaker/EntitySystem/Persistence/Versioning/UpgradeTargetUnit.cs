using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public class UpgradeTargetUnit : ContextData<UpgradeTargetUnit>
{
	public BaseUnitEntity Unit { get; private set; }

	public UpgradeTargetUnit Setup(BaseUnitEntity unit)
	{
		Unit = unit;
		return this;
	}

	protected override void Reset()
	{
		Unit = null;
	}
}
