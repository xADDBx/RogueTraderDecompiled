using System;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Facts;

public class MechanicEntityFact : EntityFact, IMechanicEntityFact, IHashable
{
	public override Type RequiredEntityType => EntityInterfacesHelper.MechanicEntityInterface;

	public new BlueprintMechanicEntityFact Blueprint => (BlueprintMechanicEntityFact)base.Blueprint;

	public new IMechanicEntity Owner => (IMechanicEntity)base.Owner;

	public MechanicEntity ConcreteOwner => (MechanicEntity)Owner;

	public virtual bool Hidden => (ConcreteOwner?.GetOptional<UnitPartHiddenFacts>()?.IsHidden(Blueprint)).GetValueOrDefault();

	[JsonConstructor]
	protected MechanicEntityFact()
	{
	}

	public MechanicEntityFact(BlueprintMechanicEntityFact fact)
		: base(fact)
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
public abstract class MechanicEntityFact<TOwner> : MechanicEntityFact, IHashable where TOwner : IMechanicEntity
{
	public override Type RequiredEntityType => typeof(TOwner);

	public new TOwner Owner => (TOwner)base.Owner;

	[JsonConstructor]
	protected MechanicEntityFact()
	{
	}

	public MechanicEntityFact(BlueprintMechanicEntityFact fact)
		: base(fact)
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
