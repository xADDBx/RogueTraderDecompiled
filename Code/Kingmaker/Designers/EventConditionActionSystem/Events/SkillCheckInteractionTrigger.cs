using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[TypeId("7b0da418b4f9e1a4781471b35b88099c")]
public class SkillCheckInteractionTrigger : EntityFactComponentDelegate, ISkillCheckInteractionTrigger, IHashable
{
	public ActionList OnSuccess;

	public ActionList OnFailure;

	private void OnInteractInternal(BaseUnitEntity unit, InteractionSkillCheckPart skillCheckInteraction, bool success)
	{
		using (ContextData<MechanicEntityData>.Request().Setup(skillCheckInteraction.Owner))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(unit))
			{
				(success ? OnSuccess : OnFailure).Run();
			}
		}
	}

	void ISkillCheckInteractionTrigger.OnInteract(BaseUnitEntity unit, InteractionSkillCheckPart skillCheckInteraction, bool success)
	{
		OnInteractInternal(unit, skillCheckInteraction, success);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
