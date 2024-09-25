using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("5091aab5196133c489dda4066af8d1fc")]
internal class RespawnNewUnit : PlayerUpgraderOnlyAction
{
	[InfoBox("Don't call this action if you dont understand what it's for")]
	[AllowedEntityType(typeof(UnitSpawnerBase))]
	public EntityReference Spawner;

	protected override void RunActionOverride()
	{
		UnitSpawnerBase unitSpawner = GameHelper.GetUnitSpawner(Spawner);
		if (unitSpawner is CompanionSpawner)
		{
			throw new Exception("CompanionSpawner is not allowed in RespawnNewUnit action");
		}
		unitSpawner.ForceReSpawn();
	}

	public override string GetCaption()
	{
		return "RespawnNewUnit " + Spawner?.EntityNameInEditor;
	}

	public override string GetDescription()
	{
		return "RespawnNewUnit " + Spawner?.EntityNameInEditor;
	}
}
