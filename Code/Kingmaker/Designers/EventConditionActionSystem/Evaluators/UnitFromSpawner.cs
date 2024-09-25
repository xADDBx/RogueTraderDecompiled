using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Spawners;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("da45a98792587f244901f1020d4b35b1")]
[PlayerUpgraderAllowed(true)]
public class UnitFromSpawner : AbstractUnitEvaluator
{
	[AllowedEntityType(typeof(UnitSpawnerBase))]
	[ValidateNotEmpty]
	public EntityReference Spawner;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return (Spawner.FindData() as UnitSpawnerBase.MyData)?.SpawnedUnit.Entity;
	}

	public override string GetCaption()
	{
		return $"Unit From Spawner ({Spawner})";
	}
}
