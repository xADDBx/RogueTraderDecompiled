using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.View.Spawners;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("2a580ffc7fd649d7a9cdad06b33c8ef9")]
public class SpawnByUnitGroup : GameAction
{
	[SerializeField]
	[AllowedEntityType(typeof(UnitGroupView))]
	public EntityReference m_Group;

	public ActionList ActionsOnSpawn;

	protected override void RunAction()
	{
		UnitGroupView unitGroupView = m_Group.FindView() as UnitGroupView;
		if (unitGroupView == null)
		{
			LogChannel.Default.Warning(this, $"Unit group not found in {this} ({m_Group?.UniqueId})");
			QAModeExceptionReporter.MaybeShowError($"Unit group not found in {this} ({m_Group?.UniqueId})");
			return;
		}
		UnitSpawner[] componentsInChildren = unitGroupView.GetComponentsInChildren<UnitSpawner>();
		foreach (UnitSpawner unitSpawner in componentsInChildren)
		{
			UnitSpawnerBase.MyData data = unitSpawner.Data;
			if (data == null || !data.IsInGame || unitSpawner.HasSpawned)
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
		return "Spawn all in " + m_Group?.EntityNameInEditor;
	}
}
