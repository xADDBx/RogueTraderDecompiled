using System;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public abstract class UnitUpgraderOnlyAction : GameAction
{
	protected BaseUnitEntity Target => ContextData<UpgradeTargetUnit>.Current?.Unit;

	protected override void RunAction()
	{
		if (!(base.Owner is BlueprintUnitUpgrader))
		{
			throw new InvalidOperationException("Action " + GetType().Name + " is only allowed in upgraders.");
		}
		if (Target == null)
		{
			throw new InvalidOperationException("Target is null");
		}
		RunActionOverride();
	}

	protected abstract void RunActionOverride();
}
