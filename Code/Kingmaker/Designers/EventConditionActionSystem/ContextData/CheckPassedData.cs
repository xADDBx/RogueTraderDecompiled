using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class CheckPassedData : ContextData<CheckPassedData>
{
	public RulePerformPartySkillCheck SkillCheck { get; private set; }

	public CheckPassedData Setup([NotNull] RulePerformPartySkillCheck skillCheck)
	{
		SkillCheck = skillCheck;
		return this;
	}

	protected override void Reset()
	{
		SkillCheck = null;
	}
}
