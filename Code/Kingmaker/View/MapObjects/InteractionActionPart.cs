using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InteractionActionPart : InteractionPart<InteractionActionSettings>, IHashable
{
	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	public override bool CanInteract()
	{
		ConditionsHolder conditionsHolder = base.Settings.Condition.Get();
		if ((bool)conditionsHolder && conditionsHolder.Conditions.HasConditions)
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				if (!conditionsHolder.Conditions.Check())
				{
					return false;
				}
			}
		}
		return base.CanInteract();
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		ActionsHolder actionsHolder = base.Settings.Actions?.Get();
		if (actionsHolder != null && actionsHolder.HasActions)
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				using (ContextData<InteractingUnitData>.Request().Setup(user))
				{
					actionsHolder.Run();
				}
			}
		}
		if (base.Settings.DisableAfterUse)
		{
			base.Enabled = false;
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
