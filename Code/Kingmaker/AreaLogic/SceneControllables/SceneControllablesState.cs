using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[Serializable]
public class SceneControllablesState : Entity, IHashable
{
	[JsonProperty]
	private Dictionary<string, ControllableState> m_States = new Dictionary<string, ControllableState>();

	public override bool NeedsView => false;

	public SceneControllablesState(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	public SceneControllablesState(JsonConstructorMark _)
		: base(_)
	{
	}

	public void SetState(string id, ControllableState state)
	{
		m_States[id] = state;
	}

	public bool TryGetValue(string id, out ControllableState state)
	{
		return m_States.TryGetValue(id, out state);
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<string, ControllableState> states = m_States;
		if (states != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<string, ControllableState> item in states)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = StringHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<ControllableState>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		return result;
	}
}
