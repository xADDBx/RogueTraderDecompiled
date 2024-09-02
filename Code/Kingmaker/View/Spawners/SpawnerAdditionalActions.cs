using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawnerBase))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("49d774cc7d71470487e8d9afe1ca953e")]
public class SpawnerAdditionalActions : EntityPartComponent<SpawnerAdditionalActions.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public new SpawnerAdditionalActions Source => (SpawnerAdditionalActions)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			using (ContextData<SpawnedUnitData>.Request().Setup(unit, base.Owner.HoldingState))
			{
				(Source.SpawnActions?.Get())?.Actions.Run();
				foreach (ActionsReference actionHolder in Source.ActionHolders)
				{
					if (actionHolder?.Get() != null)
					{
						actionHolder.Get().Actions.Run();
					}
				}
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
	[Obsolete]
	[ShowIf("ObsoleteActionsNotEmpty")]
	public ActionsReference SpawnActions;

	public List<ActionsReference> ActionHolders = new List<ActionsReference>();

	private bool ObsoleteActionsNotEmpty
	{
		get
		{
			if (SpawnActions == null)
			{
				return false;
			}
			return SpawnActions.Get()?.HasActions ?? false;
		}
	}
}
