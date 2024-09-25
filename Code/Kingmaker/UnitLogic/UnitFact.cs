using System;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class UnitFact : MechanicEntityFact<BaseUnitEntity>, IHashable
{
	public override Type RequiredEntityType => EntityInterfacesHelper.BaseUnitEntityInterface;

	public new BlueprintUnitFact Blueprint => (BlueprintUnitFact)base.Blueprint;

	[JsonConstructor]
	public UnitFact()
	{
	}

	public UnitFact(BlueprintUnitFact fact)
		: base((BlueprintMechanicEntityFact)fact)
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
public abstract class UnitFact<TBlueprint> : UnitFact, IHashable where TBlueprint : BlueprintUnitFact
{
	public new TBlueprint Blueprint => (TBlueprint)base.Blueprint;

	[JsonConstructor]
	public UnitFact()
	{
	}

	public UnitFact(TBlueprint blueprint)
		: base(blueprint)
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
