using System.Collections.Generic;
using System.Linq;
using Rewired;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public static class NavigationInputEventTypeHelper
{
	public static InputActionEventType GetEventType(List<NavigationInputEventTypeConfig> config, int action)
	{
		if (config == null)
		{
			return InputActionEventType.ButtonJustPressed;
		}
		return config.FirstOrDefault((NavigationInputEventTypeConfig i) => i.Action == action)?.InputActionEventType ?? InputActionEventType.ButtonJustPressed;
	}
}
