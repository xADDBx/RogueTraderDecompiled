using Kingmaker.Blueprints;

namespace Kingmaker.QA.Clockwork;

public class ClockworkAIConfig
{
	private SetAICommand m_currentAIConfig;

	public bool DialogsAI => ElementExtendAsObject.Or(m_currentAIConfig, null)?.Dialogs ?? false;

	public bool InteractionsAI => ElementExtendAsObject.Or(m_currentAIConfig, null)?.Intercations ?? false;

	public bool IsScenarioEnded
	{
		get
		{
			SetAICommand setAICommand = ElementExtendAsObject.Or(m_currentAIConfig, null);
			return ((setAICommand == null) ? null : ElementExtendAsObject.Or(setAICommand.EndCondition, null)?.Check()).GetValueOrDefault();
		}
	}

	public ClockworkAIConfig(SetAICommand newConfig)
	{
		ResetConfig(newConfig);
	}

	public void ResetConfig(SetAICommand newConfig)
	{
		m_currentAIConfig = newConfig;
	}

	public bool IsExcludedUnit(BlueprintUnit unit)
	{
		return (ElementExtendAsObject.Or(m_currentAIConfig, null)?.ExcludedUnits?.HasReference(unit)).GetValueOrDefault();
	}
}
