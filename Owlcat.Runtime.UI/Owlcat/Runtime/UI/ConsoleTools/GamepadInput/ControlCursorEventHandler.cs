using System;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

[Serializable]
public class ControlCursorEventHandler : UnityEvent<InputActionEventData, Vector2>
{
	public Action<InputActionEventData, Vector2> Action => base.Invoke;
}
