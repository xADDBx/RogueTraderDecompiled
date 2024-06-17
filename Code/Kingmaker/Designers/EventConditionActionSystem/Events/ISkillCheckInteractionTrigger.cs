using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

public interface ISkillCheckInteractionTrigger
{
	void OnInteract(BaseUnitEntity unit, InteractionSkillCheckPart skillCheckInteraction, bool success);
}
