using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Spawners;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Spawn")]
[AllowMultipleComponents]
[TypeId("0652c1b85291c994f8411a22deb2b6ec")]
[PlayerUpgraderAllowed(true)]
public class Spawn : GameAction
{
	[AllowedEntityType(typeof(UnitSpawnerBase))]
	public EntityReference[] Spawners;

	public ActionList ActionsOnSpawn;

	public override void RunAction()
	{
		EntityReference[] spawners = Spawners;
		for (int i = 0; i < spawners.Length; i++)
		{
			UnitSpawnerBase unitSpawner = GameHelper.GetUnitSpawner(spawners[i]);
			if (unitSpawner == null)
			{
				continue;
			}
			AbstractUnitEntity abstractUnitEntity = unitSpawner.Spawn();
			if (abstractUnitEntity != null)
			{
				using (ContextData<SpawnedUnitData>.Request().Setup(abstractUnitEntity, unitSpawner.Data.HoldingState))
				{
					ActionsOnSpawn.Run();
				}
			}
		}
	}

	public override string GetCaption()
	{
		string text = "";
		if (Spawners != null)
		{
			for (int i = 0; i < Spawners.Length; i++)
			{
				if (i != 0)
				{
					text += ", ";
				}
				if (Spawners[i] != null)
				{
					text += Spawners[i].EntityNameInEditor;
				}
			}
		}
		return "Spawn( " + text + " )";
	}
}
