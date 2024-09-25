using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items;

public class ItemFact : MechanicEntityFact<ItemEntity>, IHashable
{
	public override Type RequiredEntityType => EntityInterfacesHelper.ItemEntityInterface;

	public new BlueprintItem Blueprint => (BlueprintItem)base.Blueprint;

	[JsonConstructor]
	public ItemFact()
	{
	}

	public ItemFact(BlueprintItem fact)
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
