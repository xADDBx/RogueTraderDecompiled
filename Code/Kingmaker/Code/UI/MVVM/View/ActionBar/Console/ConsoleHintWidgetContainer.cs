using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class ConsoleHintWidgetContainer : MonoBehaviour
{
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	public ConsoleHintsWidget GetConsoleHintWidget()
	{
		return m_ConsoleHintsWidget;
	}
}
