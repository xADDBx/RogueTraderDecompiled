using JetBrains.Annotations;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawnerBase))]
[DisallowMultipleComponent]
public class SpawnerAdditionalActions : EntityPartComponent<SpawnerAdditionalActions.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public new SpawnerAdditionalActions Source => (SpawnerAdditionalActions)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			ActionsHolder actionsHolder = Source.SpawnActions?.Get();
			if (actionsHolder == null)
			{
				return;
			}
			using (ContextData<SpawnedUnitData>.Request().Setup(unit, base.Owner.HoldingState))
			{
				actionsHolder.Actions.Run();
			}
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[CanBeNull]
	public ActionsReference SpawnActions;
}
