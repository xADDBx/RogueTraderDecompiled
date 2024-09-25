using System;
using Rewired;
using UnityEngine.Events;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

[Serializable]
public class ControlButtonEventHandler : UnityEvent<InputActionEventData>
{
	public Action<InputActionEventData> Action => base.Invoke;
}
