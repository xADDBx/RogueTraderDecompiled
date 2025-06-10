using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartAbilityRedirect : BaseUnitPart, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[JsonProperty]
	public EntityFactRef<Ability> LastUsedAbility;

	[JsonProperty]
	public EntityRef<MechanicEntity> LastUsedAbilityTarget;

	public void HandleUnitJoinCombat()
	{
		Reset();
	}

	public void HandleUnitLeaveCombat()
	{
		Reset();
	}

	private void Reset()
	{
		LastUsedAbility = default(EntityFactRef<Ability>);
		LastUsedAbilityTarget = default(EntityRef<MechanicEntity>);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityFactRef<Ability> obj = LastUsedAbility;
		Hash128 val2 = StructHasher<EntityFactRef<Ability>>.GetHash128(ref obj);
		result.Append(ref val2);
		EntityRef<MechanicEntity> obj2 = LastUsedAbilityTarget;
		Hash128 val3 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj2);
		result.Append(ref val3);
		return result;
	}
}
