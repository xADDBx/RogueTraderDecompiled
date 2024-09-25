using Rewired;
using RewiredConsts;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

public class ControlButton : MonoBehaviour, IControl
{
	public ControlButtonEventHandler Handler;

	[ActionIdProperty(typeof(Action))]
	public int[] ActionIds;
}
