using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Spawners;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5a0f8a1614a840f8b0629a71b6de51f7")]
public class SpawnBySummonPool : GameAction
{
	[SerializeField]
	private BlueprintSummonPoolReference m_Pool;

	public ActionList ActionsOnSpawn;

	public BlueprintSummonPool Pool
	{
		get
		{
			return m_Pool?.Get();
		}
		set
		{
			m_Pool = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<BlueprintSummonPoolReference>();
		}
	}

	protected override void RunAction()
	{
		foreach (UnitSpawnerBase.MyData temp in EntityService.Instance.GetTempList<UnitSpawnerBase.MyData>())
		{
			UnitSpawner unitSpawner = temp.View as UnitSpawner;
			if (unitSpawner == null)
			{
				continue;
			}
			SpawnerSummonPoolSettings.Part optional = temp.GetOptional<SpawnerSummonPoolSettings.Part>();
			if (optional == null || !optional.Source.Pools.HasReference(m_Pool.Get()))
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
		return "Spawn all in " + m_Pool?.Get().NameSafe();
	}
}
