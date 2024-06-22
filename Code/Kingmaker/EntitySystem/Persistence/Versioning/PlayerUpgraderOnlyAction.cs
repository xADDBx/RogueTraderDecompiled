using System;
using Kingmaker.ElementsSystem;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public abstract class PlayerUpgraderOnlyAction : GameAction
{
	protected sealed override void RunAction()
	{
		if (!(base.Owner is BlueprintPlayerUpgrader))
		{
			throw new InvalidOperationException("Action " + GetType().Name + " is only allowed in upgraders.");
		}
		RunActionOverride();
	}

	protected abstract void RunActionOverride();
}
