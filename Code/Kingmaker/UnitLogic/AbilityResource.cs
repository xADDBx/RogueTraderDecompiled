using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class AbilityResource : IHashable
{
	[JsonProperty]
	public BlueprintScriptableObject Blueprint { get; private set; }

	[JsonProperty]
	public int Amount { get; set; }

	public int GetMaxAmount(Entity owner)
	{
		if (Blueprint is BlueprintAbilityResource blueprintAbilityResource)
		{
			return blueprintAbilityResource.GetMaxAmount(owner);
		}
		AddAbilityResources component = Blueprint.GetComponent<AddAbilityResources>();
		if (component != null && component.UseThisAsResource)
		{
			return component.Amount;
		}
		PFLog.Default.Error("Can't extract resource amount from {0}", Blueprint);
		return 0;
	}

	[JsonConstructor]
	public AbilityResource(BlueprintScriptableObject blueprint)
	{
		Blueprint = blueprint;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		int val2 = Amount;
		result.Append(ref val2);
		return result;
	}
}
