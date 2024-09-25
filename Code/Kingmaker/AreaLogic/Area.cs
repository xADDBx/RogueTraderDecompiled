using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic;

public class Area : MechanicEntity<BlueprintArea>, IHashable
{
	public enum AreaAmbienceType
	{
		Wild,
		Seized,
		Kingdom,
		Bloom,
		Neutral
	}

	public Area(BlueprintArea blueprint)
		: base(blueprint.AssetGuid, isInGame: true, blueprint)
	{
	}

	protected Area(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		foreach (BlueprintAreaPart part in base.OriginalBlueprint.GetParts())
		{
			Facts.Add(new EntityFact(part));
		}
	}

	protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return new MechanicEntityFact(blueprint)
		{
			SuppressActivationOnAttach = true
		};
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
		return result;
	}
}
