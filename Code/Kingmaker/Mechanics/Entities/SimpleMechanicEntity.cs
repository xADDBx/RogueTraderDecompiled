using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.View.Mechanics.Entities;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Mechanics.Entities;

public class SimpleMechanicEntity : MechanicEntity, IHashable
{
	[JsonProperty]
	private Vector3 m_Position;

	public override Vector3 Position
	{
		get
		{
			return m_Position;
		}
		set
		{
			m_Position = value;
		}
	}

	public SimpleMechanicEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public SimpleMechanicEntity(string uniqueId, bool isInGame, BlueprintMechanicEntityFact blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return new GameObject("simple-entity-" + base.UniqueId).AddComponent<SimpleMechanicEntityView>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Position);
		return result;
	}
}
