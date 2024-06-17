using System;
using Rewired;
using UniRx;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

public class BindDescription
{
	public Action<InputActionEventData> ActionHandler;

	public Action<InputActionEventData> HintsHandler;

	public InputActionEventType EventType;

	public int ActionId;

	public IReadOnlyReactiveProperty<bool> Enabled;

	public string Group;

	public int BindId;

	public bool Bound;

	public bool Debug;

	public void Handler(InputActionEventData data)
	{
		ActionHandler?.Invoke(data);
		HintsHandler?.Invoke(data);
	}
}
