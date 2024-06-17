using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View.Spawners;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("87993f7d955442d2b8fa399a4d4be081")]
public class EtudeBracketShowObjects : EtudeBracketTrigger, IHashable
{
	public EntityReference[] Objects;

	protected override void OnEnter()
	{
		ShowObjects();
	}

	protected override void OnResume()
	{
		ShowObjects();
	}

	protected override void OnExit()
	{
		HideObjects();
	}

	private void ShowObjects()
	{
		EntityReference[] objects = Objects;
		for (int i = 0; i < objects.Length; i++)
		{
			IEntity entity = objects[i].FindData();
			if (entity is UnitSpawnerBase.MyData { SpawnedUnit: var spawnedUnit })
			{
				entity = spawnedUnit.Entity;
			}
			if (entity != null)
			{
				entity.IsInGame = true;
			}
		}
	}

	private void HideObjects()
	{
		EntityReference[] objects = Objects;
		for (int i = 0; i < objects.Length; i++)
		{
			IEntity entity = objects[i].FindData();
			if (entity is UnitSpawnerBase.MyData { SpawnedUnit: var spawnedUnit })
			{
				entity = spawnedUnit.Entity;
			}
			if (entity != null)
			{
				entity.IsInGame = false;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
