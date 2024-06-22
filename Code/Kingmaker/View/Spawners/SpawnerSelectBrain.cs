using System;
using Kingmaker.AI.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("e44ca9c3dcc2419e9b1c73e49e829db1")]
public class SpawnerSelectBrain : EntityPartComponent<SpawnerSelectBrain.Part>
{
	[Serializable]
	public class BrainSelector
	{
		public BlueprintBrainBaseReference Current;
	}

	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public void OnSpawn(AbstractUnitEntity unit)
		{
			BlueprintBrainBase blueprintBrainBase = ((SpawnerSelectBrain)base.Source).Brain?.Current?.Get();
			if (blueprintBrainBase != null)
			{
				PartUnitBrain brainOptional = unit.GetBrainOptional();
				if (brainOptional != null)
				{
					brainOptional.SetBrain(blueprintBrainBase);
					brainOptional.RestoreAvailableActions();
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

	[SerializeField]
	public BrainSelector Brain;
}
