using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("268b3b89c0a147c18430f96c3429d7ad")]
public class ContextActionSpawn1x1Cover : ContextAction
{
	[SerializeField]
	private BlueprintSpawnableDestructibleObjectReference _blueprint;

	[SerializeField]
	private bool _destroyOnCombatEnd = true;

	private BlueprintSpawnableDestructibleObject blueprint => _blueprint?.Get();

	public override string GetCaption()
	{
		return "Spawn " + blueprint.name + " cover";
	}

	protected override void RunAction()
	{
		SceneEntitiesState mainState = Game.Instance.LoadedAreaState.MainState;
		DestructibleEntity destructibleEntity = Game.Instance.EntitySpawner.SpawnDestructibleObject(blueprint, base.Target.Point, Quaternion.identity, mainState);
		if (destructibleEntity.SizeRect.Width != destructibleEntity.SizeRect.Height || destructibleEntity.SizeRect.Width != 1)
		{
			PFLog.Default.Error("Spawned not 1x1 cover: {0}", blueprint);
			destructibleEntity.IsInGame = false;
			destructibleEntity.Dispose();
		}
		else if (_destroyOnCombatEnd)
		{
			destructibleEntity.GetOrCreate<PartDestroyOnCombatEnd>();
		}
	}
}
