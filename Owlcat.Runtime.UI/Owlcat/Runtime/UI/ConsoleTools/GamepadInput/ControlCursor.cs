using Rewired;
using RewiredConsts;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

public class ControlCursor : MonoBehaviour, IControl
{
	public ControlCursorEventHandler Handler;

	[ActionIdProperty(typeof(Action))]
	public int AxisIdX;

	[ActionIdProperty(typeof(Action))]
	public int AxisIdY;
}
