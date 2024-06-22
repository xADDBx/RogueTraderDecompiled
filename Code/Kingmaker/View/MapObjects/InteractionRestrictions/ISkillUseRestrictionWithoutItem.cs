using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public interface ISkillUseRestrictionWithoutItem
{
	StatType Skill { get; }

	int DCOverrideValue { get; }
}
