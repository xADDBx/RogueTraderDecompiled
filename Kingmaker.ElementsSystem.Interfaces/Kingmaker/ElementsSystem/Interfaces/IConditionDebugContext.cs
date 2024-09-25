using JetBrains.Annotations;

namespace Kingmaker.ElementsSystem.Interfaces;

public interface IConditionDebugContext
{
	[StringFormatMethod("messageFormat")]
	void AddConditionDebugMessage(object element, bool result, string messageFormat, params object[] @params);
}
