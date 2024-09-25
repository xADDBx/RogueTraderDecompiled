using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[AllowedOn(typeof(BlueprintActivatableAbility))]
[TypeId("4fc45af926b34ff49be18aa442ec3aff")]
public abstract class ActivatableAbilityRestriction : UnitFactComponentDelegate, IHashable
{
	protected new ActivatableAbility Fact => (ActivatableAbility)base.Fact;

	public bool IsAvailable(EntityFactComponent runtime)
	{
		using (runtime.RequestEventContext())
		{
			return IsAvailable();
		}
	}

	protected abstract bool IsAvailable();

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
