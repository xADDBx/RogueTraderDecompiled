using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
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
		ConditionsReference condition = base.Settings.Condition;
		if ((bool)condition.Get() && condition.Get().Conditions.HasConditions)
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				if (!condition.Get().Conditions.Check())
				{
					return false;
				}
			}
		}
		return base.CanInteract();
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		if (!(base.Settings.Bark == null))
		{
			BarkPlayer.Bark(base.Settings.ShowOnUser ? ((MechanicEntity)user) : ((MechanicEntity)base.Owner), base.Settings.Bark.String, -1f, playVoiceOver: false, user);
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
