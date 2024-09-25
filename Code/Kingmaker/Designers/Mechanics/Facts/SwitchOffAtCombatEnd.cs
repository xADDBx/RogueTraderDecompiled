using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.ActivatableAbilities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintActivatableAbility))]
[TypeId("d7471e22eb42a5e4591c4b27041a39d3")]
public class SwitchOffAtCombatEnd : EntityFactComponentDelegate, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	public void HandleUnitJoinCombat()
	{
	}

	public void HandleUnitLeaveCombat()
	{
		if (base.Fact is ActivatableAbility activatableAbility)
		{
			activatableAbility.IsOn = false;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
