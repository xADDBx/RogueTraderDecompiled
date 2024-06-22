using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public class ConditionalRestrictionPart : InteractionRestrictionPart<ConditionalRestrictionSettings>, IHashable
{
	public override int GetUserPriority(BaseUnitEntity user)
	{
		return 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		ConditionsReference condition = base.Settings.Condition;
		if (condition != null && condition.Get()?.Conditions.HasConditions == true)
		{
			using (ContextData<MechanicEntityData>.Request().Setup((MapObjectEntity)base.Owner))
			{
				using (ContextData<InteractingUnitData>.Request().Setup(user))
				{
					if (!condition.Get().Conditions.Check())
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
