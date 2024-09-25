using Rewired;
using RewiredConsts;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

public class ControlAxis : MonoBehaviour, IControl
{
	public ControlAxisEventHandler Handler;

	[ActionIdProperty(typeof(Action))]
	public int AxisId;
}
