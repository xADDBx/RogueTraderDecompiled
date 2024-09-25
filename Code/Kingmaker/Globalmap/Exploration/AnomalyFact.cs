using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

public class AnomalyFact : MechanicEntityFact<AnomalyEntityData>, IHashable
{
	public override Type RequiredEntityType => EntityInterfacesHelper.StarSystemObjectEntityInterface;

	public new BlueprintAnomalyFact Blueprint => (BlueprintAnomalyFact)base.Blueprint;

	[JsonConstructor]
	public AnomalyFact()
	{
	}

	public AnomalyFact(BlueprintAnomalyFact fact)
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
