using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker;

public class SettingsControlSwitchMouseOverride : MonoBehaviour
{
	[SerializeField]
	public ConsoleHint LeftUpHint;

	[SerializeField]
	public ConsoleHint LeftBottomHint;

	[SerializeField]
	public ConsoleHint RightBottomHint;

	[SerializeField]
	public ConsoleHint RightUpHint;

	[SerializeField]
	public ConsoleHint RightStickButtonHint;
}
