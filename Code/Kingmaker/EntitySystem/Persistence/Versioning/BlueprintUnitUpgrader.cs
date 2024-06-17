using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

[UnitUpgraderFilter]
[TypeId("5a89120da87448eaa36bf6295597a570")]
public class BlueprintUnitUpgrader : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintUnitUpgrader>
	{
	}

	public bool ApplyFromPlaceholder;

	public ActionList Actions;

	public void Upgrade(BaseUnitEntity unit)
	{
		using (ContextData<UpgradeTargetUnit>.Request().Setup(unit))
		{
			Actions.Run();
		}
	}
}
