using System.Collections.Generic;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public abstract class SkillUseWithoutItemRestrictionPart<T> : SkillUseRestrictionPart<T>, IHashable where T : SkillUseRestrictionSettings, new()
{
	[JsonProperty]
	public readonly HashSet<UnitReference> InteractedUnits = new HashSet<UnitReference>();

	protected override bool ShouldRestrictAfterFail(BaseUnitEntity user)
	{
		if (base.ShouldRestrictAfterFail(user))
		{
			return InteractedUnits.Contains(user.FromBaseUnitEntity());
		}
		return false;
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (base.InteractOnlyByNotInteractedUnit && InteractedUnits.Contains(user.FromBaseUnitEntity()))
		{
			return -1;
		}
		return base.GetUserPriority(user);
	}

	public override void OnDidInteract(BaseUnitEntity user)
	{
		base.OnDidInteract(user);
		InteractedUnits.Add(user.FromBaseUnitEntity());
	}

	public override void OnFailedInteract(BaseUnitEntity user)
	{
		base.OnFailedInteract(user);
		InteractionHelper.MarkUnitAsInteracted(user, base.InteractionPart);
		if (ContextData<InteractionVariantData>.Current?.VariantActor == this)
		{
			InteractedUnits.Add(user.FromBaseUnitEntity());
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		HashSet<UnitReference> interactedUnits = InteractedUnits;
		if (interactedUnits != null)
		{
			int num = 0;
			foreach (UnitReference item in interactedUnits)
			{
				UnitReference obj = item;
				num ^= UnitReferenceHasher.GetHash128(ref obj).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}
}
