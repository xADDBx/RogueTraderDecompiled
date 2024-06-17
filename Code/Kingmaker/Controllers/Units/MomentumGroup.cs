using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class MomentumGroup : IHashable
{
	[JsonProperty]
	public readonly BlueprintMomentumGroup Blueprint;

	[JsonProperty]
	public readonly List<EntityRef<MechanicEntity>> Units;

	[JsonProperty]
	public int Momentum { get; private set; }

	private static BlueprintMomentumRoot Settings => Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;

	public bool IsParty => Blueprint == Settings.PartyGroup;

	public bool IsDefaultEnemy => Blueprint == Settings.DefaultEnemyGroup;

	public int MinimalMomentum => Settings.MinimalMomentum;

	public int MaximalMomentum => Settings.MaximalMomentum;

	[JsonConstructor]
	private MomentumGroup()
	{
	}

	public MomentumGroup(BlueprintMomentumGroup blueprint)
	{
		Blueprint = blueprint;
		Units = new List<EntityRef<MechanicEntity>>();
		Momentum = Settings.StartingMomentum;
	}

	public void ResetMomentumToStartingValue()
	{
		Momentum = Settings.StartingMomentum;
	}

	public void AddMomentum(int value)
	{
		Momentum = Math.Clamp(Momentum + value, Settings.MinimalMomentum, Settings.MaximalMomentum);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		List<EntityRef<MechanicEntity>> units = Units;
		if (units != null)
		{
			for (int i = 0; i < units.Count; i++)
			{
				EntityRef<MechanicEntity> obj = units[i];
				Hash128 val2 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		int val3 = Momentum;
		result.Append(ref val3);
		return result;
	}
}
