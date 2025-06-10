using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InteractionBarkPart : InteractionPart<InteractionBarkSettings>, IHashable
{
	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Info;
	}

	public override bool CanInteract()
	{
		ConditionsHolder conditionsHolder = base.Settings.Condition?.Get();
		if (conditionsHolder != null)
		{
			ConditionsChecker conditions = conditionsHolder.Conditions;
			if (conditions != null && conditions.HasConditions)
			{
				using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
				{
					if (!conditionsHolder.Conditions.Check())
					{
						return false;
					}
				}
			}
		}
		return base.CanInteract();
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		SharedStringAsset bark = base.Settings.GetBark();
		if (bark == null)
		{
			return;
		}
		BarkPlayer.Bark(base.Settings.ShowOnUser ? user : ((base.Settings.TargetUnit != null) ? ((MechanicEntity)base.Settings.TargetUnit.GetValue()) : ((MechanicEntity)((base.Settings.TargetMapObject != null) ? base.Settings.TargetMapObject.GetValue() : base.Owner))), bark.String, -1f, base.Settings.BarkPlayVoiceOver, user);
		ActionsHolder actionsHolder = base.Settings.BarkActions?.Get();
		if (actionsHolder == null)
		{
			return;
		}
		ActionList actions = actionsHolder.Actions;
		if (actions == null || !actions.HasActions || (base.Settings.RunActionsOnce && base.Settings.ActionsRan))
		{
			return;
		}
		using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(user))
			{
				actionsHolder.Actions.Run();
				base.Settings.ActionsRan = true;
			}
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
