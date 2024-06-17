using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.QA;
using Kingmaker.View.Spawners;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitFromSpawnerIsDead")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("61d393ff6aff61647a785e561f98ffc5")]
public class UnitFromSpawnerIsDead : Condition
{
	[ValidateNotNull]
	[AllowedEntityType(typeof(UnitSpawner))]
	public EntityReference Target;

	protected override string GetConditionCaption()
	{
		return $"({Target}) dead";
	}

	protected override bool CheckCondition()
	{
		UnitSpawner unitSpawner = Target.FindView() as UnitSpawner;
		if (unitSpawner == null)
		{
			PFLog.Default.ErrorWithReport("Cannot find spawner {0} in {1} ({2})", Target, name, base.Owner.ToString());
		}
		if (!unitSpawner)
		{
			return false;
		}
		return unitSpawner.SpawnedUnitHasDied;
	}
}
